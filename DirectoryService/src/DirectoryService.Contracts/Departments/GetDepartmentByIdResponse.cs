using DirectoryService.Contracts.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentByIdResponse
    {
        public DepartmentDto Department { get; init; } = null!;
    }
}
