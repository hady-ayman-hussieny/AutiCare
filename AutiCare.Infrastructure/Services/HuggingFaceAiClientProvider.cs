using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutiCare.Infrastructure.Services;

/// <summary>
/// Real AI client that calls the HuggingFace /predict/all endpoint.
/// </summary>
public class HuggingFaceAiClientProvider : IAiClientProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HuggingFaceAiClientProvider> _logger;

    public HuggingFaceAiClientProvider(HttpClient httpClient, ILogger<HuggingFaceAiClientProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AiScreeningResponse> GetPredictionAsync(AiScreeningPayload payload)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = null // Keep exact property names (A1, A2, Jauundice, etc.)
        });

        _logger.LogInformation("Sending AI prediction request: {Payload}", json);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync("/predict/all", content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach AI model endpoint");
            throw new InvalidOperationException("AI model is currently unavailable. Please try again later.", ex);
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        _logger.LogInformation("AI model response (status {StatusCode}): {Response}", (int)response.StatusCode, responseBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("AI model returned error status {StatusCode}: {Response}", (int)response.StatusCode, responseBody);
            throw new InvalidOperationException($"AI model returned error status {(int)response.StatusCode}. Please try again later.");
        }

        // Parse the HuggingFace response
        HuggingFaceRawResponse? rawResponse;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            rawResponse = JsonSerializer.Deserialize<HuggingFaceRawResponse>(responseBody, options);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize AI model response: {Response}", responseBody);
            throw new InvalidOperationException("AI model returned an invalid JSON response.", ex);
        }

        if (rawResponse?.MajorityVote == null)
        {
            _logger.LogError("AI model returned missing majority_vote: {Response}", responseBody);
            throw new InvalidOperationException("AI model returned an incomplete response (missing majority_vote).");
        }

        // Map prediction to YES/NO
        // 1 = YES, 0 = NO
        string predictionClass = rawResponse.MajorityVote.Prediction == 1 ? "YES" : "NO";

        // Extract best confidence from individual model results
        // Requirement: Calculate confidence as the maximum probability found across all models.
        decimal confidence = 0.0m;
        if (rawResponse.Results != null && rawResponse.Results.Count > 0)
        {
            try 
            {
                // Safely extract all numeric values from all probability dictionaries across all models
                var allProbabilities = rawResponse.Results.Values
                    .Where(r => r.Probability != null && r.Probability.Count > 0)
                    .SelectMany(r => r.Probability.Values)
                    .ToList();

                if (allProbabilities.Any())
                {
                    confidence = allProbabilities.Max();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error occurred while extracting probabilities. Falling back to 0.0.");
            }
        }

        _logger.LogInformation("AI prediction successful: Class={Class} (Raw={RawPrediction}), Confidence={Confidence}", 
            predictionClass, rawResponse.MajorityVote.Prediction, confidence);

        return new AiScreeningResponse
        {
            Class = predictionClass,
            Confidence = confidence
        };
    }
}
