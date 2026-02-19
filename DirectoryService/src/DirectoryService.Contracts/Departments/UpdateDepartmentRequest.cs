using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record UpdateDepartmentRequest(
        string Name,
        string Identifier,
        Guid? ParentId);
}
