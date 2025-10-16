﻿using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using DirectoryService.Domain.ValueObjects.LocationVO;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record CreateDepartmentCommand(CreateDepartmentRequest Request);

    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.Name.Name)
                .MustBeValueObject(DepartmentName.Create);

            RuleFor(x => x.Request.Identifier.Identifier)
                .MustBeValueObject(DepartmentIdentifier.Create);

            RuleFor(x => x.Request.ParentId)
                .Must(id => id == null || id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("parent id"));

            RuleFor(x => x.Request.LocationIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("locations"));

            RuleFor(x => x.Request.LocationIds)
                .Must(list => list is { Count: > 0 })
                .WithError(Error.Validation("department.location", "Department locations must contain at least one location"));

            RuleFor(x => x.Request.LocationIds)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithError(Error.Validation("locationIds.must.be.unique", "LocationIds must be unique"));

            RuleForEach(x => x.Request.LocationIds)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("location id"));
        }
    }
    public class CreateDepartmentHandler
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<CreateDepartmentCommand> _validator;

        public CreateDepartmentHandler(IDepartmentsRepository departmentsRepository,
            ILogger<CreateDepartmentHandler> logger,
            IValidator<CreateDepartmentCommand> validator,
            ILocationsRepository locationsRepository)
        {
            _departmentsRepository = departmentsRepository;
            _logger = logger;
            _validator = validator;
            _locationsRepository = locationsRepository;
        }

        public async Task<Result<Guid, Errors>> Handle(CreateDepartmentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received CreateDepartmentRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var departmentNameDto = command.Request.Name;
            var departmentName = DepartmentName.Create(departmentNameDto.Name);

            var departmentIdentifierDto = command.Request.Identifier;
            var departmentIdentifier = DepartmentIdentifier.Create(departmentIdentifierDto.Identifier);

            Department? parent = null;

            if (command.Request.ParentId != null)
            {
                var parentResult = await _departmentsRepository.GetById(command.Request.ParentId, cancellationToken);

                if (parentResult.IsFailure)
                {
                    return parentResult.Error;
                }

                parent = parentResult.Value;
            }

            var locationIds = command.Request.LocationIds;
            var allLocationsExistResult = await _locationsRepository.AllExistAsync(locationIds, cancellationToken);
            if (allLocationsExistResult.IsFailure)
                return allLocationsExistResult.Error;

            if (allLocationsExistResult.Value == false)
                return Error.NorFound("locations.not.found", "One or more locations were not found").ToErrors();

            var departmentId = Guid.NewGuid();

            var departmentLocations = locationIds.Select(li =>
            new DepartmentLocation(departmentId, li)).ToList();

            var department = parent is null
                ? Department.CreateParent(departmentName.Value, departmentIdentifier.Value, departmentLocations, departmentId)
                : Department.CreateChild(departmentId, departmentName.Value, departmentIdentifier.Value, parent, departmentLocations);

            if (department.IsFailure)
                return department.Error.ToErrors();

            var result = await _departmentsRepository.Add(department.Value, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Department created succesfully: {DepartmentId}", department.Value.Id);

                return result.Value;
            }
            else
            {
                return result.Error;
            }
        }
    }
}
