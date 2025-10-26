using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public sealed class DepartmentPosition
    {
        public Guid Id { get; private set; }

        public Guid DepartmentId { get; private set; }

        public Guid PositionId { get; private set; }

        public DepartmentPosition(Guid departmentId, Guid positionId)
        {
            DepartmentId = departmentId;

            PositionId = positionId;
        }
    }
}
