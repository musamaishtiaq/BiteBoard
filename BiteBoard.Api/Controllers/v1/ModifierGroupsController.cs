using Asp.Versioning;
using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.API.Controllers.v1;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Extensions;
using BiteBoard.ResultWrapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.Api.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ModifierGroupsController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ModifierGroupsController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/v1/modifiergroups
        [HttpGet]
        [Authorize(Policy = ModifierGroups.View)]
        public async Task<IActionResult> GetModifierGroups()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var modifierGroups = await _context.ModifierGroups
                .Include(mg => mg.ModifierOptions)
                .ToListAsync();

            return Ok(Result<List<ModifierGroupDto>>.Success(_mapper.Map<List<ModifierGroupDto>>(modifierGroups)));
        }

        // GET: api/v1/modifiergroups/5
        [HttpGet("{id}")]
        [Authorize(Policy = ModifierGroups.View)]
        public async Task<IActionResult> GetModifierGroup(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var modifierGroup = await _context.ModifierGroups
                .Include(mg => mg.ModifierOptions)
                .FirstOrDefaultAsync(mg => mg.Id == id.ToGuidFromString());

            if (modifierGroup == null)
                return NotFound(Result.Fail($"Modifier group with id: {id} not found."));

            return Ok(Result<ModifierGroupDto>.Success(_mapper.Map<ModifierGroupDto>(modifierGroup)));
        }

        // POST: api/v1/modifiergroups
        [HttpPost]
        [Authorize(Policy = ModifierGroups.Create)]
        public async Task<IActionResult> CreateModifierGroup(ModifierGroupDto modifierGroupDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            bool alreadyExists = await _context.ModifierGroups
                .AnyAsync(mg => mg.Name == modifierGroupDto.Name);

            if (alreadyExists)
                return Conflict(Result.Fail($"Modifier group name: {modifierGroupDto.Name} already exists."));

            var newModifierGroup = _mapper.Map<ModifierGroup>(modifierGroupDto);
            await _context.ModifierGroups.AddAsync(newModifierGroup);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/modifiergroups/{newModifierGroup.Id.ToStringFromGuid()}");
                return Created(uri, Result<ModifierGroupDto>.Success(
                    _mapper.Map<ModifierGroupDto>(newModifierGroup), "Modifier group created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/modifiergroups/5
        [HttpPut("{id}")]
        [Authorize(Policy = ModifierGroups.Edit)]
        public async Task<IActionResult> UpdateModifierGroup(string id, ModifierGroupDto modifierGroupDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (id != modifierGroupDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and modifier group id: {modifierGroupDto.Id}."));

            var modifierGroupInDb = await _context.ModifierGroups
                .FirstOrDefaultAsync(mg => mg.Id == id.ToGuidFromString());

            if (modifierGroupInDb == null)
                return NotFound(Result.Fail($"Modifier group with id: {id} not found."));

            bool nameExists = await _context.ModifierGroups
                .AnyAsync(mg => mg.Name == modifierGroupDto.Name && mg.Id != id.ToGuidFromString());

            if (nameExists)
                return Conflict(Result.Fail($"Modifier group name: {modifierGroupDto.Name} already taken."));

            _mapper.Map(modifierGroupDto, modifierGroupInDb);
            _context.Entry(modifierGroupInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<ModifierGroupDto>.Success(
                    _mapper.Map<ModifierGroupDto>(modifierGroupInDb), "Modifier group updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/modifiergroups/5
        [HttpDelete("{id}")]
        [Authorize(Policy = ModifierGroups.Delete)]
        public async Task<IActionResult> DeleteModifierGroup(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var modifierGroupInDb = await _context.ModifierGroups
                .Include(mg => mg.ModifierOptions)
                .FirstOrDefaultAsync(mg => mg.Id == id.ToGuidFromString());

            if (modifierGroupInDb == null)
                return NotFound(Result.Fail($"Modifier group with id: {id} not found."));

            if (modifierGroupInDb.ModifierOptions?.Any() == true)
                return BadRequest(Result.Fail("Cannot delete modifier group that has options."));

            _context.ModifierGroups.Remove(modifierGroupInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Modifier group deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // POST: api/v1/modifiergroups/5/options
        [HttpPost("{groupId}/options")]
        [Authorize(Policy = ModifierGroups.Manage)]
        public async Task<IActionResult> AddModifierOption(string groupId, ModifierOptionDto optionDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var modifierGroup = await _context.ModifierGroups
                .FirstOrDefaultAsync(mg => mg.Id == groupId.ToGuidFromString());

            if (modifierGroup == null)
                return NotFound(Result.Fail($"Modifier group with id: {groupId} not found."));

            bool optionExists = await _context.ModifierOptions
                .AnyAsync(mo => mo.Name == optionDto.Name && mo.ModifierGroupId == modifierGroup.Id);

            if (optionExists)
                return Conflict(Result.Fail($"Modifier option name: {optionDto.Name} already exists in this group."));

            var newOption = _mapper.Map<ModifierOption>(optionDto);
            newOption.ModifierGroupId = modifierGroup.Id;
            await _context.ModifierOptions.AddAsync(newOption);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<ModifierOptionDto>.Success(
                    _mapper.Map<ModifierOptionDto>(newOption), "Modifier option created successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/modifiergroups/5/options/10
        [HttpPut("{groupId}/options/{optionId}")]
        [Authorize(Policy = ModifierGroups.Manage)]
        public async Task<IActionResult> UpdateModifierOption(string groupId, string optionId, ModifierOptionDto optionDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (optionId != optionDto.Id)
                return BadRequest(Result.Fail($"Invalid option id: {optionId} and option dto id: {optionDto.Id}."));

            var modifierOption = await _context.ModifierOptions
                .FirstOrDefaultAsync(mo => mo.Id == optionId.ToGuidFromString() && mo.ModifierGroupId == groupId.ToGuidFromString());

            if (modifierOption == null)
                return NotFound(Result.Fail($"Modifier option with id: {optionId} not found in group: {groupId}."));

            bool optionExists = await _context.ModifierOptions
                .AnyAsync(mo => mo.Name == optionDto.Name && mo.ModifierGroupId == groupId.ToGuidFromString() && mo.Id != optionId.ToGuidFromString());

            if (optionExists)
                return Conflict(Result.Fail($"Modifier option name: {optionDto.Name} already exists in this group."));

            _mapper.Map(optionDto, modifierOption);
            _context.Entry(modifierOption).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<ModifierOptionDto>.Success(
                    _mapper.Map<ModifierOptionDto>(modifierOption), "Modifier option updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/modifiergroups/5/options/10
        [HttpDelete("{groupId}/options/{optionId}")]
        [Authorize(Policy = ModifierGroups.Manage)]
        public async Task<IActionResult> DeleteModifierOption(string groupId, string optionId)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var modifierOption = await _context.ModifierOptions
                .FirstOrDefaultAsync(mo => mo.Id == optionId.ToGuidFromString() && mo.ModifierGroupId == groupId.ToGuidFromString());

            if (modifierOption == null)
                return NotFound(Result.Fail($"Modifier option with id: {optionId} not found in group: {groupId}."));

            _context.ModifierOptions.Remove(modifierOption);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(optionId, "Modifier option deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}
