using DirectoryService.Contracts.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentsResponse
    {
        public List<DepartmentDto> Departments { get; init; } = null!;
    }
}
