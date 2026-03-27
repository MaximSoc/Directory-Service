using Core.Handlers;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Core.FilesStorage;
using FileService.Core.MediaAssets;
using FileService.Domain;
using FluentValidation;
using Framework.EndpointResults;
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

public sealed class GetChunckUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/url", async Task<EndpointResult<string>> (
                [FromBody] GetChunckUploadUrlRequest request,
                [FromServices] GetChunckUploadUrlHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new GetChunckUploadUrlCommand(request), cancellationToken));
    }
}

public sealed class GetChunckUploadUrlValidator : AbstractValidator<GetChunckUploadUrlCommand>
{
    public GetChunckUploadUrlValidator()
    {
        RuleFor(x => x.Request.MediaAssetId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("media asset id"));

        RuleFor(x => x.Request.UploadId)
            .Must(id => id != String.Empty)
            .WithError(GeneralErrors.ValueIsInvalid("upload id"));

        RuleFor(x => x.Request.PartNumber)
            .Must(pn => pn > 0)
            .WithError(GeneralErrors.ValueIsInvalid("PartNumber must be greater than one"));
    }
}

public sealed record GetChunckUploadUrlCommand(GetChunckUploadUrlRequest Request) : ICommand;

public sealed class GetChunckUploadUrlHandler : ICommandHandler<string, GetChunckUploadUrlCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly IValidator<GetChunckUploadUrlCommand> _validator;
    private readonly ILogger<GetChunckUploadUrlHandler> _logger;

    public GetChunckUploadUrlHandler(
        IS3Provider s3Provider,
        IMediaRepository mediaRepository,
        IValidator<GetChunckUploadUrlCommand> validator,
        ILogger<GetChunckUploadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(GetChunckUploadUrlCommand command, CancellationToken cancellationToken = default)
    {
        var validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToList();

        var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == command.Request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error;

        if (mediaAssetResult.Value.Status != MediaAsset.MediaStatus.UPLOADING)
            return GeneralErrors.Failure("Загрузка уже завершена или отменена.").ToErrors();

        var generateChunkUploadUrlResult = await _s3Provider.GenerateChunkUploadUrl(
            mediaAssetResult.Value.Key,
            command.Request.UploadId,
            command.Request.PartNumber,
            cancellationToken);
        if (generateChunkUploadUrlResult.IsFailure)
            return generateChunkUploadUrlResult.Error.ToErrors();

        return generateChunkUploadUrlResult.Value.UploadUrl;
    }
}
