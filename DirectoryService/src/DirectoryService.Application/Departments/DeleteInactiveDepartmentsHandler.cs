using Core.Shared;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DirectoryService.Application.Departments
{
    public class DeleteInactiveDepartmentsHandler
    {
        private readonly IDepartmentsRepository _departmentsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly IPositionsRepository _positionsRepository;
        private readonly ILogger _logger;
        private readonly ITransactionManager _transactionManager;
        public DeleteInactiveDepartmentsHandler(
            IDepartmentsRepository departmentsRepository,
            ILocationsRepository locationsRepository,
            IPositionsRepository positionsRepository,
            ILogger<DeleteDepartmentHandler> logger,
            ITransactionManager transactionManager)
        {
            _departmentsRepository = departmentsRepository;
            _locationsRepository = locationsRepository;
            _positionsRepository = positionsRepository;
            _logger = logger;
            _transactionManager = transactionManager;            
        }

        public async Task<UnitResult<Errors>> Handle(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Clean of departments is starting");

            var transactionResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
            if (transactionResult.IsFailure)
                return transactionResult.Error.ToErrors();

            using var transaction = transactionResult.Value;

            var dateNow = DateTime.UtcNow;
            var dateMonthAgo = dateNow.AddMonths(-1);

            // Обновление parent_id, path, depth, updated_at у всех потомков неактивных департаментов. Все потомки поднимаются на 1 вверх

            var inactiveDepartmentsResult = await _departmentsRepository.GetInactiveByDate(dateMonthAgo, cancellationToken);
            if (inactiveDepartmentsResult.IsFailure)
            {
                _logger.LogInformation("Receiving departments inactive failed");
                transaction.Rollback();
                return inactiveDepartmentsResult.Error;
            }

            if (inactiveDepartmentsResult.Value.Count == 0)
            {
                _logger.LogInformation("No departments to delete");
                transaction.Rollback();
                return UnitResult.Success<Errors>();
            }

            var inactiveDepartments = inactiveDepartmentsResult.Value;

            var parentsOfInactiveDepartmentsResult = await _departmentsRepository.GetByIds(inactiveDepartments.Select(id => id.ParentId), cancellationToken);
            if (parentsOfInactiveDepartmentsResult.IsFailure)
            {
                _logger.LogInformation("Receiving parents of inactive departments failed");
                transaction.Rollback();
                return parentsOfInactiveDepartmentsResult.Error;
            }

            foreach (var inactiveDepartment in inactiveDepartments)
            {
                var children = inactiveDepartment.ChildrenDepartments
                    .Where(cd => cd.Depth == inactiveDepartment.Depth + 1);

                foreach (var child in children)
                {
                    var newParent = parentsOfInactiveDepartmentsResult.Value
                        .Where(pd => pd.Id == inactiveDepartment.ParentId)
                        .FirstOrDefault();

                    var updateParent = child.UpdateParent(newParent);
                    if (updateParent.IsFailure)
                    {
                        transaction.Rollback();
                        return updateParent.Error.ToErrors();
                    }
                }
            }

            var saveChangesOne = await _departmentsRepository.SaveChanges(cancellationToken);
            if (saveChangesOne.IsFailure)
            {
                transaction.Rollback();
                return saveChangesOne.Error;
            }

            // Удаление неактивных департаментов с датой удаления более месяца назад (вместе со связями DL, DP)

            var removeDepartmentsResult = await _departmentsRepository.RemoveInactiveByDate(dateMonthAgo, cancellationToken);
            if (removeDepartmentsResult.IsFailure)
            {
                _logger.LogInformation("Departments remove failed");
                transaction.Rollback();
                return removeDepartmentsResult.Error;
            }

            var saveChangesTwo = await _departmentsRepository.SaveChanges(cancellationToken);
            if (saveChangesTwo.IsFailure)
            {
                transaction.Rollback();
                return saveChangesTwo.Error;
            }

            // Удаление неактивных локаций без связей с департаментами

            var removeLocationsResult = await _locationsRepository.RemoveInactive(cancellationToken);
            if (removeLocationsResult.IsFailure)
            {
                _logger.LogInformation("Locations remove failed");
                transaction.Rollback();
                return removeLocationsResult.Error;
            }

            // Удаление неактивных позиций без связей с департаментами

            var removePositionsResult = await _positionsRepository.RemoveInactive(cancellationToken);
            if (removePositionsResult.IsFailure)
            {
                _logger.LogInformation("Positions remove failed");
                transaction.Rollback();
                return removePositionsResult.Error;
            }

            transaction.Commit();

            _logger.LogInformation("Departments remove succesfully");
            return UnitResult.Success<Errors>();
        }
    }
}
