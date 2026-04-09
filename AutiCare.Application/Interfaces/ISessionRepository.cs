using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface ISessionRepository : IGenericRepository<Session>
{
    Task<IEnumerable<Session>> GetByTreatmentIdAsync(int treatmentId);
}
