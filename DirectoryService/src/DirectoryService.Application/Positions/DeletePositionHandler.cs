using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Positions
{
    public record DeletePositionCommand(DeletePositionRequest Request) : ICommand;

    public class DeletePositionCommandValidator : AbstractValidator<DeletePositionCommand>
    {
        public DeletePositionCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.PositionId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("position id"));
        }
    }
    public class DeletePositionHandler : ICommandHandler<Guid, DeletePositionCommand>
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<DeletePositionCommand> _validator;
        private readonly ITransactionManager _transactionManager;
        private readonly HybridCache _cache;

        public DeletePositionHandler(
            IPositionsRepository positionsRepository,
            ILogger<DeletePositionHandler> logger,
            IValidator<DeletePositionCommand> validator,
            ITransactionManager transactionManager,
            HybridCache cache)
        {
            _positionsRepository = positionsRepository;
            _logger = logger;
            _validator = validator;
            _transactionManager = transactionManager;
            _cache = cache;
        }

        public async Task<Result<Guid, Errors>> Handle(DeletePositionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DeletePositionRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var positionResult = await _positionsRepository.GetById(command.Request.PositionId, cancellationToken);
            if (positionResult.IsFailure)
            {
                return positionResult.Error;
            }

            var position = positionResult.Value;

            if (position.IsActive == false)
            {
                _logger.LogInformation("Position is not active");
                return GeneralErrors.Failure("Position is not active").ToErrors();
            }

            var deletePositionsResult = await _positionsRepository.SoftDeleteByPositionId(position.Id, cancellationToken);
            if (deletePositionsResult.IsFailure)
            {
                _logger.LogInformation("Positions soft deleted failed");
                return deletePositionsResult.Error.ToErrors();
            }

            _logger.LogInformation("Position soft deleted succesfully: {PositionId}", position.Id);

            await _cache.RemoveByTagAsync(Constants.POSITIONS_CACHE_TAG, cancellationToken);

            return position.Id;
        }
    }
}
