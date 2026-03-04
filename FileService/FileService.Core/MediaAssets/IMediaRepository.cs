using CSharpFunctionalExtensions;
using FileService.Domain;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core.MediaAssets
{
    public interface IMediaRepository
    {
        Task<Guid> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default);

        Task<Result<MediaAsset, Errors>> GetById(Guid? mediaAssetId, CancellationToken cancellationToken);

        Task<Result<List<MediaAsset>, Errors>> GetByIds(IEnumerable<Guid> mediaAssetIds, CancellationToken cancellationToken);
    }
}
