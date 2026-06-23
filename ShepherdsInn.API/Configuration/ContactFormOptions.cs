namespace ShepherdsInn.API.Configuration
{
    public sealed class ContactFormOptions
    {
        public int MaxNameLength { get; set; } = ContactFormLimits.MaxNameLength;
        public int MaxEmailLength { get; set; } = ContactFormLimits.MaxEmailLength;
        public int MaxPhoneLength { get; set; } = ContactFormLimits.MaxPhoneLength;
        public int MaxPreferredContactLength { get; set; } = ContactFormLimits.MaxPreferredContactLength;
        public int MaxSubjectLength { get; set; } = ContactFormLimits.MaxSubjectLength;
        public int MinMessageLength { get; set; } = ContactFormLimits.MinMessageLength;
        public int MaxMessageLength { get; set; } = ContactFormLimits.MaxMessageLength;

        public bool SaveIpAddress { get; set; } = true;
        public bool SaveUserAgent { get; set; } = true;

        public ContactFormRateLimitOptions RateLimit { get; set; } = new();
        public string[] AllowedOrigins { get; set; } = [];
    }

    public sealed class ContactFormRateLimitOptions
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowMinutes { get; set; } = 10;
    }
}
