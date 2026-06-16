using GoingMy.Auth.API.Data;
using GoingMy.Auth.API.Models;
using GoingMy.Auth.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GoingMy.Auth.Tests;

public class UserServiceTest
{
    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WhenDataIsValid()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new ApplicationDbContext(dbContextOptions);

        var userManager = MockUserManager<ApplicationUser>();

        userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        var userService = new UserService(context, userManager.Object);

        var username = "newuser";
        var password = "NewUser@123";
        var email = "newuser@yopmail.com";
        var firstName = "New";
        var lastName = "User";
        var roles = new List<string> { "User" };

        // Act
        var result = await userService.CreateUserAsync(username, password, email, firstName, lastName, roles);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.UserName);
        Assert.Equal(email, result.Email);
        Assert.Equal(firstName, result.FirstName);
        Assert.Equal(lastName, result.LastName);
        Assert.Single(result.Roles);
        Assert.Contains(result.Roles, r => r.ToString() == "User");
    }

    [Theory]
    [InlineData("existinguser", "existinguser@example.com", true, true)]
    [InlineData("existinguser", "newuser@example.com", true, false)]
    [InlineData("newuser", "existinguser@example.com", false, true)]
    public async Task CreateUserAsync_ShouldThrowException_WhenUsernameOrEmailIsTaken(string username, string email, bool isUsernameTaken, bool isEmailTaken)
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new ApplicationDbContext(dbContextOptions);
        var userManager = MockUserManager<ApplicationUser>();

        userManager.Setup(um => um.FindByNameAsync(username))
            .ReturnsAsync(isUsernameTaken 
            ? new ApplicationUser { UserName = username, Email = email, FirstName = "Existing", LastName = "User" } 
            : null);

        userManager.Setup(um => um.FindByEmailAsync(email))
            .ReturnsAsync(isEmailTaken
            ? new ApplicationUser { UserName = username, Email = email, FirstName = "Existing", LastName = "User" }
            : null);

        var userService = new UserService(context, userManager.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => userService.CreateUserAsync(username, "Password@123", email, "New", "User", new List<string> { "User" })
        );

        if (isUsernameTaken)
        {
            Assert.Contains($"User with username '{username}' already exists.", exception.Message);
        }
        else if (isEmailTaken)
        {
            Assert.Contains($"User with email '{email}' already exists.", exception.Message);
        }
    }


    [Fact]
    public async Task ValidateCredentialsAsync_ShouldReturnTrue_WhenCredentialsAreValid()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;

        using var context = new ApplicationDbContext(dbContextOptions);
        var userManager = MockUserManager<ApplicationUser>();

        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "testuser@example.com",
            FirstName = "Test",
            LastName = "User",
        };

        var passwordHasher = new PasswordHasher<ApplicationUser>();
        user.PasswordHash = passwordHasher.HashPassword(user, "Test@123");

        userManager.Setup(um => um.FindByNameAsync("testuser"))
            .ReturnsAsync(user);

        userManager.Setup(um => um.CheckPasswordAsync(user, "Test@123"))
            .ReturnsAsync(true);

        var userService = new UserService(context, userManager.Object);

        // Act
        var result = await userService.ValidateCredentialsAsync("testuser", "Test@123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.UserName);
    }

    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(
            store.Object,
            null, null, null, null, null, null, null, null
        );
    }
}
