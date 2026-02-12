using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Handlers
{
    public interface IQueryHandler<TResponse, in TQuery>
        where TQuery : class
    {
        Task<Result<TResponse, Errors>> Handle(TQuery query, CancellationToken cancellationToken);
    }

    public interface IQueryHandler<TResponse>
    {
        Task<Result<TResponse, Errors>> Handle(CancellationToken cancellationToken);
    }
}
