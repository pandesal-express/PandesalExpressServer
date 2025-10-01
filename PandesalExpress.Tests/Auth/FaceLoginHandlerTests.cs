using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Features.FaceLogin;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;

namespace PandesalExpress.Tests.Auth;

public class FaceLoginHandlerTests
{
    private readonly Mock<ILogger<FaceLoginHandler>> _loggerMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    [Fact]
    public async Task Handle_ValidUserId_ReturnsAuthResponse()
    {
        // Arrange
        var userId = Ulid.NewUlid();
        var employee = new Employee
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            Position = "Cashier",
            Department = new Department
            {
                Id = Ulid.NewUlid(),
                Name = "Store Operations"
            }
        };

        var command = new FaceLoginCommand(userId);

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options;

        await using var context = new AppDbContext(options);
        await context.Employees.AddAsync(employee);
        await context.SaveChangesAsync();

        var store = new Mock<IUserStore<Employee>>();

        // Lots of nulls because we're not using the UserManager's functionality
        var userManager = new Mock<UserManager<Employee>>(store.Object, null, null, null, null, null, null, null, null);

        userManager.Setup(x => x.Users).Returns(context.Employees);
        userManager.Setup(x => x.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<Employee>()))
                         .ReturnsAsync(("test-token", DateTime.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                         .Returns("test-refresh-token");

        var handler = new FaceLoginHandler(userManager.Object, _tokenServiceMock.Object, _loggerMock.Object);

        // Act
        AuthResponseDto result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("test-refresh-token", result.RefreshToken);
        Assert.Equal(employee.Email, result.User.Email);
        Assert.Equal(employee.FirstName, result.User.FirstName);
        Assert.Equal(employee.LastName, result.User.LastName);
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var userId = Ulid.NewUlid();
        var command = new FaceLoginCommand(userId);

        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options;

        await using var context = new AppDbContext(options);

        var store = new Mock<IUserStore<Employee>>();

        // Lots of nulls because we're not using the UserManager's functionality
        var userManager = new Mock<UserManager<Employee>>(store.Object, null, null, null, null, null, null, null, null);
        userManager.Setup(x => x.Users).Returns(context.Employees);

        var handler = new FaceLoginHandler(userManager.Object, _tokenServiceMock.Object, _loggerMock.Object);

        // Act & Assert
        UnauthorizedAccessException exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));

        Assert.IsType<UnauthorizedAccessException>(exception);
        Assert.Equal("User not found.", exception.Message);
    }
}
