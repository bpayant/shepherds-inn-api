using ShepherdsInn.API.Models;

namespace ShepherdsInn.API.Services;

public sealed class NullContactNotificationService : IContactNotificationService
{
    public NullContactNotificationService() { }

    public Task SendNewContactMessageNotificationAsync(
        ContactMessage contactMessage,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}