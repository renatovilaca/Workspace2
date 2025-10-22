namespace Robot.ED.FacebookConnector.Common.DTOs;

public class ProcessRequestDto
{
    public int QueueId { get; set; }
    public string AiConfig { get; set; } = string.Empty;
    public string TrackId { get; set; } = string.Empty;
    public string BridgeKey { get; set; } = string.Empty;
    public string OriginType { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string Phrase { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<DataItemDto> Data { get; set; } = new();
}
