using Core.Database;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.PositionVO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.Repositories
{
    public class PositionRepository : IPositionsRepository
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<PositionRepository> _logger;
        private readonly IDbConnectionFactory _connectionFactory;

        public PositionRepository(
            DirectoryServiceDbContext dbContext,
            ILogger<PositionRepository> logger,
            IDbConnectionFactory connectionFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<Result<Guid, Errors>> Add(Position position, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Positions.AddAsync(position, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return position.Id;
            }

            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                _logger.LogWarning(ex, "Position already exists");

                return GeneralErrors.AlreadyExist().ToErrors();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to insert position");

                return GeneralErrors.Failure().ToErrors();
            }
        }

        private bool IsDuplicateKeyException(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgex)
            {
                return pgex.SqlState == PostgresErrorCodes.UniqueViolation;
            }

            return false;
        }

        public async Task<Result<bool, Errors>> PositionExistAsync(PositionName positionName, CancellationToken cancellationToken)
        {
            var existingPosition = await _dbContext.Positions.FirstOrDefaultAsync(p => p.Name == positionName && p.IsActive == true);

            if (existingPosition != null)
                return false;

            return true;
        }

        public async Task<UnitResult<Error>> SoftDelete(Guid departmentId, CancellationToken cancellationToken)
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                UPDATE positions p
                SET is_active = false,
                deleted_at = NOW() AT TIME ZONE 'UTC'
                FROM department_positions dp
                WHERE p.id = dp.position_id
                AND dp.department_id = {departmentId}
                AND NOT EXISTS (
                SELECT 1
                FROM department_positions dp2
                JOIN departments d ON d.id = dp2.department_id
                WHERE dp2.position_id = p.id
                AND d.is_active = true
                AND dp2.department_id <> {departmentId}
                )
                """,
                cancellationToken);

            return UnitResult.Success<Error>();
        }

        public async Task<UnitResult<Errors>> RemoveInactive(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.Database.ExecuteSqlAsync(
                    $"""
                    DELETE FROM positions p
                    WHERE p.is_active = false
                    AND NOT EXISTS (
                    SELECT 1
                    FROM department_positions dp
                    WHERE dp.position_id = p.id
                    )
                    """);

                return UnitResult.Success<Errors>();
            }
            catch
            {
                return UnitResult.Failure<Errors>(GeneralErrors.Failure());
            }
        }
    }
}
