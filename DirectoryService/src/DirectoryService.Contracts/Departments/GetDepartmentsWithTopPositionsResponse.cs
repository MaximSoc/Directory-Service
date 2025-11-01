using DirectoryService.Contracts.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentsWithTopPositionsResponse
    {
        public List<DepartmentDto> DepartmentsWithTopPositions { get; init; } = null!;
    }
}
