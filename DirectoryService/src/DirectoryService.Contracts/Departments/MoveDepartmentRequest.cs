using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record MoveDepartmentRequest(Guid DepartmentId, Guid? ParentId);
}
