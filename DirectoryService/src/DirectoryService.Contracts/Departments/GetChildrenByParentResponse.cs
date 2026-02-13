using Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetChildrenByParentResponse
    {
        public List<DepartmentWithHasMoreChildrenDto> DepartmentsWithHasMoreChildren { get; init; } = null!;
    }
}
