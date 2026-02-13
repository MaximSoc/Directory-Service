using DirectoryService.Domain;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Database
{
    public interface IReadDbContext
    {
        IQueryable<Location> LocationsRead { get; }

        IDbConnection Connection { get; }
    }
}
