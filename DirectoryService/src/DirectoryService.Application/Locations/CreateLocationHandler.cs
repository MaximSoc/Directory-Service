using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.LocationVO;
using Shared;
using Microsoft.Extensions.Logging;
using FluentValidation;
using DirectoryService.Application.Validation;
using System.Security.Cryptography.X509Certificates;

namespace DirectoryService.Application
{
    public record CreateLocationCommand(CreateLocationRequest Request);

    public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
    {
        public CreateLocationCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.Name.Name)
                .MustBeValueObject(LocationName.Create);

            RuleFor(x => x.Request.Address)
                .MustBeValueObject(a => LocationAddress.Create(
                    a.Country,
                    a.Region,
                    a.City,
                    a.PostalCode,
                    a.Street,
                    a.ApartamentNumber));

            RuleFor(x => x.Request.Timezone.Timezone)
                .MustBeValueObject(LocationTimeZone.Create);
        }
    }

    public class CreateLocationHandler
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<CreateLocationCommand> _validator;

        public CreateLocationHandler(ILocationsRepository locationsRepository,
            ILogger<CreateLocationHandler> logger,
            IValidator<CreateLocationCommand> validator)
        {
            _locationsRepository = locationsRepository;
            _logger = logger;
            _validator = validator;
        }

        // Метод для создания локации
        public async Task<Result<Guid, Errors>> Handle(CreateLocationCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received CreateLocationRequest: {Request}", command.Request);
            
            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var locationNameDto = command.Request.Name;
            var locationName = LocationName.Create(locationNameDto.Name);
                
            var locationAddressDto = command.Request.Address;
            var locationAddress = LocationAddress.Create
            (locationAddressDto.Country,
            locationAddressDto.Region,
            locationAddressDto.City,
            locationAddressDto.PostalCode,
            locationAddressDto.Street,
            locationAddressDto.ApartamentNumber
            );
                
            var locationTimezoneDto = command.Request.Timezone;
            var locationTimezone = LocationTimeZone.Create(locationTimezoneDto.Timezone);

            var location = new Location(
            locationName.Value,
            locationAddress.Value,
            locationTimezone.Value);


            var result = await _locationsRepository.Add(location, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Location created succesfully: {LocationId}", location.Id);

                return result.Value;
            }
            else
            {
                return result.Error;
            }
        }
    }
}
