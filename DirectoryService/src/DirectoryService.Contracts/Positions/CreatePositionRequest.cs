using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Positions
{
    public record CreatePositionRequest(string Name, string? Description, List<Guid> DepartmentsIds);
}
