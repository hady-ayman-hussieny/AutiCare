using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface IChildRepository : IGenericRepository<Child>
{
    Task<IEnumerable<Child>> GetByParentIdAsync(int parentId);
    Task<Child?> GetWithDetailsAsync(int childId);
}
