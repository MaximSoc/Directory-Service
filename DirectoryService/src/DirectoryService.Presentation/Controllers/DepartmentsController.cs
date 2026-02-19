using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Positions;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Contracts.Positions;
using Framework.EndpointResults;
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

        [HttpPut("{departmentId}")]
        public async Task<EndpointResult<Guid>> Update(
            [FromRoute] Guid departmentId,
            [FromServices] UpdateDepartmentHandler handler,
            [FromBody] UpdateDepartmentRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentCommand(departmentId, request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpPut("{departmentId}/locations")]
        public async Task<EndpointResult<Guid>> UpdateLocations(
            [FromRoute] Guid departmentId,
            [FromServices] UpdateDepartmentLocationsHandler handler,
            [FromBody] UpdateDepartmentLocationsRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateDepartmentLocationsCommand(departmentId ,request);

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

        [HttpGet("top-positions")]
        public async Task<ActionResult<GetDepartmentsWithTopPositionsResponse>> GetTopPositions(
            [FromServices] GetDepartmentsWithTopPositionsHandler handler,
            CancellationToken cancellationToken)
        {
            var result = await handler.Handle(cancellationToken);

            return result;
        }

        [HttpGet("roots")]
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

        [HttpGet("{parentId}/children")]
        public async Task<EndpointResult<PaginationResponse<DepartmentDto>>> GetChildrenByParent(
            [FromRoute] Guid parentId,
            [FromServices] GetChildrenByParentHandler handler,
            [FromQuery] GetChildrenByParentRequest request,
            CancellationToken cancellationToken
            )
        {
            var query = new GetChildrenByParentQuery(parentId, request);

            var result = await handler.Handle(query, cancellationToken);

            return result;
        }

        [HttpGet("{departmentId}")]
        public async Task<EndpointResult<GetDepartmentByIdResponse>> GetDepartmentById(
            [FromRoute] Guid departmentId,
            [FromServices] GetDepartmentByIdHandler handler,
            CancellationToken cancellationToken
            )
        {
            var request = new GetDepartmentByIdRequest(departmentId);

            var department = await handler.Handle(request, cancellationToken);

            return department;
        }

        [HttpDelete("{departmentId}")] 
        public async Task<EndpointResult<Guid>> DeleteDepartment(
            [FromRoute] Guid departmentId,
            [FromServices] DeleteDepartmentHandler handler,
            CancellationToken cancellationToken)
        {
            var request = new DeleteDepartmentRequest(departmentId);
            var command = new DeleteDepartmentCommand(request);

            var result = await handler.Handle(command, cancellationToken);

            return result;
        }

        [HttpGet]
        public async Task<EndpointResult<PaginationResponse<DepartmentDto>>> GetDepartments(
            [FromServices] GetDepartmentsHandler handler,
            [FromQuery] GetDepartmentsRequest request,
            CancellationToken cancellationToken
            )
        {
            var result = await handler.Handle(request, cancellationToken);

            return result;
        }
    }
}
