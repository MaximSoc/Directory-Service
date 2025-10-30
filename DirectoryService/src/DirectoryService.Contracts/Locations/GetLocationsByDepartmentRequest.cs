using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Locations
{
    public record GetLocationsByDepartmentRequest
    {
        public List<Guid> DepartmentIds { get; init; } = [];
        public string? Search { get; init; }
        public bool? IsActive { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;

        public string? SortBy { get; init; }

        public string? SortDirection { get; init; }
    }
}
