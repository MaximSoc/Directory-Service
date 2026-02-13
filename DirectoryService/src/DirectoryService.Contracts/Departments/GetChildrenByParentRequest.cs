using Core.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Departments
{
    public class GetChildrenByParentRequest : IQuery
    {
        public Guid ParentId { get; init; }

        public int Page { get; init; } = 1;

        public int Size { get; init; } = 20;
    }
}
