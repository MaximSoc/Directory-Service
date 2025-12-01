using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Locations;
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
        public async Task<ActionResult<GetLocationsByDepartmentResponse>> GetLocationsByDepartment (
            [FromServices] GetLocationsByDepartmentHandler handler,
            [FromQuery] GetLocationsByDepartmentRequest request,
            CancellationToken cancellationToken
            )
        {
            var locations = await handler.Handle(request, cancellationToken);

            return Ok(locations);
        }
    }
}
