using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutiCare.Infrastructure.Persistence.Repositories;

public class AssessmentRepository : GenericRepository<ParentTest>, IAssessmentRepository
{
    public AssessmentRepository(ApplicationDbContext db) : base(db) { }

    public async Task<IEnumerable<AIQuestion>> GetQuestionsByTestIdAsync(int testId) =>
        await _db.AIQuestions
            .Where(q => q.TestId == testId)
            .OrderBy(q => q.QuestionOrder)
            .ToListAsync();

    public async Task<AIResult?> GetLatestResultByChildIdAsync(int childId) =>
        await _db.AIResults
            .Include(r => r.ParentTest)
            .Where(r => r.ParentTest.ChildId == childId)
            .OrderByDescending(r => r.GeneratedAt)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<AIResult>> GetAllResultsByChildIdAsync(int childId) =>
        await _db.AIResults
            .Include(r => r.ParentTest)
            .Where(r => r.ParentTest.ChildId == childId)
            .OrderByDescending(r => r.GeneratedAt)
            .ToListAsync();

    public async Task AddAnswersAsync(IEnumerable<ParentAnswer> answers) =>
        await _db.ParentAnswers.AddRangeAsync(answers);

    public async Task AddResultAsync(AIResult result) =>
        await _db.AIResults.AddAsync(result);
}
