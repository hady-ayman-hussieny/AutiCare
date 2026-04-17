using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AutiCare.Application.DTOs;

public record ScreeningAnswerItem(int QuestionId, int AnswerValue);

public record SubmitScreeningRequest(
    int ChildId,
    List<ScreeningAnswerItem> Answers
);

public class AiScreeningPayload
{
    public int A1 { get; set; }
    public int A2 { get; set; }
    public int A3 { get; set; }
    public int A4 { get; set; }
    public int A5 { get; set; }
    public int A6 { get; set; }
    public int A7 { get; set; }
    public int A8 { get; set; }
    public int A9 { get; set; }
    public int A10 { get; set; }
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;
    public string Jaundice { get; set; } = string.Empty;
    public string Family_ASD { get; set; } = string.Empty;
}

public class AiScreeningResponse
{
    [JsonPropertyName("Class")]
    public string Class { get; set; } = string.Empty;
}

public record ScreeningResultResponse(
    int Id,
    int ChildId,
    string ChildName,
    string PredictionClass,
    decimal? ConfidenceScore,
    DateTime CreatedAt
);

public record SubmitScreeningResponse(
    string PredictionClass,
    decimal? ConfidenceScore,
    DateTime CreatedAt
);
