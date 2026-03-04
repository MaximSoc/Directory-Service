using CSharpFunctionalExtensions;
using FileService.Core.MediaAssets;
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
using static FileService.Core.Features.DeleteFile;
using static FileService.Core.Features.DownloadFileEndPoint;

namespace FileService.Core.Features
{
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
            private readonly ILogger<DownloadFileHandler> _logger;

            public GeneratePresignedDownloadUrlHandler(
                IS3Provider s3Provider,
                IMediaRepository mediaRepository,
                ILogger<DownloadFileHandler> logger)
            {
                _s3Provider = s3Provider;
                _mediaRepository = mediaRepository;
                _logger = logger;
            }

            public async Task<Result<string, Errors>> Handle(GeneratePresignedDownloadUrlRequest request, CancellationToken cancellationToken)
            {
                var mediaAssetResult = await _mediaRepository.GetById(request.FileId, cancellationToken);
                if (mediaAssetResult.IsFailure)
                    return mediaAssetResult.Error;

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
}
