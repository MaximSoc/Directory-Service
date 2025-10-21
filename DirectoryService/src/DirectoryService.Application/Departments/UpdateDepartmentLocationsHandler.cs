using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record UpdateDepartmentLocationsCommand(UpdateDepartmentLocationsRequest Request);

    public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
    {
        public UpdateDepartmentLocationsValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.DepartmentId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("department id"));

            RuleFor(x => x.Request.LocationsIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("locations"));

            RuleFor(x => x.Request.LocationsIds)
                .Must(list => list is { Count: > 0 })
                .WithError(Error.Validation("department.location", "Department locations must contain at least one location"));

            RuleFor(x => x.Request.LocationsIds)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithError(Error.Validation("locationIds.must.be.unique", "LocationIds must be unique"));

            RuleForEach(x => x.Request.LocationsIds)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("location id"));
        }
    }
    public class UpdateDepartmentLocationsHandler
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;

        public UpdateDepartmentLocationsHandler(IDepartmentsRepository departmentsRepository,
            ILogger<UpdateDepartmentLocationsHandler> logger,
            IValidator<UpdateDepartmentLocationsCommand> validator,
            ILocationsRepository locationsRepository)
        {
            _departmentsRepository = departmentsRepository;
            _logger = logger;
            _validator = validator;
            _locationsRepository = locationsRepository;
        }

        public async Task<UnitResult<Errors>> Handle(UpdateDepartmentLocationsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UpdateDepartmentLocationsRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var departmentResult = await _departmentsRepository.GetById(command.Request.DepartmentId, cancellationToken);
            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var department = departmentResult.Value;

            var locationIds = command.Request.LocationsIds.ToList();

            var allLocationsExistResult = await _locationsRepository.AllExistAsync(locationIds, cancellationToken);

            if (allLocationsExistResult.IsFailure)
                return allLocationsExistResult.Error;

            if (allLocationsExistResult.Value == false)
                return Error.NorFound("locations.not.found", "One or more locations were not found").ToErrors();

            var departmentLocations = command.Request.LocationsIds.Select(li => new DepartmentLocation(
                department.Id, li)).ToList();

            var updateResult = department.UpdateLocations(departmentLocations);
            if (updateResult.IsFailure)
                return updateResult.Error.ToErrors();

            return await _departmentsRepository.SaveChanges(cancellationToken);
        }
    }
}
