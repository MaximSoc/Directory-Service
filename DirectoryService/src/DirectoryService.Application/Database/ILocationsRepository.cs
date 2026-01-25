using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using SharedKernel;
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
        Task<UnitResult<Error>> SoftDelete(Guid departmentId, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> RemoveInactive(CancellationToken cancellationToken);
        Task<Result<Location, Errors>> GetById(Guid? locationId, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> SaveChanges(CancellationToken cancellationToken);
    }
}
