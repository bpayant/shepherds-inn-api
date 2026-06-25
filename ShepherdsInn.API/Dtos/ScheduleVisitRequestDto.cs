namespace ShepherdsInn.API.Dtos;

public sealed record ScheduleVisitRequestDto(
    string Name,
    string Phone,
    string? Email,
    string InquiryFor,
    string TourReadiness,
    string Timeline,
    string? Website
);