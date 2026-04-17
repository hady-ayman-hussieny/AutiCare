using System.Text.Json;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace AutiCare.Application.Services;

public class MockAiClientProvider : IAiClientProvider
{
    private readonly ILogger<MockAiClientProvider> _logger;

    public MockAiClientProvider(ILogger<MockAiClientProvider> logger)
    {
        _logger = logger;
    }

    public Task<AiScreeningResponse> GetPredictionAsync(AiScreeningPayload payload)
    {
        _logger.LogInformation("Sending payload to AI: {payload}", JsonSerializer.Serialize(payload));
        
        int totalScore = payload.A1 + payload.A2 + payload.A3 + payload.A4 + payload.A5 +
                         payload.A6 + payload.A7 + payload.A8 + payload.A9 + payload.A10;

        string predictionClass = totalScore > 3 ? "YES" : "NO";

        return Task.FromResult(new AiScreeningResponse
        {
            Class = predictionClass
        });
    }
}
