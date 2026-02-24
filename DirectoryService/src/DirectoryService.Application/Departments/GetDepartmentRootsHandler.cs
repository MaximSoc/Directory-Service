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
    public record GetDepartmentRootsQuery(GetDepartmentRootsRequest Request) : IQuery;

    public class GetDepartmentRootsHandler : IQueryHandler<PaginationResponse<DepartmentWithHasMoreChildrenDto>, GetDepartmentRootsQuery>
    {
        private readonly IReadDbContext _dbContext;
        private readonly HybridCache _cache;

        public GetDepartmentRootsHandler(IReadDbContext dbContext, HybridCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }
        public async Task<Result<PaginationResponse<DepartmentWithHasMoreChildrenDto>, Errors>> Handle(GetDepartmentRootsQuery query, CancellationToken cancellationToken)
        {
            var size = query.Request.PageSize;

            var page = query.Request.Page;

            var cacheKey = DepartmentCacheKeys.GetRootsKey(page, size);

            return await _cache.GetOrCreateAsync<PaginationResponse<DepartmentWithHasMoreChildrenDto>>(
                cacheKey,
                ct => GetDepartmentRootsFromDb(page, size, ct),
                tags: new[] { Constants.DEPARTMENTS_CACHE_TAG },
                cancellationToken: cancellationToken);
        }

        private async ValueTask<PaginationResponse<DepartmentWithHasMoreChildrenDto>> GetDepartmentRootsFromDb(
            int page,
            int size,
            CancellationToken ct)
        {
            var offset = (page - 1) * size;
            var connection = _dbContext.Connection;

            var totalCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM departments WHERE parent_id IS NULL");

            var roots = await connection.QueryAsync<DepartmentWithHasMoreChildrenDto>(
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

            return new PaginationResponse<DepartmentWithHasMoreChildrenDto>(
                roots.ToList(),
                totalCount,
                page,
                size);
        }
    }
}
