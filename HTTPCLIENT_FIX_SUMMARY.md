# HttpClient Disposal Error Fix

## Problem Description

The `RpaAllocationService` class in the `Robot.ED.FacebookConnector.Service.API` project was throwing a `System.ObjectDisposedException` error when executing the `SendProcessRequestToRpaAsync` method at the line:

```csharp
var response = await _httpClient.PostAsync(url, content);
```

Error message: `Cannot access a disposed object. Object name: 'System.Net.Http.HttpClient'`

## Root Cause Analysis

The issue had two contributing factors:

### 1. Duplicate Service Registration

In `Program.cs`, the service was registered twice with conflicting lifetimes:

```csharp
// Old code
builder.Services.AddHttpClient<IRpaAllocationService, RpaAllocationService>();
builder.Services.AddScoped<IRpaAllocationService, RpaAllocationService>();
```

This created a scenario where:
- `AddHttpClient<T>` creates a typed HttpClient registration with transient lifetime
- `AddScoped<T>` creates another instance without the HttpClient dependency
- The DI container would sometimes use the scoped registration, resulting in a null or disposed HttpClient

### 2. HttpClient Lifecycle Issue

The `HttpClient` was being injected directly into the constructor and stored as a field. When used in a fire-and-forget `Task.Run` (line 91-101 in the original code), the HttpClient could be disposed by the DI container before the async task completed, especially when the service scope ended.

```csharp
// Problematic pattern
_ = Task.Run(async () =>
{
    try
    {
        await SendProcessRequestToRpaAsync(availableRobot.Url, pendingQueue);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending process request to RPA {RobotId}", availableRobot.Id);
    }
});
```

## Solution Implemented

### 1. Use IHttpClientFactory Instead of HttpClient

Changed the service to use `IHttpClientFactory` which is the recommended pattern for managing HttpClient instances:

**Before:**
```csharp
private readonly HttpClient _httpClient;

public RpaAllocationService(
    IServiceScopeFactory scopeFactory,
    HttpClient httpClient,
    IOptions<OrchestratorSettings> settings,
    ILogger<RpaAllocationService> logger)
{
    _httpClient = httpClient;
    // ...
}
```

**After:**
```csharp
private readonly IHttpClientFactory _httpClientFactory;

public RpaAllocationService(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<OrchestratorSettings> settings,
    ILogger<RpaAllocationService> logger)
{
    _httpClientFactory = httpClientFactory;
    // ...
}
```

### 2. Create HttpClient Instances On-Demand

Modified `SendProcessRequestToRpaAsync` to create HttpClient instances as needed with proper disposal:

**Before:**
```csharp
private async Task SendProcessRequestToRpaAsync(string rpaUrl, Common.Models.Queue queue)
{
    // ...
    var response = await _httpClient.PostAsync(url, content);
    // ...
}
```

**After:**
```csharp
private async Task SendProcessRequestToRpaAsync(string rpaUrl, Common.Models.Queue queue)
{
    // ...
    using var httpClient = _httpClientFactory.CreateClient();
    var response = await httpClient.PostAsync(url, content);
    // ...
}
```

### 3. Remove Duplicate Service Registration

Cleaned up `Program.cs` to have a single, correct registration:

**Before:**
```csharp
builder.Services.AddHttpClient<IWebhookService, WebhookService>();
builder.Services.AddHttpClient<IRpaAllocationService, RpaAllocationService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IRpaAllocationService, RpaAllocationService>();
```

**After:**
```csharp
builder.Services.AddHttpClient<IWebhookService, WebhookService>();
builder.Services.AddScoped<IRpaAllocationService, RpaAllocationService>();
```

## Benefits of This Approach

1. **No Disposal Issues**: `IHttpClientFactory` manages the lifecycle of HttpClient instances, ensuring they're not disposed prematurely
2. **Better Resource Management**: Each HTTP request gets a fresh HttpClient instance that is properly disposed after use
3. **Socket Exhaustion Prevention**: The factory pools and reuses underlying HttpMessageHandler instances
4. **Thread Safety**: Creating new HttpClient instances per request avoids potential threading issues
5. **Best Practice**: This is the recommended pattern by Microsoft for HttpClient usage in ASP.NET Core applications

## Testing

The solution was verified by:
1. ✅ Building the entire solution successfully (0 errors, 2 unrelated warnings)
2. ✅ Confirming all dependencies resolve correctly
3. ✅ Ensuring no breaking changes to the public API

## Related Documentation

- [IHttpClientFactory with .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)
- [Make HTTP requests using IHttpClientFactory](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests)
- [HttpClient usage guidelines](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
