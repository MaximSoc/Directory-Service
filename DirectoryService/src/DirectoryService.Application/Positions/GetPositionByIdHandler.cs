using Core.Handlers;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Positions;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Positions
{

    public class GetPositionByIdHandler : IQueryHandler<GetPositionByIdResponse, GetPositionByIdRequest>
    {
        private readonly IReadDbContext _dbContext;
        private readonly ILogger<GetPositionByIdHandler> _logger;

        public GetPositionByIdHandler(IReadDbContext dbContext, ILogger<GetPositionByIdHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<GetPositionByIdResponse, Errors>> Handle(GetPositionByIdRequest request, CancellationToken cancellationToken)
        {
            Guid positionId = request.PositionId;

            var connection = _dbContext.Connection;

            var sql = """
                SELECT
                    p.id,
                    p.name,
                    p.description,
                    p.is_active AS isActive,
                    p.created_at AS createdAt,
                    p.updated_at AS updatedAt,
                    d.id,
                    d.name
                    FROM positions p
                    LEFT JOIN department_positions dp ON dp.position_id = p.id AND dp.is_active = true
                    LEFT JOIN departments d ON d.id = dp.department_id AND d.is_active = true
                    WHERE p.id = @Id
                """;

            PositionDto? position = null;

            await connection.QueryAsync<PositionDto, PositionDepartmentDto, PositionDto>(
                sql,
                (pos, dept) =>
                {
                    if (position == null)
                        position = pos;
                    if (dept != null)
                        position.Departments.Add(dept);
                    return pos;
                },
                new { Id = positionId },
                splitOn: "id");

            if (position == null)
            {
                return GeneralErrors.NotFound(positionId).ToErrors();
            }

            return new GetPositionByIdResponse
            {
                Position = position
            };
        }
    }
}
