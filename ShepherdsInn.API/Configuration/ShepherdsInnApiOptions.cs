namespace ShepherdsInn.API.Configuration;

public sealed class ShepherdsInnApiOptions
{
    public ContactFormOptions ContactForm { get; init; } = new();
    public DatabaseOptions Database { get; init; } = new();
    public EmailNotificationOptions EmailNotifications { get; init; } = new();

    public static ShepherdsInnApiOptions Create(IHostEnvironment environment)
    {
        var options = new ShepherdsInnApiOptions();

        if (environment.IsDevelopment())
        {
            options.ContactForm.RateLimit.PermitLimit = 100;
            options.ContactForm.RateLimit.WindowMinutes = 10;
            options.ContactForm.AllowedOrigins =
            [
                "http://localhost:4200",
                "https://localhost:4200"
            ];
        }
        else
        {
            options.ContactForm.RateLimit.PermitLimit = 10;
            options.ContactForm.RateLimit.WindowMinutes = 10;
            options.ContactForm.AllowedOrigins =
            [
                "https://shepherdsinnwells.com",
                "https://www.shepherdsinnwells.com"
            ];
        }

        return options;
    }
}