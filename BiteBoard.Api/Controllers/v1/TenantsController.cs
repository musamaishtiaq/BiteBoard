using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BiteBoard.API.DTOs.Requests;
using BiteBoard.API.Services.Interfaces;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Extensions;
using BiteBoard.ResultWrapper;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.API.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class TenantsController : TenantAwaresControllerBase
    {
        private readonly ITenantService _tenantService;
        protected readonly TenantDbContext _tenantContext;
        private readonly IMapper _mapper;

        public TenantsController(ITenantService tenantService, TenantDbContext tenantContext, IMapper mapper) : base (tenantContext)
        {
            _tenantService = tenantService;
            _tenantContext = tenantContext;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.View)]
        public async Task<IActionResult> GetTenants()
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var tenants = await _tenantService.GetAllTenantsAsync();
            return Ok(Result<List<TenantDto>>.Success(_mapper.Map<List<TenantDto>>(tenants)));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.View)]
        public async Task<IActionResult> GetTenant(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var tenantInDb = await _tenantService.GetTenantById(id);
            if (tenantInDb == null)
                return NotFound(Result.Fail($"Tenant with id: {id} not found."));
            return Ok(Result<TenantDto>.Success(_mapper.Map<TenantDto>(tenantInDb)));
        }

        [HttpPost]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.Create)]
        public async Task<IActionResult> CreateTenant([FromBody] TenantDto request)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            try
            {
                var tenantInDb = await _tenantService.CreateTenantAsync(request);
                var uri = new Uri($"{Request.Scheme}://{tenantInDb.Identifier}.{Request.Host}/api/v1/tenant/{tenantInDb.Id}");
                return Created(uri, Result<TenantDto>.Success(_mapper.Map<TenantDto>(tenantInDb), "Tenant created successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.Fail("Something went wrong. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.Edit)]
        public async Task<IActionResult> UpdateTenant(string id, TenantDto request)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            try
            {
                var tenantInDb = await _tenantService.UpdateTenantAsync(id, request);
                return Ok(Result<TenantDto>.Success(_mapper.Map<TenantDto>(tenantInDb), "Tenant Updated successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.Fail("Something went wrong. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
            }
        }

        [HttpPut("enable")]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.Enable)]
        public async Task<IActionResult>EnableTenant(string id, bool isEnabled)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            try
            {
                var tenantInDb = await _tenantService.EnableTenantAsync(id, isEnabled);
                return Ok(Result<string>.Success(tenantInDb.Id, "Tenant " + (isEnabled ? "enabled" : "disabled") + " successfully."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(Result.Fail("Something went wrong. Please try again."));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail("Something went wrong. Please try again."));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Data.Constants.Permissions.Tenants.Delete)]
        public async Task<IActionResult> DeleteTenant(string id)
        {
            var tenant = GetTenant();

            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var result = await _tenantService.DeleteTenantAsync(id);
            if (!result)
                return NotFound(Result.Fail($"Tenant with id: {id} not found."));

            return Ok(Result<string>.Success(id, "Tenant deleted successfully."));
        }
    }
}
