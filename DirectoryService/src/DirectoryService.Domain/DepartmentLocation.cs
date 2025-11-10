using DirectoryService.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public sealed class DepartmentLocation : ISoftDeletable
    {
        public Guid Id { get; private set; }

        public Guid DepartmentId { get; private set; }

        public Guid LocationId { get; private set; }

        public bool IsActive { get; private set; }

        public DepartmentLocation(Guid departmentId, Guid locationId)
        {
            DepartmentId = departmentId;

            LocationId = locationId;

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
