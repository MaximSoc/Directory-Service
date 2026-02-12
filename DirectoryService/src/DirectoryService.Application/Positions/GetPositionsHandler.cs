using Core.Database;
using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts;
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
    public class GetPositionsHandler : IQueryHandler<PaginationResponse<PositionDto>, GetPositionsRequest>
    {
        private readonly IReadDbContext _dbContext;
        private readonly ILogger<GetPositionsHandler> _logger;

        public GetPositionsHandler(IReadDbContext dbContext, ILogger<GetPositionsHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<PaginationResponse<PositionDto>, Errors>> Handle(GetPositionsRequest request, CancellationToken cancellationToken)
        {
            var connection = _dbContext.Connection;

            var parameters = new DynamicParameters();

            var searchConditions = new List<string>();

            var departmentIds = request.DepartmentIds;

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                searchConditions.Add("p.name ILIKE @search");
                parameters.Add("search", $"%{request.Search}%", DbType.String);
            }

            if (request.IsActive != null)
            {
                searchConditions.Add("""p."is_active" = @isActive""");
                parameters.Add("isActive", request.IsActive, DbType.Boolean);
            }

            if (request.DepartmentIds.Count > 0)
            {
                searchConditions.Add(@"EXISTS (
                    SELECT 1 FROM department_positions dp 
                    WHERE dp.position_id = p.id AND dp.department_id = ANY(@departmentIds) AND dp.is_active = true
                )");
                parameters.Add("departmentIds", request.DepartmentIds.ToArray());
            }

            var whereClause = searchConditions.Count > 0 ? "WHERE " + string.Join(" AND ", searchConditions) : "";

            var totalCountQuery = $"SELECT COUNT(*) FROM positions p {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(totalCountQuery, parameters);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var direction = request.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

            var orderByField = request.SortBy?.ToLower() switch
            {
                "name" => "name",
                "date" => "created_at",
                "isactive" => "is_active",
                _ => "name"
            };

            parameters.Add("pageSize", request.PageSize);
            parameters.Add("offset", (request.Page - 1) * request.PageSize);

            var mainQuery = $"""
                WITH paginated_positions AS (
                    SELECT p.id, p.name, p.description, p.is_active, p.created_at, p.updated_at
                    FROM positions p
                    {whereClause}
                    ORDER BY p.{orderByField} {direction}, p.id ASC
                    LIMIT @pageSize OFFSET @offset
                )
                SELECT 
                    pp.id, pp.name, pp.description, pp.is_active AS isActive, pp.created_at AS createdAt, pp.updated_at AS updatedAt,
                    d.id, d.name
                FROM paginated_positions pp
                LEFT JOIN department_positions dp ON dp.position_id = pp.id AND dp.is_active = true
                LEFT JOIN departments d ON d.id = dp.department_id AND d.is_active = true
                ORDER BY pp.{orderByField} {direction}, pp.id ASC
                """;

            var positionDict = new Dictionary<Guid, PositionDto>();

            await connection.QueryAsync<PositionDto, PositionDepartmentDto, PositionDto>(
                mainQuery,
                (pos, dept) =>
                {
                    if (!positionDict.TryGetValue(pos.Id, out var existingPosition))
                    {
                        existingPosition = pos;
                        positionDict.Add(pos.Id, existingPosition);
                    }

                    if (dept != null)
                    {
                        existingPosition.Departments.Add(dept);
                    }

                    return existingPosition;
                },
                parameters,
                splitOn: "id"
            );

            return new PaginationResponse<PositionDto>
            (
                positionDict.Values.ToList(),
                totalCount,
                request.Page,
                request.PageSize,
                totalPages              
            );
        }
    }
}
