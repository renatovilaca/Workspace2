using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Robot.ED.FacebookConnector.Common.DTOs;
using System.Text.Json;

namespace Robot.ED.FacebookConnector.Service.RPA.UI.Services;

public interface IRpaApiService
{
    Task StartAsync();
    Task StopAsync();
    bool IsRunning { get; }
}

public class RpaApiService : IRpaApiService
{
    private readonly ILogger<RpaApiService> _logger;
    private readonly IRpaProcessingService _processingService;
    private readonly RpaStateService _stateService;
    private IHost? _webHost;

    public bool IsRunning { get; private set; }

    public RpaApiService(
        ILogger<RpaApiService> logger,
        IRpaProcessingService processingService,
        RpaStateService stateService)
    {
        _logger = logger;
        _processingService = processingService;
        _stateService = stateService;
    }

    public async Task StartAsync()
    {
        if (IsRunning)
        {
            _logger.LogWarning("API server is already running");
            return;
        }

        try
        {
            _logger.LogInformation("Starting RPA API server...");

            _webHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_processingService);
                        services.AddSingleton(_stateService);
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapPost("/api/rpa/process", async context =>
                            {
                                try
                                {
                                    var request = await JsonSerializer.DeserializeAsync<ProcessRequestDto>(
                                        context.Request.Body);

                                    if (request == null)
                                    {
                                        context.Response.StatusCode = 400;
                                        await context.Response.WriteAsync("Invalid request");
                                        return;
                                    }

                                    var logger = context.RequestServices.GetRequiredService<ILogger<RpaApiService>>();
                                    var service = context.RequestServices.GetRequiredService<IRpaProcessingService>();
                                    var state = context.RequestServices.GetRequiredService<RpaStateService>();

                                    logger.LogInformation("Received process request for QueueId: {QueueId}", request.QueueId);

                                    // Only process if not paused
                                    if (state.State != RpaProcessState.Paused)
                                    {
                                        // Process asynchronously
                                        _ = Task.Run(async () =>
                                        {
                                            try
                                            {
                                                state.CurrentCycleStartTime = DateTime.Now;
                                                state.State = RpaProcessState.Running;
                                                
                                                await service.ProcessAsync(request);
                                                
                                                state.LastExecutionSuccess = true;
                                                state.LastExecutionMessage = "Execution completed successfully";
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.LogError(ex, "Error processing queue {QueueId}", request.QueueId);
                                                state.LastExecutionSuccess = false;
                                                state.LastExecutionMessage = $"Error: {ex.Message}";
                                            }
                                            finally
                                            {
                                                state.CurrentCycleStartTime = null;
                                                if (state.State == RpaProcessState.Running)
                                                {
                                                    state.State = RpaProcessState.Stopped;
                                                }
                                            }
                                        });

                                        context.Response.StatusCode = 202;
                                        await context.Response.WriteAsJsonAsync(new { message = "Request accepted", queueId = request.QueueId });
                                    }
                                    else
                                    {
                                        logger.LogWarning("Request rejected - RPA is paused");
                                        context.Response.StatusCode = 503;
                                        await context.Response.WriteAsJsonAsync(new { message = "RPA is paused" });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    var logger = context.RequestServices.GetRequiredService<ILogger<RpaApiService>>();
                                    logger.LogError(ex, "Error handling process request");
                                    context.Response.StatusCode = 500;
                                    await context.Response.WriteAsync("Internal server error");
                                }
                            });

                            endpoints.MapGet("/api/health", async context =>
                            {
                                await context.Response.WriteAsJsonAsync(new { status = "healthy" });
                            });
                        });
                    });

                    webBuilder.UseUrls("http://localhost:8080", "https://localhost:8081");
                })
                .Build();

            await _webHost.StartAsync();
            IsRunning = true;
            _stateService.State = RpaProcessState.Stopped;

            _logger.LogInformation("RPA API server started successfully on ports 8080/8081");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting RPA API server");
            throw;
        }
    }

    public async Task StopAsync()
    {
        if (!IsRunning || _webHost == null)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Stopping RPA API server...");
            await _webHost.StopAsync();
            _webHost.Dispose();
            _webHost = null;
            IsRunning = false;
            _stateService.State = RpaProcessState.Stopped;
            _logger.LogInformation("RPA API server stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping RPA API server");
            throw;
        }
    }
}
