using System;
using System.Collections.Generic;

namespace AutiCare.Application.DTOs;

public record StartTestRequest(int ChildId, int TestId);

public record SubmitAnswersRequest(
    int ParentTestId,
    List<AnswerItem> Answers
);

public record AnswerItem(int QuestionId, int AnswerValue);

public record AIQuestionResponse(
    int QuestionId,
    string QuestionText,
    int QuestionOrder
);

public record AIResultResponse(
    int AIResultId,
    int ChildId,
    string ChildName,
    decimal Score,
    string StatusLevel,
    string? Recommendation,
    DateTime GeneratedAt
);

public record RiskAssessmentResult(
    int TotalScore,
    string RiskLevel,
    double Probability
);

public record AnalyticsBreakdownResponse(
    int ChildId,
    List<HistoricalScore> History
);

public record HistoricalScore(
    DateTime TestDate,
    decimal Score,
    string StatusLevel
);

