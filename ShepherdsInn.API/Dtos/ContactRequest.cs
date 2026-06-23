namespace ShepherdsInn.API.Dtos
{
    public class ContactRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PreferredContact { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        // Hidden anti-spam field. Real visitors should never fill this in.
        public string Website { get; set; } = string.Empty;
    }
}
