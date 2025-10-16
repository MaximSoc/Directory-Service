using CSharpFunctionalExtensions;
using DirectoryService.Application;
using DirectoryService.Application.Departments;
using DirectoryService.Contracts;
using DirectoryService.Contracts.Departments;
using DirectoryService.Presentation.EndpointResult;
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
    }
}
