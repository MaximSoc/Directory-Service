using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Positions
{
    public record PositionDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public string? Description { get; init; }

        public bool IsActive { get; init; }

        public DateTime CreatedAt { get; init; }

        public DateTime UpdatedAt { get; init; }

        public List<PositionDepartmentDto> Departments { get; set; } = [];

        public int DepartmentCount => Departments.Count;
    }
}
