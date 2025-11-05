using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentWithNumberOfPositionsDto : DepartmentDto
    {
        public int NumberOfPositions { get; init; }
    }
}
