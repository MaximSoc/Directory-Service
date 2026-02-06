using Core.Database;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public class GetDepartmentsHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<GetDepartmentsHandler> _logger;

        public GetDepartmentsHandler(IDbConnectionFactory dbConnectionFactory, ILogger<GetDepartmentsHandler> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<Result<GetDepartmentsResponse, Errors>> Handle(GetDepartmentsRequest request, CancellationToken cancellationToken)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var parameters = new DynamicParameters();

            var conditions = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                conditions.Add("d.name ILIKE @search");
                parameters.Add("search", $"%{request.Search}%", DbType.String);
            }

            if (request.IsActive != null)
            {
                conditions.Add("""d."is_active" = @isActive""");
                parameters.Add("isActive", request.IsActive, DbType.Boolean);
            }

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            IEnumerable<DepartmentDto> departments = await connection.QueryAsync<DepartmentDto>($"""
                SELECT d.id,
                d.name,
                d.identifier,
                d.parent_id AS parentId,
                d.path,
                d.depth,
                d.is_active AS isActive,
                d.created_at AS createdAt,
                d.updated_at AS updatedAt
                FROM departments d
                {whereClause}
                ORDER BY d.name
                """,
                parameters);

            return new GetDepartmentsResponse
            {
                Departments = departments.ToList()
            };
        }

    }
}
