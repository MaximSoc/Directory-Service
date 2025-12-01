using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain;
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
    public record MoveDepartmentCommand(MoveDepartmentRequest Request);

    public class MoveDepartmentValidator : AbstractValidator<MoveDepartmentCommand>
    {
        public MoveDepartmentValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.DepartmentId)
                .Must(id => id != Guid.Empty)
                .WithError(GeneralErrors.ValueIsInvalid("department id"));
        }
    }

    public class MoveDepartmentHandler
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<MoveDepartmentCommand> _validator;
        private readonly ITransactionManager _transactionManager;
        private readonly HybridCache _cache;

        public MoveDepartmentHandler(IDepartmentsRepository departmentsRepository,
            ILogger<MoveDepartmentHandler> logger,
            IValidator<MoveDepartmentCommand> validator,
            ITransactionManager transactionManager,
            HybridCache cache)
        {
            _departmentsRepository = departmentsRepository;
            _logger = logger;
            _validator = validator;
            _transactionManager = transactionManager;
            _cache = cache;
        }

        public async Task<UnitResult<Errors>> Handle(MoveDepartmentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received MoveDepartmentRequest: {Request}", command.Request);

            var validationResult = await _validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                return validationResult.ToList();
            }

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
                return transactionResult.Error.ToErrors();

            using var transaction = transactionResult.Value;

            try
            {
                var departmentResult = await _departmentsRepository.GetByIdWithLock(command.Request.DepartmentId, cancellationToken);
                if (departmentResult.IsFailure)
                {
                    transaction.Rollback();
                    return departmentResult.Error;
                }

                var department = departmentResult.Value;

                int oldDepth = department.Depth;

                Department? parentDepartment = null;
                if (command.Request.ParentId != null)
                {
                    var parentDepartmentResult = await _departmentsRepository.GetByIdWithLock(command.Request.ParentId, cancellationToken);

                    if (parentDepartmentResult.IsFailure)
                    {
                        transaction.Rollback();
                        return parentDepartmentResult.Error;
                    }

                    parentDepartment = parentDepartmentResult.Value;
                }

                var oldPath = department.Path.Value;

                var updateParent = department.UpdateParent(parentDepartment);
                if (updateParent.IsFailure)
                {
                    transaction.Rollback();
                    return updateParent.Error.ToErrors();
                }

                var saveResult = await _departmentsRepository.SaveChanges(cancellationToken);

                var lockChildrens = await _departmentsRepository.LockChildrens(oldPath, cancellationToken);
                if (lockChildrens.IsFailure)
                {
                    transaction.Rollback();
                    return lockChildrens.Error;
                }

                int newDepth = department.Depth;
                int depthDelta = newDepth - oldDepth;

                var updateChildrensPathAndDepth = await _departmentsRepository.UpdateChildrensPathAndDepth(
                    oldPath,
                    department.Path.Value,
                    depthDelta,
                    cancellationToken);

                if (updateChildrensPathAndDepth.IsFailure)
                {
                    transaction.Rollback();
                    return updateChildrensPathAndDepth.Error;
                }

                transaction.Commit();

                await _cache.RemoveByTagAsync($"departmentsCache_tag", cancellationToken);

                return UnitResult.Success<Errors>();
            }

            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
