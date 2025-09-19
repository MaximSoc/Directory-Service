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

namespace DirectoryService.Application
{
    public class CreateLocationHandler
    {
        private readonly ILocationsRepository _locationsRepository;
        public CreateLocationHandler(ILocationsRepository locationsRepository)
        {
            _locationsRepository = locationsRepository;
        }

        // Метод для создания локации
        public async Task<Result<Guid, string>> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
        {
            // бизнес валидация



            // создание доменных моделей
            var locationNameDto = request.Name;
            var locationNameResult = LocationName.Create(locationNameDto.Name);
            if (!locationNameResult.IsSuccess)
                return locationNameResult.Error;

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
                return locationAddressResult.Error;

            var locationTimezone = request.Timezone;
            var locationTimezoneResult = LocationTimeZone.Create(locationTimezone.Timezone);
            if (!locationTimezoneResult.IsSuccess)
                return locationTimezoneResult.Error;

            var location = new Location(
            locationNameResult.Value,
            locationAddressResult.Value,
            locationTimezoneResult.Value);

            // сохранение доменных моделей в базу данных
            await _locationsRepository.Add(location, cancellationToken);

            return location.Id;
        }
    }

    
}
