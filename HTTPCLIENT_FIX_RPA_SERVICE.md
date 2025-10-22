# HttpClient Disposal Error Fix - RPA Service

## Problem Description

The `RpaProcessingService` class in the `Robot.ED.FacebookConnector.Service.RPA` project was throwing a `System.ObjectDisposedException` error when executing the `SendResultToOrchestratorAsync` method at the line:

```csharp
var response = await _httpClient.PostAsync(url, content);
```

Error message: `Cannot access a disposed object. Object name: 'System.Net.Http.HttpClient'`

## Root Cause Analysis

The issue had two contributing factors:

### 1. Duplicate Service Registration

In `Program.cs`, the service was registered twice with conflicting lifetimes:

```csharp
// Old code - PROBLEMATIC
builder.Services.AddHttpClient<IRpaProcessingService, RpaProcessingService>();
builder.Services.AddScoped<IRpaProcessingService, RpaProcessingService>();
```

This created a scenario where:
- `AddHttpClient<T>` creates a typed HttpClient registration with transient lifetime
- `AddScoped<T>` creates another instance without the HttpClient dependency
- The DI container could sometimes use the scoped registration, resulting in a null or disposed HttpClient

### 2. HttpClient Lifecycle Issue

The `HttpClient` was being injected directly into the constructor and stored as a field. When the service scope ended, especially in background operations or long-running processes, the HttpClient could be disposed by the DI container before the async task completed, leading to the `ObjectDisposedException`.

```csharp
// Problematic pattern
private readonly HttpClient _httpClient;

public RpaProcessingService(HttpClient httpClient, ...)
{
    _httpClient = httpClient;
}

private async Task SendResultToOrchestratorAsync(RpaResultDto result)
{
    // This could fail if _httpClient was disposed
    var response = await _httpClient.PostAsync(url, content);
}
```

## Solution Implemented

### 1. Use IHttpClientFactory Instead of HttpClient

Changed the service to use `IHttpClientFactory` which is the recommended pattern for managing HttpClient instances:

**Before:**
```csharp
private readonly HttpClient _httpClient;

public RpaProcessingService(
    HttpClient httpClient,
    IOptions<RpaSettings> settings,
    ILogger<RpaProcessingService> logger)
{
    _httpClient = httpClient;
    _settings = settings.Value;
    _logger = logger;
}
```

**After:**
```csharp
private readonly IHttpClientFactory _httpClientFactory;

public RpaProcessingService(
    IHttpClientFactory httpClientFactory,
    IOptions<RpaSettings> settings,
    ILogger<RpaProcessingService> logger)
{
    _httpClientFactory = httpClientFactory;
    _settings = settings.Value;
    _logger = logger;
}
```

### 2. Create HttpClient Instances On-Demand

Modified `SendResultToOrchestratorAsync` to create HttpClient instances as needed with proper disposal:

**Before:**
```csharp
private async Task SendResultToOrchestratorAsync(RpaResultDto result)
{
    var json = JsonSerializer.Serialize(result);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    _httpClient.DefaultRequestHeaders.Clear();
    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OrchestratorToken}");

    var url = $"{_settings.OrchestratorUrl.TrimEnd('/')}/api/rpa/result";
    var response = await _httpClient.PostAsync(url, content);
    // ...
}
```

**After:**
```csharp
private async Task SendResultToOrchestratorAsync(RpaResultDto result)
{
    var json = JsonSerializer.Serialize(result);
    var content = new StringContent(json, Encoding.UTF8, "application/json");

    using var httpClient = _httpClientFactory.CreateClient();
    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.OrchestratorToken}");

    var url = $"{_settings.OrchestratorUrl.TrimEnd('/')}/api/rpa/result";
    var response = await httpClient.PostAsync(url, content);
    // ...
}
```

Key changes:
- Create a new HttpClient instance using `_httpClientFactory.CreateClient()` for each request
- Use `using` statement to ensure proper disposal after use
- No need to call `DefaultRequestHeaders.Clear()` since it's a fresh instance
- The factory manages the underlying HttpMessageHandler lifecycle correctly

### 3. Fix Service Registration

Fixed the duplicate service registration in `Program.cs`:

**Before:**
```csharp
builder.Services.AddHttpClient<IRpaProcessingService, RpaProcessingService>();
builder.Services.AddScoped<IRpaProcessingService, RpaProcessingService>();
```

**After:**
```csharp
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRpaProcessingService, RpaProcessingService>();
```

Key changes:
- Removed the typed HttpClient registration `AddHttpClient<T, T>()` 
- Use the generic `AddHttpClient()` to register the factory
- Keep only the scoped service registration for `IRpaProcessingService`

## Benefits of This Solution

1. **Thread Safety**: Each request gets its own HttpClient instance, avoiding concurrency issues
2. **No Disposal Issues**: The factory manages the lifecycle correctly, preventing `ObjectDisposedException`
3. **Connection Pooling**: HttpClientFactory reuses underlying connections efficiently
4. **No DNS Issues**: The factory handles DNS changes properly (avoiding the "stale DNS" problem)
5. **Cleaner Code**: Single service registration, no confusion about DI lifetimes
6. **Proper Resource Management**: `using` statement ensures HttpClient is disposed correctly

## Testing

The solution was verified by:
1. Building the entire solution successfully
2. Ensuring no compilation errors or warnings related to the changes
3. Following the same pattern that was successfully applied to `RpaAllocationService` in the API project

## References

- [Microsoft Documentation: IHttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests)
- [You're using HttpClient wrong](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)
- [HttpClient Lifecycle Management Best Practices](https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines)
