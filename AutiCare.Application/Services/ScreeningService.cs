using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AutiCare.Application.Services;

public class ScreeningService : IScreeningService
{
    private readonly IChildRepository _childRepo;
    private readonly IParentRepository _parentRepo;
    private readonly IGenericRepository<PredictionResult> _predictionRepo;
    private readonly IAiClientProvider _aiClient;
    private readonly ILogger<ScreeningService> _logger;

    // ── Static screening questions (M-CHAT-R inspired) ──────────────────
    private static readonly IReadOnlyList<ScreeningQuestionResponse> _questions = new List<ScreeningQuestionResponse>
    {
        new(1,  "Does your child look at you when you call his/her name?"),
        new(2,  "How easy is it for you to get eye contact with your child?"),
        new(3,  "Does your child point to indicate that s/he wants something?"),
        new(4,  "Does your child point to share interest with you?"),
        new(5,  "Does your child pretend? (e.g., care for dolls, talk on a toy phone)"),
        new(6,  "Does your child follow where you're looking?"),
        new(7,  "If you or someone in the family is visibly upset, does your child show signs of wanting to comfort them?"),
        new(8,  "Would you describe your child's first words as typical?"),
        new(9,  "Does your child use simple gestures? (e.g., wave goodbye)"),
        new(10, "Does your child stare at nothing with no apparent purpose?")
    };

    // ── Scoring polarity ─────────────────────────────────────────────────
    // For Q1–Q9: answer = 0 (NO) is the concerning response (child lacks typical behaviour).
    // For Q10:   answer = 1 (YES) is the concerning response (atypical staring behaviour).
    private static readonly HashSet<int> _invertedQuestions = new() { 10 };

    // ── Question → Category mapping ──────────────────────────────────────
    private static readonly Dictionary<int, string> _questionCategories = new()
    {
        { 1, "SocialAttention"      },
        { 2, "SocialAttention"      },
        { 3, "JointAttention"       },
        { 4, "JointAttention"       },
        { 5, "Imagination"          },
        { 6, "JointAttention"       },
        { 7, "SocialCommunication"  },
        { 8, "Language"             },
        { 9, "SocialCommunication"  },
        { 10,"RepetitiveBehavior"   }
    };

    public ScreeningService(
        IChildRepository childRepo,
        IParentRepository parentRepo,
        IGenericRepository<PredictionResult> predictionRepo,
        IAiClientProvider aiClient,
        ILogger<ScreeningService> logger)
    {
        _childRepo      = childRepo;
        _parentRepo     = parentRepo;
        _predictionRepo = predictionRepo;
        _aiClient       = aiClient;
        _logger         = logger;
    }

