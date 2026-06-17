using GoingMy.Auth.API.Controllers;
using GoingMy.Auth.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OpenIddict.Abstractions;

namespace GoingMy.Auth.Tests;

public class AuthorizationControllerTest
{
    private Mock<IOpenIddictApplicationManager> _applicationManagerMock;
    private Mock<IUserService> _userServiceMock;
    private Mock<IRefreshTokenBlacklistService> _refreshTokenBlacklistServiceMock;
    private Mock<ILogger<AuthorizationController>> _loggerMock;

    public AuthorizationControllerTest()
    {
        _applicationManagerMock = new Mock<IOpenIddictApplicationManager>();
        _userServiceMock = new Mock<IUserService>();
        _refreshTokenBlacklistServiceMock = new Mock<IRefreshTokenBlacklistService>();
        _loggerMock = new Mock<ILogger<AuthorizationController>>();
    }

    [Fact]
    public async Task Authorize_ShouldThrowException_WhenInvalidCredentials()
    {
        // Arrange
        var authorizationController = new AuthorizationController(
            _applicationManagerMock.Object,
            _userServiceMock.Object,
            _refreshTokenBlacklistServiceMock.Object,
            _loggerMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                _ = await authorizationController.Authorize();
            }
        );
    }
}