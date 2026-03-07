using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Requests;
using FileService.Core.FilesStorage;
using FileService.Core.MediaAssets;
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

public sealed class CompleteMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/files/multipart/complete", async Task<EndpointResult> (
            [FromBody] CompleteMultipartUploadRequest request,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new CompleteMultipartUploadCommand(request), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed class CompleteMultipartUploadValidator : AbstractValidator<CompleteMultipartUploadCommand>
    {
        public CompleteMultipartUploadValidator()
        {
            RuleFor(x => x.Request.MediaAssetId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("media asset id"));

            RuleFor(x => x.Request.UploadId)
                .Must(id => id != String.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("upload id"));

            RuleFor(x => x.Request.PartETags)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("partETags"));
        }
    }

    public sealed record CompleteMultipartUploadCommand(CompleteMultipartUploadRequest Request) : ICommand;

    public sealed class CompleteMultipartUploadHandler : ICommandHandler<CompleteMultipartUploadCommand>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IS3Provider _s3Provider;
        private readonly IValidator<CompleteMultipartUploadCommand> _validator;
        private readonly ILogger<CompleteMultipartUploadHandler> _logger;

        public CompleteMultipartUploadHandler(
            IMediaRepository mediaRepository,
            ITransactionManager transactionManager,
            IS3Provider s3Provider,
            IValidator<CompleteMultipartUploadCommand> validator,
            ILogger<CompleteMultipartUploadHandler> logger)
        {
            _mediaRepository = mediaRepository;
            _transactionManager = transactionManager;
            _s3Provider = s3Provider;
            _validator = validator;
            _logger = logger;
        }

        public async Task<UnitResult<Errors>> Handle(CompleteMultipartUploadCommand command, CancellationToken cancellationToken)
        {
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
                return validationResult.ToList();

            var mediaAssetResult = await _mediaRepository.GetBy(m => m.Id == command.Request.MediaAssetId, cancellationToken);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error;

            if (mediaAssetResult.Value.MediaData.ExpectedChunksCount != command.Request.PartETags.Count)
                return GeneralErrors.Failure("Количество eTags не соответствует количеству чанков").ToErrors();

            var completeResult = await _s3Provider.CompleteMultipartUploadAsync(
                mediaAssetResult.Value.Key,
                command.Request.UploadId,
                command.Request.PartETags.ToList(),
                cancellationToken
                );
            if (completeResult.IsFailure)
            {
                mediaAssetResult.Value.MarkFailed();

                var saveChangesResultAfterFailed = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveChangesResultAfterFailed.IsFailure)
                    return saveChangesResultAfterFailed.Error;

                return completeResult.Error.ToErrors();
            }
                
            mediaAssetResult.Value.MarkUploaded();

            var saveChangesResultAfterSuccesful = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResultAfterSuccesful.IsFailure)
                return saveChangesResultAfterSuccesful.Error;

            _logger.LogInformation("Complete multipart upload is succesful");

            return UnitResult.Success<Errors>();
        }
    }
}