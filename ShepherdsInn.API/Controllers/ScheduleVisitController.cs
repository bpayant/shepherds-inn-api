#region Using Statements

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using ShepherdsInn.API.Configuration;
using ShepherdsInn.API.Data;
using ShepherdsInn.API.Dtos;
using ShepherdsInn.API.Models;

#endregion

namespace ShepherdsInn.API.Controllers;

[ApiController]
[Route("api/schedule-visit")]
[EnableRateLimiting(ApiPolicyNames.ContactFormRateLimit)]
public sealed class ScheduleVisitController : ControllerBase
{
    private static readonly HashSet<string> ValidInquiryForValues =
    [
        "Myself",
        "A loved one"
    ];

    private static readonly HashSet<string> ValidTourReadinessValues =
    [
        "Yes, please call me to schedule a day/time",
        "Not yet, I'd just like to get more information"
    ];

    private static readonly HashSet<string> ValidTimelineValues =
    [
        "Immediately",
        "1-2 Months",
        "3+ Months",
        "Just Researching"
    ];

    private readonly ShepherdsInnDbContext _dbContext;
    private readonly ILogger<ScheduleVisitController> _logger;

    public ScheduleVisitController(
        ShepherdsInnDbContext dbContext,
        ILogger<ScheduleVisitController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse>> SubmitScheduleVisitRequest(
        ScheduleVisitRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Schedule visit request started.");

        if (!string.IsNullOrWhiteSpace(request.Website))
        {
            _logger.LogWarning("Schedule visit honeypot field was populated. Submission ignored.");

            return Ok(new ApiResponse(true, "Thank you. Your request has been received."));
        }

        var errors = ValidateRequest(request);

        if (errors.Count > 0)
        {
            _logger.LogWarning(
                "Schedule visit validation failed. ErrorFields: {ErrorFields}",
                string.Join(", ", errors.Keys));

            return BadRequest(new
            {
                title = "Please check the form and try again.",
                status = StatusCodes.Status400BadRequest,
                errors
            });
        }

        var scheduleVisitRequest = new ScheduleVisitRequest
        {
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Email = request.Email?.Trim() ?? string.Empty,
            InquiryFor = request.InquiryFor.Trim(),
            TourReadiness = request.TourReadiness.Trim(),
            Timeline = request.Timeline.Trim(),
            Submitted = DateTime.UtcNow,
            IsRead = false,
            UserAgent = Request.Headers.UserAgent.ToString(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty
        };

        try
        {
            _dbContext.ScheduleVisitRequests.Add(scheduleVisitRequest);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Schedule visit request saved. ScheduleVisitRequestId: {ScheduleVisitRequestId}",
                scheduleVisitRequest.Id);

            return Ok(new ApiResponse(true, "Thank you. Your request has been received."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Schedule visit request failed while saving.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse(false, "Sorry, your request could not be submitted. Please call us at (507) 553-6271 instead."));
        }
    }

    private static Dictionary<string, string[]> ValidateRequest(
        ScheduleVisitRequestDto request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors[nameof(request.Name)] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            errors[nameof(request.Phone)] = ["Phone number is required."];
        }
        else
        {
            var digits = new string(request.Phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 11 && digits.StartsWith('1'))
            {
                digits = digits[1..];
            }

            if (digits.Length != 10)
            {
                errors[nameof(request.Phone)] = ["Please enter a valid 10-digit phone number."];
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            try
            {
                _ = new System.Net.Mail.MailAddress(request.Email);
            }
            catch
            {
                errors[nameof(request.Email)] = ["Please enter a valid email address."];
            }
        }

        if (string.IsNullOrWhiteSpace(request.InquiryFor) ||
            !ValidInquiryForValues.Contains(request.InquiryFor))
        {
            errors[nameof(request.InquiryFor)] = ["Please select who you are inquiring for."];
        }

        if (string.IsNullOrWhiteSpace(request.TourReadiness) ||
            !ValidTourReadinessValues.Contains(request.TourReadiness))
        {
            errors[nameof(request.TourReadiness)] = ["Please select whether you are ready to tour."];
        }

        if (string.IsNullOrWhiteSpace(request.Timeline) ||
            !ValidTimelineValues.Contains(request.Timeline))
        {
            errors[nameof(request.Timeline)] = ["Please select your ideal timeline."];
        }

        return errors;
    }
}