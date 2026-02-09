using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
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
    public record CreatePositionCommand(CreatePositionRequest Request);
    public class CreatePositionCommandValidator : AbstractValidator<CreatePositionCommand>
    {
        public CreatePositionCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.Name)
                .MustBeValueObject(PositionName.Create);

            RuleFor(x => x.Request.Description)
                .MustBeValueObject(PositionDescription.Create)
                .When(x => x.Request.Description != null);

            RuleFor(x => x.Request.DepartmentsIds)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("departments"));

            RuleFor(x => x.Request.DepartmentsIds)
                .Must(list => list is { Count: > 0 })
                .WithError(Error.Validation("department.location", "Department locations must contain at least one location"));

            RuleFor(x => x.Request.DepartmentsIds)
                .Must(list => list == null || list.Distinct().Count() == list.Count)
                .WithError(Error.Validation("departmentsIds.must.be.unique", "departmentsIds must be unique"));

            RuleForEach(x => x.Request.DepartmentsIds)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("location id"));
        }
    }
    public class CreatePositionHandler
    {
        private readonly IPositionsRepository _positionsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<CreatePositionCommand> _validator;
        private readonly HybridCache _cache;

        public CreatePositionHandler(IPositionsRepository positionsRepository,
            ILogger<CreatePositionHandler> logger,
            IValidator<CreatePositionCommand> validator,
            IDepartmentsRepository departmentsRepository,
            HybridCache cache)
        {
            _positionsRepository = positionsRepository;
            _logger = logger;
            _validator = validator;
            _departmentsRepository = departmentsRepository;
            _cache = cache;
        }

        public async Task<Result<Guid, Errors>> Handle(CreatePositionCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received CreatePositionRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var name = command.Request.Name;
            var positionNameResult = PositionName.Create(name);

            var positionExistResult = await _positionsRepository.PositionExistAsync(positionNameResult.Value, cancellationToken);
            if (positionExistResult.IsFailure)
                return positionExistResult.Error;
            if (positionExistResult.Value == false)
                return GeneralErrors.AlreadyExist().ToErrors();

            var description = command.Request.Description;
            Result<PositionDescription, Error>? positionDescriptionResult = null;
            if (description != null)
            {
                positionDescriptionResult = PositionDescription.Create(description);
            }

            var positionId = Guid.NewGuid();

            var departmentsIds = command.Request.DepartmentsIds;
            var allDepartmentsExistResult = await _departmentsRepository.AllExistAsync(departmentsIds, cancellationToken);
            if (allDepartmentsExistResult.IsFailure)
                return allDepartmentsExistResult.Error;
            if (allDepartmentsExistResult.Value == false)
                return Error.NorFound("departments.not.found", "One or more departments were not found").ToErrors();

            var departmentPositions = departmentsIds.Select(di => new DepartmentPosition(di, positionId)).ToList();

            var position = new Position(positionId, positionNameResult.Value, positionDescriptionResult?.Value, departmentPositions);

            var result = await _positionsRepository.Add(position, cancellationToken);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Position created succesfully: {PositionId}", position.Id);

                await _cache.RemoveByTagAsync(Constants.POSITIONS_CACHE_TAG, cancellationToken);

                return result.Value;
            }
            else
            {
                return result.Error;
            }
        }
    }
}
