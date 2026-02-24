using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using DirectoryService.Domain.ValueObjects.PositionVO;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public sealed class Department : ISoftDeletable
    {
        private readonly List<Department> _childrenDepartments = [];

        private List<DepartmentLocation> _departmentLocations = [];

        private readonly List<DepartmentPosition> _departmentPositions = [];

        // EF Core
        private Department()
        {           
        }

        public Department
            (Guid id,
            DepartmentName name,
            DepartmentIdentifier identifier,
            int depth,
            DepartmentPath path,
            IEnumerable<DepartmentLocation> departmentLocations,
            Guid? parentId)
        {
            Id = id;

            Name = name;
            
            Identifier = identifier;

            ParentId = parentId;

            IsActive = true;

            Depth = depth;

            Path = path;

            _departmentLocations = departmentLocations.ToList();

            CreatedAt = DateTime.UtcNow;

            UpdatedAt = DateTime.UtcNow;
        }

        public Guid Id { get; private set; }

        public DepartmentName Name { get; private set; } = null!;

        public DepartmentIdentifier Identifier { get; private set; } = null!;

        public Guid? ParentId { get; private set; } = null;

        public DepartmentPath Path { get; private set; } = null!;

        public int Depth { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; } = null;

        public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

        public IReadOnlyList<DepartmentLocation> DepartmentLocations => _departmentLocations;

        public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

        public static Result<Department, Error> CreateParent(
            DepartmentName name,
            DepartmentIdentifier identifier,
            IEnumerable<DepartmentLocation> departmentLocations,
            Guid departmentId)
        {
            var departmentLocationsList = departmentLocations.ToList();

            if (departmentLocationsList.Count == 0)
            {
                return Error.Validation("department.location", "Department locations must contain at least one location");
            }

            Guid? parentId = null;

            var path = DepartmentPath.CreateParent(identifier);
            return new Department(departmentId ,name, identifier, 0, path, departmentLocationsList, parentId);
        }

        public static Result<Department, Error> CreateChild(
            Guid departmentId,
            DepartmentName name,
            DepartmentIdentifier identifier,
            Department parent,
            IEnumerable<DepartmentLocation> departmentLocations)
        {
            var departmentLocationsList = departmentLocations.ToList();

            if (departmentLocationsList.Count == 0)
            {
                return Error.Validation("department.location", "Department locations must contain at least one location");
            }

            var path = parent.Path.CreateChild(identifier);

            return new Department(departmentId, name, identifier, parent.Depth + 1, path, departmentLocationsList, parent.Id);
        }

        public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> newLocations)
        {
            _departmentLocations = newLocations.ToList();

            UpdatedAt = DateTime.UtcNow;

            return UnitResult.Success<Error>();
        }

        public UnitResult<Error> UpdateParent(Department? parent)
        {
            if (this == parent)
            {
                return Error.Conflict("value.is.conflict", "It is forbidden to appoint yourself as a parent");
            }

            if (ChildrenDepartments.Contains(parent))
            {
                return Error.Conflict("value.is.conflict", "It is forbidden to appoint children as a parent");
            }
            var newPath = parent is null
                ? DepartmentPath.CreateParent(Identifier)
                : parent.Path.CreateChild(Identifier);

            Path = newPath;

            var newDepth = parent is null
                ? 0
                : parent.Depth + 1;

            Depth = newDepth;

            ParentId = parent?.Id;

            UpdatedAt = DateTime.UtcNow;

            return UnitResult.Success<Error>();
        }

        public void Update(
            DepartmentName newName,
            DepartmentIdentifier newIdentifier,
            Department? parent)
        {
            Name = newName;

            Identifier = newIdentifier;

            Path = parent is null
                ? DepartmentPath.CreateParent(Identifier)
                : parent.Path.CreateChild(Identifier);

            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            IsActive = false;

            DeletedAt = DateTime.UtcNow;

            var parts = Path.Value.Split('.');
            parts[Depth] = "deleted-" + parts[Depth];
            var newPath = string.Join(".", parts);

            Path = DepartmentPath.Create(newPath).Value;
        }

        public void Restore()
        {
            IsActive = true;

            DeletedAt = DateTime.MinValue;
        }
    }
}
