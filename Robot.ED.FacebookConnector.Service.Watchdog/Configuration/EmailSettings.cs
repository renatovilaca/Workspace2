namespace Robot.ED.FacebookConnector.Service.Watchdog.Configuration;

public class EmailSettings
{
    public bool NotificationEnabled { get; set; } = true;
    public string SenderEmail { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
    public string AwsRegion { get; set; } = "us-east-1";
    public string? AwsAccessKeyId { get; set; }
    public string? AwsSecretAccessKey { get; set; }
}
