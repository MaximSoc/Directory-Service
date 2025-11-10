using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Shared;
using DirectoryService.Application.Validation;
using DirectoryService.Contracts.Departments;
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
    public record DeleteDepartmentCommand(DeleteDepartmentRequest Request);

    public class DeleteDepartmentCommandValidator : AbstractValidator<DeleteDepartmentCommand>
    {
        public DeleteDepartmentCommandValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.DepartmentId)
                .NotEmpty()
                .WithError(GeneralErrors.ValueIsRequired("department id"));
        }
    }

    public class DeleteDepartmentHandler
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<DeleteDepartmentCommand> _validator;
        private readonly ITransactionManager _transactionManager;

        public DeleteDepartmentHandler(
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IPositionsRepository positionsRepository,
            ILogger<DeleteDepartmentHandler> logger,
            IValidator<DeleteDepartmentCommand> validator,
            ITransactionManager transactionManager)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _positionsRepository = positionsRepository;
            _logger = logger;
            _validator = validator;
            _transactionManager = transactionManager;
        }

        public async Task<Result<Guid, Errors>> Handle(DeleteDepartmentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received DeleteDepartmentRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
                return transactionResult.Error.ToErrors();

            using var transaction = transactionResult.Value;

            var departmentResult = await _departmentsRepository.GetById(command.Request.DepartmentId, cancellationToken);
            if (departmentResult.IsFailure)
            {
                transaction.Rollback();
                return departmentResult.Error;
            }


            var department = departmentResult.Value;
            if (department.IsActive == false)
            {
                _logger.LogInformation("Department is not active");
                transaction.Rollback();
                return GeneralErrors.Failure("Department is not active").ToErrors();
            }

            var deleteLocationsResult = await _locationsRepository.SoftDelete(department.Id, cancellationToken);
            if (deleteLocationsResult.IsFailure)
            {
                _logger.LogInformation("Locations soft deleted failed");
                transaction.Rollback();
                return deleteLocationsResult.Error.ToErrors();
            }

            var deletePositionsResult = await _positionsRepository.SoftDelete(department.Id, cancellationToken);
            if (deletePositionsResult.IsFailure)
            {
                _logger.LogInformation("Positions soft deleted failed");
                transaction.Rollback();
                return deletePositionsResult.Error.ToErrors();
            }

            var oldPath = department.Path.Value;

            var result = _departmentsRepository.SoftDelete(department, cancellationToken);

            if (result.IsFailure)
            {
                _logger.LogInformation("Department soft deleted failed: {DepartmentId}", department.Id);
                transaction.Rollback();
                return result.Error;
            }

            var saveChanges = await _departmentsRepository.SaveChanges(cancellationToken);
            if (saveChanges.IsFailure)
            {
                transaction.Rollback();
                return saveChanges.Error;
            }

            var newPath = department.Path.Value;

            var updateChildrensPathAndDepth = await _departmentsRepository.UpdateChildrensPathAndDepth(
                oldPath,
                newPath,
                0,
                cancellationToken);

            if (updateChildrensPathAndDepth.IsFailure)
            {
                transaction.Rollback();
                return updateChildrensPathAndDepth.Error;
            }

            transaction.Commit();

            _logger.LogInformation("Department soft deleted succesfully: {DepartmentId}", department.Id);
            return result.Value;
        }
    }
}
