namespace ShepherdsInn.API.Configuration
{
    public sealed class DatabaseOptions
    {
        public string RelativePath { get; set; } = Path.Combine("..", "db", "shepherdsinn.db");
    }
}
