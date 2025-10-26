using CSharpFunctionalExtensions;
using Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Shared
{
    public interface ITransactionManager
    {
        Task<Result<IDbTransaction, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
        Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
