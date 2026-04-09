using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IAiAssessmentService
{
    Task<IEnumerable<AIQuestionResponse>> GetQuestionsAsync(int testId);
    Task<int> StartTestAsync(Guid parentUserId, StartTestRequest request);
    Task<AIResultResponse> SubmitAnswersAsync(Guid parentUserId, SubmitAnswersRequest request);
    Task<IEnumerable<AIResultResponse>> GetChildResultsAsync(int childId, Guid userId, string role);
    Task<AnalyticsBreakdownResponse> GetAnalyticsAsync(int childId, Guid userId, string role);
    RiskAssessmentResult CalculateRisk(List<int> scores);
}
