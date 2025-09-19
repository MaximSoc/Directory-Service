using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        [HttpPost]
        public async Task<Result<Guid, string>> Create(
            [FromServices] CreateLocationHandler handler,
            [FromBody] CreateLocationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(request, cancellationToken);

            return result;
        }
    }
}
