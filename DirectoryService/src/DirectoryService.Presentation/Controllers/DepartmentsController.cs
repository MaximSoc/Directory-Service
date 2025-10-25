using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;

namespace DirectoryService.Presentation.Controllers
{
    [ApiController]
    [Route("api/departments")]
    public class DepartmentsController : ControllerBase
    {
        [HttpPost]
        public async Task<EndpointResult<Guid>> Create(
            [FromServices] CreateDepartmentHandler handler,
            [FromBody] CreateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateDepartmentCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpPut("{departmentId}/locations")]
        public async Task<EndpointResult> UpdateLocations(
            [FromServices] UpdateDepartmentLocationsHandler handler,
            [FromBody] UpdateDepartmentLocationsRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentLocationsCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpPut("{departmentId}/parent")]
        public async Task<EndpointResult> MoveDepartment(
            [FromServices] MoveDepartmentHandler handler,
            [FromBody] MoveDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new MoveDepartmentCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }
    }
}
