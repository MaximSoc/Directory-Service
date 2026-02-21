using Core.Database;
using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using NodaTime;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record GetRootsQuery(GetRootsRequest Request) : IQuery;

    public class GetRootsHandler : IQueryHandler<PaginationResponse<DepartmentWithHasMoreChildrenDto>, GetRootsQuery>
    {
        private readonly IReadDbContext _dbContext;
        private readonly HybridCache _cache;

        public GetRootsHandler(IReadDbContext dbContext, HybridCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }
        public async Task<Result<PaginationResponse<DepartmentWithHasMoreChildrenDto>, Errors>> Handle(GetRootsQuery query, CancellationToken cancellationToken)
        {
            var size = query.Request.PageSize;

            var page = query.Request.Page;

            var offset = (page - 1) * size;

            var cacheKey = $"department_with_children_page={page}_size={size}";

            var rootsWithHasMoreChildren = await _cache.GetOrCreateAsync<PaginationResponse<DepartmentWithHasMoreChildrenDto>>(
                cacheKey,
                async ct =>
                {
                    var connection = _dbContext.Connection;

                    var totalCount = await connection.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM departments WHERE parent_id IS NULL");

                    var rootsWithHasMoreChildren = await connection.QueryAsync<DepartmentWithHasMoreChildrenDto>(
                        $"""
                        SELECT 
                            d.id,
                            d.name,
                            d.identifier,
                            d.parent_id AS parentId,
                            d.path,
                            d.depth,
                            d.is_active AS isActive,
                            d.created_at AS createdAt,
                            d.updated_at AS updatedAt,
                            EXISTS (SELECT 1 FROM departments WHERE parent_id = d.id) AS hasMoreChildren
                        FROM departments d
                        WHERE d.parent_id IS NULL
                        ORDER BY d.name
                        LIMIT @size OFFSET @offset
                        """, new { size, offset });

                    var totalPages = (int)Math.Ceiling((double)totalCount / size);

                    return new PaginationResponse<DepartmentWithHasMoreChildrenDto>(
                        rootsWithHasMoreChildren.ToList(),
                        totalCount,
                        page,
                        size,
                        totalPages);
                },
                tags: new[] { Constants.DEPARTMENTS_CACHE_TAG },
                cancellationToken: cancellationToken);

            return rootsWithHasMoreChildren;
        }
    }
}
