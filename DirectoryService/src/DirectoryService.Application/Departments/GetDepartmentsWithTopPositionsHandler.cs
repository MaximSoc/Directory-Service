using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public class GetDepartmentsWithTopPositionsHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetDepartmentsWithTopPositionsHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<GetDepartmentsWithTopPositionsResponse?> Handle(CancellationToken cancellationToken)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

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
        }
    }
}
