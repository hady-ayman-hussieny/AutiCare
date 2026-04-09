using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IParentRepository : IGenericRepository<Parent>
{
    Task<Parent?> GetByUserIdAsync(Guid userId);
    Task<Parent?> GetByEmailAsync(string email);
}
