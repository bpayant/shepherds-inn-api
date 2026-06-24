namespace ShepherdsInn.API.Configuration;

public sealed class EmailNotificationOptions
{
    public bool Enabled { get; set; } = true;
    public string FromName { get; set; } = "Shepherds Inn Website";
    public string SubjectPrefix { get; set; } = "New Shepherds Inn website inquiry";
    public int TimeoutMilliseconds { get; set; } = 10000; // 10 seconds
}