using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentWithHasMoreChildrenDto : DepartmentDto
    {
        public bool HasMoreChildren { get; init; }
    }
}
