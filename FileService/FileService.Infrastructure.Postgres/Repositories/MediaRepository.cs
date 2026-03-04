using CSharpFunctionalExtensions;
using FileService.Core.MediaAssets;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Infrastructure.Postgres.Repositories
{
    public class MediaRepository : IMediaRepository
    {
        private readonly FileServiceDbContext _dbContext;

        public MediaRepository(FileServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Guid> AddAsync(MediaAsset mediaAsset, CancellationToken cancellationToken = default)
        {
            await _dbContext.MediaAssets.AddAsync(mediaAsset, cancellationToken);
            return mediaAsset.Id;
        }

        public async Task<Result<MediaAsset, Errors>> GetById(Guid? mediaAssetId, CancellationToken cancellationToken)
        {
            var mediaAsset = await _dbContext.MediaAssets
                .FirstOrDefaultAsync(ma => ma.Id == mediaAssetId, cancellationToken);

            if (mediaAsset == null)
                return GeneralErrors.NotFound(mediaAssetId).ToErrors();

            return mediaAsset;
        }

        public async Task<Result<List<MediaAsset>, Errors>> GetByIds(IEnumerable<Guid> mediaAssetIds, CancellationToken cancellationToken)
        {
            var mediaAssets = await _dbContext.MediaAssets
                .Where(ma => mediaAssetIds.Contains(ma.Id))
                .ToListAsync(cancellationToken);

            return mediaAssets;
        }
    }
}
