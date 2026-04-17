using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class ScreeningService : IScreeningService
{
    private readonly IChildRepository _childRepo;
    private readonly IGenericRepository<PredictionResult> _predictionRepo;
    private readonly IAiClientProvider _aiClient;

    public ScreeningService(
        IChildRepository childRepo,
        IGenericRepository<PredictionResult> predictionRepo,
        IAiClientProvider aiClient)
    {
        _childRepo = childRepo;
        _predictionRepo = predictionRepo;
        _aiClient = aiClient;
    }

    public async Task<SubmitScreeningResponse> SubmitScreeningAsync(SubmitScreeningRequest request)
    {
        if (request.Answers == null || request.Answers.Count != 10)
            throw new ArgumentException("Exactly 10 answers must be provided.");

        var distinctQuestionIds = request.Answers.Select(a => a.QuestionId).Distinct().Count();
        if (distinctQuestionIds != 10)
            throw new ArgumentException("Duplicate or invalid question IDs detected.");
            
        if (request.Answers.Any(a => a.AnswerValue != 0 && a.AnswerValue != 1))
            throw new ArgumentException("Answer values must be exactly 0 or 1.");

        if (request.Answers.Any(a => a.QuestionId < 1 || a.QuestionId > 10))
            throw new ArgumentException("Question IDs must be exactly between 1 and 10.");

        var child = await _childRepo.GetByIdAsync(request.ChildId);
        if (child == null)
            throw new KeyNotFoundException("Child not found.");

        int ageInMonths = ((DateTime.UtcNow.Year - child.DateOfBirth.Year) * 12) + DateTime.UtcNow.Month - child.DateOfBirth.Month;
        if (DateTime.UtcNow.Day < child.DateOfBirth.Day) ageInMonths--;

        if (ageInMonths < 0) ageInMonths = 0;

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
            Sex = string.Equals(child.Gender, "Male", StringComparison.OrdinalIgnoreCase) || string.Equals(child.Gender, "m", StringComparison.OrdinalIgnoreCase) ? "m" : "f",
            Jaundice = child.Jaundice ? "yes" : "no",
            Family_ASD = child.FamilyAsd ? "yes" : "no"
        };

        var predictionResponse = await _aiClient.GetPredictionAsync(payload);

        var predictionResult = new PredictionResult
        {
            ChildId = child.ChildId,
            PredictionClass = predictionResponse.Class,
            ConfidenceScore = null, // Add if API provides it
            RawResponse = System.Text.Json.JsonSerializer.Serialize(predictionResponse),
            CreatedAt = DateTime.UtcNow
        };

        await _predictionRepo.AddAsync(predictionResult);
        await _predictionRepo.SaveChangesAsync();

        return new SubmitScreeningResponse(
            predictionResult.PredictionClass,
            predictionResult.ConfidenceScore,
            predictionResult.CreatedAt
        );
    }

    public async Task<IEnumerable<ScreeningResultResponse>> GetResultsByChildIdAsync(int childId)
    {
        var child = await _childRepo.GetByIdAsync(childId);
        if (child == null) throw new KeyNotFoundException("Child not found");

        var results = await _predictionRepo.GetAllAsync();
        
        return results
            .Where(r => r.ChildId == childId)
            .Select(pr => new ScreeningResultResponse(
                pr.Id,
                pr.ChildId,
                $"{child.FirstName} {child.LastName}",
                pr.PredictionClass,
                pr.ConfidenceScore,
                pr.CreatedAt
            )).OrderByDescending(r => r.CreatedAt);
    }
}
