using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.PositionVO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;
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

        public PositionRepository(DirectoryServiceDbContext dbContext, ILogger<PositionRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
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
    }
}
