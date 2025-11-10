using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Domain.Shared
{
    public interface ISoftDeletable
    {
        void Delete();

        void Restore();
    }
}
