using CSharpFunctionalExtensions;
using FileService.Core.FilesStorage;
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

namespace FileService.Core.Features;
public sealed class GenerateDownloadUrls : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/files/download-urls", async Task<EndpointResult<IReadOnlyList<string>>> (
            [FromQuery] Guid[] fileIds,
            [FromServices] GeneratePresignedDownloadUrlsHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new GeneratePresignedDownloadUrlsRequest(fileIds), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed record GeneratePresignedDownloadUrlsRequest(IEnumerable<Guid> FileIds);

    public sealed class GeneratePresignedDownloadUrlsHandler
    {
        private readonly IS3Provider _s3Provider;
        private readonly IMediaRepository _mediaRepository;
        private readonly ILogger<GeneratePresignedDownloadUrlsHandler> _logger;

        public GeneratePresignedDownloadUrlsHandler(
            IS3Provider s3Provider,
            IMediaRepository mediaRepository,
            ILogger<GeneratePresignedDownloadUrlsHandler> logger)
        {
            _s3Provider = s3Provider;
            _mediaRepository = mediaRepository;
            _logger = logger;
        }

        public async Task<Result<IReadOnlyList<string>, Errors>> Handle(GeneratePresignedDownloadUrlsRequest request, CancellationToken cancellationToken)
        {
            var mediaAssetsResult = await _mediaRepository.GetListBy(x => request.FileIds.Contains(x.Id), cancellationToken);
            if (mediaAssetsResult.IsFailure)
                return mediaAssetsResult.Error;

            var keys = mediaAssetsResult.Value.Select(x => x.Key).ToList();

            var downloadUrlsResult = await _s3Provider.GenerateDownloadUrlsAsync(
                keys,
                cancellationToken);
            if (downloadUrlsResult.IsFailure)
                return downloadUrlsResult.Error.ToErrors();

            _logger.LogInformation("Generated download URLs for many files.");

            return Result.Success<IReadOnlyList<string>, Errors>(downloadUrlsResult.Value);
        }
    }
}
