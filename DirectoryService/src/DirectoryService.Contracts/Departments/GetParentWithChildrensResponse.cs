using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetParentWithChildrensResponse
    {
        public List<DepartmentWithHasMoreChildrenDto> DepartmentsWithHasMoreChildren { get; init; } = null!;
    }
}
