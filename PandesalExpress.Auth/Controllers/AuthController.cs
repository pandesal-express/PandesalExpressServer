using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using PandesalExpress.Auth.Dtos;
using PandesalExpress.Auth.Exceptions;
using PandesalExpress.Auth.Features.FaceLogin;
using PandesalExpress.Auth.Features.FaceRegister;
using PandesalExpress.Auth.Features.Login;
using PandesalExpress.Auth.Features.RefreshToken;
using PandesalExpress.Auth.Features.Register;
using PandesalExpress.Infrastructure.Abstractions;
using PandesalExpress.Infrastructure.Models;
using Shared.Dtos;

namespace PandesalExpress.Auth.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ILogger<AuthController> logger) : ControllerBase
{
    private void _appendCookieDelegate(string key, string value, CookieOptions options) => Response.Cookies.Append(key, value, options);

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto, [FromServices] IMediator mediator)
    {
        try
        {
            var command = new LoginCommand(loginDto.Email, loginDto.Password, _appendCookieDelegate);

            AuthResponseDto result = await mediator.Send(command, HttpContext.RequestAborted);
            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during login for {Email}", loginDto.Email);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerDto, [FromServices] IMediator mediator)
    {
        try
        {
            var command = new RegisterCommand(registerDto);
            AuthResponseDto result = await mediator.Send(command, HttpContext.RequestAborted);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during registration for {Email}", registerDto.Email);
            return BadRequest(
                new
                {
                    message = "Registration failed. Please check your inputs.",
                    details = ex.Message
                }
            );
        }
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuthResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromServices] IMediator mediator)
    {
        string? refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            logger.LogWarning("Refresh token endpoint called without a refresh_token cookie.");
            return Unauthorized(new { message = "Invalid session. Please log in again." });
        }

        string accessToken = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        if (string.IsNullOrEmpty(accessToken))
        {
            logger.LogWarning("Refresh token endpoint called without an expired access token header.");
            return Unauthorized(new { message = "Invalid request." });
        }

        try
        {
            var command = new RefreshTokenCommand(accessToken, refreshToken, _appendCookieDelegate);
            AuthResponseDto response = await mediator.Send(command, HttpContext.RequestAborted);

            return Ok(response);
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Token refresh failed due to invalid token.");
            return Unauthorized(new { message = "Invalid session. Please log in again." });
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Token refresh failed due to authorization logic.");
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during token refresh.");
            return StatusCode(500, new { message = "Something went bad. Please try again." });
        }
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(EmployeeDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetUser([FromServices] UserManager<Employee> userManager)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Ulid.TryParse(userId, out Ulid userUlid)) return Unauthorized();

        Employee? employee = await userManager.Users
                                              .AsNoTracking()
                                              .Include(e => e.Department)
                                              .FirstOrDefaultAsync(e => e.Id == userUlid);

        if (employee == null) return NotFound();

        return Ok(
            new EmployeeDto
            {
                Id = employee.Id.ToString(),
                DepartmentId = employee.DepartmentId.ToString(),
                Email = employee.Email!,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Position = employee.Position,
                StoreId = employee.StoreId?.ToString(),
                Department = new DepartmentDto
                {
                    Id = employee.Department.Id.ToString(),
                    Name = employee.Department.Name
                }
            }
        );
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Logout([FromServices] SignInManager<Employee> signInManager)
    {
        Response.Cookies.Delete(
            "jwt_token",
            new CookieOptions
            {
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            }
        );
        Response.Cookies.Delete(
            "refresh_token",
            new CookieOptions
            {
                Path = "/",
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            }
        );
        await signInManager.SignOutAsync();

        return Ok(new { message = "Logged out successfully." });
    }

    [Authorize(AuthenticationSchemes = "FaceAuthScheme")]
    [HttpPost("face-login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FaceLogin(
        [FromBody] DateTime timeLogged,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            string? userIdClaim = User.FindFirst("user_id")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                logger.LogWarning("JWT missing user_id claim.");
                throw new UnauthorizedAccessException("User ID not found in claims");
            }

            if (!Ulid.TryParse(userIdClaim, out Ulid userUlid))
            {
                logger.LogWarning("Invalid user id format in token: {UserId}", userIdClaim);
                throw new UnauthorizedAccessException("Invalid user ID format");
            }

            var command = new FaceLoginCommand(userUlid, timeLogged);
            AuthResponseDto result = await mediator.Send(command, HttpContext.RequestAborted);

            Response.Cookies.Append(
                "jwt_token",
                result.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = result.Expiration,
                    Path = "/"
                }
            );

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken!,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = result.RefreshTokenExpiration,
                    Path = "/api/Auth/refresh-token"
                }
            );

            return Ok(result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred during authentication");
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }

    [Authorize(AuthenticationSchemes = "FaceAuthScheme")]
    [HttpPost("face-register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FaceRegister(
        [FromBody] RegisterRequestDto registerDto,
        [FromServices] IMediator mediator
    )
    {
        try
        {
            var command = new FaceRegisterCommand(registerDto);
            AuthResponseDto result = await mediator.Send(command, HttpContext.RequestAborted);

            Response.Cookies.Append(
                "jwt_token",
                result.Token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = result.Expiration,
                    Path = "/"
                }
            );

            Response.Cookies.Append(
                "refresh_token",
                result.RefreshToken!,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = result.RefreshTokenExpiration,
                    Path = "/api/Auth/refresh-token"
                }
            );

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (UnauthorizedAccessException) { return Unauthorized(); }
        catch (DuplicateEmailException ex) { return Conflict(new { message = ex.Message }); }
        catch (Exception ex)
        {
            logger.LogError(ex, "Registration error: {Message}", ex.Message);
            return StatusCode(500, new { message = "An internal server error occurred." });
        }
    }
}
