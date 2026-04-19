using System;
using System.Collections.Generic;
using System.Linq;
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

    public ScreeningService(
        IChildRepository childRepo,
        IParentRepository parentRepo,
        IGenericRepository<PredictionResult> predictionRepo,
        IAiClientProvider aiClient,
        ILogger<ScreeningService> logger)
    {
        _childRepo = childRepo;
        _parentRepo = parentRepo;
        _predictionRepo = predictionRepo;
        _aiClient = aiClient;
        _logger = logger;
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
    public IReadOnlyList<ScreeningQuestionResponse> GetQuestions()
    {
        return _questions;
    }

    /// <summary>
    /// Static accessor for unit tests.
    /// </summary>
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

        // Calculate age in months
        int ageInMonths = ((DateTime.UtcNow.Year - child.DateOfBirth.Year) * 12)
                        + DateTime.UtcNow.Month - child.DateOfBirth.Month;
        if (DateTime.UtcNow.Day < child.DateOfBirth.Day) ageInMonths--;
        if (ageInMonths < 0) ageInMonths = 0;

        // Build payload — all numeric, matching HuggingFace API
        var answersDict = request.Answers.ToDictionary(a => a.QuestionId, a => a.AnswerValue);

        var payload = new AiScreeningPayload
        {
            A1 = answersDict[1],
            A2 = answersDict[2],
            A3 = answersDict[3],
            A4 = answersDict[4],
            A5 = answersDict[5],
            A6 = answersDict[6],
            A7 = answersDict[7],
            A8 = answersDict[8],
            A9 = answersDict[9],
            A10 = answersDict[10],
            Age = ageInMonths,
            Sex = string.Equals(child.Gender, "Male", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
            Jauundice = child.Jaundice ? 1 : 0,   // double 'u' matches the AI API typo
            Family_ASD = child.FamilyAsd ? 1 : 0
        };

        _logger.LogInformation("Calling AI model for ChildId={ChildId}", request.ChildId);

        // Call AI model
        var predictionResponse = await _aiClient.GetPredictionAsync(payload);

        // Save result
        var predictionResult = new PredictionResult
        {
            ChildId = child.ChildId,
            PredictionClass = predictionResponse.Class,
            ConfidenceScore = predictionResponse.Confidence,
            RawResponse = System.Text.Json.JsonSerializer.Serialize(predictionResponse),
            CreatedAt = DateTime.UtcNow
        };

        await _predictionRepo.AddAsync(predictionResult);
        await _predictionRepo.SaveChangesAsync();

        _logger.LogInformation("Screening result saved: Class={Class}, Confidence={Confidence} for ChildId={ChildId}",
            predictionResult.PredictionClass, predictionResult.ConfidenceScore, predictionResult.ChildId);

        return new SubmitScreeningResponse(
            predictionResult.PredictionClass,
            predictionResult.ConfidenceScore,
            predictionResult.CreatedAt
        );
    }

    // ── 4. Get Results ──────────────────────────────────────────────────
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
        var childResults = allResults
            .Where(r => r.ChildId == childId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(pr => new ScreeningResultResponse(
                pr.Id,
                pr.ChildId,
                $"{child.FirstName} {child.LastName}",
                pr.PredictionClass,
                pr.ConfidenceScore,
                pr.CreatedAt
            ));

        return childResults;
    }

    // ── 5. Get Analytics ────────────────────────────────────────────────
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
        {
            return new ScreeningAnalyticsResponse(0, 0, 0, null, null);
        }

        int totalTests = childResults.Count;
        int highRiskCount = childResults.Count(r => r.PredictionClass == "YES");
        int lowRiskCount = childResults.Count(r => r.PredictionClass == "NO");
        string lastPrediction = childResults.First().PredictionClass;
        decimal? latestConfidence = childResults.First().ConfidenceScore;

        return new ScreeningAnalyticsResponse(
            totalTests,
            highRiskCount,
            lowRiskCount,
            lastPrediction,
            latestConfidence
        );
    }
}
