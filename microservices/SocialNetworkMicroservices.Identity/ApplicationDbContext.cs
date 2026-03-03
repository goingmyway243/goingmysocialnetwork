using Microsoft.EntityFrameworkCore;

namespace SocialNetworkMicroservices.Identity;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
