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
    }
}
