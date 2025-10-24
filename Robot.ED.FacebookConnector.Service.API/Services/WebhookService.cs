using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public class WebhookService : IWebhookService
{
    private readonly HttpClient _httpClient;
    private readonly OrchestratorSettings _settings;
    private readonly ILogger<WebhookService> _logger;

    public WebhookService(
        HttpClient httpClient, 
        IOptions<OrchestratorSettings> settings,
        ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ForwardResultAsync(RpaResultDto result)
    {
        try
        {
            var json = JsonSerializer.Serialize(result);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.WebhookBearerToken}");

            var response = await _httpClient.PostAsync(_settings.WebhookUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Result forwarded to webhook successfully for TrackId: {TrackId}", result.TrackId);
            }
            else
            {
                _logger.LogWarning("Failed to forward result to webhook. Status: {StatusCode}, TrackId: {TrackId}", 
                    response.StatusCode, result.TrackId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forwarding result to webhook for TrackId: {TrackId}", result.TrackId);
        }
    }
}
