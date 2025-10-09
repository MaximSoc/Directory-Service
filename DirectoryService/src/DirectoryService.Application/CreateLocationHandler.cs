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

namespace DirectoryService.Application
{
    public class CreateLocationHandler
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;

        public CreateLocationHandler(ILocationsRepository locationsRepository, ILogger<CreateLocationHandler> logger)
        {
            _locationsRepository = locationsRepository;
            _logger = logger;
        }

        // Метод для создания локации
        public async Task<Result<Guid, Errors>> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received CreateLocationRequest: {Request}", request);
            // бизнес валидация



            // создание доменных моделей
            var locationNameDto = request.Name;
            var locationNameResult = LocationName.Create(locationNameDto.Name);
            if (!locationNameResult.IsSuccess)
            {
                _logger.LogWarning("LocationName validation failed: {Error}", locationNameResult.Error);

                return locationNameResult.Error.ToErrors();
            }
                

            var locationAddressDto = request.Address;
            var locationAddressResult = LocationAddress.Create
            (locationAddressDto.Country,
            locationAddressDto.Region,
            locationAddressDto.City,
            locationAddressDto.PostalCode,
            locationAddressDto.Street,
            locationAddressDto.ApartamentNumber
            );
            if (!locationAddressResult.IsSuccess)
            {
                _logger.LogWarning("LocationAddress validation failed: {Error}", locationAddressResult.Error);

                return locationAddressResult.Error.ToErrors();
            }
                

            var locationTimezone = request.Timezone;
            var locationTimezoneResult = LocationTimeZone.Create(locationTimezone.Timezone);
            if (!locationTimezoneResult.IsSuccess)
            {
                _logger.LogWarning("LocationTimezone validation failed: {Error}", locationTimezoneResult.Error);

                return locationTimezoneResult.Error.ToErrors();
            }
               

            var location = new Location(
            locationNameResult.Value,
            locationAddressResult.Value,
            locationTimezoneResult.Value);

            // сохранение доменных моделей в базу данных
            try
            {
                await _locationsRepository.Add(location, cancellationToken);

                _logger.LogInformation("Location created succesfully: {LocationId}", location.Id);
            }
            catch ( Exception ex)
            {
                _logger.LogError(ex, "Error occured while saving Location: {LocationId}", location.Id);
            }
            

            return location.Id;
        }
    }

    
}
