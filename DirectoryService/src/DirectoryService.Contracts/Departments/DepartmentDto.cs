using DirectoryService.Contracts.Positions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record DepartmentDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string Identifier { get; init; } = null!;

        public Guid? ParentId { get; init; } = null;

        public string Path { get; init; } = null!;

        public int Depth { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }

        public List<DepartmentPositionDto> Positions { get; set; } = [];

        public List<DepartmentLocationDto> Locations { get; set; } = [];
    }
}
