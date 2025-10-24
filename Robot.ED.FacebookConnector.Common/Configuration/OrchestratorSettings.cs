namespace Robot.ED.FacebookConnector.Common.Configuration;

public class OrchestratorSettings
{
    public int AllocationIntervalSeconds { get; set; } = 60;
    public int RobotTimeoutMinutes { get; set; } = 5;
    public int MaxRetryAttempts { get; set; } = 3;
    public int DataRetentionDays { get; set; } = 90;
    public string WebhookUrl { get; set; } = "https://webhook.site/991a0259-b787-4231-b13c-5271f9837469";
    public string WebhookBearerToken { get; set; } = "i70uzjNiZ0VhEBQ6m9xrpnX57ws9e97zaQe+V43C9SZyWbRZ6jdJzYkuWd26e5Ag+VcAqV4zgHcaujsUzSebhPuEbMPHs536oxpy6nwvKaAAkK27kFgCwbVMxgkrh9IhNvsr7p8xLK3l75pwAT+ZODGm8ReBc6r0ytEMeRpzBA9swfTAgxUXgsSTJdDZ8QM0noPfMS033DHrRWvYa0FGZ5UeNCOChoDoT7F2n/pn518GhtdmCzHQImlLYGg50U+IQEqQ5Z7MwgGtahJags+aIQ==";
    public int QueueTimeoutCheckIntervalSeconds { get; set; } = 60;
    public int QueueTimeoutMinutes { get; set; } = 5;
}
