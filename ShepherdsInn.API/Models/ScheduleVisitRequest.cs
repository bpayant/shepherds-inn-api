namespace ShepherdsInn.API.Models;

public sealed class ScheduleVisitRequest
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public string InquiryFor { get; set; } = string.Empty;
    public string TourReadiness { get; set; } = string.Empty;
    public string Timeline { get; set; } = string.Empty;

    public DateTime Submitted { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }

    public string UserAgent { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}