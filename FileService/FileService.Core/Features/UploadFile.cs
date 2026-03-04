using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Requests;
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
    public sealed class UploadFileEndPoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/upload", async Task<EndpointResult<Guid>> (
            [FromForm] UploadFileRequest request,
            [FromServices] UploadFileHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new UploadFileCommand(request), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed class UploadFileValidator : AbstractValidator<UploadFileCommand>
    {
        public UploadFileValidator()
        {
            RuleFor(u => u.Request.AssetType)
                .Must(at => Enum.IsDefined(typeof(AssetType), at.ToUpperInvariant()))
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));

            RuleFor(x => x.Request).MustBeValueObject(r => MediaOwner.Create(r.Context, r.EntityId));

            RuleFor(x => x.Request.File.FileName).MustBeValueObject(FileName.Create);

            RuleFor(x => x.Request.File.ContentType).MustBeValueObject(ContentType.Create);

            RuleFor(x => x.Request.File.Length)
                .Must(l => l > 0);
        }
    }

    public sealed record UploadFileCommand(UploadFileRequest Request) : ICommand;

    public sealed class UploadFileHandler : ICommandHandler<Guid, UploadFileCommand>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IS3Provider _s3Provider;
        private readonly IValidator<UploadFileCommand> _validator;
        private readonly ILogger<UploadFileHandler> _logger;

        public UploadFileHandler(
            IMediaRepository mediaRepository,
            ITransactionManager transactionManager,
            IS3Provider s3Provider,
            IValidator<UploadFileCommand> validator,
            ILogger<UploadFileHandler> logger)
        {
            _mediaRepository = mediaRepository;
            _transactionManager = transactionManager;
            _s3Provider = s3Provider;
            _validator = validator;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToList();

            var mediaAssetId = Guid.NewGuid();

            var mediaDataResult = MediaData.Create(
                FileName.Create(command.Request.File.FileName).Value,
                ContentType.Create(command.Request.File.ContentType).Value,
                command.Request.File.Length,
                1);
            if (mediaDataResult.IsFailure)
                return mediaDataResult.Error.ToErrors();

            var mediaOwnerResult = MediaOwner.Create(
                command.Request.Context,
                command.Request.EntityId);
            if (mediaOwnerResult.IsFailure)
                return mediaOwnerResult.Error.ToErrors();

            Result<MediaAsset, Error> mediaAssetResult = MediaAsset.CreateForUpload(
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

            UnitResult<Error> uploadResult = await _s3Provider.UploadFileAsync(
                mediaAssetResult.Value.Key,
                command.Request.File.OpenReadStream(),
                mediaDataResult.Value,
                cancellationToken);

            if (uploadResult.IsFailure)
            {
                mediaAssetResult.Value.MarkFailed();

                var saveChangesResultAfterFailedUpload = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveChangesResultAfterFailedUpload.IsFailure)
                    return saveChangesResultAfterFailedUpload.Error;

                return uploadResult.Error.ToErrors();
            }

            var markUploadedResult = mediaAssetResult.Value.MarkUploaded();
            if (markUploadedResult.IsFailure)
                return markUploadedResult.Error.ToErrors();

            var saveChangesResultAfterSuccesUpload = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResultAfterSuccesUpload.IsFailure)
                return saveChangesResultAfterSuccesUpload.Error;

            _logger.LogInformation("File {FileName} was successfully uploaded", command.Request.File.FileName);

            return mediaAssetId;
        }
    }
}
