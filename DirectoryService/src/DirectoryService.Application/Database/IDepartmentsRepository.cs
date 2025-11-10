using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Database
{
    public interface IDepartmentsRepository
    {
        Task<Result<Guid, Errors>> Add(Department department, CancellationToken cancellationToken);
        Task<Result<Department, Errors>> GetById(Guid? departmentId, CancellationToken cancellationToken);
        Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> departmentIds, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> SaveChanges(CancellationToken cancellationToken);
        Task<Result<Department, Errors>> GetByIdWithLock(Guid? departmentId, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> LockChildrens(string path, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> UpdateChildrensPathAndDepth(string oldPath, string newPath, int depthDelta, CancellationToken cancellationToken);
        Result<Guid, Errors> SoftDelete(Department department, CancellationToken cancellationToken);
    }
}
