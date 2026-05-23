using System.Threading.Tasks;
using AutiCare.Application.DTOs;

namespace AutiCare.Application.Interfaces;

public interface IAiClientProvider
{
    Task<AiScreeningResponse> GetPredictionAsync(AiScreeningPayload payload);
}
