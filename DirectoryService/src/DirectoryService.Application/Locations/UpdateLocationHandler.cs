using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.LocationVO;
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
    public record UpdateLocationCommand(Guid LocationId, UpdateLocationRequest Request);

    public class UpdateLocationValidator : AbstractValidator<UpdateLocationCommand>
    {
        public UpdateLocationValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.LocationId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("location id"));

            RuleFor(x => x.Request.Name)
                .MustBeValueObject(LocationName.Create);

            RuleFor(x => x.Request)
                .MustBeValueObject(a => LocationAddress.Create(
                    a.Country,
                    a.Region,
                    a.City,
                    a.PostalCode,
                    a.Street,
                    a.ApartamentNumber));

            RuleFor(x => x.Request.Timezone)
                .MustBeValueObject(LocationTimeZone.Create);
        }
    }
    public class UpdateLocationHandler
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<UpdateLocationCommand> _validator;
        private readonly HybridCache _cache;

        public UpdateLocationHandler(
            ILogger<UpdateLocationHandler> logger,
            IValidator<UpdateLocationCommand> validator,
            ILocationsRepository locationsRepository,
            HybridCache cache)
        {
            _logger = logger;
            _validator = validator;
            _locationsRepository = locationsRepository;
            _cache = cache;
        }

        public async Task<UnitResult<Errors>> Handle(UpdateLocationCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UpdateLocationRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var locationResult = await _locationsRepository.GetById(command.LocationId, cancellationToken);
            if (locationResult.IsFailure)
                return locationResult.Error;

            var location = locationResult.Value;

            LocationName locationName = LocationName.Create(command.Request.Name).Value;

            LocationAddress locationAddress = LocationAddress.Create(
                command.Request.Country,
                command.Request.Region,
                command.Request.City,
                command.Request.PostalCode,
                command.Request.Street,
                command.Request.ApartamentNumber).Value;

            LocationTimeZone locationTimezone = LocationTimeZone.Create(command.Request.Timezone).Value;

            var updateResult = location.Update(locationName, locationAddress, locationTimezone);
            if (updateResult.IsFailure)
                return updateResult.Error.ToErrors();

            await _cache.RemoveByTagAsync(Constants.LOCATIONS_CACHE_TAG, cancellationToken);

            return await _locationsRepository.SaveChanges(cancellationToken);
        }
    }
}
