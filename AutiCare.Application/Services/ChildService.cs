using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using AutiCare.Domain.Entities;

namespace AutiCare.Application.Services;

public class ChildService : IChildService
{
    private readonly IChildRepository _childRepo;
    private readonly IParentRepository _parentRepo;

    public ChildService(IChildRepository childRepo, IParentRepository parentRepo)
    {
        _childRepo = childRepo;
        _parentRepo = parentRepo;
    }

    public async Task<ChildResponse> CreateAsync(Guid parentUserId, CreateChildRequest request)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        var child = new Child
        {
            ParentId = parent.ParentId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = DateTime.SpecifyKind(request.DateOfBirth, DateTimeKind.Utc),
            Gender = request.Gender,
            MedicalHistory = request.MedicalHistory
        }; 

        await _childRepo.AddAsync(child);
        await _childRepo.SaveChangesAsync();
        return ToResponse(child);
    }

    public async Task<IEnumerable<ChildResponse>> GetMyChildrenAsync(Guid parentUserId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(  parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        var children = await _childRepo.GetByParentIdAsync(parent.ParentId);
        return children.Select(ToResponse);
    }

    public async Task<ChildResponse?> GetByIdAsync(int childId, Guid userId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(userId)
            ?? throw new KeyNotFoundException("Parent not found");

        var child = await _childRepo.GetByIdAsync(childId);

        if (child == null || child.ParentId != parent.ParentId)
            return null;

        return ToResponse(child);
    }

    public async Task<ChildResponse> UpdateAsync(int childId, Guid parentUserId, UpdateChildRequest request)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        var child = await _childRepo.GetByIdAsync(childId)
            ?? throw new KeyNotFoundException("Child not found");

        if (child.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("Not your child");

        if (request.FirstName != null) child.FirstName = request.FirstName;
        if (request.LastName != null) child.LastName = request.LastName;
        if (request.MedicalHistory != null) child.MedicalHistory = request.MedicalHistory;

        _childRepo.Update(child);
        await _childRepo.SaveChangesAsync();
        return ToResponse(child);
    }

    public async Task DeleteAsync(int childId, Guid parentUserId)
    {
        var parent = await _parentRepo.GetByUserIdAsync(parentUserId)
            ?? throw new KeyNotFoundException("Parent not found");

        var child = await _childRepo.GetByIdAsync(childId)
            ?? throw new KeyNotFoundException("Child not found");

        if (child.ParentId != parent.ParentId)
            throw new UnauthorizedAccessException("Not your child");

        child.IsDeleted = true;
        child.DeletedAt = DateTime.UtcNow;
        child.DeletedBy =  parentUserId .ToString();

        _childRepo.Update(child);
        await _childRepo.SaveChangesAsync();
    }

    private static ChildResponse ToResponse(Child c) => new(
        c.ChildId, c.FirstName, c.LastName, c.DateOfBirth, c.Gender,
        DateTime.Today.Year - c.DateOfBirth.Year,
        c.MedicalHistory, c.CreatedAt);
}
