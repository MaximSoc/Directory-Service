using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.LocationVO;
using DirectoryService.Domain.ValueObjects.PositionVO;
using FluentValidation;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Positions
{
    public record UpdatePositionCommand(Guid PositionId, UpdatePositionRequest Request);

    public class UpdatePositionValidator : AbstractValidator<UpdatePositionCommand>
    {
        public UpdatePositionValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.PositionId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("position id"));

            RuleFor(x => x.Request.Name)
                .MustBeValueObject(PositionName.Create);

            RuleFor(x => x.Request.Description)
                .MustBeValueObject(PositionDescription.Create);

            RuleFor(x => x.Request.DepartmentIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("departments"));

            RuleFor(x => x.Request.DepartmentIds)
                .Must(list => list is { Count: > 0 })
                .WithError(Error.Validation("department.position", "Positions must contain at least one department"));

            RuleFor(x => x.Request.DepartmentIds)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithError(Error.Validation("departmentIds.must.be.unique", "DepartmentIds must be unique"));

            RuleForEach(x => x.Request.DepartmentIds)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("department id"));
        }
    }
    public class UpdatePositionHandler
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<UpdatePositionCommand> _validator;
        private readonly HybridCache _cache;

        public UpdatePositionHandler(IDepartmentsRepository departmentsRepository,
            ILogger<UpdatePositionHandler> logger,
            IValidator<UpdatePositionCommand> validator,
            IPositionsRepository positionsRepository,
            HybridCache cache)
        {
            _logger = logger;
            _validator = validator;
            _positionsRepository = positionsRepository;
            _departmentsRepository = departmentsRepository;
            _cache = cache;
        }

        public async Task<UnitResult<Errors>> Handle(UpdatePositionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UpdatePositionRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var positionResult = await _positionsRepository.GetById(command.PositionId, cancellationToken);
            if (positionResult.IsFailure)
                return positionResult.Error;

            var position = positionResult.Value;

            var allDepartmentsExistResult = await _departmentsRepository.AllExistAsync(command.Request.DepartmentIds, cancellationToken);

            if (allDepartmentsExistResult.IsFailure)
                return allDepartmentsExistResult.Error;

            if (allDepartmentsExistResult.Value == false)
                return Error.NorFound("departments.not.found", "One or more departments were not found").ToErrors();

            var departmentPositions = command.Request.DepartmentIds.Select(di => new DepartmentPosition(
                di, position.Id)).ToList();

            PositionName positionName = PositionName.Create(command.Request.Name).Value;

            PositionDescription positionDescription = PositionDescription.Create(command.Request.Description).Value;

            var updateResult = position.Update(positionName, positionDescription, departmentPositions);
            if (updateResult.IsFailure)
                return updateResult.Error.ToErrors();

            await _cache.RemoveByTagAsync(Constants.POSITIONS_CACHE_TAG, cancellationToken);

            return await _positionsRepository.SaveChanges(cancellationToken);
        }
    }
}
