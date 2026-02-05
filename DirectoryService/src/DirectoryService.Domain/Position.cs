using CSharpFunctionalExtensions;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.PositionVO;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain
{
    public class Position : ISoftDeletable
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

            UpdatedAt = DateTime.UtcNow;

            IsActive = true;

            _departmentPositions = departmentPositions.ToList();
        }
        public Guid Id { get; private set; }

        public PositionName Name { get; private set; } = null!;

        public PositionDescription? Description { get; private set; }

        public bool IsActive { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime UpdatedAt { get; private set; }

        public DateTime? DeletedAt { get; private set; } = null;

        public IReadOnlyList<DepartmentPosition> DepartmentPositions => _departmentPositions;

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

        public UnitResult<Error> Update(
            PositionName newName,
            PositionDescription? newDescription,
            IEnumerable<DepartmentPosition> newPositions)
        {
            bool isUpdated = false;

            if (newName != Name)
            {
                Name = newName;
                isUpdated = true;
            }

            if (newDescription != Description)
            {
                Description = newDescription;
                isUpdated = true;
            }

            if (newPositions != DepartmentPositions)
            {
                _departmentPositions = newPositions.ToList();
                isUpdated = true;
            }

            if (isUpdated)
            {
                UpdatedAt = DateTime.UtcNow;
            }

            return UnitResult.Success<Error>();
        }
    }
}
