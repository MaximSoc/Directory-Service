using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain.Shared
{
    public static class DepartmentCacheKeys
    {
        public static string GetChildrenKey(Guid parentId, int page, int size)
            => $"departments_children:parent={parentId}:page={page}:size={size}";

        public static string GetRootsKey(int page, int size)
            => $"departments_roots: page={page}_size={size}";
    }
}
