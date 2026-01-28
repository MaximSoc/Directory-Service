using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using Framework.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Create(
            [FromServices] CreateLocationHandler handler,
            [FromBody] CreateLocationRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateLocationCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpGet]
        public async Task<EndpointResult<GetLocationsByDepartmentResponse>> GetLocationsByDepartment (
            [FromServices] GetLocationsByDepartmentHandler handler,
            [FromQuery] GetLocationsByDepartmentRequest request,
            CancellationToken cancellationToken
            )
        {
            var locations = await handler.Handle(request, cancellationToken);

            return locations;
        }

        [HttpPut ("{locationId}")]
        public async Task<EndpointResult> Update(
            [FromRoute] Guid locationId,
            [FromServices] UpdateLocationHandler handler,
            [FromBody] UpdateLocationRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateLocationCommand(locationId, request);

            var result = await handler.Handle( command, cancellationToken );

            return result;
        }

        [HttpDelete("{locationId}")]
        public async Task<EndpointResult<Guid>> DeleteLocation(
            [FromRoute] Guid locationId,
            [FromServices] DeleteLocationHandler handler,
            CancellationToken cancellationToken)
        {
            var request = new DeleteLocationRequest(locationId);
            var command = new DeleteLocationCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }
    }
}
