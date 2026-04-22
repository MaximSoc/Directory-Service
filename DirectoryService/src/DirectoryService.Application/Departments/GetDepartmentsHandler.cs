using Core.Database;
using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using FileService.Communication;
using FileService.Contracts.Dtos;
using FileService.Contracts.MediaAssets.Requests;
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
    public class GetDepartmentsHandler : IQueryHandler<PaginationResponse<DepartmentDto>, GetDepartmentsRequest>
    {
        private readonly IReadDbContext _dbContext;

        private readonly ILogger<GetDepartmentsHandler> _logger;

        private readonly IFileCommunicationService _fileCommunicationService;

        public GetDepartmentsHandler(
            IReadDbContext dbContext,
            ILogger<GetDepartmentsHandler> logger,
            IFileCommunicationService fileCommunicationService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _fileCommunicationService = fileCommunicationService;
        }

        public async Task<Result<PaginationResponse<DepartmentDto>, Errors>> Handle(GetDepartmentsRequest request, CancellationToken cancellationToken)
        {
            var connection = _dbContext.Connection;

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

            if (request.ParentId != null)
            {
                conditions.Add("""d."parent_id" = @parentId""");
                parameters.Add("parentId", request.ParentId);
            }

            if (request.LocationIds.Count > 0)
            {
                conditions.Add(@"EXISTS (
                    SELECT 1 FROM department_locations dl 
                    WHERE dl.department_id = d.id AND dl.location_id = ANY(@locationIds) AND dl.is_active = true
                )");
                parameters.Add("locationIds", request.LocationIds.ToArray());
            }

            var whereClause = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";

            var totalCountQuery = $"SELECT COUNT(*) FROM departments d {whereClause}";
            var totalCount = await connection.ExecuteScalarAsync<int>(totalCountQuery, parameters);
            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var direction = request.SortDirection?.ToLower() == "asc" ? "ASC" : "DESC";

            var orderByField = request.SortBy?.ToLower() switch
            {
                "name" => "name",
                "path" => "path",
                "createdat" => "created_at",
                _ => "name"
            };

            parameters.Add("pageSize", request.PageSize);
            parameters.Add("offset", (request.Page - 1) * request.PageSize);

            var mainQuery = $"""
                WITH paginated_departments AS (
                    SELECT d.id,
                    d.name,
                    d.identifier,
                    d.parent_id,
                    d.path,
                    d.depth,
                    d.is_active,
                    d.created_at,
                    d.updated_at,
                    d.video_id
                    FROM departments d
                    {whereClause}
                    ORDER BY d.{orderByField} {direction}, d.id ASC
                    LIMIT @pageSize OFFSET @offset
                )
                SELECT 
                    pd.id,
                    pd.name,
                    pd.identifier,
                    pd.parent_id AS parentId,
                    pd.path,
                    pd.depth,
                    pd.is_active AS isActive,
                    pd.created_at AS createdAt,
                    pd.updated_at AS updatedAt,
                    pd.video_id AS videoId,
                    l.id,
                    l.name
                FROM paginated_departments pd
                LEFT JOIN department_locations dl ON dl.department_id = pd.id AND dl.is_active = true
                LEFT JOIN locations l ON l.id = dl.location_id AND l.is_active = true
                ORDER BY pd.{orderByField} {direction}, pd.id ASC
                """;

            var departmentDict = new Dictionary<Guid, DepartmentDto>();

            var departments = await connection.QueryAsync<DepartmentDto, Guid?, DepartmentLocationDto, DepartmentDto>(
                mainQuery,
                (dept, videoId, loc) =>
                {
                    if (!departmentDict.TryGetValue(dept.Id, out var existingDepartment))
                    {
                        existingDepartment = dept;
                        if (videoId != null)
                        {
                            existingDepartment.Video = new MediaDto { Id = videoId.Value };
                        }
                        departmentDict.Add(dept.Id, existingDepartment);
                    }

                    if (loc != null)
                    {
                        existingDepartment.Locations.Add(loc);
                    }

                    return existingDepartment;
                },
                parameters,
                splitOn: "videoId,id"
            );

            IReadOnlyList<Guid> mediaAssetIds = departments
                .Where(d => d.Video != null && d.Video.Id.HasValue)
                .Select(d => d.Video!.Id!.Value)
                .ToList();

            _logger.LogInformation("Found {Count} unique video IDs to fetch: {Ids}",
                mediaAssetIds.Count, string.Join(", ", mediaAssetIds));

            var mediaAssets = await _fileCommunicationService.GetMediaAssetsInfo(new GetMediaAssetInfoBatchRequest(mediaAssetIds), cancellationToken);

            if (mediaAssets.IsFailure)
            {
                _logger.LogError("File service call failed: {Error}", mediaAssets.Error);
                return new PaginationResponse<DepartmentDto>
                (
                    departmentDict.Values.ToList(),
                    totalCount,
                    request.Page,
                    request.PageSize
                );
            }

            var receivedAssets = mediaAssets.Value.MediaAssets;
            _logger.LogInformation("Received {Count} assets from file service", receivedAssets.Count);

            var mediaAssetsDict = mediaAssets.Value.MediaAssets.ToDictionary(x => x.Id, x => x);

            foreach (DepartmentDto departmentDto in departmentDict.Values)
            {
                if (departmentDto.Video != null && departmentDto.Video.Id.HasValue
                    && mediaAssetsDict.TryGetValue(departmentDto.Video.Id.Value, out GetMediaAssetsDto? mediaAsset))
                {
                    departmentDto.Video = new MediaDto
                    {
                        Id = mediaAsset.Id,
                        Status = mediaAsset.Status,
                        Url = mediaAsset.DownloadUrl,
                    };
                }
            }


            return new PaginationResponse<DepartmentDto>
            (
                departmentDict.Values.ToList(),
                totalCount,
                request.Page,
                request.PageSize);
        }

    }
}
