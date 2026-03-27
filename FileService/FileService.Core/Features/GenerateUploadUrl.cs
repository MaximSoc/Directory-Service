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
public sealed class GenerateUploadUrl : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/{fileId:guid}/upload-url", async Task<EndpointResult<string>> (
            [FromRoute] Guid fileId,
            [FromServices] GeneratePresignedUploadUrlHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new GeneratePresignedUploadUrlRequest(fileId), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed record GeneratePresignedUploadUrlRequest(Guid FileId);

    public sealed class GeneratePresignedUploadUrlHandler
    {
        private readonly IS3Provider _s3Provider;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<GeneratePresignedUploadUrlHandler> _logger;

        public GeneratePresignedUploadUrlHandler(
            IS3Provider s3Provider,
            IMediaRepository mediaRepository,
            ILogger<GeneratePresignedUploadUrlHandler> logger)
        {
            _s3Provider = s3Provider;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<Result<string, Errors>> Handle(GeneratePresignedUploadUrlRequest request, CancellationToken cancellationToken)
        {
            var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.FileId, cancellationToken);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error;

            var mediaAsset = mediaAssetResult.Value;

            if (mediaAsset.Status == MediaAsset.MediaStatus.UPLOADED)
            {
                return GeneralErrors.Failure("Файл уже загружен. Перезапись запрещена.").ToErrors();
            }

            var uploadUrlResult = await _s3Provider.GenerateUploadUrlAsync(
                mediaAssetResult.Value.Key,
                mediaAssetResult.Value.MediaData,
                cancellationToken);
            if (uploadUrlResult.IsFailure)
                return uploadUrlResult.Error.ToErrors();

            _logger.LogInformation("Generated upload URL for file with id {fileId}.", request.FileId);

            return uploadUrlResult.Value;
        }
    }
}
