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

    private const string DefaultApiUrl = "https://localhost:5001";

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

        // Validate AiConfig is valid JSON if provided
        if (!string.IsNullOrWhiteSpace(ProcessRequest.AiConfig))
        {
            try
            {
                JsonDocument.Parse(ProcessRequest.AiConfig);
            }
            catch (JsonException)
            {
                ErrorMessage = "O campo 'Configuração AI' deve conter JSON válido.";
                return Page();
            }
        }

        try
        {
            var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? DefaultApiUrl;
            
            // Validate API URL
            if (!Uri.TryCreate(apiUrl, UriKind.Absolute, out var validatedUri) || 
                (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
            {
                ErrorMessage = "Configuração de URL da API inválida.";
                _logger.LogError("Invalid API URL configuration: {Url}", apiUrl);
                return Page();
            }

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = validatedUri;

            var json = JsonSerializer.Serialize(ProcessRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending allocation request to {Url}/api/rpa/allocate for TrackId: {TrackId}", 
                apiUrl, ProcessRequest.TrackId);

            var response = await httpClient.PostAsync("/api/rpa/allocate", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AllocateResponseDto>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                {
                    ErrorMessage = "Erro ao processar resposta da API.";
                    _logger.LogError("Failed to deserialize API response");
                    return Page();
                }

                QueueId = result.Id;
                UniqueId = result.UniqueId;
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
}
