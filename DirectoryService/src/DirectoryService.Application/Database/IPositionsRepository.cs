using CSharpFunctionalExtensions;
using DirectoryService.Domain;
using DirectoryService.Domain.ValueObjects.PositionVO;
using Shared;
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
    }
}
