using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Dashboard.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.ManualProcess;

[Authorize(Roles = "Admin")]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDashboardService _dashboardService;
    private readonly IConfiguration _configuration;

    [BindProperty]
    public AllocateRequestDto ProcessRequest { get; set; } = new();

    public string? StatusMessage { get; set; }
    public string? ErrorMessage { get; set; }
    public bool ShowStatus { get; set; }
    public int? QueueId { get; set; }
    public string? UniqueId { get; set; }

    public IndexModel(
        ILogger<IndexModel> logger,
        IHttpClientFactory httpClientFactory,
        IDashboardService dashboardService,
        IConfiguration configuration)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _dashboardService = dashboardService;
        _configuration = configuration;
    }

    public void OnGet()
    {
        // Initialize with default values
        ProcessRequest = new AllocateRequestDto
        {
            TrackId = Guid.NewGuid().ToString(),
            Channel = "manual",
            Customer = "Dashboard User",
            Tags = new List<string>()
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Por favor, preencha todos os campos obrigatórios.";
            return Page();
        }

        try
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:5001";
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(apiUrl);

            var json = JsonSerializer.Serialize(ProcessRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending allocation request to {Url}/api/rpa/allocate for TrackId: {TrackId}", 
                apiUrl, ProcessRequest.TrackId);

            var response = await httpClient.PostAsync("/api/rpa/allocate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AllocateResponse>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                QueueId = result?.Id;
                UniqueId = result?.UniqueId;
                ShowStatus = true;
                StatusMessage = $"Processo iniciado com sucesso! Queue ID: {QueueId}, Unique ID: {UniqueId}";

                // Log user action
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown";
                var userName = User.Identity?.Name ?? "unknown";
                await _dashboardService.LogUserActionAsync(userId, userName, "ManualProcessStart", 
                    $"TrackId: {ProcessRequest.TrackId}, QueueId: {QueueId}");

                _logger.LogInformation("Process started successfully. QueueId: {QueueId}, TrackId: {TrackId}", 
                    QueueId, ProcessRequest.TrackId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"Erro ao iniciar o processo: {response.StatusCode}. Detalhes: {errorContent}";
                _logger.LogError("Failed to start process. Status: {Status}, Error: {Error}", 
                    response.StatusCode, errorContent);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Erro ao conectar com o serviço API: {ex.Message}";
            _logger.LogError(ex, "Error starting manual process");
        }

        return Page();
    }

    private class AllocateResponse
    {
        public int Id { get; set; }
        public string UniqueId { get; set; } = string.Empty;
    }
}
