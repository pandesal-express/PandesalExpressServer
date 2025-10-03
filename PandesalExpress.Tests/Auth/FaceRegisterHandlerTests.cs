using System.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Features.FaceRegister;
using PandesalExpress.Infrastructure.Context;
using PandesalExpress.Infrastructure.Models;
using PandesalExpress.Infrastructure.Services;

namespace PandesalExpress.Tests.Auth;

public sealed class FaceRegisterHandlerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly FaceRegisterHandler _handler;
    private readonly Mock<ILogger<FaceRegisterHandler>> _loggerMock;
    private readonly Mock<RoleManager<AppRole>> _roleManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<UserManager<Employee>> _userManagerMock;

    #pragma warning disable CS8625
    public FaceRegisterHandlerTests()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
                                                 .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                                 .Options;
        _context = new AppDbContext(options);

        var userStore = new Mock<IUserStore<Employee>>();
        _userManagerMock = new Mock<UserManager<Employee>>(userStore.Object, null, null, null, null, null, null, null, null);

        var roleStore = new Mock<IRoleStore<AppRole>>();
        _roleManagerMock = new Mock<RoleManager<AppRole>>(roleStore.Object, null, null, null, null);

        _tokenServiceMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<FaceRegisterHandler>>();

        _handler = new FaceRegisterHandler(_userManagerMock.Object, _roleManagerMock.Object, _context, _tokenServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose() { _context.Dispose(); }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsAuthResponse()
    {
        // Arrange
        var department = new Department
        {
            Id = Ulid.NewUlid(),
            Name = "IT"
        };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Position = "Developer",
            DepartmentId = department.Id.ToString(),
            TimeLogged = DateTime.UtcNow
        };

        var command = new FaceRegisterCommand(registerDto);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((Employee?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<Employee>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<Employee>()))
                         .ReturnsAsync(("test-token", DateTime.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("test-refresh-token");

        // Act
        AuthResponseDto result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-token", result.Token);
        Assert.Equal("test-refresh-token", result.RefreshToken);
        Assert.Equal(registerDto.Email, result.User.Email);
        Assert.Equal(registerDto.FirstName, result.User.FirstName);
        Assert.Equal(registerDto.LastName, result.User.LastName);
        Assert.Equal(registerDto.Position, result.User.Position);
        Assert.Equal(department.Name, result.User.Department?.Name);
    }

    [Fact]
    public async Task Handle_ExistingUser_ThrowsInvalidOperationException()
    {
        // Arrange
        var department = new Department
        {
            Id = Ulid.NewUlid(),
            Name = "Store Operations"
        };

        var existingEmployee = new Employee
        {
            Email = "existing@example.com",
            FirstName = "Existing",
            LastName = "User",
            Position = "Cashier",
            DepartmentId = department.Id,
            Department = department
        };
        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com",
            Position = "Cashier",
            DepartmentId = department.Id.ToString()
        };

        var command = new FaceRegisterCommand(registerDto);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync(existingEmployee);

        // Act & Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("An account with this email already exists.", exception.Message);
    }

    [Fact]
    public async Task Handle_DepartmentNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Position = "Cashier",
            DepartmentId = Ulid.NewUlid().ToString()
        };

        var command = new FaceRegisterCommand(registerDto);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((Employee?)null);

        // Act & Assert
        KeyNotFoundException exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("The specified department does not exist.", exception.Message);
    }

    [Fact]
    public async Task Handle_UserCreationFails_ThrowsDataException()
    {
        // Arrange
        var department = new Department
        {
            Id = Ulid.NewUlid(),
            Name = "Store Operations"
        };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Position = "Developer",
            DepartmentId = department.Id.ToString()
        };

        var command = new FaceRegisterCommand(registerDto);

        IdentityError[] identityErrors =
        [
            new() { Description = "Email format invalid" }
        ];

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((Employee?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Failed(identityErrors));

        // Act & Assert
        DataException exception = await Assert.ThrowsAsync<DataException>(() => _handler.Handle(command, CancellationToken.None));

        Assert.Contains("User registration failed:", exception.Message);
        Assert.Contains("Email format invalid", exception.Message);
    }

    [Theory]
    [InlineData("Store Operations", "Cashier")]
    [InlineData("Commissary", "Stock Manager")]
    public async Task Handle_CreatesRolesForDepartmentAndPosition(string departmentName, string position)
    {
        // Arrange
        var department = new Department
        {
            Id = Ulid.NewUlid(),
            Name = departmentName
        };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Position = position,
            DepartmentId = department.Id.ToString(),
            TimeLogged = DateTime.UtcNow
        };

        var command = new FaceRegisterCommand(registerDto);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((Employee?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<Employee>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
                        .ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppRole>()))
                        .ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<Employee>()))
                         .ReturnsAsync(("test-token", DateTime.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken())
                         .Returns("test-refresh-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _roleManagerMock.Verify(x => x.CreateAsync(It.Is<AppRole>(r => r.Name == departmentName)), Times.Once);
        _roleManagerMock.Verify(x => x.CreateAsync(It.Is<AppRole>(r => r.Name == position)), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<Employee>(), departmentName), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<Employee>(), position), Times.Once);
    }

    [Fact]
    public async Task Handle_Attendance_HasBeenAdded()
    {
        // Arrange
        var department = new Department
        {
            Id = Ulid.NewUlid(),
            Name = "Stocks and Inventory"
        };
        await _context.Departments.AddAsync(department);
        await _context.SaveChangesAsync();

        DateTime timeLogged = DateTime.Now;
        var registerDto = new RegisterRequestDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Position = "Commissary",
            DepartmentId = department.Id.ToString(),
            TimeLogged = timeLogged
        };

        var command = new FaceRegisterCommand(registerDto);

        _userManagerMock.Setup(x => x.FindByEmailAsync(registerDto.Email)).ReturnsAsync((Employee?)null);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<Employee>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

        _roleManagerMock.Setup(x => x.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
        _roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Success);

        _tokenServiceMock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<Employee>()))
                         .ReturnsAsync(("test-token", DateTime.UtcNow.AddHours(1)));
        _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("test-refresh-token");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Attendance? attendance = await _context.Attendances.FirstOrDefaultAsync();

        Assert.NotNull(attendance);
        Assert.Equal(AttendanceStatus.Present, attendance.Status);
        Assert.Equal(timeLogged.TimeOfDay, attendance.CheckIn);
    }
}
