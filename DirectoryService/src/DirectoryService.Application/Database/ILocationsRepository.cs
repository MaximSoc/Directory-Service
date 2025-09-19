using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain;

namespace DirectoryService.Application.Database
{
    public interface ILocationsRepository
    {
        Task<Guid> Add(Location location, CancellationToken cancellationToken);
    }
}
