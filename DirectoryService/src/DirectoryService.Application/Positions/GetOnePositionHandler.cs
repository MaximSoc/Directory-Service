using Core.Database;
using CSharpFunctionalExtensions;
using Dapper;
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

    public class GetOnePositionHandler
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly ILogger<GetPositionsHandler> _logger;

        public GetOnePositionHandler(IDbConnectionFactory dbConnectionFactory, ILogger<GetPositionsHandler> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _logger = logger;
        }

        public async Task<Result<GetOnePositionResponse, Errors>> Handle(Guid positionId, CancellationToken cancellationToken)
        {
            using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

            var position = await connection.QuerySingleOrDefaultAsync<PositionDto>($"""
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
                        WHERE dp.position_id = p.id AND dp.is_active = true AND d.is_active = true
                    ), 
                    ARRAY[]::text[]
                ) AS DepartmentNames,
                COALESCE(
                    ARRAY(
                        SELECT d.id 
                        FROM department_positions dp 
                        JOIN departments d ON d.id = dp.department_id 
                        WHERE dp.position_id = p.id AND dp.is_active = true AND d.is_active = true
                    ), 
                    ARRAY[]::uuid[]
                ) AS DepartmentIds
                FROM positions p
                WHERE p.id = @Id
                """,
                new {Id = positionId});

            if (position is null)
            {
                return GeneralErrors.NotFound(positionId).ToErrors();
            }

            return new GetOnePositionResponse
            {
                Position = position
            };
        }
    }
}
