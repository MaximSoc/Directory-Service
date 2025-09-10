using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain.ValueObjects.LocationVO;
using TimeZone = DirectoryService.Domain.ValueObjects.LocationVO.TimeZone;

namespace DirectoryService.Domain
{
    public class Location
    {
        private List<DepartmentLocation> _departmentLocations = [];
        public Location
            (LocationName name,
            Address address,
            TimeZone timezone,
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

        public LocationName Name { get; private set; }

        public Address Address { get; private set; }

        public TimeZone Timezone { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyList<DepartmentLocation> Departments => _departmentLocations;
    }
}
