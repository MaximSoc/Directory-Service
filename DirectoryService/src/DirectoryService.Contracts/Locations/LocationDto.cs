using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Locations
{
    public record LocationDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string Country { get; init; } = null!;
        public string Region { get; init; } = null!;
        public string City { get; init; } = null!;
        public string PostalCode { get; init; } = null!;
        public string Street { get; init; } = null!;
        public string ApartamentNumber { get; init; } = null!;

        public string Timezone { get; init; } = null!;

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }
    }
}
