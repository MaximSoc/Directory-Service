using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.LocationVO;
using SharedKernel;
using LocationTimeZone = DirectoryService.Domain.ValueObjects.LocationVO.LocationTimeZone;

namespace DirectoryService.Domain
{
    public class Location : ISoftDeletable
    {
        private List<DepartmentLocation> _departmentLocations = [];

        // EF Core
        private Location()
        {
            
        }

        public Location
            (LocationName name,
            LocationAddress address,
            LocationTimeZone timezone
            )
        {
            Id = Guid.NewGuid();

            Name = name;

            Address = address;

            Timezone = timezone;

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;

            IsActive = true;
        }

        public Guid Id { get; private set; }

        public LocationName Name { get; private set; } = null!;

        public LocationAddress Address { get; private set; } = null!;

        public LocationTimeZone Timezone { get; private set; } = null!;

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; } = null;

        public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

        public UnitResult<Error> Update(
            LocationName newName, 
            LocationAddress newAddress,
            LocationTimeZone newTimezone)
        {

            bool isUpdated = false;

            if (newName != Name)
            {
                Name = newName;
                isUpdated = true;
            }

            if (newAddress != Address)
            {
                Address = newAddress;
                isUpdated = true;
            }

            if (newTimezone != Timezone)
            {
                Timezone = newTimezone;
                isUpdated = true;
            }

            if (isUpdated)
            {
                UpdatedAt = DateTime.UtcNow;
            }
            
            return UnitResult.Success<Error>();
        }

        public void Delete()
        {
            IsActive = false;

            DeletedAt = DateTime.UtcNow;
        }

        public void Restore()
        {
            IsActive = true;

            DeletedAt = DateTime.MinValue;
        }
    }
}
