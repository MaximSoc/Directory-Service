using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentsRepository
    {
        private readonly DirectoryServiceDbContext _dbContext;
        private readonly ILogger<DepartmentRepository> _logger;

        public DepartmentRepository(DirectoryServiceDbContext dbContext, ILogger<DepartmentRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<Result<Guid, Errors>> Add(Department department, CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.Departments.AddAsync(department, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return department.Id;
            }

            catch (DbUpdateException ex) when (IsDuplicateKeyException(ex))
            {
                _logger.LogWarning(ex, "Department already exists");

                return GeneralErrors.AlreadyExist().ToErrors();
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Fail to insert department");

                return GeneralErrors.Failure().ToErrors();
            }
        }

        private bool IsDuplicateKeyException(DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgex)
            {
                return pgex.SqlState == PostgresErrorCodes.UniqueViolation;
            }

            return false;
        }

        public async Task<Result<Department, Errors>> GetById(Guid? departmentId, CancellationToken cancellationToken)
        {
            var department = await _dbContext.Departments.
                FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            if (department == null)
                return GeneralErrors.NotFound(departmentId).ToErrors();

            return department;
        }

        public async Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken)
        {
            if (departmentIds.Count == 0)
                return GeneralErrors.ValueIsRequired("Departments").ToErrors();

            var existingCount = await _dbContext.Departments
                .Where(d => departmentIds.Contains(d.Id))
                .CountAsync(cancellationToken);

            return existingCount == departmentIds.Count;
        }
    }
}
