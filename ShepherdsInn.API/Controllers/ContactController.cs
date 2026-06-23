#region Using Statements

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ShepherdsInn.API.Configuration;
using ShepherdsInn.API.Data;
using ShepherdsInn.API.Dtos;
using ShepherdsInn.API.Models;

#endregion

namespace ShepherdsInn.API.Controllers
{
    [ApiController]
    [Route("api/contact")]
    public class ContactController : ControllerBase
    {
        private readonly ShepherdsInnDbContext _db;
        private readonly ContactFormOptions _contactFormOptions;
        private readonly ILogger<ContactController> _logger;

        public ContactController(ShepherdsInnDbContext db, ContactFormOptions contactFormOptions, ILogger<ContactController> logger)
        {
            _db = db;
            _contactFormOptions = contactFormOptions;
            _logger = logger;
        }

        [HttpPost]
        [EnableRateLimiting(ApiPolicyNames.ContactFormRateLimit)]
        public async Task<ActionResult<ContactResponse>> Submit(
            [FromBody] ContactRequest request,
            CancellationToken cancellationToken)
        {
            request ??= new ContactRequest();

            // Honeypot: bots may fill hidden fields. Return a normal response without saving.
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                return Ok(new ContactResponse
                {
                    Success = true,
                    Message = "Thank you. Your message has been received."
                });
            }

            var validationErrors = ValidateRequest(request, _contactFormOptions);

            if (validationErrors.Count > 0)
            {
                return ValidationProblem(new ValidationProblemDetails(validationErrors)
                {
                    Title = "Please check the form and try again.",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var contactMessage = new ContactMessage
            {
                Name = Clean(request.Name),
                Phone = Clean(request.Phone),
                Email = Clean(request.Email),
                PreferredContact = Clean(request.PreferredContact),
                Subject = string.IsNullOrWhiteSpace(request.Subject) ? "General Inquiry" : Clean(request.Subject),
                Message = CleanMultiline(request.Message),
                Submitted = DateTime.UtcNow,
                IsRead = false,
                UserAgent = _contactFormOptions.SaveUserAgent
                    ? Truncate(Request.Headers.UserAgent.ToString(), ContactFormLimits.MaxUserAgentLength) : string.Empty,
                IpAddress = _contactFormOptions.SaveIpAddress
                    ? Truncate(HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty, ContactFormLimits.MaxIpAddressLength) : string.Empty
            };

            _db.ContactMessages.Add(contactMessage);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Saved Shepherds Inn contact message {ContactMessageId} from {Name}.",
                contactMessage.Id,
                contactMessage.Name);

            return Ok(new ContactResponse
            {
                Success = true,
                Message = "Thank you. Your message has been received."
            });
        }

        private static Dictionary<string, string[]> ValidateRequest(ContactRequest request, ContactFormOptions contactFormOptions)
        {
            var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            AddRequired(errors, nameof(request.Name), request.Name, "Name is required.");
            AddMaxLength(errors, nameof(request.Name), request.Name, contactFormOptions.MaxNameLength, $"Name must be {contactFormOptions.MaxNameLength} characters or fewer.");

            AddMaxLength(errors, nameof(request.Phone), request.Phone, contactFormOptions.MaxPhoneLength, $"Phone must be {contactFormOptions.MaxPhoneLength} characters or fewer.");
            AddMaxLength(errors, nameof(request.Email), request.Email, contactFormOptions.MaxEmailLength, $"Email must be {contactFormOptions.MaxEmailLength} characters or fewer.");
            AddMaxLength(errors, nameof(request.PreferredContact), request.PreferredContact, contactFormOptions.MaxPreferredContactLength, $"Preferred contact must be {contactFormOptions.MaxPreferredContactLength} characters or fewer.");
            AddMaxLength(errors, nameof(request.Subject), request.Subject, contactFormOptions.MaxSubjectLength, $"Subject must be {contactFormOptions.MaxSubjectLength} characters or fewer.");
            AddMaxLength(errors, nameof(request.Message), request.Message, contactFormOptions.MaxMessageLength, $"Message must be {contactFormOptions.MaxMessageLength} characters or fewer.");

            if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Email))
            {
                AddError(errors, "Contact", "Please provide a phone number or email address.");
            }

            if (!string.IsNullOrWhiteSpace(request.Phone))
            {
                var digits = new string(request.Phone.Where(char.IsDigit).ToArray());

                if (digits.Length is not (10 or 11))
                {
                    AddError(errors, nameof(request.Phone), "Please enter a valid phone number.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var validator = new EmailAddressAttribute();

                if (!validator.IsValid(request.Email))
                {
                    AddError(errors, nameof(request.Email), "Please enter a valid email address.");
                }
            }

            if (!string.IsNullOrWhiteSpace(request.PreferredContact))
            {
                var preferred = request.PreferredContact.Trim();

                if (!string.Equals(preferred, "Call", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(preferred, "Email", StringComparison.OrdinalIgnoreCase))
                {
                    AddError(errors, nameof(request.PreferredContact), "Preferred contact must be Call or Email.");
                }

                if (string.Equals(preferred, "Call", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(request.Phone))
                {
                    AddError(errors, nameof(request.Phone), "Please enter a phone number if you prefer a call.");
                }

                if (string.Equals(preferred, "Email", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    AddError(errors, nameof(request.Email), "Please enter an email address if you prefer email.");
                }
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                AddError(errors, nameof(request.Message), "Message is required.");
            }
            else if (request.Message.Trim().Length < contactFormOptions.MinMessageLength)
            {
                AddError(errors, nameof(request.Message), $"Message must be at least {contactFormOptions.MinMessageLength} characters.");
            }

            return errors.ToDictionary(
                pair => pair.Key,
                pair => pair.Value.ToArray(),
                StringComparer.OrdinalIgnoreCase);
        }

        private static void AddRequired(
            Dictionary<string, List<string>> errors,
            string field,
            string? value,
            string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(errors, field, message);
            }
        }

        private static void AddMaxLength(
            Dictionary<string, List<string>> errors,
            string field,
            string? value,
            int maxLength,
            string message)
        {
            if (!string.IsNullOrWhiteSpace(value) && value.Length > maxLength)
            {
                AddError(errors, field, message);
            }
        }

        private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
        {
            if (!errors.TryGetValue(field, out var fieldErrors))
            {
                fieldErrors = new List<string>();
                errors[field] = fieldErrors;
            }

            fieldErrors.Add(message);
        }

        private static string Clean(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static string CleanMultiline(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Replace("\r\n", "\n").Trim();
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= maxLength
                ? value
                : value[..maxLength];
        }
    }
}
