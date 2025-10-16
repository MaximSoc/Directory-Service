using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Database
{
    public interface ILocationsRepository
    {
        Task<Result<Guid, Errors>> Add(Location location, CancellationToken cancellationToken);
        Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> locationIds, CancellationToken cancellationToken);
    }
}
