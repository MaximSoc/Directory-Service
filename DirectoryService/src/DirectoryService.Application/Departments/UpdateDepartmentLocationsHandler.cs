using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments
{
    public record UpdateDepartmentLocationsCommand(Guid DepartmentId, UpdateDepartmentLocationsRequest Request) : ICommand;

    public class UpdateDepartmentLocationsValidator : AbstractValidator<UpdateDepartmentLocationsCommand>
    {
        public UpdateDepartmentLocationsValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.DepartmentId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("department id"));

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
    public class UpdateDepartmentLocationsHandler : ICommandHandler<Guid, UpdateDepartmentLocationsCommand>
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<UpdateDepartmentLocationsCommand> _validator;
        private readonly HybridCache _cache;
        private readonly ITransactionManager _transactionManager;

        public UpdateDepartmentLocationsHandler(IDepartmentsRepository departmentsRepository,
            ILogger<UpdateDepartmentLocationsHandler> logger,
            IValidator<UpdateDepartmentLocationsCommand> validator,
            ILocationsRepository locationsRepository,
            HybridCache cache,
            ITransactionManager transactionManager)
        {
            _departmentsRepository = departmentsRepository;
            _logger = logger;
            _validator = validator;
            _locationsRepository = locationsRepository;
            _cache = cache;
            _transactionManager = transactionManager;
        }

        public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentLocationsCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UpdateDepartmentLocationsRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var departmentResult = await _departmentsRepository.GetById(command.DepartmentId, cancellationToken);
            if (departmentResult.IsFailure)
                return departmentResult.Error;

            var department = departmentResult.Value;

            var locationIds = command.Request.LocationIds.ToList();

            var allLocationsExistResult = await _locationsRepository.AllExistAsync(locationIds, cancellationToken);

            if (allLocationsExistResult.IsFailure)
                return allLocationsExistResult.Error;

            if (allLocationsExistResult.Value == false)
                return Error.NorFound("locations.not.found", "One or more locations were not found").ToErrors();

            var departmentLocations = command.Request.LocationIds.Select(li => new DepartmentLocation(
                department.Id, li)).ToList();

            var updateResult = department.UpdateLocations(departmentLocations);
            if (updateResult.IsFailure)
                return updateResult.Error.ToErrors();

            await _cache.RemoveByTagAsync(Constants.DEPARTMENTS_CACHE_TAG, cancellationToken);

            await _transactionManager.SaveChangesAsync(cancellationToken);

            return department.Id;
        }
    }
}
