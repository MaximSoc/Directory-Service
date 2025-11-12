using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Presentation.EndpointResults;
using Microsoft.AspNetCore.Mvc;
using Shared;

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

        [HttpGet("/top-positions")]
        public async Task<ActionResult<GetDepartmentsWithTopPositionsResponse>> GetTopPositions(
            [FromServices] GetDepartmentsWithTopPositionsHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(cancellationToken);

            return result;
        }

        [HttpGet("/roots")]
        public async Task<ActionResult<GetParentWithChildrensResponse>> GetParentWithChildrens(
            [FromServices] GetParentWithChildrensHandler handler,
            [FromQuery] GetParentWithChildrensRequest request,
            CancellationToken cancellationToken
            )
        {
            var command = new GetParentWithChildrensCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpGet("/{parentId}/children")]
        public async Task<ActionResult<GetChildrenByParentResponse>> GetChildrenByParent(
            [FromServices] GetChildrenByParentHandler handler,
            [FromQuery] GetChildrenByParentRequest request,
            CancellationToken cancellationToken
            )
        {
            var command = new GetChildrenByParentCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpDelete("/{departmentId}")] 
        public async Task<EndpointResult<Guid>> DeleteDepartment(
            [FromQuery] Guid departmentId,
            [FromServices] DeleteDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var request = new DeleteDepartmentRequest(departmentId);
            var command = new DeleteDepartmentCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }
    }
}
