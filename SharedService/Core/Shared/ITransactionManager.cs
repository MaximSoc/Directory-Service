using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Shared
{
    public interface ITransactionManager
    {
        Task<Result<IDbTransaction, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
        Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
