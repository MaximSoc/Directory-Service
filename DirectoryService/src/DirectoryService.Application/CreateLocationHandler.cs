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
        public async Task<Guid> Handle(CreateLocationRequest request, CancellationToken cancellationToken)
        {
            // бизнес валидация



            // создание доменных моделей
            var locationNameDto = request.Name;
            var name = LocationName.Create(locationNameDto.Name).Value;

            var locationAddressDto = request.Address;
            var address = LocationAddress.Create
            (locationAddressDto.Country,
            locationAddressDto.Region,
            locationAddressDto.City,
            locationAddressDto.PostalCode,
            locationAddressDto.Street,
            locationAddressDto.ApartamentNumber
            ).Value;

            var locationTimezone = request.Timezone;
            var timezone = LocationTimeZone.Create(locationTimezone.Timezone).Value;

            var location = Location.Create(name, address, timezone);

            // сохранение доменных моделей в базу данных
            await _locationsRepository.Add(location.Value, cancellationToken);

            return location.Value.Id;
        }
    }

    
}