    // ── 1. Start Screening ──────────────────────────────────────────────
    public async Task<StartScreeningResponse> StartScreeningAsync(int childId, Guid parentUserId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent profile not found.");

        var child = await _childRepo.GetByIdAsync(childId)
            ?? throw new KeyNotFoundException("Child not found.");

        if (child.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("You are not authorized to start a screening for this child.");

        _logger.LogInformation("Screening session started for ChildId={ChildId} by ParentId={ParentId}", childId, parent.ParentId);

        return new StartScreeningResponse("Screening session started successfully. Please answer all 10 questions.");
    }

    // ── 2. Get Questions ────────────────────────────────────────────────
    public IReadOnlyList<ScreeningQuestionResponse> GetQuestions() => _questions;

    /// <summary>Static accessor for unit tests.</summary>
    public static IReadOnlyList<ScreeningQuestionResponse> GetQuestionsStatic() => _questions;

    // ── 3. Submit Screening ─────────────────────────────────────────────
    public async Task<SubmitScreeningResponse> SubmitScreeningAsync(SubmitScreeningRequest request, Guid parentUserId)
    {
        // Validate answers
        if (request.Answers == null || request.Answers.Count != 10)
            throw new ArgumentException("Exactly 10 answers must be provided.");

        var distinctQuestionIds = request.Answers.Select(a => a.QuestionId).Distinct().Count();
        if (distinctQuestionIds != 10)
            throw new ArgumentException("Duplicate or invalid question IDs detected.");

        if (request.Answers.Any(a => a.AnswerValue != 0 && a.AnswerValue != 1))
            throw new ArgumentException("Answer values must be exactly 0 or 1.");

        if (request.Answers.Any(a => a.QuestionId < 1 || a.QuestionId > 10))
            throw new ArgumentException("Question IDs must be between 1 and 10.");

        // Verify parent ownership
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent profile not found.");

        var child = await _childRepo.GetByIdAsync(request.ChildId)
            ?? throw new KeyNotFoundException("Child not found.");

        if (child.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("You are not authorized to submit screening for this child.");

        // Validate child metadata for AI prediction
        if (string.IsNullOrWhiteSpace(child.Gender))
            throw new ArgumentException("Child profile is incomplete. Gender is required for AI prediction.");
            
        if (child.DateOfBirth == default || child.DateOfBirth > DateTime.UtcNow)
            throw new ArgumentException("Child profile is incomplete. A valid Date of Birth is required.");

        // Calculate age in months
        int ageInMonths = ((DateTime.UtcNow.Year - child.DateOfBirth.Year) * 12)
                        + DateTime.UtcNow.Month - child.DateOfBirth.Month;
        if (DateTime.UtcNow.Day < child.DateOfBirth.Day) ageInMonths--;
        if (ageInMonths < 0) ageInMonths = 0;

        // Build AI payload — all numeric, matching HuggingFace API
        var answersDict = request.Answers.ToDictionary(a => a.QuestionId, a => a.AnswerValue);

        var payload = new AiScreeningPayload
        {
            A1         = answersDict[1],
            A2         = answersDict[2],
            A3         = answersDict[3],
            A4         = answersDict[4],
            A5         = answersDict[5],
            A6         = answersDict[6],
            A7         = answersDict[7],
            A8         = answersDict[8],
            A9         = answersDict[9],
            A10        = answersDict[10],
            Age        = ageInMonths,
            Sex        = string.Equals(child.Gender, "Male", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
            Jaundice   = child.Jaundice  ? 1 : 0,   
            Family_ASD = child.FamilyAsd ? 1 : 0
        };

        _logger.LogInformation("Calling AI model for ChildId={ChildId}", request.ChildId);

        // Call AI model (UNCHANGED)
        var predictionResponse = await _aiClient.GetPredictionAsync(payload);

        // Persist result — now includes AnswersJson for later score calculation
        var predictionResult = new PredictionResult
        {
            ChildId         = child.ChildId,
            PredictionClass = predictionResponse.Class,
            ConfidenceScore = predictionResponse.Confidence,
            RawResponse     = JsonSerializer.Serialize(predictionResponse),
            AnswersJson     = JsonSerializer.Serialize(answersDict),   // NEW: store answers
            CreatedAt       = DateTime.UtcNow
        };

        await _predictionRepo.AddAsync(predictionResult);
        await _predictionRepo.SaveChangesAsync();

        _logger.LogInformation(
            "Screening result saved: Class={Class}, Confidence={Confidence} for ChildId={ChildId}",
            predictionResult.PredictionClass, predictionResult.ConfidenceScore, predictionResult.ChildId);

        return new SubmitScreeningResponse(
            predictionResult.PredictionClass,
            predictionResult.ConfidenceScore,
            predictionResult.CreatedAt
        );
    }

    // ── 4. Get Results (enhanced) ────────────────────────────────────────
    public async Task<IEnumerable<ScreeningResultResponse>> GetResultsByChildIdAsync(int childId, Guid userId, string role)
    {
        var child = await _childRepo.GetByIdAsync(childId)
            ?? throw new KeyNotFoundException("Child not found.");

        // IDOR protection for parents
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("You are not authorized to view this child's results.");
        }

        var allResults = await _predictionRepo.GetAllAsync();
        var childName  = $"{child.FirstName} {child.LastName}";

        var childResults = allResults
            .Where(r  => r.ChildId == childId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(pr => BuildEnrichedResult(pr, childName));

        return childResults;
    }

    // ── 5. Get Analytics (UNCHANGED) ────────────────────────────────────
    public async Task<ScreeningAnalyticsResponse> GetAnalyticsAsync(int childId, Guid userId, string role)
    {
        var child = await _childRepo.GetByIdAsync(childId)
            ?? throw new KeyNotFoundException("Child not found.");

        // IDOR protection
        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("You are not authorized to view this child's analytics.");
        }

        var allResults = await _predictionRepo.GetAllAsync();
        var childResults = allResults
            .Where(r => r.ChildId == childId)
            .OrderByDescending(r => r.CreatedAt)
            .ToList();

        if (childResults.Count == 0)
            return new ScreeningAnalyticsResponse(0, 0, 0, null, null);

        int totalTests    = childResults.Count;
        int highRiskCount = childResults.Count(r => r.PredictionClass == "YES");
        int lowRiskCount  = childResults.Count(r => r.PredictionClass == "NO");
        string lastPrediction    = childResults.First().PredictionClass;
        decimal? latestConfidence = childResults.First().ConfidenceScore;

        return new ScreeningAnalyticsResponse(totalTests, highRiskCount, lowRiskCount, lastPrediction, latestConfidence);
    }

    // ── Private score-computation helpers ───────────────────────────────

    /// <summary>
    /// Builds a fully-enriched <see cref="ScreeningResultResponse"/> for one PredictionResult row.
    /// Handles null AnswersJson gracefully — old records return default/zero values.
    /// </summary>
    private static ScreeningResultResponse BuildEnrichedResult(PredictionResult pr, string childName)
    {
        // Parse stored answers; fall back to empty dict for legacy records
        Dictionary<int, int>? answers = null;
        if (!string.IsNullOrWhiteSpace(pr.AnswersJson))
        {
            try
            {
                answers = JsonSerializer.Deserialize<Dictionary<int, int>>(pr.AnswersJson);
            }
            catch
            {
                // Malformed JSON — treat as missing; no exception propagated
                answers = null;
            }
        }

        bool hasAnswers = answers != null && answers.Count == 10;

        int aqScore              = hasAnswers ? ComputeAqScore(answers!)                             : 0;
        string riskLevel         = hasAnswers ? $"{aqScore * 10}%"                                   : "N/A";
        string probability       = hasAnswers ? MapProbability(aqScore)                              : "Unknown";
        int socialAttention      = hasAnswers ? ComputeCategoryPercentage(answers!, "SocialAttention")     : 0;
        int jointAttention       = hasAnswers ? ComputeCategoryPercentage(answers!, "JointAttention")      : 0;
        int socialCommunication  = hasAnswers ? ComputeCategoryPercentage(answers!, "SocialCommunication") : 0;
        int language             = hasAnswers ? ComputeCategoryPercentage(answers!, "Language")            : 0;
        int imagination          = hasAnswers ? ComputeCategoryPercentage(answers!, "Imagination")         : 0;
        int repetitiveBehavior   = hasAnswers ? ComputeCategoryPercentage(answers!, "RepetitiveBehavior")  : 0;

        return new ScreeningResultResponse(
            pr.Id,
            pr.ChildId,
            childName,
            pr.PredictionClass,
            pr.ConfidenceScore,
            aqScore,
            riskLevel,
            probability,
            socialAttention,
            jointAttention,
            socialCommunication,
            language,
            imagination,
            repetitiveBehavior,
            pr.CreatedAt
        );
    }

    /// <summary>
    /// Counts the number of "concerning" answers out of 10.
    /// Polarity: Q1–Q9 → 0 is concerning; Q10 → 1 is concerning.
    /// </summary>
    private static int ComputeAqScore(Dictionary<int, int> answers)
    {
        int score = 0;
        foreach (var (qId, value) in answers)
        {
            bool concerning = _invertedQuestions.Contains(qId) ? value == 1 : value == 0;
            if (concerning) score++;
        }
        return score;
    }

    /// <summary>
    /// Returns the percentage (0–100) of concerning answers within a named category.
    /// Result is rounded to the nearest integer.
    /// </summary>
    private static int ComputeCategoryPercentage(Dictionary<int, int> answers, string category)
    {
        var categoryQIds = _questionCategories
            .Where(kv => kv.Value == category)
            .Select(kv => kv.Key)
            .ToList();

        if (categoryQIds.Count == 0) return 0;

        int concerning = categoryQIds
            .Where(qId => answers.ContainsKey(qId))
            .Count(qId => _invertedQuestions.Contains(qId) ? answers[qId] == 1 : answers[qId] == 0);

        return (int)Math.Round((double)concerning / categoryQIds.Count * 100);
    }

    /// <summary>
    /// Maps an AQ score (0–10) to a human-readable probability label.
    /// Thresholds based on standard M-CHAT-R risk classification.
    /// </summary>
    private static string MapProbability(int aqScore) => aqScore switch
    {
        >= 7 => "High",
        >= 4 => "Medium",
        _    => "Low"
    };
}
