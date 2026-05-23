using AutiCare.Application.DTOs;
using AutiCare.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace AutiCare.Application.Interfaces;

public interface ITherapistRepository : IGenericRepository<Specialist>
{
    Task<IEnumerable<Specialist>> GetTherapistsAsync();
}
