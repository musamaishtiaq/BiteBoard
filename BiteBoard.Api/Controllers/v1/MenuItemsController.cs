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
    public class MenuItemsController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public MenuItemsController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/v1/menuitems
        [HttpGet]
        [Authorize(Policy = MenuItems.View)]
        public async Task<IActionResult> GetMenuItems()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var menuItems = await _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.MenuItemModifiers)
                .ThenInclude(mim => mim.ModifierGroup)
                .ToListAsync();

            return Ok(Result<List<MenuItemDto>>.Success(_mapper.Map<List<MenuItemDto>>(menuItems)));
        }

        // GET: api/v1/menuitems/5
        [HttpGet("{id}")]
        [Authorize(Policy = MenuItems.View)]
        public async Task<IActionResult> GetMenuItem(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var menuItem = await _context.MenuItems
                .Include(m => m.Category)
                .Include(m => m.MenuItemModifiers)
                .ThenInclude(mim => mim.ModifierGroup)
                .FirstOrDefaultAsync(m => m.Id == id.ToGuidFromString());

            if (menuItem == null)
                return NotFound(Result.Fail($"Menu item with id: {id} not found."));

            return Ok(Result<MenuItemDto>.Success(_mapper.Map<MenuItemDto>(menuItem)));
        }

        // POST: api/v1/menuitems
        [HttpPost]
        [Authorize(Policy = MenuItems.Create)]
        public async Task<IActionResult> CreateMenuItem(MenuItemDto menuItemDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            bool alreadyExists = await _context.MenuItems
                .AnyAsync(m => m.Name == menuItemDto.Name);

            if (alreadyExists)
                return Conflict(Result.Fail($"Menu item name: {menuItemDto.Name} already exists."));

            var newMenuItem = _mapper.Map<MenuItem>(menuItemDto);
            await _context.MenuItems.AddAsync(newMenuItem);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/menuitems/{newMenuItem.Id.ToStringFromGuid()}");
                return Created(uri, Result<MenuItemDto>.Success(
                    _mapper.Map<MenuItemDto>(newMenuItem), "Menu item created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/menuitems/5
        [HttpPut("{id}")]
        [Authorize(Policy = MenuItems.Edit)]
        public async Task<IActionResult> UpdateMenuItem(string id, MenuItemDto menuItemDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (id != menuItemDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and menu item id: {menuItemDto.Id}."));

            var menuItemInDb = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == id.ToGuidFromString());

            if (menuItemInDb == null)
                return NotFound(Result.Fail($"Menu item with id: {id} not found."));

            bool nameExists = await _context.MenuItems
                .AnyAsync(m => m.Name == menuItemDto.Name && m.Id != id.ToGuidFromString());

            if (nameExists)
                return Conflict(Result.Fail($"Menu item name: {menuItemDto.Name} already taken."));

            _mapper.Map(menuItemDto, menuItemInDb);
            _context.Entry(menuItemInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<MenuItemDto>.Success(
                    _mapper.Map<MenuItemDto>(menuItemInDb), "Menu item updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/menuitems/5
        [HttpDelete("{id}")]
        [Authorize(Policy = MenuItems.Delete)]
        public async Task<IActionResult> DeleteMenuItem(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var menuItemInDb = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == id.ToGuidFromString());

            if (menuItemInDb == null)
                return NotFound(Result.Fail($"Menu item with id: {id} not found."));

            _context.MenuItems.Remove(menuItemInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Menu item deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // POST: api/v1/menuitems/5/modifiers
        [HttpPost("{itemId}/modifiers")]
        [Authorize(Policy = MenuItems.Manage)]
        public async Task<IActionResult> AddMenuItemModifier(string itemId, string modifierGroupId)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == itemId.ToGuidFromString());

            if (menuItem == null)
                return NotFound(Result.Fail($"Menu item with id: {itemId} not found."));

            var modifierGroup = await _context.ModifierGroups
                .FirstOrDefaultAsync(mg => mg.Id == modifierGroupId.ToGuidFromString());

            if (modifierGroup == null)
                return NotFound(Result.Fail($"Modifier group with id: {modifierGroupId} not found."));

            bool alreadyExists = await _context.MenuItemModifiers
                .AnyAsync(mim => mim.MenuItemId == menuItem.Id && mim.ModifierGroupId == modifierGroup.Id);

            if (alreadyExists)
                return Conflict(Result.Fail($"Modifier group already added to this menu item."));

            var menuItemModifier = new MenuItemModifier
            {
                MenuItemId = menuItem.Id,
                ModifierGroupId = modifierGroup.Id
            };

            await _context.MenuItemModifiers.AddAsync(menuItemModifier);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result.Success("Modifier group added to menu item successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/menuitems/5/modifiers/10
        [HttpDelete("{itemId}/modifiers/{modifierGroupId}")]
        [Authorize(Policy = MenuItems.Manage)]
        public async Task<IActionResult> RemoveMenuItemModifier(string itemId, string modifierGroupId)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var menuItemModifier = await _context.MenuItemModifiers
                .FirstOrDefaultAsync(mim => mim.MenuItemId == itemId.ToGuidFromString() &&
                                          mim.ModifierGroupId == modifierGroupId.ToGuidFromString());

            if (menuItemModifier == null)
                return NotFound(Result.Fail($"Modifier group not found for this menu item."));

            _context.MenuItemModifiers.Remove(menuItemModifier);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result.Success("Modifier group removed from menu item successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}

