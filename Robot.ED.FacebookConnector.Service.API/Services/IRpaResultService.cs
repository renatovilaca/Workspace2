using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public interface IRpaResultService
{
    Task<(bool Success, string? ErrorMessage)> ProcessResultAsync(RpaResultDto result);
}
