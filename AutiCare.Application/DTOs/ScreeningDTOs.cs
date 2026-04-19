using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AutiCare.Application.DTOs;

// ── Request DTOs ────────────────────────────────────────

public record StartScreeningRequest(int ChildId);

public record ScreeningAnswerItem(int QuestionId, int AnswerValue);

public record SubmitScreeningRequest(
    int ChildId,
    List<ScreeningAnswerItem> Answers
);

// ── Response DTOs ───────────────────────────────────────

public record StartScreeningResponse(string Message);

public record ScreeningQuestionResponse(int QuestionId, string QuestionText);

public record SubmitScreeningResponse(
    string PredictionClass,
    decimal? ConfidenceScore,
    DateTime CreatedAt
);

public record ScreeningResultResponse(
    int Id,
    int ChildId,
    string ChildName,
    string PredictionClass,
    decimal? ConfidenceScore,
    DateTime CreatedAt
);

public record ScreeningAnalyticsResponse(
    int TotalTests,
    int HighRiskCount,
    int LowRiskCount,
    string? LastPrediction,
    decimal? LatestConfidenceScore
);

// ── AI Integration DTOs ─────────────────────────────────

/// <summary>
/// Payload sent to the HuggingFace AI model.
/// All fields are numeric. Note: "Jauundice" has a double 'u' (API typo).
/// </summary>
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
    public int Sex { get; set; }           // 0 = female, 1 = male
    public int Jauundice { get; set; }     // 0 = no, 1 = yes (double 'u' matches API)
    public int Family_ASD { get; set; }    // 0 = no, 1 = yes
}

/// <summary>
/// Parsed response from the HuggingFace AI model.
/// </summary>
public class AiScreeningResponse
{
    /// <summary>Prediction label: "YES" or "NO"</summary>
    public string Class { get; set; } = string.Empty;

    /// <summary>Confidence score between 0.0 and 1.0</summary>
    public decimal Confidence { get; set; }
}

/// <summary>
/// Raw JSON structure from HuggingFace /predict/all endpoint
/// </summary>
public class HuggingFaceRawResponse
{
    [JsonPropertyName("majority_vote")]
    public MajorityVoteResult? MajorityVote { get; set; }

    [JsonPropertyName("results")]
    public Dictionary<string, ModelResult>? Results { get; set; }
}

public class MajorityVoteResult
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("prediction")]
    public int Prediction { get; set; }
}

public class ModelResult
{
    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("prediction")]
    public int Prediction { get; set; }

    [JsonPropertyName("probability")]
    public Dictionary<string, decimal> Probability { get; set; } = new();
}
