using CSharpFunctionalExtensions;
using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.Domain.MediaProcessing;

public class ProcessingStep
{
    public Guid Id { get; private set; }

    public StepType StepType { get; private set; }

    public int Order { get; private set; }

    public int Weight { get; private set; }

    public StepStatus Status { get; private set; }

    public string? ResultData { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public int Progress { get; private set; }

    public void SetProgress(int progress)
    {
        Progress = Math.Clamp(progress, 0, 100);
    }

    // EF Core
    private ProcessingStep()
    {
        
    }

    public ProcessingStep(StepType stepType, int order, int weight)
    {
        Id = Guid.NewGuid();
        StepType = stepType;
        Order = order;
        Weight = weight;
        Status = StepStatus.PENDING;
    }

    public UnitResult<Error> Start()
    {
        if (Status != StepStatus.PENDING)
            return Error.Validation("step.invalid.status", $"Can only start step from PENDING status, current: {Status}");

        Status = StepStatus.IN_PROGRESS;
        StartedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Complete(string? resultData = null)
    {
        if (Status != StepStatus.IN_PROGRESS)
            return Error.Validation("step.invalid.status", $"Can only start step from IN_PROGRESS status, current: {Status}");

        Status = StepStatus.COMPLETED;
        ResultData = resultData;
        CompletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> Fail(string errorMessage)
    {
        if (Status != StepStatus.IN_PROGRESS)
            return Error.Validation("step.invalid.status", $"Can only start step from IN_PROGRESS status, current: {Status}");

        if (string.IsNullOrWhiteSpace(errorMessage))
            return Error.Validation("step.error.required", "Error message is required");

        Status = StepStatus.FAILED;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }

    public void Reset()
    {
        Status = StepStatus.PENDING;
        ResultData = null;
        ErrorMessage = null;
        StartedAt = null;
        CompletedAt = null;
    }
}

public enum StepType
{
    INITIALIZE,
    EXTRACT_METADATA,
    GENERATE_HLS,
    UPLOAD_HLS,
    GENERATE_PREVIEW,
    CLEANUP
}

public enum StepStatus
{
    PENDING,
    IN_PROGRESS,
    COMPLETED,
    FAILED
}
