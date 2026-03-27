using CSharpFunctionalExtensions;
using FileService.Core.FilesStorage;
using FileService.Core.MediaAssets;
using FileService.Domain;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Core.Features;

public sealed class GenerateDownloadUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}/download-url", async Task<EndpointResult<string>> (
            [FromRoute] Guid fileId,
            [FromServices] GeneratePresignedDownloadUrlHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new GeneratePresignedDownloadUrlRequest(fileId), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed record GeneratePresignedDownloadUrlRequest(Guid FileId);

    public sealed class GeneratePresignedDownloadUrlHandler
    {
        private readonly IS3Provider _s3Provider;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<GeneratePresignedDownloadUrlHandler> _logger;

        public GeneratePresignedDownloadUrlHandler(
            IS3Provider s3Provider,
            IMediaRepository mediaRepository,
            ILogger<GeneratePresignedDownloadUrlHandler> logger)
        {
            _s3Provider = s3Provider;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<Result<string, Errors>> Handle(GeneratePresignedDownloadUrlRequest request, CancellationToken cancellationToken)
        {
            var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.FileId, cancellationToken);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error;

            var mediaAsset = mediaAssetResult.Value;

            if (mediaAsset.Status != MediaAsset.MediaStatus.UPLOADED)
            {
                _logger.LogWarning("Attempt to generate download URL for file {fileId} with status {status}",
                    request.FileId, mediaAsset.Status);

                return GeneralErrors.Failure($"Файл еще не загружен или недоступен. Текущий статус: {mediaAsset.Status}")
                    .ToErrors();
            }

            var downloadUrlResult = await _s3Provider.GenerateDownloadUrlAsync(
                mediaAssetResult.Value.Key,
                cancellationToken);
            if (downloadUrlResult.IsFailure)
                return downloadUrlResult.Error.ToErrors();

            _logger.LogInformation("Generated download URL for file with id {fileId}.", request.FileId);

            return downloadUrlResult.Value;
        }
    }
}
