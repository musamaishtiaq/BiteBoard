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
    public class ApplicationRoleController : TenantAwareControllerBase
    {
        private readonly IdentityContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        protected readonly TenantDbContext _tenantContext;
        private readonly IMapper _mapper;

        public ApplicationRoleController(IdentityContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, SignInManager<ApplicationUser> signInManager, TenantDbContext tenantContext, IMapper mapper) : base (tenantContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _tenantContext = tenantContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = Data.Constants.Permissions.Roles.View)]
        public async Task<IActionResult> GetApplicationRoles()
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            List<ApplicationRole> roles = await _roleManager.Roles.ToListAsync();
            return Ok(Result<List<RoleDto>>.Success(_mapper.Map<List<RoleDto>>(roles)));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Roles.View)]
        public async Task<IActionResult> GetApplicationRole(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var applicationRole = await _roleManager.Roles.FirstAsync(r => r.Id == id.ToGuidFromString());
            if (applicationRole == null)
                return NotFound(Result.Fail($"Role with id: {id} not found."));
            return Ok(Result<RoleDto>.Success(_mapper.Map<RoleDto>(applicationRole)));
        }

        [HttpPost]
        [Authorize(Policy = Data.Constants.Permissions.Roles.Create)]
        public async Task<IActionResult> CreateApplicationRole(RoleDto roleDto)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (await _roleManager.Roles.AnyAsync(r => r.Name == roleDto.Name))
                return Conflict(Result.Fail($"Role name: {roleDto.Name} already taken."));
            ApplicationRole role = new()
            {
                Name = roleDto.Name,
                NormalizedName = CapitalizeString(roleDto.Name)
            };
            _context.Add(role);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/applicationrole/{role.Id.ToStringFromGuid()}");
                return Created(uri, Result<RoleDto>.Success(_mapper.Map<RoleDto>(role), "Role created successfully."));
            }
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Roles.Edit)]
        public async Task<IActionResult> UpdateApplicationRole(string id, RoleDto roleDto)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var errors = new List<string>();
            if (id != roleDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and application role id: {roleDto.Id}."));
            var roleInDb = await _roleManager.Roles.FirstAsync(r => r.Id == id.ToGuidFromString());
            if (roleInDb.Name == "SuperAdmin" || roleInDb.Name == "Basic")
                return BadRequest(Result.Fail("You cannot edit default roles (SuperAdmin or Basic)."));
            if (await _roleManager.Roles.AnyAsync(r => r.Name == roleDto.Name && r.Id != id.ToGuidFromString()))
                return Conflict(Result.Fail($"Role name: {roleDto.Name} already taken."));
            ApplicationRole role = new()
            {
                Id = roleDto.Id.ToGuidFromString(),
                Name = roleDto.Name,
                NormalizedName = CapitalizeString(roleDto.Name)
            };
            var UpdatedRole = _mapper.Map(role, roleInDb);
            _context.Entry(roleInDb).CurrentValues.SetValues(UpdatedRole);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<RoleDto>.Success(_mapper.Map<RoleDto>(roleInDb), "Role updated successfully."));
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Roles.Delete)]
        public async Task<IActionResult> DeleteApplicationRole(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var roleInDb = await _roleManager.Roles.FirstAsync(e => e.Id == id.ToGuidFromString());
            if (roleInDb == null)
                return NotFound(Result.Fail($"Role with id: {id} not found."));
            if (roleInDb.Name == "SuperAdmin" || roleInDb.Name == "Basic")
                return BadRequest(Result.Fail("You cannot delete default roles (SuperAdmin or Basic)."));
            _context.Roles.Remove(roleInDb);
            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<string>.Success(id, "Role deleted successfully."));
            else
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
        }

        public static string CapitalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return input.ToUpper();
        }
    }
}
