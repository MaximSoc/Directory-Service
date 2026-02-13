using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.PositionVO;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Database
{
    public interface IPositionsRepository
    {
        Task<Result<Guid, Errors>> Add(Position position, CancellationToken cancellationToken);
        Task<Result<bool, Errors>> PositionExistAsync(PositionName positionName, CancellationToken cancellationToken);
        Task<UnitResult<Error>> SoftDeleteByDepartmentId(Guid departmentId, CancellationToken cancellationToken);
        Task<UnitResult<Errors>> RemoveInactive(CancellationToken cancellationToken);
        Task<Result<Position, Errors>> GetById(Guid? positionId, CancellationToken cancellationToken);
        Task<UnitResult<Error>> SoftDeleteByPositionId(Guid positionId, CancellationToken cancellationToken);
    }
}
