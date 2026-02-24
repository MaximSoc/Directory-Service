using Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetDepartmentRootsRequest : IQuery
    {
        public int Page { get; init; } = 1;

        public int PageSize { get; init; } = 20;
    }
}
