using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Service.Watchdog.Configuration;

namespace Robot.ED.FacebookConnector.Service.Watchdog.Services;

public interface IEmailNotificationService
{
    Task SendApplicationStoppedNotificationAsync(string applicationName, CancellationToken cancellationToken = default);
    Task SendApplicationRestartedNotificationAsync(string applicationName, CancellationToken cancellationToken = default);
}

public class EmailNotificationService : IEmailNotificationService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IAmazonSimpleEmailService? _sesClient;

    public EmailNotificationService(
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailNotificationService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;

        if (_emailSettings.NotificationEnabled)
        {
            try
            {
                var region = RegionEndpoint.GetBySystemName(_emailSettings.AwsRegion);
                
                if (!string.IsNullOrEmpty(_emailSettings.AwsAccessKeyId) && 
                    !string.IsNullOrEmpty(_emailSettings.AwsSecretAccessKey))
                {
                    var credentials = new BasicAWSCredentials(
                        _emailSettings.AwsAccessKeyId, 
                        _emailSettings.AwsSecretAccessKey);
                    _sesClient = new AmazonSimpleEmailServiceClient(credentials, region);
                }
                else
                {
                    // Use default credential chain (IAM role, environment variables, etc.)
                    _sesClient = new AmazonSimpleEmailServiceClient(region);
                }
                
                _logger.LogInformation("Email notification service initialized with AWS SES in region {Region}", _emailSettings.AwsRegion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize AWS SES client. Email notifications will be disabled.");
            }
        }
        else
        {
            _logger.LogInformation("Email notifications are disabled");
        }
    }

    public async Task SendApplicationStoppedNotificationAsync(string applicationName, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.NotificationEnabled || _sesClient == null)
        {
            _logger.LogDebug("Email notification skipped for {ApplicationName} - notifications disabled", applicationName);
            return;
        }

        var subject = $"⚠️ Application Stopped: {applicationName}";
        var body = $@"
<html>
<head></head>
<body>
    <h2>Application Monitoring Alert</h2>
    <p><strong>Application:</strong> {applicationName}</p>
    <p><strong>Status:</strong> <span style='color: red;'>STOPPED</span></p>
    <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
    <p>The watchdog service detected that the application has stopped running and will attempt to restart it.</p>
</body>
</html>";

        await SendEmailAsync(subject, body, cancellationToken);
    }

    public async Task SendApplicationRestartedNotificationAsync(string applicationName, CancellationToken cancellationToken = default)
    {
        if (!_emailSettings.NotificationEnabled || _sesClient == null)
        {
            _logger.LogDebug("Email notification skipped for {ApplicationName} - notifications disabled", applicationName);
            return;
        }

        var subject = $"✅ Application Restarted: {applicationName}";
        var body = $@"
<html>
<head></head>
<body>
    <h2>Application Monitoring Alert</h2>
    <p><strong>Application:</strong> {applicationName}</p>
    <p><strong>Status:</strong> <span style='color: green;'>RESTARTED</span></p>
    <p><strong>Time:</strong> {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
    <p>The watchdog service successfully restarted the application.</p>
</body>
</html>";

        await SendEmailAsync(subject, body, cancellationToken);
    }

    private async Task SendEmailAsync(string subject, string htmlBody, CancellationToken cancellationToken)
    {
        if (_sesClient == null || !_emailSettings.Recipients.Any())
        {
            _logger.LogWarning("Cannot send email: SES client is null or no recipients configured");
            return;
        }

        try
        {
            var sendRequest = new SendEmailRequest
            {
                Source = _emailSettings.SenderEmail,
                Destination = new Destination
                {
                    ToAddresses = _emailSettings.Recipients
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content(htmlBody)
                    }
                }
            };

            var response = await _sesClient.SendEmailAsync(sendRequest, cancellationToken);
            _logger.LogInformation("Email sent successfully. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification: {Subject}", subject);
        }
    }
}
