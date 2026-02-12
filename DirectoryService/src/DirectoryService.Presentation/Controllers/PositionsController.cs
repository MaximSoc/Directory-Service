using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/positions")]
    public class PositionsController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Create(
            [FromServices] CreatePositionHandler handler,
            [FromBody] CreatePositionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreatePositionCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpPut("{positionId}")]
        public async Task<EndpointResult<Guid>> Update(
            [FromRoute] Guid positionId,
            [FromServices] UpdatePositionHandler handler,
            [FromBody] UpdatePositionRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdatePositionCommand(positionId, request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpGet]
        public async Task<EndpointResult<PaginationResponse<PositionDto>>> GetPositions(
            [FromServices] GetPositionsHandler handler,
            [FromQuery] GetPositionsRequest request,
            CancellationToken cancellationToken
            )
        {
            var positions = await handler.Handle(request, cancellationToken);

            return positions;
        }

        [HttpGet("{positionId}")]
        public async Task<EndpointResult<GetPositionByIdResponse>> GetPositionById(
            [FromRoute] Guid positionId,
            [FromServices] GetPositionByIdHandler handler,
            CancellationToken cancellationToken
            )
        {
            var request = new GetPositionByIdRequest(positionId);

            var position = await handler.Handle(request, cancellationToken);

            return position;
        }

        [HttpDelete("{positionId}")]
        public async Task<EndpointResult<Guid>> DeletePosition(
            [FromRoute] Guid positionId,
            [FromServices] DeletePositionHandler handler,
            CancellationToken cancellationToken)
        {
            var request = new DeletePositionRequest(positionId);
            var command = new DeletePositionCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }
    }
}
