using GoingMy.Auth.API.Controllers;
using GoingMy.Auth.API.Dtos;
using GoingMy.Auth.API.Models;
using GoingMy.Auth.API.Services;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace GoingMy.Auth.Tests;

public class UserControllerTest
{
    private Mock<IUserService> _userServiceMock;
    private Mock<IPublishEndpoint> _publishEndpointMock;
    private Mock<ILogger<UserController>> _loggerMock;

    public UserControllerTest()
    {
        _userServiceMock = new Mock<IUserService>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<UserController>>();
    }

    [Fact]
    public async Task SignUp_ShouldReturnCreatedResult_WhenDataIsValid()
    {
        // Arrange
        var userController = new UserController(_userServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object);

        var signUpRequest = new SignUpRequest
        {
            Username = "newuser",
            Password = "NewUser@123",
            Email = "new-user@example.com",
            FirstName = "New",
            LastName = "User"
        };

        var createdUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = signUpRequest.Username,
            Email = signUpRequest.Email,
            FirstName = signUpRequest.FirstName,
            LastName = signUpRequest.LastName,
            CreatedAt = DateTime.UtcNow
        };

        _userServiceMock.Setup(us => us.CreateUserAsync(
          signUpRequest.Username,
          signUpRequest.Password,
          signUpRequest.Email,
          signUpRequest.FirstName,
          signUpRequest.LastName,
          It.IsAny<List<string>>()))
          .ReturnsAsync(createdUser);

        // Act
        var result = await userController.SignUp(signUpRequest);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        var routeValues = createdResult.RouteValues as IDictionary<string, object>;
        var userResponse = Assert.IsType<UserResponse>(createdResult.Value);
        Assert.Equal(nameof(UserController.SignUp), createdResult.ActionName);
        Assert.NotNull(routeValues);
        Assert.Contains("id", routeValues.Keys);
        Assert.Equal(createdUser.Id, routeValues["id"]);
        Assert.Equal(createdUser.UserName, userResponse.Username);
    }

    [Fact]
    public async Task SignUp_ShouldReturnBadRequest_WhenDataIsInvalid()
    {
        // Arrange
        var userController = new UserController(_userServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object);

        var signUpRequest = new SignUpRequest
        {
            Username = "", // Invalid username
            Password = "NewUser@123",
            Email = "invalid-email",
            FirstName = "New",
            LastName = "User"
        };

        var invalidOperationException = new InvalidOperationException("Invalid user data");

        _userServiceMock.Setup(us => us.CreateUserAsync(
          signUpRequest.Username,
          signUpRequest.Password,
          signUpRequest.Email,
          signUpRequest.FirstName,
          signUpRequest.LastName,
          It.IsAny<List<string>>()))
          .ThrowsAsync(invalidOperationException);

        // Act
        var result = await userController.SignUp(signUpRequest);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var errorResponse = badRequestResult.Value;
        Assert.NotNull(errorResponse);
        Assert.Contains("message", errorResponse.GetType().GetProperties().Select(p => p.Name));
        Assert.Equal(invalidOperationException.Message, errorResponse.GetType().GetProperty("message")?.GetValue(errorResponse));
    }

    [Fact]
    public async Task UpdateUser_ShouldReturnForbidden_WhenClaimsDoNotMatch()
    {
        // Arrange
        var userController = new UserController(_userServiceMock.Object, _publishEndpointMock.Object, _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var updateUserRequest = new UpdateUserRequest
        {
            FirstName = "Updated",
            LastName = "User"
        };

        var testUserId = Guid.NewGuid();
        
        // Act
        var result = await userController.UpdateUser(testUserId, updateUserRequest);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}