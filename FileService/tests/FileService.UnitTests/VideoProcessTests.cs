using FileService.Domain.MediaProcessing;

namespace FileService.UnitTests;

public class VideoProcessTests
{
    private readonly Guid _videoAssetId = Guid.NewGuid();

    [Fact]
    public void Constructor_ShouldInitializeWithCorrectDefaults()
    {
        // Act
        var process = new VideoProcess(_videoAssetId);

        // Assert
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(6, process.Steps.Count);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Equal(_videoAssetId, process.VideoAssetId);
        Assert.NotEqual(Guid.Empty, process.Id);
    }

    [Fact]
    public void ProcessNextStep_ShouldStartFirstStep_AndFollowSequence()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act & Assert 1: Первый шаг
        var result1 = process.ProcessNextStep();
        Assert.True(result1.IsSuccess);
        Assert.NotNull(result1.Value);
        Assert.Equal(StepType.INITIALIZE, result1.Value.StepType);
        Assert.Equal(StepStatus.IN_PROGRESS, result1.Value.Status);

        // Act & Assert 2: Завершение и переход к следующему
        process.CompleteCurrentStep();
        var result2 = process.ProcessNextStep();

        Assert.True(result2.IsSuccess);
        Assert.Equal(StepType.EXTRACT_METADATA, result2.Value.StepType);
    }

    [Fact]
    public void CompleteCurrentStep_ShouldRecalculateProgress_BasedOnWeights()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // INITIALIZE (weight 0)
        process.ProcessNextStep();
        process.CompleteCurrentStep();
        Assert.Equal(0, process.ProgressPercentage);

        // EXTRACT_METADATA (weight 10)
        process.ProcessNextStep();
        process.CompleteCurrentStep();

        // Assert
        Assert.Equal(10, process.ProgressPercentage);
    }

    [Fact]
    public void FailCurrentStep_ShouldSetStatusToFailed()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.ProcessNextStep();
        string error = "Conversion failed";

        // Act
        var result = process.FailCurrentStep(error);

        // Assert
        Assert.True(result.IsSuccess);
        var step = process.Steps.First(s => s.StepType == StepType.INITIALIZE);
        Assert.Equal(StepStatus.FAILED, step.Status);
        Assert.Equal(error, step.ErrorMessage);
    }

    [Fact]
    public void Reset_ShouldClearAllProgressAndSteps()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.ProcessNextStep();
        process.FailCurrentStep("Error");
        process.Fail("Process failed", isCritical: false);

        // Act
        var result = process.Reset();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(ProcessingStatus.IN_PROGRESS, process.Status);
        Assert.Equal(0, process.ProgressPercentage);
        Assert.Null(process.ErrorMessage);
        Assert.All(process.Steps, s => Assert.Equal(StepStatus.PENDING, s.Status));
    }

    [Fact]
    public void CompleteProcess_ShouldBeAutomatic_WhenLastStepFinished()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        int totalSteps = process.Steps.Count;
        for (int i = 0; i < totalSteps; i++)
        {
            process.ProcessNextStep();
            process.CompleteCurrentStep();
        }

        // Act
        var result = process.ProcessNextStep();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ProcessingStatus.COMPLETED, process.Status);
        Assert.Equal(100, process.ProgressPercentage);
        Assert.NotNull(process.CompletedAt);
    }

    [Fact]
    public void ScheduleRetry_ShouldReturnError_WhenMaxRetriesExceeded()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Error");

        // Имитируем 3 успешных планирования ретрая
        for (int i = 0; i < 3; i++)
        {
            process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));
            process.Reset();
            process.Fail("Error");
        }

        // Act
        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("processing.retry.exhausted", result.Error.Code);
    }

    [Fact]
    public void ScheduleRetry_ShouldReturnError_WhenFailureIsCritical()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.Fail("Critical DB Crash", isCritical: true);

        // Act
        var result = process.ScheduleRetry(DateTime.UtcNow.AddMinutes(5));

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(process.CanRetry());
    }

    [Fact]
    public void Reset_ShouldReturnError_IfStatusIsNotFailed()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.Reset();

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("processing.invalid.status", result.Error.Code);
    }

    [Fact]
    public void FailCurrentStep_ShouldReturnError_WhenMessageIsEmpty()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);
        process.ProcessNextStep();

        // Act
        var result = process.FailCurrentStep(" ");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("step.error.required", result.Error.Code);
    }

    [Fact]
    public void FailCurrentStep_ShouldFail_WhenNoStepIsInProgress()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        // Act
        var result = process.FailCurrentStep("Error");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("processing.no.active.step", result.Error.Code);
    }

    [Fact]
    public void RecalculateProgress_ShouldSumCompletedStepsWeights()
    {
        // Arrange
        var process = new VideoProcess(_videoAssetId);

        process.ProcessNextStep();
        process.CompleteCurrentStep();

        process.ProcessNextStep();
        process.CompleteCurrentStep();

        // Assert
        Assert.Equal(10, process.ProgressPercentage);
    }
}
