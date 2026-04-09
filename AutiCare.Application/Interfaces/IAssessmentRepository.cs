using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IAssessmentRepository : IGenericRepository<ParentTest>
{
    Task<IEnumerable<AIQuestion>> GetQuestionsByTestIdAsync(int testId);
    Task<AIResult?> GetLatestResultByChildIdAsync(int childId);
    Task<IEnumerable<AIResult>> GetAllResultsByChildIdAsync(int childId);
    Task AddAnswersAsync(IEnumerable<ParentAnswer> answers);
    Task AddResultAsync(AIResult result);
}
