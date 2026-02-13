using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.LocationVO;
using Microsoft.Extensions.Logging;
using FluentValidation;
using System.Security.Cryptography.X509Certificates;
using DirectoryService.Contracts.Locations;
using Core.Validation;
using SharedKernel;
using Core.Handlers;

namespace DirectoryService.Application.Locations
{
    public record CreateLocationCommand(CreateLocationRequest Request) : ICommand;

    public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
    {
        public CreateLocationCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

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

    public class CreateLocationHandler : ICommandHandler<Guid, CreateLocationCommand>
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

            var locationName = LocationName.Create(command.Request.Name);
                
            var locationAddress = LocationAddress.Create
            (command.Request.Country,
            command.Request.Region,
            command.Request.City,
            command.Request.PostalCode,
            command.Request.Street,
            command.Request.ApartamentNumber
            );
                
            var locationTimezone = LocationTimeZone.Create(command.Request.Timezone);

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
