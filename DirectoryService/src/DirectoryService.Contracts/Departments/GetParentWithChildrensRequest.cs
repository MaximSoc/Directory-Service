using Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public record GetParentWithChildrensRequest : IQuery
    {
        public int Page { get; init; } = 1;

        public int Size { get; init; } = 20;

        public int Prefetch { get; init; } = 3;
    }
}
