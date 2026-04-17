using System.Collections.Generic;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IScreeningService
{
    Task<SubmitScreeningResponse> SubmitScreeningAsync(SubmitScreeningRequest request);
    Task<IEnumerable<ScreeningResultResponse>> GetResultsByChildIdAsync(int childId);
}
