using Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentsRequest : IQuery
    {
        public string? Search { get; init; }
        public bool? IsActive { get; init; }

        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;

        public string? SortBy { get; init; }

        public string? SortDirection { get; init; }

        public Guid? ParentId { get; init; }

        public List<Guid> LocationIds { get; init; } = [];
    }
}
