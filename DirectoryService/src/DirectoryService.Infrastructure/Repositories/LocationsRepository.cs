using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;

namespace DirectoryService.Infrastructure.Repositories
{
    public class LocationsRepository : ILocationsRepository
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<LocationsRepository> _logger;
        private readonly IDbConnectionFactory _connectionFactory;

        public LocationsRepository(
            DirectoryServiceDbContext dbContext,
            ILogger<LocationsRepository> logger,
            IDbConnectionFactory connectionFactory)
        {
            _dbContext = dbContext;
            _logger = logger;
            _connectionFactory = connectionFactory;
        }

        public async Task<Result<Guid, Errors>> Add(Location location, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Locations.AddAsync(location, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return location.Id;
            }

            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                _logger.LogWarning(ex, "Location already exists");

                return GeneralErrors.AlreadyExist().ToErrors();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to insert location");

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

        public async Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> locationIds, CancellationToken cancellationToken)
        {
            if (locationIds.Count == 0)
                return GeneralErrors.ValueIsRequired("Locations").ToErrors();

            var existingCount = await _dbContext.Locations
                .Where(l => locationIds.Contains(l.Id))
                .CountAsync(cancellationToken);

            return existingCount == locationIds.Count;
        }

        public async Task<UnitResult<Error>> SoftDelete(Guid departmentId, CancellationToken cancellationToken)
        {
            await _dbContext.Database.ExecuteSqlAsync(
                $"""
                UPDATE locations l
                SET is_active = false,
                deleted_at = NOW() AT TIME ZONE 'UTC'
                FROM department_locations dl
                WHERE l.id = dl.location_id
                AND dl.department_id = {departmentId}
                AND NOT EXISTS (
                SELECT 1
                FROM department_locations dl2
                JOIN departments d ON d.id = dl2.department_id
                WHERE dl2.location_id = l.id
                AND d.is_active = true
                AND dl2.department_id <> {departmentId}
                )
                """,
                cancellationToken);

            return UnitResult.Success<Error>();
        }
    }
}
