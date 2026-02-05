using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Positions
{
    public record UpdatePositionRequest(
        string Name,
        string? Description,
        List<Guid> DepartmentIds);
}
