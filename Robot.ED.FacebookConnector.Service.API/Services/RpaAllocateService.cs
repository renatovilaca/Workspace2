using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Common.Models;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public class RpaAllocateService : IRpaAllocateService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RpaAllocateService> _logger;

    public RpaAllocateService(
        AppDbContext dbContext,
        ILogger<RpaAllocateService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(int QueueId, Guid UniqueId)> AllocateAsync(AllocateRequestDto request)
    {
        _logger.LogInformation("Processing allocation request for TrackId: {TrackId}", request.TrackId);

        var queue = new Queue
        {
            AiConfig = request.AiConfig,
            TrackId = request.TrackId,
            BridgeKey = request.BridgeKey,
            OriginType = request.OriginType,
            MediaId = request.MediaId,
            Customer = request.Customer,
            Channel = request.Channel,
            Phrase = request.Phrase,
            CreatedAt = DateTime.UtcNow,
            IsProcessed = false
        };

        // Add tags
        foreach (var tag in request.Tags ?? new List<string>())
        {
            queue.QueueTags.Add(new QueueTag { Tag = tag });
        }

        // Add data items
        foreach (var dataItem in request.Data ?? new List<DataItemDto>())
        {
            queue.QueueData.Add(new QueueData 
            { 
                Header = dataItem.Header, 
                Value = dataItem.Value 
            });
        }

        _dbContext.Queues.Add(queue);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Queue item created with Id: {Id}, UniqueId: {UniqueId}", queue.Id, queue.UniqueId);

        return (queue.Id, queue.UniqueId);
    }
}
