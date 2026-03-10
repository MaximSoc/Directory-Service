using FileService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core;

public interface IReadDbContext
{
    IQueryable<MediaAsset> MediaAssetsRead { get; }
}
