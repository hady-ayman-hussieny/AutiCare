using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AutiCare.Application.Services;

public class AiAssessmentService : IAiAssessmentService
{
    private readonly IAssessmentRepository _assessmentRepo;
    private readonly IParentRepository _parentRepo;
    private readonly IChildRepository _childRepo;
    private readonly ILogger<AiAssessmentService> _logger;

    // Parameterless constructor for unit testing
    public AiAssessmentService() 
    {
        _assessmentRepo = null!;
        _parentRepo = null!;
        _childRepo = null!;
        _logger = null!;
    }

    public AiAssessmentService(
        IAssessmentRepository assessmentRepo,
        IParentRepository parentRepo,
        IChildRepository childRepo,
        ILogger<AiAssessmentService> logger)
    {
        _assessmentRepo = assessmentRepo;
        _parentRepo = parentRepo;
        _childRepo = childRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<AIQuestionResponse>> GetQuestionsAsync(int testId)
    {
        var questions = await _assessmentRepo.GetQuestionsByTestIdAsync(testId);
        return questions.Select(q => new AIQuestionResponse(
            q.QuestionId, q.QuestionText, q.QuestionOrder));
    }

    public async Task<int> StartTestAsync(Guid parentUserId, StartTestRequest request)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        // IDOR Fix: Validate child belongs to the parent
        var child = await _childRepo.GetByIdAsync(request.ChildId);
        if (child == null || child.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("You are not authorized to start a test for this child.");

        var parentTest = new ParentTest
        {
            ParentId = parent.ParentId,
            ChildId = request.ChildId,
            TestId = request.TestId,
            IsCompleted = false
        };

        await _assessmentRepo.AddAsync(parentTest);
        await _assessmentRepo.SaveChangesAsync();
        return parentTest.ParentTestId;
    }

    public async Task<AIResultResponse> SubmitAnswersAsync(Guid parentUserId, SubmitAnswersRequest request)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        var parentTest = await _assessmentRepo.GetByIdAsync(request.ParentTestId)
            ?? throw new KeyNotFoundException("Test session not found");

        // IDOR Fix: Validate test belongs to the parent
        if (parentTest.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("You are not authorized to submit answers for this test.");

        if (parentTest.IsCompleted)
            throw new InvalidOperationException("Test already completed");

        // Save answers
        var answers = request.Answers.Select(a => new ParentAnswer
        {
            ParentTestId = request.ParentTestId,
            QuestionId = a.QuestionId,
            AnswerValue = a.AnswerValue
        }).ToList();

        await _assessmentRepo.AddAnswersAsync(answers);

        // Calculate risk locally
        var scores = request.Answers.Select(a => a.AnswerValue).ToList();
        var risk = CalculateRisk(scores);

        var aiResult = new AIResult
        {
            ParentTestId = request.ParentTestId,
            Score = (decimal)risk.Probability * 100,
            StatusLevel = risk.RiskLevel,
            Recommendation = GenerateRecommendation(risk.RiskLevel)
        };

        await _assessmentRepo.AddResultAsync(aiResult);

        parentTest.IsCompleted = true;
        _assessmentRepo.Update(parentTest);
        await _assessmentRepo.SaveChangesAsync();

        var child = await _childRepo.GetByIdAsync(parentTest.ChildId);

        return new AIResultResponse(
            aiResult.AIResultId,
            parentTest.ChildId,
            child != null ? $"{child.FirstName} {child.LastName}" : "",
            aiResult.Score,
            aiResult.StatusLevel,
            aiResult.Recommendation,
            aiResult.GeneratedAt
        );
    }

    public async Task<IEnumerable<AIResultResponse>> GetChildResultsAsync(int childId, Guid userId, string role)
    {
        var child = await _childRepo.GetByIdAsync(childId);
        if (child == null) throw new KeyNotFoundException("Child not found");

        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("You are not authorized to view this child's results.");
        }

        var results = await _assessmentRepo.GetAllResultsByChildIdAsync(childId);
        var childName = $"{child.FirstName} {child.LastName}";

        return results.Select(r => new AIResultResponse(
            r.AIResultId,
            childId,
            childName,
            r.Score,
            r.StatusLevel,
            r.Recommendation,
            r.GeneratedAt
        ));
    }

    public async Task<AnalyticsBreakdownResponse> GetAnalyticsAsync(int childId, Guid userId, string role)
    {
        var child = await _childRepo.GetByIdAsync(childId);
        if (child == null) throw new KeyNotFoundException("Child not found");

        if (role == "Parent")
        {
            var parent = await _parentRepo.GetByUserIdAsync(userId);
            if (parent == null || child.ParentId != parent.ParentId)
                throw new UnauthorizedAccessException("You are not authorized to view this child's results.");
        }

        var results = await _assessmentRepo.GetAllResultsByChildIdAsync(childId);
        
        var history = results
            .OrderBy(r => r.GeneratedAt)
            .Select(r => new HistoricalScore(r.GeneratedAt, r.Score, r.StatusLevel))
            .ToList();

        return new AnalyticsBreakdownResponse(childId, history);
    }

    // ── Core calculation logic (also used by unit tests) ──────────────────────
    public RiskAssessmentResult CalculateRisk(List<int> scores)
    {
        if (scores.Count != 20)
            throw new ArgumentException("Exactly 20 scores are required");

        int total = scores.Sum();
        // Max possible = 20 * 2 = 40
        double probability = total / 40.0;

        string riskLevel = total switch
        {
            <= 10 => "Low",
            <= 25 => "Moderate",
            _     => "High"
        };

        return new RiskAssessmentResult(total, riskLevel, probability);
    }

    private static string GenerateRecommendation(string riskLevel) => riskLevel switch
    {
        "Low"      => "No immediate concerns. Continue regular developmental monitoring.",
        "Moderate" => "Some indicators present. Consultation with a specialist is recommended.",
        "High"     => "Multiple indicators detected. Please schedule a clinical evaluation as soon as possible.",
        _          => "Please consult a medical professional."
    };
}
