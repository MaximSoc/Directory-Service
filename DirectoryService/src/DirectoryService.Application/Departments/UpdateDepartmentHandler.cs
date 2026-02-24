using Core.Handlers;
using Core.Shared;
using Core.Validation;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain;
using DirectoryService.Domain.Shared;
using DirectoryService.Domain.ValueObjects.DepartmentVO;
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

namespace DirectoryService.Application.Departments
{
    public record UpdateDepartmentCommand(Guid DepartmentId, UpdateDepartmentRequest Request) : ICommand;

    public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentValidator()
        {
            RuleFor(x => x.Request)
                .NotNull()
                .WithError(GeneralErrors.ValueIsRequired("request"));

            RuleFor(x => x.Request.Name)
                .MustBeValueObject(DepartmentName.Create);

            RuleFor(x => x.Request.Identifier)
                .MustBeValueObject(DepartmentIdentifier.Create);
        }
    }
    public class UpdateDepartmentHandler : ICommandHandler<Guid, UpdateDepartmentCommand>
    {
        private readonly ILocationsRepository _locationsRepository;
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILogger _logger;
        private readonly IValidator<UpdateDepartmentCommand> _validator;
        private readonly HybridCache _cache;
        private readonly ITransactionManager _transactionManager;

        public UpdateDepartmentHandler(IDepartmentsRepository departmentsRepository,
            ILogger<CreateDepartmentHandler> logger,
            IValidator<UpdateDepartmentCommand> validator,
            ILocationsRepository locationsRepository,
            ITransactionManager transactionManager,
            HybridCache cache)
        {
            _departmentsRepository = departmentsRepository;
            _logger = logger;
            _validator = validator;
            _locationsRepository = locationsRepository;
            _transactionManager = transactionManager;
            _cache = cache;
        }
        public async Task<Result<Guid, Errors>> Handle(UpdateDepartmentCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Received UpdateDepartmentRequest: {Request}", command.Request);

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
                var departmentResult = await _departmentsRepository.GetById(command.DepartmentId, cancellationToken);
                if (departmentResult.IsFailure)
                    return departmentResult.Error;

                var department = departmentResult.Value;
                var oldPath = department.Path.Value;
                var oldDepth = department.Depth;

                var newDepartmentName = DepartmentName.Create(command.Request.Name);

                var newDepartmentIdentifier = DepartmentIdentifier.Create(command.Request.Identifier);

                Department? parent = null;

                if (department.ParentId != null)
                {
                    var parentResult = (await _departmentsRepository.GetById(department.ParentId, cancellationToken));
                    if (parentResult.IsFailure)
                    {
                        transaction.Rollback();
                        return parentResult.Error;
                    }
                    parent = parentResult.Value;
                }

                department.Update(newDepartmentName.Value, newDepartmentIdentifier.Value, parent);

                if (department.Path.Value != oldPath)
                {
                    await _departmentsRepository.UpdateBranchPath(oldPath, oldDepth, department, cancellationToken);
                }

                var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
                if (saveResult.IsFailure)
                {
                    transaction.Rollback();
                    return saveResult.Error;
                }

                transaction.Commit();

                await _cache.RemoveByTagAsync(Constants.DEPARTMENTS_CACHE_TAG, cancellationToken);
                return department.Id;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Update department failed");
                transaction.Rollback();
                throw;
            }
        }
    }
}
