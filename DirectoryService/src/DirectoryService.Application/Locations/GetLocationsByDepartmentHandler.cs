using Core.Database;
using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Locations
{
    public class GetLocationsByDepartmentHandler : IQueryHandler<PaginationResponse<LocationDto>, GetLocationsByDepartmentRequest>
    {
        private readonly IReadDbContext _dbContext;
        private readonly ILogger<GetLocationsByDepartmentHandler> _logger;

        public GetLocationsByDepartmentHandler(IReadDbContext dbContext, ILogger<GetLocationsByDepartmentHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task<Result<PaginationResponse<LocationDto>, Errors>> Handle(GetLocationsByDepartmentRequest request, CancellationToken cancellationToken)
        {
            var connection = _dbContext.Connection;

            var parameters = new DynamicParameters();

            var conditions = new List<string>();

            var departmentIds = request.DepartmentIds;

            if (departmentIds != null && departmentIds.Count != 0)
            {
                conditions.Add("dl.department_id = ANY(@departmentIds)");
                parameters.Add("departmentIds", request.DepartmentIds);
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                conditions.Add("l.name ILIKE @search");
                parameters.Add("search", $"%{request.Search}%", DbType.String);
            }

            if (request.IsActive != null)
            {
                conditions.Add("""l."is_active" = @isActive""");
                parameters.Add("isActive", request.IsActive, DbType.Boolean);
            }
            
            parameters.Add("pageSize", request.PageSize, DbType.Int32);
            parameters.Add("page", (request.Page - 1) * request.PageSize, DbType.Int32);

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            var direction = request.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

            var orderByField = request.SortBy?.ToLower() switch
            {
                "name" => "l.name",
                "date" => "l.created_at",
                "isactive" => "l.is_active",
                "country" => "l.country",
                "region" => "l.region",
                "city" => "l.city",
                _ => "l.name"
            };

            var orderByClause = $"ORDER BY {orderByField} {direction}, l.id ASC";

            var totalQuery = "";

            var totalCount = 0;

            var totalPages = 0;

            IEnumerable<LocationDto> locations = [];

            if (request.DepartmentIds.Count == 0)
            {
                totalQuery = $@"
                SELECT COUNT(*) 
                FROM locations l
                {whereClause}";

                totalCount = await connection.ExecuteScalarAsync<int>(totalQuery, parameters);

                totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                locations = await connection.QueryAsync<LocationDto>($"""
                SELECT l.id,
                l.name,
                l.country,
                l.region,
                l.city,
                l.postal_code AS postalCode,
                l.street,
                l.apartament_number AS apartamentNumber,
                l.timezone,
                l.is_active AS isActive,
                l.created_at AS createdAt,
                l.updated_at AS updatedAt
                FROM locations l
                {whereClause}
                {orderByClause}
                LIMIT @pageSize OFFSET @page
                """,
                parameters);

                return new PaginationResponse<LocationDto>
                (
                    locations.ToList(),
                    totalCount,
                    request.Page,
                    request.PageSize,
                    totalPages
                );
            }
            totalQuery = $@"
                SELECT COUNT(*) 
                FROM locations l 
                JOIN department_locations dl ON l.id = dl.location_id
                {whereClause}"
                ;

            totalCount = await connection.ExecuteScalarAsync<int>(totalQuery, parameters);

            totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            locations = await connection.QueryAsync<LocationDto>($"""
                SELECT l.id,
                l.name,
                l.country,
                l.region,
                l.city,
                l.postal_code AS postalCode,
                l.street,
                l.apartament_number AS apartamentNumber,
                l.timezone,
                l.is_active AS isActive,
                l.created_at AS createdAt,
                l.updated_at AS updatedAt
                FROM locations l
                JOIN department_locations dl ON l.id = dl.location_id
                {whereClause}
                {orderByClause}
                LIMIT @pageSize OFFSET @page
                """,
                parameters);

            return new PaginationResponse<LocationDto>
                (
                    locations.ToList(),
                    totalCount,
                    request.Page,
                    request.PageSize,
                    totalPages
                );
        }
    }
}
