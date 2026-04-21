using Core.Handlers;
using Core.Shared;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Contracts.Departments;
using FileService.Communication;
using Microsoft.Extensions.Logging;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryService.Application.Departments;

public record UpdateVideoDepartmentCommand(Guid DepartmentId, UpdateVideoDepartmentRequest Request) : ICommand;

public sealed class UpdateVideoDepartmentHandler : ICommandHandler<Guid, UpdateVideoDepartmentCommand>
{
    private readonly ILogger<UpdateVideoDepartmentHandler> _logger;

    private readonly IDepartmentsRepository _departmentsRepository;

    private readonly ITransactionManager _transactionManager;

    private readonly IFileCommunicationService _fileCommunicationService;

    public UpdateVideoDepartmentHandler(
        ILogger<UpdateVideoDepartmentHandler> logger,
        IDepartmentsRepository departmentsRepository,
        ITransactionManager transactionManager,
        IFileCommunicationService fileCommunicationService)
    {
        _logger = logger;
        _departmentsRepository = departmentsRepository;
        _transactionManager = transactionManager;
        _fileCommunicationService = fileCommunicationService;
    }

    public async Task<Result<Guid, Errors>> Handle(UpdateVideoDepartmentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received UpdateVideoDepartmentRequest: {Request}", command.Request);

        if (command.Request.VideoId.HasValue)
        {
            var existsResult = await _fileCommunicationService.CheckMediaAssetExists(command.Request.VideoId.Value, cancellationToken);

            if (existsResult.IsFailure)
                return existsResult.Error;

            if (!existsResult.Value.Exists)
                return GeneralErrors.NotFound(command.Request.VideoId.Value).ToErrors();
        }

        var departmentResult = await _departmentsRepository.GetById(command.DepartmentId, cancellationToken);
        if (departmentResult.IsFailure)
            return departmentResult.Error;

        departmentResult.Value.UpdateVideoId(command.Request.VideoId);

        await _transactionManager.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated video for department {Id}", departmentResult.Value.Id);

        return departmentResult.Value.Id;
    }
}
