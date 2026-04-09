using AutiCare.Application.Services;
using System;
using System.Collections.Generic;
using Xunit;

namespace AutiCare.Tests;

public class AiAssessmentServiceTests
{
    private readonly AiAssessmentService _service;

    public AiAssessmentServiceTests()
    {
        _service = new AiAssessmentService();
    }

    [Fact]
    public void CalculateRisk_LowScore_ReturnsLowRisk()
    {
        // Arrange - 20 zeros = total 0
        var scores = new List<int>(new int[20]);

        // Act
        var result = _service.CalculateRisk(scores);

        // Assert
        Assert.Equal("Low", result.RiskLevel);
        Assert.Equal(0, result.TotalScore);
        Assert.True(result.Probability < 0.5);
    }

    [Fact]
    public void CalculateRisk_HighScore_ReturnsHighRisk()
    {
        // Arrange - 20 twos = total 40
        var scores = new List<int>();
        for (int i = 0; i < 20; i++) scores.Add(2);

        // Act
        var result = _service.CalculateRisk(scores);

        // Assert
        Assert.Equal("High", result.RiskLevel);
        Assert.Equal(40, result.TotalScore);
        Assert.True(result.Probability > 0.9);
    }

    [Fact]
    public void CalculateRisk_ModerateScore_ReturnsModerateRisk()
    {
        // Arrange - 20 ones = total 20
        var scores = new List<int>();
        for (int i = 0; i < 20; i++) scores.Add(1);

        // Act
        var result = _service.CalculateRisk(scores);

        // Assert
        Assert.Equal("Moderate", result.RiskLevel);
        Assert.Equal(20, result.TotalScore);
    }

    [Fact]
    public void CalculateRisk_InvalidCount_ThrowsException()
    {
        // Arrange - only 3 answers instead of 20
        var scores = new List<int> { 1, 2, 3 };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _service.CalculateRisk(scores));
    }
}
