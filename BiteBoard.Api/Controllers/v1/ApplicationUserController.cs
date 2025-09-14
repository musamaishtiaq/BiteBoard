using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiteBoard.API.DTOs.Requests.Identity;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;
using BiteBoard.ResultWrapper;
using System.Net.Mail;
using System.Text.RegularExpressions;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.API.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ApplicationUsersController : TenantAwaresControllerBase
    {
        private readonly IdentityContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        protected readonly TenantDbContext _tenantContext;
        private readonly IMapper _mapper;

        public ApplicationUsersController(IdentityContext context, UserManager<ApplicationUser> userManager, TenantDbContext tenantContext, IMapper mapper) : base (tenantContext)
        {
            _context = context;
            _userManager = userManager;
            _tenantContext = tenantContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = Data.Constants.Permissions.Users.View)]
        public async Task<IActionResult> GetApplicationUsers()
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            ApplicationUser currentUser = await _userManager.GetUserAsync(HttpContext.User);
            List<ApplicationUser> allUsersExceptCurrentUser = await _userManager.Users.Where(a => a.Id != currentUser.Id).ToListAsync();
            return Ok(Result<List<ApplicationUserDto>>.Success(_mapper.Map<List<ApplicationUserDto>>(allUsersExceptCurrentUser)));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Users.View)]
        public async Task<IActionResult> GetApplicationUser(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var applicationUser = await _userManager.Users.FirstAsync(e => e.Id == id.ToGuidFromString());
            if (applicationUser == null)
                return NotFound(Result.Fail($"User with id: {id} not found."));
            return Ok(Result<ApplicationUserDto>.Success(_mapper.Map<ApplicationUserDto>(applicationUser)));
        }

        [HttpPost]
        [Authorize(Policy = Data.Constants.Permissions.Users.Create)]
        public async Task<IActionResult> CreateApplicationUser(ApplicationUserDto userDto)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            string normalizedUserName = Regex.Replace(userDto.UserName.Trim(), @"\s+", "");
            MailAddress mailAddress = new(userDto.Email);
            ApplicationUser user = new()
            {
                UserName = normalizedUserName,
                Email = userDto.Email,
                EmailConfirmed = true,
            };
            IdentityResult result = await _userManager.CreateAsync(user, userDto.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Data.Enums.Roles.Basic.ToString());

                ApplicationUser UpdatedUser = _mapper.Map(userDto, user);
                _context.Entry(user).CurrentValues.SetValues(UpdatedUser);
                await _context.SaveChangesAsync();
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/applicationuser/{UpdatedUser.Id.ToStringFromGuid()}");
                return Created(uri, Result<ApplicationUserDto>.Success(_mapper.Map<ApplicationUserDto>(UpdatedUser), "User created successfully."));
            }
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Users.Edit)]
        public async Task<IActionResult> UpdateApplicationUser(string id, ApplicationUserDto userDto)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var errors = new List<string>();
            if (id != userDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and application user id: {userDto.Id}."));
            var userInDB = await _userManager.Users.FirstAsync(r => r.Id == id.ToGuidFromString());
            if (userInDB == null)
                return NotFound($"User with {id} Not Found");
            if (userInDB.UserName == "superadmin" || userInDB.UserName == "basicuser")
                return BadRequest(Result.Fail("You cannot edit default users (superadmin or basicuser)."));
            if (await _userManager.Users.AnyAsync(r => r.UserName == userDto.UserName && r.Id != id.ToGuidFromString()))
                return Conflict(Result.Fail($"UserName: {userDto.UserName} already taken."));
            ApplicationUser UpdatedUser = _mapper.Map(userDto, userInDB);
            _context.Entry(userInDB).CurrentValues.SetValues(UpdatedUser);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<ApplicationUserDto>.Success(_mapper.Map<ApplicationUserDto>(userInDB), "User updated successfully."));
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Users.Delete)]
        public async Task<IActionResult> DeleteApplicationUser(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var userInDb = await _userManager.Users.FirstAsync(e => e.Id == id.ToGuidFromString());
            if (userInDb == null)
                return NotFound(Result.Fail($"User with id: {id} not found."));
            if (userInDb.UserName == "superadmin" || userInDb.UserName == "basicuser")
                return BadRequest(Result.Fail("You cannot delete default users (superadmin or basicuser)."));
            _context.Users.Remove(userInDb);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<string>.Success(id, "User deleted successfully."));
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        [HttpPut("confirmemail")]
        public async Task<IActionResult> ConfirmEmail(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var errors = new List<string>();
            if (string.IsNullOrEmpty(id))
                return BadRequest(Result.Fail($"Invalid id: {id}."));
            var userInDB = await _userManager.Users.FirstAsync(r => r.Id == id.ToGuidFromString());
            if (userInDB == null)
                return NotFound($"User with id: {id} Not Found");
            if (userInDB.UserName == "superadmin" || userInDB.UserName == "basicuser")
                return BadRequest(Result.Fail("You cannot edit default users (superadmin or basicuser)."));
            userInDB.EmailConfirmed = true;
            _context.Entry(userInDB).CurrentValues.SetValues(userInDB);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<ApplicationUserDto>.Success(_mapper.Map<ApplicationUserDto>(userInDB), "User email confirm successfully."));
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }
    }
}
