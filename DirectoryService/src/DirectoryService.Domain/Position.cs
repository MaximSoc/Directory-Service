using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectoryService.Domain.ValueObjects.PositionVO;

namespace DirectoryService.Domain
{
    public class Position
    {
        private List<DepartmentPosition> _departmentPositions = [];

        // EF Core
        private Position()
        {
            
        }

        public Position(Guid id, PositionName name, PositionDescription? description, IEnumerable<DepartmentPosition> departmentPositions)
        {
            Id = id;

            Name = name;

            Description = description;

            CreatedAt = DateTime.UtcNow;

            IsActive = true;

            _departmentPositions = departmentPositions.ToList();
        }
        public Guid Id { get; private set; }

        public PositionName Name { get; private set; } = null!;

        public PositionDescription? Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;
    }
}
