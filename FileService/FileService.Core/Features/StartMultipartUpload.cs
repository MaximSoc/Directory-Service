using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Contracts.MediaAssets.Responses;
using FileService.Core.FilesStorage;
using FileService.Core.MediaAssets;
using FileService.Domain;
using FluentValidation;
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

public sealed class StartMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>> (
            [FromBody] StartMultipartUploadRequest request,
            [FromServices] StartMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new StartMultipartUploadCommand(request), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed class StartMultipartUploadValidator : AbstractValidator<StartMultipartUploadCommand>
    {
        public StartMultipartUploadValidator()
        {
            RuleFor(u => u.Request.AssetType)
                .Must(at => Enum.IsDefined(typeof(AssetType), at.ToUpperInvariant()))
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));

            RuleFor(x => x.Request).MustBeValueObject(r => MediaOwner.Create(r.Context, r.ContextId));

            RuleFor(x => x.Request.FileName).MustBeValueObject(FileName.Create);

            RuleFor(x => x.Request.ContentType).MustBeValueObject(ContentType.Create);

            RuleFor(x => x.Request.Size)
                .Must(l => l > 0)
                .WithError(GeneralErrors.ValueIsInvalid("size"));
        }
    }

    public sealed record StartMultipartUploadCommand(StartMultipartUploadRequest Request) : ICommand;

    public sealed class StartMultipartUploadHandler : ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IS3Provider _s3Provider;
        private readonly IValidator<StartMultipartUploadCommand> _validator;
        private readonly ILogger<StartMultipartUploadHandler> _logger;
        private readonly IChunkSizeCalculator _chunkSizeCalculator;

        public StartMultipartUploadHandler(
            IMediaRepository mediaRepository,
            ITransactionManager transactionManager,
            IS3Provider s3Provider,
            IValidator<StartMultipartUploadCommand> validator,
            ILogger<StartMultipartUploadHandler> logger,
            IChunkSizeCalculator chunkSizeCalculator)
        {
            _mediaRepository = mediaRepository;
            _transactionManager = transactionManager;
            _s3Provider = s3Provider;
            _validator = validator;
            _logger = logger;
            _chunkSizeCalculator = chunkSizeCalculator;
        }

        public async Task<Result<StartMultipartUploadResponse, Errors>> Handle(StartMultipartUploadCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToList();

            var fileNameResult = FileName.Create(command.Request.FileName);
            if (fileNameResult.IsFailure)
                return fileNameResult.Error.ToErrors();

            var contentTypeResult = ContentType.Create(command.Request.ContentType);
            if (contentTypeResult.IsFailure)
                return contentTypeResult.Error.ToErrors();

            Result<(int ChunkSize, int TotalChunks), Error> chunkCalculationResult = _chunkSizeCalculator.CalculateChunkSize(command.Request.Size);
            if (chunkCalculationResult.IsFailure)
                return chunkCalculationResult.Error.ToErrors();

            var mediaDataResult = MediaData.Create(
                fileNameResult.Value,
                contentTypeResult.Value,
                command.Request.Size,
                chunkCalculationResult.Value.TotalChunks);
            if (mediaDataResult.IsFailure)
                return mediaDataResult.Error.ToErrors();

            var mediaOwnerResult = MediaOwner.Create(command.Request.Context, command.Request.ContextId);
            if (mediaOwnerResult.IsFailure)
                return mediaOwnerResult.Error.ToErrors();

            var mediaAssetId = Guid.NewGuid();

            var mediaAssetResult = MediaAsset.CreateForUpload(
                mediaAssetId,
                mediaDataResult.Value,
                command.Request.AssetType.ToAssetType(),
                mediaOwnerResult.Value);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error.ToErrors();

            await _mediaRepository.AddAsync(mediaAssetResult.Value, cancellationToken);

            var saveChangesResultAfterAdding = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResultAfterAdding.IsFailure)
                return saveChangesResultAfterAdding.Error;

            var startUploadResult = await _s3Provider.StartMultipartUpload(
                mediaAssetResult.Value.Key,
                mediaDataResult.Value,
                cancellationToken);
            if (startUploadResult.IsFailure)
                return startUploadResult.Error.ToErrors();

            var chunkUploadUrlsResult = await _s3Provider.GenerateAllChunksUploadUrls(
                mediaAssetResult.Value.Key,
                startUploadResult.Value,
                chunkCalculationResult.Value.TotalChunks,
                cancellationToken);
            if (chunkUploadUrlsResult.IsFailure)
                return chunkUploadUrlsResult.Error.ToErrors();

            _logger.LogInformation("Media asset started uploading: {MediaAssetId} with key {StorageKey}", mediaAssetId, mediaAssetResult.Value.Key);

            return new StartMultipartUploadResponse(
                mediaAssetResult.Value.Id,
                startUploadResult.Value,
                chunkUploadUrlsResult.Value,
                chunkCalculationResult.Value.ChunkSize);
        }
    }
}