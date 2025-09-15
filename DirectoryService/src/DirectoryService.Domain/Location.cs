using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain.ValueObjects.LocationVO;
using LocationTimeZone = DirectoryService.Domain.ValueObjects.LocationVO.LocationTimeZone;

namespace DirectoryService.Domain
{
    public class Location
    {
        private List<DepartmentLocation> _departmentLocations = [];

        // EF Core
        private Location()
        {
            
        }

        public Location
            (LocationName name,
            LocationAddress address,
            LocationTimeZone timezone,
            IEnumerable<DepartmentLocation> departmentLocations)
        {
            Id = Guid.NewGuid();

            Name = name;

            Address = address;

            Timezone = timezone;

            _departmentLocations = departmentLocations.ToList();

            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public LocationName Name { get; private set; } = null!;

        public LocationAddress Address { get; private set; } = null!;

        public LocationTimeZone Timezone { get; private set; } = null!;

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyList<DepartmentLocation> Departments => _departmentLocations;
    }
}
