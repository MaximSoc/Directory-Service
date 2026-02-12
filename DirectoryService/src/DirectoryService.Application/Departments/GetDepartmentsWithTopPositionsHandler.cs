using Core.Database;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Shared;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public class GetDepartmentsWithTopPositionsHandler
    {
        private readonly IReadDbContext _dbContext;
        private readonly HybridCache _cache;

        public GetDepartmentsWithTopPositionsHandler(IReadDbContext dbContext, HybridCache cache)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<GetDepartmentsWithTopPositionsResponse?> Handle(CancellationToken cancellationToken)
        {
            var cacheKey = $"departments_top_5";

            var departmentsWithTopPositions = await _cache.GetOrCreateAsync<GetDepartmentsWithTopPositionsResponse?>(
                cacheKey,
                async ct =>
                {
                    var connection = _dbContext.Connection;

                    var departmentsWithTopPositions = await connection.QueryAsync<DepartmentWithNumberOfPositionsDto>("""
                        SELECT DISTINCT d.id,
                        d.name,
                        d.identifier,
                        d.parent_id AS parentId,
                        d.path,
                        d.depth,
                        d.is_active AS isActive,
                        d.created_at AS createdAt,
                        d.updated_at AS updatedAt,
                        dp_counts.positions_count AS numberOfPositions
                        FROM departments d
                        JOIN (
                        SELECT department_id, COUNT(*) AS positions_count
                        FROM department_positions
                        GROUP BY department_id
                        ) dp_counts ON d.id = dp_counts.department_id
                        ORDER BY dp_counts.positions_count DESC
                        LIMIT 5
                        """);

                    return new GetDepartmentsWithTopPositionsResponse
                    {
                        DepartmentsWithTopPositions = departmentsWithTopPositions.ToList()
                    };
                },
                tags: new[] { Constants.DEPARTMENTS_CACHE_TAG },
                cancellationToken: cancellationToken);

            return departmentsWithTopPositions;
        }
    }
}
