namespace ShepherdsInn.API.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PreferredContact { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime Submitted { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; }

        public string UserAgent { get; set; } = string.Empty;

        public string IpAddress { get; set; } = string.Empty;
    }
}
