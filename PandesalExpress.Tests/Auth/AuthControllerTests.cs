using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PandesalExpress.Auth.Controllers;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Features.FaceLogin;
using PandesalExpress.Auth.Features.FaceRegister;
using PandesalExpress.Infrastructure.Abstractions;
using Shared.Dtos;

namespace PandesalExpress.Tests.Auth;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<ILogger<AuthController>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;

    public AuthControllerTests()
    {
        _loggerMock = new Mock<ILogger<AuthController>>();
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_loggerMock.Object) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
    }

    [Fact]
    public async Task FaceLogin_ValidUserId_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var userId = Ulid.NewUlid().ToString();
        var expectedResponse = new AuthResponseDto
        {
            Token = "test-token",
            RefreshToken = "test-refresh-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(3).ToString("O"),
            User = new EmployeeDto
            {
                Id = userId,
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                Position = "Cashier",
                Department = new DepartmentDto
                {
                    Id = Ulid.NewUlid().ToString(),
                    Name = "Store Operations"
                }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceLoginCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await _controller.FaceLogin(userId, _mediatorMock.Object);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        AuthResponseDto response = Assert.IsType<AuthResponseDto>(okResult.Value);

        // Check if the token, refresh token are present in the response
        Assert.NotNull(response.Token);
        Assert.NotNull(response.RefreshToken);
        Assert.NotNull(response.RefreshTokenExpiration);

        Assert.Equal(expectedResponse.Token, response.Token);
        Assert.Equal(expectedResponse.User.Email, response.User.Email);
    }

    [Fact]
    public async Task FaceLogin_UnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Ulid.NewUlid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceLoginCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        IActionResult result = await _controller.FaceLogin(userId, _mediatorMock.Object);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task FaceLogin_Exception_ReturnsInternalServerError()
    {
        // Arrange
        var userId = Ulid.NewUlid().ToString();
        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceLoginCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Database error"));

        // Act
        IActionResult result = await _controller.FaceLogin(userId, _mediatorMock.Object);

        // Assert
        ObjectResult statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task FaceRegister_ValidRequest_ReturnsOkWithAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterRequestDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Position = "Commissary",
            DepartmentId = Ulid.NewUlid().ToString()
        };

        var expectedResponse = new AuthResponseDto
        {
            Token = "test-token",
            RefreshToken = "test-refresh-token",
            Expiration = DateTime.UtcNow.AddHours(1),
            RefreshTokenExpiration = DateTime.UtcNow.AddDays(3).ToString("O"),
            User = new EmployeeDto
            {
                Id = Ulid.NewUlid().ToString(),
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Position = registerDto.Position,
                Department = new DepartmentDto
                {
                    Id = registerDto.DepartmentId,
                    Name = "Engineering"
                }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceRegisterCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(expectedResponse);

        // Act
        IActionResult result = await _controller.FaceRegister(registerDto, _mediatorMock.Object);

        // Assert
        OkObjectResult okResult = Assert.IsType<OkObjectResult>(result);
        AuthResponseDto response = Assert.IsType<AuthResponseDto>(okResult.Value);

        Assert.NotNull(response.Token);
        Assert.NotNull(response.RefreshToken);
        Assert.NotNull(response.RefreshTokenExpiration);

        Assert.Equal(expectedResponse.Token, response.Token);
        Assert.Equal(expectedResponse.User.Email, response.User.Email);
    }

    [Fact]
    public async Task FaceRegister_UnauthorizedAccess_ReturnsUnauthorized()
    {
        // Arrange
        var registerDto = new RegisterRequestDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Position = "Developer",
            DepartmentId = Ulid.NewUlid().ToString()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceRegisterCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        IActionResult result = await _controller.FaceRegister(registerDto, _mediatorMock.Object);

        // Assert
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task FaceRegister_Exception_ReturnsInternalServerError()
    {
        // Arrange
        var registerDto = new RegisterRequestDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Position = "Developer",
            DepartmentId = Ulid.NewUlid().ToString()
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<FaceRegisterCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Database error"));

        // Act
        IActionResult result = await _controller.FaceRegister(registerDto, _mediatorMock.Object);

        // Assert
        ObjectResult statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
