using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using Path = DirectoryService.Domain.ValueObjects.DepartmentVO.Path;

namespace DirectoryService.Domain
{
    public class Department
    {
        private List<Department> _children = [];

        private List<DepartmentLocation> _departmentLocations = [];

        private List<DepartmentPosition> _departmentPositions = [];

        public Department
            (DepartmentName name,
            DepartmentIdentifier identifier,
            short depth,
            Guid? parentId,
            Path path,
            IEnumerable<DepartmentLocation> departmentLocations,
            IEnumerable<DepartmentPosition> departmentPositions)
        {
            Id = Guid.NewGuid();

            Name = name;
            
            Identifier = identifier;

            Depth = depth;

            if (parentId != null)
            {
                ParentId = parentId;
            }

            Path = path;

            _departmentLocations = departmentLocations.ToList();

            _departmentPositions = departmentPositions.ToList();

            CreatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public DepartmentName Name { get; private set; }

        public DepartmentIdentifier Identifier { get; private set; }

        public Guid? ParentId { get; private set; } = null;

        public IReadOnlyList<Department> Children => _children;

        public Path Path { get; private set; }

        public short Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

        public IReadOnlyList<DepartmentPosition> Positions => _departmentPositions;
    }
}
