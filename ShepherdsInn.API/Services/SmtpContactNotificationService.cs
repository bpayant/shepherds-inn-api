#region Using Statements

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using ShepherdsInn.API.Configuration;
using ShepherdsInn.API.Models;

#endregion

namespace ShepherdsInn.API.Services;

public sealed class SmtpContactNotificationService : IContactNotificationService
{
    private readonly IConfiguration _configuration;
    private readonly EmailNotificationOptions _options;
    private readonly ILogger<SmtpContactNotificationService> _logger;

    public SmtpContactNotificationService(
        IConfiguration configuration,
        EmailNotificationOptions options,
        ILogger<SmtpContactNotificationService> logger)
    {
        _configuration = configuration;
        _options = options;
        _logger = logger;
    }

    public async Task SendNewContactMessageNotificationAsync(
        ContactMessage contactMessage,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Email notifications are disabled. ContactMessageId: {ContactMessageId}",
                contactMessage.Id);

            return;
        }

        var host = _configuration["Email:SmtpHost"];
        var portText = _configuration["Email:SmtpPort"];
        var username = _configuration["Email:SmtpUsername"];
        var password = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"];
        var toEmail = _configuration["Email:ToEmail"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(portText) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromEmail) ||
            string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning(
                "Email notification skipped because SMTP settings are incomplete. ContactMessageId: {ContactMessageId}",
                contactMessage.Id);

            return;
        }

        if (!int.TryParse(portText, out var port))
        {
            _logger.LogWarning(
                "Email notification skipped because SMTP port is invalid. ContactMessageId: {ContactMessageId}",
                contactMessage.Id);

            return;
        }

        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(_options.FromName, fromEmail));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = $"{_options.SubjectPrefix} #{contactMessage.Id}";

        if (!string.IsNullOrWhiteSpace(contactMessage.Email))
        {
            email.ReplyTo.Add(MailboxAddress.Parse(contactMessage.Email));
        }

        email.Body = new TextPart("plain")
        {
            Text = $"""
            A new message was submitted through the Shepherds Inn website.

            Contact Message ID:
            {contactMessage.Id}

            Submitted:
            {contactMessage.Submitted:u}

            Name:
            {contactMessage.Name}

            Phone:
            {contactMessage.Phone}

            Email:
            {contactMessage.Email}

            Preferred Contact:
            {contactMessage.PreferredContact}

            Subject:
            {contactMessage.Subject}

            Message:
            {contactMessage.Message}
            """
        };

        using var smtpClient = new SmtpClient();
        smtpClient.Timeout = _options.TimeoutMilliseconds;

        await smtpClient.ConnectAsync(
            host,
            port,
            SecureSocketOptions.StartTls,
            cancellationToken);

        await smtpClient.AuthenticateAsync(
            username,
            password,
            cancellationToken);

        await smtpClient.SendAsync(email, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation(
            "Email notification sent. ContactMessageId: {ContactMessageId}",
            contactMessage.Id);
    }
}