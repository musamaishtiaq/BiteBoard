using Asp.Versioning;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BiteBoard.API.DTOs.Mail;
using BiteBoard.API.DTOs.Requests.Account;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
using BiteBoard.Data.Extensions;
using BiteBoard.ResultWrapper;
using System;
using System.Security.Claims;

namespace BiteBoard.API.Controllers.v1;

[AllowAnonymous]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AccountsController : TenantAwaresControllerBase
{
	private readonly UserManager<ApplicationUser> _userManager;
	private readonly RoleManager<ApplicationRole> _roleManager;
	private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;
    protected readonly TenantDbContext _tenantContext;
    private readonly IJWTService _jwtService;
	private readonly IMailService _mailService;
	private string _apiVersion = "1.0";

	public AccountsController(UserManager<ApplicationUser> userManager,
		RoleManager<ApplicationRole> roleManager,
		SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext context,
        IJWTService jwtService,
		IMailService mailService,
        TenantDbContext tenantContext) : base(tenantContext)
    {
		_userManager = userManager;
		_roleManager = roleManager;
		_signInManager = signInManager;
		_context = context;
        _jwtService = jwtService;
		_mailService = mailService;
	}

    [HttpPost("login")]
	public async Task<IActionResult> Login(LoginRequest model)
	{
        var tenant = GetTenant();

        if (!await ValidateTenant(tenant))
            return NotFound(Result.Fail($"Tenant not specified or invalid."));

        var errors = new List<string>();
		var user = await _userManager.FindByNameAsync(model.Username);
		if (user != null)
		{
			if (!await _userManager.IsEmailConfirmedAsync(user))
			{
				errors.Add("Email not confirmed. Please confirm your email to proceed.");
				return Unauthorized(Result.Fail("Authentication failed.").AddErrors(string.Empty, errors));
			}

			var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
			if (result.Succeeded)
			{
				var roles = await _userManager.GetRolesAsync(user);
				List<Claim> userClaims = new();
				foreach (var roleName in roles)
				{
					var role = await _roleManager.FindByNameAsync(roleName);
					if (role != null)
					{
						var roleClaims = await _roleManager.GetClaimsAsync(role);
						foreach (var claim in roleClaims)
						{
							if (!userClaims.Where(uc => uc.Value == claim.Value).Any())
                            {
                                userClaims.Add(claim);
                            }
						}
					}
				}
                var userRoles = roles
                    .Select(roleName => Enum.TryParse<Roles>(roleName, out var role) ? role : (Roles?)null)
                    .Where(role => role != null)
                    .Cast<Roles>()
                    .ToList();
                var sortedRoles = userRoles.OrderBy(role => (int)role).ToList();
                var firstRole = sortedRoles.FirstOrDefault();
                var response = new LoginResponse
				{
					Token = _jwtService.GenerateToken(user, userClaims, tenant),
					Role = firstRole,
					Tenant = tenant
                };
				return Ok(Result<LoginResponse>.Success(response, $"Welcome back, '{user.UserName}'! You have logged in successfully."));
			}
			else
				errors.Add("Invalid username or password.");
		}
		else
			errors.Add("Invalid username or password.");
		return Unauthorized(Result.Fail("Authentication failed.").AddErrors(string.Empty, errors));
	}

    [HttpPost("register")]
	public async Task<IActionResult> Register(RegisterRequest model)
	{
		var errors = new List<string>();
		var user = new ApplicationUser
		{
			UserName = model.Username,
			Email = model.Email,
			PhoneNumber = model.PhoneNumber
		};
		var result = await _userManager.CreateAsync(user, model.Password);
		if (result.Succeeded)
		{
			if (!await _roleManager.RoleExistsAsync(Roles.Basic.ToString()))
			{
				await _roleManager.CreateAsync(new ApplicationRole
				{
					Name = Roles.Basic.ToString()
				});
			}
            await _context.SaveChangesAsync();
            await _userManager.AddToRoleAsync(user, Roles.Basic.ToString());
			var token = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
			var bytes = System.Text.Encoding.UTF8.GetBytes(token);
			var base64EncodedLink = Convert.ToBase64String(bytes);
			var baseUrl = $"{Request.Scheme}://{Request.Host}";
			var confirmationLink = $"{model.ConfirmEmailUrl}?token={base64EncodedLink}&email={user.Email}";
			var mailRequest = new MailRequest
			{
				To = model.Email,
				Subject = "Registration Confirmation",
				Body = $"Please confirm your registration by clicking the link: <a href=\"{confirmationLink}\">Confirm Email</a>",
				From = string.Empty,
				AttachmentPath = string.Empty
			};
			await _mailService.SendAsync(mailRequest);
			return Ok(Result.Success($"User '{user.UserName}' registered successfully. Please confirm your email."));
		}
		foreach (var error in result.Errors)
		{
			errors.Add(error.Description);
		}
		return BadRequest(Result.Fail("Error occurred while register new user.").AddErrors(string.Empty, errors));
	}

	[HttpGet("confirm-email")]
	public async Task<IActionResult> ConfirmEmail(string token, string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null)
			return NotFound(Result.Fail("User not found."));

		var tokenBytes = Convert.FromBase64String(token);
		var decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
		var result = await _userManager.ChangeEmailAsync(user, email, decodedToken);
		if (result.Succeeded)
			return Ok(Result.Success($"User '{user.UserName}' email confirmed."));
		List<string> errors = new();
		foreach (var error in result.Errors)
		{
			errors.Add(error.Description);
		}
		return Ok(Result.Fail("Email confirmation failed. Please fix the given errors").AddErrors(string.Empty, errors));
	}

	[Authorize]
	[HttpPost("change-password")]
	public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
	{
		var user = await _userManager.GetUserAsync(User);
		if (user == null)
		{
			return Ok(Result.Fail("You need to be logged in to change your password."));
		}
		var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

		if (!result.Succeeded)
		{
			var errors = new List<string>();
			foreach (var error in result.Errors)
			{
				errors.Add(error.Description);
			}
			if (errors.Contains("Incorrect password."))
			{
				return Ok(Result.Fail("The current password is incorrect."));
			}
			return Ok(Result.Fail("Password not changed. Please fixe the given errors.").AddErrors(string.Empty, errors));
		}
		return Ok(Result.Success($"Password for '{user.UserName}' has been changed successfully."));
	}

	[HttpPost("forgot-password")]
	public async Task<IActionResult> ForgotPassword(string email)
	{
		var user = await _userManager.FindByEmailAsync(email);
		if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
		{
			return Ok(Result.Success("Password reset link has been sent successfully."));
		}
		var token = await _userManager.GeneratePasswordResetTokenAsync(user);
		var bytes = System.Text.Encoding.UTF8.GetBytes(token);
		var base64EncodedLink = Convert.ToBase64String(bytes);
		var baseUrl = $"{Request.Scheme}://{Request.Host}";
		var resetLink = $"{baseUrl}/api/v{_apiVersion}/account/reset-password?token={base64EncodedLink}&email={user.Email}";
		var mailRequest = new MailRequest
		{
			To = email,
			Subject = "Password Reset",
			Body = $"Please reset your password by clicking the link: <a href=\"{resetLink}\">Reset Password</a>",
			From = string.Empty,
			AttachmentPath = string.Empty
		};
		await _mailService.SendAsync(mailRequest);
		return Ok(Result.Success("Password reset link has been sent successfully."));
	}


	[HttpPost("reset-password")]
	[AllowAnonymous]
	public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
	{
		var user = await _userManager.FindByEmailAsync(model.Email);
		if (user == null)
		{
			return NotFound(Result.Fail("User not found."));
		}
		var tokenBytes = Convert.FromBase64String(model.Token);
		var decodedToken = System.Text.Encoding.UTF8.GetString(tokenBytes);
		var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);
		if (!result.Succeeded)
		{
			var errors = new List<string>();
			foreach (var error in result.Errors)
			{
				errors.Add(error.Description);
			}
			return Ok(Result.Fail("Failed to reset password. Please fix the given errors.").AddErrors(string.Empty, errors));
		}
		return Ok(Result<string>.Success(null, "Password has been reset successfully."));
	}
}