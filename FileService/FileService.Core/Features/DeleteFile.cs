using Core.Handlers;
using Core.Shared;
using CSharpFunctionalExtensions;
using FileService.Core.MediaAssets;
using Framework.EndpointResults;
using Framework.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileService.Core.FilesStorage;

namespace FileService.Core.Features;
public sealed class DeleteFile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/delete/{fileId:guid}", async Task<EndpointResult<string>> (
            [FromRoute] Guid fileId,
            [FromServices] DeleteFileHandler handler,
            CancellationToken cancellationToken) =>
                await handler.Handle(new DeleteFileCommand(fileId), cancellationToken))
                    .DisableAntiforgery();
    }

    public sealed record DeleteFileCommand(Guid FileId) : ICommand;

    public sealed class DeleteFileHandler : ICommandHandler<string, DeleteFileCommand>
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly ITransactionManager _transactionManager;
        private readonly IS3Provider _s3Provider;
        private readonly ILogger<DeleteFileHandler> _logger;

        public DeleteFileHandler(
            IMediaRepository mediaRepository,
            ITransactionManager transactionManager,
            IS3Provider s3Provider,
            ILogger<DeleteFileHandler> logger)
        {
            _mediaRepository = mediaRepository;
            _transactionManager = transactionManager;
            _s3Provider = s3Provider;
            _logger = logger;
        }
        public async Task<Result<string, Errors>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
        {
            var mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == command.FileId, cancellationToken);
            if (mediaAssetResult.IsFailure)
                return mediaAssetResult.Error;

            var markDeletedResult = mediaAssetResult.Value.MarkDeleted();
            if (markDeletedResult.IsFailure)
                return markDeletedResult.Error.ToErrors();

            var saveChangesResult = await _transactionManager.SaveChangesAsync(cancellationToken);
            if (saveChangesResult.IsFailure)
                return saveChangesResult.Error;

            var deleteResult = await _s3Provider.DeleteFileAsync(mediaAssetResult.Value.Key, cancellationToken);
            if (deleteResult.IsFailure)
                return deleteResult.Error.ToErrors();

            _logger.LogInformation("File {fileName} deleted.", mediaAssetResult.Value.MediaData.FileName);

            return deleteResult.Value;
        }
    }
}
