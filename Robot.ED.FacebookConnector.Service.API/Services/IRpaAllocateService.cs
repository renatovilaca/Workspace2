using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public interface IRpaAllocateService
{
    Task<(int QueueId, Guid UniqueId)> AllocateAsync(AllocateRequestDto request);
}
