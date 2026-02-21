using Core.Database;
using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record GetChildrenByParentQuery(Guid ParentId, GetChildrenByParentRequest Request) : IQuery;

    public class GetChildrenByParentHandler : IQueryHandler<PaginationResponse<DepartmentWithHasMoreChildrenDto>, GetChildrenByParentQuery>
    {
        private readonly IReadDbContext _dbContext;
        private readonly HybridCache _cache;

        public GetChildrenByParentHandler(IReadDbContext dbContext, HybridCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }
        public async Task<Result<PaginationResponse<DepartmentWithHasMoreChildrenDto>, Errors>> Handle(GetChildrenByParentQuery query, CancellationToken cancellationToken)
        {
            var size = query.Request.PageSize;

            var page = query.Request.Page;

            var offset = (page - 1) * size;

            var parentId = query.ParentId;

            var cacheKey = $"departments_children_parent={parentId}_page={page}_size={size}";

            var children = await _cache.GetOrCreateAsync<PaginationResponse<DepartmentWithHasMoreChildrenDto>>(
                cacheKey,
                async ct =>
                {
                    var connection = _dbContext.Connection;

                    var totalCountSql = "SELECT COUNT(*) FROM departments WHERE parent_id = @parentId AND is_active = true";
                    var totalCount = await connection.ExecuteScalarAsync<int>(totalCountSql, new { parentId });

                    var totalPages = (int)Math.Ceiling(totalCount / (double)size);

                    var children = await connection.QueryAsync<DepartmentWithHasMoreChildrenDto>(
                        $"""
                        SELECT 
                            id, 
                            name, 
                            identifier, 
                            parent_id AS parentId, 
                            path, 
                            depth, 
                            is_active AS isActive, 
                            created_at AS createdAt, 
                            updated_at AS updatedAt,
                            EXISTS (SELECT 1 FROM departments WHERE parent_id = d.id AND is_active = true) AS hasMoreChildren
                        FROM departments d
                        WHERE d.parent_id = @parentId AND d.is_active = true
                        ORDER BY d.name
                        LIMIT @size OFFSET @offset
                        """,
                        new { parentId, size, offset });

                    return new PaginationResponse<DepartmentWithHasMoreChildrenDto>(
                        children.ToList(),
                        totalCount,
                        page,
                        size,
                        totalPages);
                },
                tags: new[] { Constants.DEPARTMENTS_CACHE_TAG },
                cancellationToken: cancellationToken);

            return children;
        }
    }
}
