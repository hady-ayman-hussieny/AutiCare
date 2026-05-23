using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IChildService
{
    Task<ChildResponse> CreateAsync(Guid parentUserId, CreateChildRequest request);
    Task<IEnumerable<ChildResponse>> GetMyChildrenAsync(Guid parentUserId);
    Task<ChildResponse?> GetByIdAsync(int childId,Guid userId);
    Task<ChildResponse> UpdateAsync(int childId, Guid parentUserId, UpdateChildRequest request);
    Task DeleteAsync(int childId, Guid parentUserId);
}
