using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Locations
{
    public record GetLocationsByDepartmentResponse
    {
        public List<LocationDto> Locations { get; init; } = null!;
    }
}
