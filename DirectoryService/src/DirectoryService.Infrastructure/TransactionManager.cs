using CSharpFunctionalExtensions;
using DirectoryService.Application.Shared;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure
{
    public class TransactionManager : ITransactionManager
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<TransactionManager> _logger;
        public TransactionManager(DirectoryServiceDbContext dbContext, ILogger<TransactionManager> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<IDbTransaction, Error>> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
                return transaction.GetDbTransaction();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to begin transaction");
                return Error.Failure("database", "Failed to begin transaction");
            }
            
        }

        public async Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return UnitResult.Success<Errors>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes");
                return UnitResult.Failure<Errors>(GeneralErrors.Failure());
            }
        }
    }
}
