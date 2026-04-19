using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IScreeningService
{
    Task<StartScreeningResponse> StartScreeningAsync(int childId, Guid parentUserId);
    IReadOnlyList<ScreeningQuestionResponse> GetQuestions();
    Task<SubmitScreeningResponse> SubmitScreeningAsync(SubmitScreeningRequest request, Guid parentUserId);
    Task<IEnumerable<ScreeningResultResponse>> GetResultsByChildIdAsync(int childId, Guid userId, string role);
    Task<ScreeningAnalyticsResponse> GetAnalyticsAsync(int childId, Guid userId, string role);
}
