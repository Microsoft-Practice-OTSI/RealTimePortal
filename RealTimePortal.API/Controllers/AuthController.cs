using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealTimePortal.API.Models;
using RealTimePortal.API.Services;

namespace RealTimePortal.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        JwtService jwtService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        _logger.LogInformation(
            "Login attempt for user {UserName}.",
            request.UserName);

        var user = await _userManager.FindByNameAsync(
            request.UserName);

        if (user == null)
        {
            _logger.LogWarning(
                "Login failed for user {UserName}.",
                request.UserName);

            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var result =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            _logger.LogWarning(
                "Login failed for user {UserName}.",
                request.UserName);

            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var roles =
            await _userManager.GetRolesAsync(user);

        var tokenResult =
            _jwtService.GenerateAccessToken(
                user,
                roles);

        _logger.LogInformation(
            "User {UserName} logged in successfully.",
            request.UserName);

        return Ok(new LoginResponse
        {
            AccessToken = tokenResult.Token,
            ExpiresIn = tokenResult.ExpiresIn
        });
    }


    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
    [FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return BadRequest(new
            {
                success = false,
                message = "Username is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new
            {
                success = false,
                message = "Email is required."
            });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new
            {
                success = false,
                message = "Password is required."
            });
        }

        if (request.Password != request.ConfirmPassword)
        {
            return BadRequest(new
            {
                success = false,
                message = "Password and confirm password do not match."
            });
        }

        var existingUser =
            await _userManager.FindByNameAsync(request.UserName);

        if (existingUser != null)
        {
            return Conflict(new
            {
                success = false,
                message = "Username already exists."
            });
        }

        var existingEmail =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingEmail != null)
        {
            return Conflict(new
            {
                success = false,
                message = "Email already exists."
            });
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FirstName = request.FirstName?.Trim(),
            LastName = request.LastName?.Trim(),
            EmailConfirmed = false
        };

        var result =
            await _userManager.CreateAsync(
                user,
                request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(x => x.Description)
                .ToList();

            _logger.LogWarning(
                "Registration failed for user {UserName}. Errors: {Errors}",
                request.UserName,
                string.Join("; ", errors));

            return BadRequest(new
            {
                success = false,
                message = "User registration failed.",
                errors
            });
        }

        // Assign default User role
        const string roleName = "User";

        if (await _userManager.IsInRoleAsync(user, roleName) == false)
        {
            var roleResult =
                await _userManager.AddToRoleAsync(
                    user,
                    roleName);

            if (!roleResult.Succeeded)
            {
                var errors = roleResult.Errors
                    .Select(x => x.Description)
                    .ToList();

                _logger.LogError(
                    "User {UserName} was created but assigning role {RoleName} failed. Errors: {Errors}",
                    user.UserName,
                    roleName,
                    string.Join("; ", errors));

                return StatusCode(500, new
                {
                    success = false,
                    message = "User was created, but assigning the default role failed.",
                    errors
                });
            }
        }

        _logger.LogInformation(
            "User {UserName} registered successfully.",
            user.UserName);

        return Ok(new RegisterResponse
        {
            Success = true,
            Message = "User registered successfully.",
            UserId = user.Id,
            UserName = user.UserName
        });
    }
}