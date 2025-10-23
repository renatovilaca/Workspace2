using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.RPA.Services;

public interface IRpaProcessingService
{
    Task ProcessAsync(ProcessRequestDto request);
}
