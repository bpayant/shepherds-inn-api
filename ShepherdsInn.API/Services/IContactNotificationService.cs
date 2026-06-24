using ShepherdsInn.API.Models;

namespace ShepherdsInn.API.Services;

public interface IContactNotificationService
{
    Task SendNewContactMessageNotificationAsync(
        ContactMessage contactMessage,
        CancellationToken cancellationToken);
}