using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
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

namespace DirectoryService.Application.Locations
{
    public record DeleteLocationCommand(DeleteLocationRequest Request);

    public class DeleteLocationCommandValidator : AbstractValidator<DeleteLocationCommand>
    {
        public DeleteLocationCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.LocationId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("location id"));
        }
    }

    public class DeleteLocationHandler
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<DeleteLocationCommand> _validator;
        private readonly ITransactionManager _transactionManager;
        private readonly HybridCache _cache;

        public DeleteLocationHandler(
            ILocationsRepository locationsRepository,
            ILogger<DeleteLocationHandler> logger,
            IValidator<DeleteLocationCommand> validator,
            ITransactionManager transactionManager,
            HybridCache cache)
        {
            _locationsRepository = locationsRepository;
            _logger = logger;
            _validator = validator;
            _transactionManager = transactionManager;
            _cache = cache;
        }

        public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DeleteLocationRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
                return transactionResult.Error.ToErrors();

            using var transaction = transactionResult.Value;

            var locationResult = await _locationsRepository.GetById(command.Request.LocationId, cancellationToken);
            if (locationResult.IsFailure)
            {
                transaction.Rollback();
                return locationResult.Error;
            }

            var location = locationResult.Value;
            if (location.IsActive == false)
            {
                _logger.LogInformation("Location is not active");
                transaction.Rollback();
                return GeneralErrors.Failure("Location is not active").ToErrors();
            }

            var deleteLocationsResult = await _locationsRepository.SoftDeleteByLocationId(location.Id, cancellationToken);
            if (deleteLocationsResult.IsFailure)
            {
                _logger.LogInformation("Locations soft deleted failed");
                transaction.Rollback();
                return deleteLocationsResult.Error.ToErrors();
            }

            var saveChanges = await _locationsRepository.SaveChanges(cancellationToken);
            if (saveChanges.IsFailure)
            {
                transaction.Rollback();
                return saveChanges.Error;
            }

            transaction.Commit();

            _logger.LogInformation("Location soft deleted succesfully: {LocationId}", location.Id);

            await _cache.RemoveByTagAsync(Constants.LOCATIONS_CACHE_TAG, cancellationToken);

            return location.Id;
        }
    }
}
