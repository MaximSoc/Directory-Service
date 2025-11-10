using DirectoryService.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public sealed class DepartmentPosition : ISoftDeletable
    {
        public Guid Id { get; private set; }

        public Guid DepartmentId { get; private set; }

        public Guid PositionId { get; private set; }

        public bool IsActive { get; private set; }

        public DepartmentPosition(Guid departmentId, Guid positionId)
        {
            DepartmentId = departmentId;

            PositionId = positionId;

            IsActive = true;
        }

        public void Delete()
        {
            IsActive = false;
        }

        public void Restore()
        {
            IsActive = true;
        }
    }
}
