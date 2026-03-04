using CSharpFunctionalExtensions;
using FileService.Core.MediaAssets;
using Framework.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
public sealed class DownloadFileEndPoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/download", async (
            [FromQuery] Guid fileId,
            [FromQuery] string path,
            [FromServices] DownloadFileHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.Handle(new DownloadFileRequest(fileId, path), cancellationToken);

            return Results.Ok(result.Value);
        });
    }

    public sealed record DownloadFileRequest(Guid FileId, string Path);

    public sealed class DownloadFileHandler
    {
        private readonly IS3Provider _s3Provider;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<DownloadFileHandler> _logger;

        public DownloadFileHandler(
            IS3Provider s3Provider,
            IMediaRepository mediaRepository,
            ILogger<DownloadFileHandler> logger)
        {
            _s3Provider = s3Provider;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<Result<string, Errors>> Handle(DownloadFileRequest request, CancellationToken cancellationToken)
        {
            var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == request.FileId, cancellationToken);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error;

            var downloadResult = await _s3Provider.DownloadFileAsync(
                mediaAssetResult.Value.Key,
                request.Path,
                cancellationToken);
            if (downloadResult.IsFailure)
                return downloadResult.Error.ToErrors();


            _logger.LogInformation("File with id {fileId} downloaded.", request.FileId);

            return downloadResult.Value;
        }
    }
}
