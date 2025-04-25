using Microsoft.AspNetCore.Identity;
using SocialNetworkMicroservices.Identity.Entities;

namespace SocialNetworkMicroservices.Identity.Services;

public class OptOutEmailService : IEmailSender<User>
{
    public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
    {
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
    {
        return Task.CompletedTask;
    }
}
