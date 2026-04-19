using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutiCare.Application.DTOs;
using AutiCare.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace AutiCare.Tests
{
    public class HuggingFaceParsingTests
    {
        [Fact]
        public async Task GetPredictionAsync_CorrectlyParsesProbabilityDictionary()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""majority_vote"": {
                    ""label"": ""NO"",
                    ""prediction"": 0
                },
                ""results"": {
                    ""adaboost"": {
                        ""label"": ""NO"",
                        ""prediction"": 0,
                        ""probability"": {
                            ""0"": 0.82,
                            ""1"": 0.18
                        }
                    },
                    ""xgboost"": {
                        ""label"": ""NO"",
                        ""prediction"": 0,
                        ""probability"": {
                            ""0"": 0.91,
                            ""1"": 0.09
                        }
                    }
                }
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://hf.space/")
            };

            var loggerMock = new Mock<ILogger<HuggingFaceAiClientProvider>>();
            var provider = new HuggingFaceAiClientProvider(httpClient, loggerMock.Object);

            var payload = new AiScreeningPayload { A1 = 1, Age = 24 };

            // Act
            var result = await provider.GetPredictionAsync(payload);

            // Assert
            Assert.Equal("NO", result.Class);
            Assert.Equal(0.91m, result.Confidence); // Max prob across all models
        }

        [Fact]
        public async Task GetPredictionAsync_HandlesMissingResultsGracefully()
        {
            // Arrange
            var jsonResponse = @"
            {
                ""majority_vote"": {
                    ""label"": ""YES"",
                    ""prediction"": 1
                },
                ""results"": {}
            }";

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
               });

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://hf.space/")
            };

            var loggerMock = new Mock<ILogger<HuggingFaceAiClientProvider>>();
            var provider = new HuggingFaceAiClientProvider(httpClient, loggerMock.Object);

            // Act
            var result = await provider.GetPredictionAsync(new AiScreeningPayload());

            // Assert
            Assert.Equal("YES", result.Class);
            Assert.Equal(0.0m, result.Confidence);
        }
    }
}
