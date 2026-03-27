using Core.Handlers;
using Core.Shared;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Requests;
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

public class AbortMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/abort", async Task<EndpointResult> (
                [FromBody] AbortMultipartUploadRequest request,
                [FromServices] AbortMultipartUploadHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new AbortMultipartUploadCommand(request), cancellationToken));
    }
}

public sealed record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;

public sealed class AbortMultipartUploadHandler : ICommandHandler<AbortMultipartUploadCommand>
{
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IS3Provider _s3Provider;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IMediaRepository mediaRepository,
        ITransactionManager transactionManager,
        IS3Provider s3Provider,
        ILogger<AbortMultipartUploadHandler> logger)
    {
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
        _s3Provider = s3Provider;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> Handle(
        AbortMultipartUploadCommand command,
        CancellationToken cancellationToken = new CancellationToken())
    {
        var mediaAssetResult = await _mediaRepository.GetBy(ma => ma.Id == command.Request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        if (mediaAssetResult.Value.Status == MediaAsset.MediaStatus.UPLOADED)
        {
            return GeneralErrors.Failure("Файл уже загружен").ToErrors();
        }

        var abortResult = await _s3Provider.AbortMultipartUploadAsync(mediaAssetResult.Value.Key, command.Request.UploadId, cancellationToken);
        if (abortResult.IsFailure)
            return abortResult.Error.ToErrors();

        var markFailedResult = mediaAssetResult.Value.MarkFailed();
        if (markFailedResult.IsFailure)
            return markFailedResult.Error.ToErrors();

        var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChangesResult.IsFailure)
            return saveChangesResult.Error;

        return UnitResult.Success<Errors>();
    }
}
