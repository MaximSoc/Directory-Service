using FileService.Domain.MediaProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileService.UnitTests;

public class ProcessingStepTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithValidData()
    {
        // Arrange
        var type = StepType.GENERATE_HLS;
        int order = 1;
        int weight = 50;

        // Act
        var step = new ProcessingStep(type, order, weight);

        // Assert
        Assert.NotEqual(Guid.Empty, step.Id);
        Assert.Equal(type, step.StepType);
        Assert.Equal(order, step.Order);
        Assert.Equal(weight, step.Weight);
        Assert.Equal(StepStatus.PENDING, step.Status);
        Assert.Null(step.StartedAt);
        Assert.Equal(0, step.Progress);
    }

    [Fact]
    public void Start_ShouldTransitionToInProgress_WhenStatusIsPending()
    {
        // Arrange
        var step = new ProcessingStep(StepType.INITIALIZE, 1, 10);

        // Act
        var result = step.Start();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StepStatus.IN_PROGRESS, step.Status);
        Assert.NotNull(step.StartedAt);
    }

    [Fact]
    public void SetProgress_ShouldApplyClamping()
    {
        // Arrange
        var step = new ProcessingStep(StepType.GENERATE_HLS, 1, 60);

        // Act & Assert
        step.SetProgress(50);
        Assert.Equal(50, step.Progress);

        step.SetProgress(-10);
        Assert.Equal(0, step.Progress);

        step.SetProgress(150);
        Assert.Equal(100, step.Progress);
    }

    [Fact]
    public void Complete_ShouldTransitionToCompleted_WhenInProgress()
    {
        // Arrange
        var step = new ProcessingStep(StepType.EXTRACT_METADATA, 1, 10);
        step.Start();
        string resultData = "{ \"width\": 1920 }";

        // Act
        var result = step.Complete(resultData);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StepStatus.COMPLETED, step.Status);
        Assert.Equal(resultData, step.ResultData);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void Fail_ShouldSaveErrorMessageAndSetStatusFailed()
    {
        // Arrange
        var step = new ProcessingStep(StepType.UPLOAD_HLS, 1, 20);
        step.Start();
        string error = "Network timeout";

        // Act
        var result = step.Fail(error);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(StepStatus.FAILED, step.Status);
        Assert.Equal(error, step.ErrorMessage);
        Assert.NotNull(step.CompletedAt);
    }

    [Fact]
    public void Reset_ShouldReturnToPendingState()
    {
        // Arrange
        var step = new ProcessingStep(StepType.CLEANUP, 1, 5);
        step.Start();
        step.Fail("Some error");

        // Act
        step.Reset();

        // Assert
        Assert.Equal(StepStatus.PENDING, step.Status);
        Assert.Null(step.ErrorMessage);
        Assert.Null(step.StartedAt);
        Assert.Null(step.CompletedAt);
        Assert.Null(step.ResultData);
    }
}
