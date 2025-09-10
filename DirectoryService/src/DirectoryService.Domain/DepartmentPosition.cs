using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public class DepartmentPosition
    {
        public Guid DepartmentId { get; init; }

        public Guid PositionId { get; init; }
    }
}
