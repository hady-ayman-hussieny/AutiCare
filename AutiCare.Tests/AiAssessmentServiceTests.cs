using AutiCare.Application.DTOs;
using AutiCare.Application.Services;
using System.Collections.Generic;
using Xunit;

namespace AutiCare.Tests;

public class ScreeningServiceTests
{
    [Fact]
    public void GetQuestions_Returns10Questions()
    {
        // The questions are static — we can test by creating a real instance
        // with null deps (only GetQuestions doesn't use them)
        var questions = ScreeningService.GetQuestionsStatic();

        Assert.Equal(10, questions.Count);
        
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal(i + 1, questions[i].QuestionId);
            Assert.False(string.IsNullOrWhiteSpace(questions[i].QuestionText));
        }
    }

    [Fact]
    public void AiScreeningPayload_AllFieldsAreNumeric()
    {
        var payload = new AiScreeningPayload
        {
            A1 = 1, A2 = 0, A3 = 1, A4 = 0, A5 = 1,
            A6 = 1, A7 = 0, A8 = 1, A9 = 0, A10 = 1,
            Age = 36,
            Sex = 1,          // male = 1
            Jauundice = 0,    // double 'u' matches API
            Family_ASD = 0
        };

        Assert.Equal(1, payload.A1);
        Assert.Equal(36, payload.Age);
        Assert.Equal(1, payload.Sex);
        Assert.Equal(0, payload.Jauundice);
        Assert.Equal(0, payload.Family_ASD);
    }

    [Fact]
    public void ScreeningAnalyticsResponse_EmptyResults_ReturnsZeros()
    {
        var analytics = new ScreeningAnalyticsResponse(0, 0, 0, null, null);

        Assert.Equal(0, analytics.TotalTests);
        Assert.Equal(0, analytics.HighRiskCount);
        Assert.Equal(0, analytics.LowRiskCount);
        Assert.Null(analytics.LastPrediction);
        Assert.Null(analytics.LatestConfidenceScore);
    }

    [Fact]
    public void ScreeningAnalyticsResponse_WithResults_CalculatesCorrectly()
    {
        var analytics = new ScreeningAnalyticsResponse(3, 1, 2, "NO", 0.82m);

        Assert.Equal(3, analytics.TotalTests);
        Assert.Equal(1, analytics.HighRiskCount);
        Assert.Equal(2, analytics.LowRiskCount);
        Assert.Equal("NO", analytics.LastPrediction);
        Assert.Equal(0.82m, analytics.LatestConfidenceScore);
    }
}
