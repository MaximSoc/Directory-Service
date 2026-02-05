using Core.Database;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Positions
{ 
    public class GetPositionsHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<GetPositionsHandler> _logger;

        public GetPositionsHandler(IDbConnectionFactory dbConnectionFactory, ILogger<GetPositionsHandler> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<Result<GetPositionsResponse, Errors>> Handle(GetPositionsRequest request, CancellationToken cancellationToken)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var parameters = new DynamicParameters();

            var conditions = new List<string>();

            var departmentIds = request.DepartmentIds;

            if (departmentIds != null && departmentIds.Count != 0)
            {
                conditions.Add("dp.department_id = ANY(@departmentIds)");
                parameters.Add("departmentIds", request.DepartmentIds);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                conditions.Add("p.name ILIKE @search");
                parameters.Add("search", $"%{request.Search}%", DbType.String);
            }

            if (request.IsActive != null)
            {
                conditions.Add("""p."is_active" = @isActive""");
                parameters.Add("isActive", request.IsActive, DbType.Boolean);
            }

            parameters.Add("pageSize", request.PageSize, DbType.Int32);
            parameters.Add("page", (request.Page - 1) * request.PageSize, DbType.Int32);

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            var direction = request.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

            var orderByField = request.SortBy?.ToLower() switch
            {
                "name" => "p.name",
                "date" => "p.created_at",
                "isactive" => "p.is_active",
                _ => "p.name"
            };

            var orderByClause = $"ORDER BY {orderByField} {direction}, p.id ASC";

            var totalQuery = "";

            var totalCount = 0;

            var totalPages = 0;

            IEnumerable<PositionDto> positions = [];

            if (request.DepartmentIds.Count == 0)
            {
                totalQuery = $@"
                SELECT COUNT(*) 
                FROM positions p
                {whereClause}";

                totalCount = await connection.ExecuteScalarAsync<int>(totalQuery, parameters);

                totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                positions = await connection.QueryAsync<PositionDto>($"""
                SELECT p.id,
                p.name,
                p.description,
                p.is_active AS isActive,
                p.created_at AS createdAt,
                p.updated_at AS updatedAt,
                (
                    SELECT COUNT(*)
                    FROM department_positions dp
                    WHERE dp.position_id = p.id AND dp.is_active = true
                ) AS DepartmentCount,
                COALESCE(
                    ARRAY(
                        SELECT d.name 
                        FROM department_positions dp 
                        JOIN departments d ON d.id = dp.department_id 
                        WHERE dp.position_id = p.id AND dp.is_active = true
                    ), 
                    ARRAY[]::text[]
                ) AS DepartmentNames
                FROM positions p
                {whereClause}
                {orderByClause}
                LIMIT @pageSize OFFSET @page
                """,
                parameters);

                return new GetPositionsResponse
                {
                    Positions = positions.ToList(),
                    TotalPages = totalPages,
                    Page = request.Page,
                };
            }

            totalQuery = $@"
                SELECT COUNT(*) 
                FROM positions p 
                JOIN department_positions dp ON p.id = dp.position_id
                {whereClause}"
                ;

            totalCount = await connection.ExecuteScalarAsync<int>(totalQuery, parameters);

            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            positions = await connection.QueryAsync<PositionDto>($"""
                SELECT p.id,
                p.name,
                p.description,
                p.is_active AS isActive,
                p.created_at AS createdAt,
                p.updated_at AS updatedAt,
                (
                    SELECT COUNT(*)
                    FROM department_positions dp
                    WHERE dp.position_id = p.id AND dp.is_active = true
                ) AS DepartmentCount,
                COALESCE(
                    ARRAY(
                        SELECT d.name 
                        FROM department_positions dp 
                        JOIN departments d ON d.id = dp.department_id 
                        WHERE dp.position_id = p.id AND dp.is_active = true
                    ), 
                    ARRAY[]::text[]
                ) AS DepartmentNames
                FROM positions p
                JOIN department_positions dp ON p.id = dp.position_id
                {whereClause}
                {orderByClause}
                LIMIT @pageSize OFFSET @page
                """,
                parameters);

            return new GetPositionsResponse
            {
                Positions = positions.ToList(),
                TotalPages = totalPages,
                Page = request.Page,
            };
        }
    }
}
