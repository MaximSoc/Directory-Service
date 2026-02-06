using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Contracts.Positions
{
    public record GetPositionsResponse
    {
        public List<PositionDto> Positions { get; init; } = null!;

        public int TotalPages { get; init; }

        public int Page { get; init; }
    }
}
