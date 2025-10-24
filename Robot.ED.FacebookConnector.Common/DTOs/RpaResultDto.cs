namespace Robot.ED.FacebookConnector.Common.DTOs;

public class RpaResultDto
{
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
    public string TrackId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string MediaId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string? Tag { get; set; }
    public List<string> Messages { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
