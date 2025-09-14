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
    public class DealsController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DealsController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/v1/deals
        [HttpGet]
        [Authorize(Policy = Deals.View)]
        public async Task<IActionResult> GetDeals()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var deals = await _context.Deals
                .Include(d => d.DealItems)
                .ThenInclude(di => di.MenuItem)
                .ToListAsync();

            return Ok(Result<List<DealDto>>.Success(_mapper.Map<List<DealDto>>(deals)));
        }

        // GET: api/v1/deals/5
        [HttpGet("{id}")]
        [Authorize(Policy = Deals.View)]
        public async Task<IActionResult> GetDeal(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var deal = await _context.Deals
                .Include(d => d.DealItems)
                .ThenInclude(di => di.MenuItem)
                .FirstOrDefaultAsync(d => d.Id == id.ToGuidFromString());

            if (deal == null)
                return NotFound(Result.Fail($"Deal with id: {id} not found."));

            return Ok(Result<DealDto>.Success(_mapper.Map<DealDto>(deal)));
        }

        // POST: api/v1/deals
        [HttpPost]
        [Authorize(Policy = Deals.Create)]
        public async Task<IActionResult> CreateDeal(DealDto dealDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            bool alreadyExists = await _context.Deals
                .AnyAsync(d => d.Name == dealDto.Name);

            if (alreadyExists)
                return Conflict(Result.Fail($"Deal name: {dealDto.Name} already exists."));

            var newDeal = _mapper.Map<Deal>(dealDto);
            await _context.Deals.AddAsync(newDeal);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/deals/{newDeal.Id.ToStringFromGuid()}");
                return Created(uri, Result<DealDto>.Success(
                    _mapper.Map<DealDto>(newDeal), "Deal created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/deals/5
        [HttpPut("{id}")]
        [Authorize(Policy = Deals.Edit)]
        public async Task<IActionResult> UpdateDeal(string id, DealDto dealDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (id != dealDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and deal id: {dealDto.Id}."));

            var dealInDb = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id.ToGuidFromString());

            if (dealInDb == null)
                return NotFound(Result.Fail($"Deal with id: {id} not found."));

            bool nameExists = await _context.Deals
                .AnyAsync(d => d.Name == dealDto.Name && d.Id != id.ToGuidFromString());

            if (nameExists)
                return Conflict(Result.Fail($"Deal name: {dealDto.Name} already taken."));

            _mapper.Map(dealDto, dealInDb);
            _context.Entry(dealInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<DealDto>.Success(
                    _mapper.Map<DealDto>(dealInDb), "Deal updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/deals/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Deals.Delete)]
        public async Task<IActionResult> DeleteDeal(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var dealInDb = await _context.Deals
                .Include(d => d.DealItems)
                .FirstOrDefaultAsync(d => d.Id == id.ToGuidFromString());

            if (dealInDb == null)
                return NotFound(Result.Fail($"Deal with id: {id} not found."));

            _context.Deals.Remove(dealInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Deal deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // POST: api/v1/deals/5/items
        [HttpPost("{dealId}/items")]
        [Authorize(Policy = Deals.Manage)]
        public async Task<IActionResult> AddDealItem(string dealId, DealItemDto dealItemDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == dealId.ToGuidFromString());

            if (deal == null)
                return NotFound(Result.Fail($"Deal with id: {dealId} not found."));

            var menuItem = await _context.MenuItems
                .FirstOrDefaultAsync(m => m.Id == dealItemDto.MenuItemId.ToGuidFromString());

            if (menuItem == null)
                return NotFound(Result.Fail($"Menu item with id: {dealItemDto.MenuItemId} not found."));

            var dealItem = _mapper.Map<DealItem>(dealItemDto);
            dealItem.DealId = deal.Id;
            await _context.DealItems.AddAsync(dealItem);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<DealItemDto>.Success(
                    _mapper.Map<DealItemDto>(dealItem), "Deal item added successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/deals/5/items/10
        [HttpPut("{dealId}/items/{itemId}")]
        [Authorize(Policy = Deals.Manage)]
        public async Task<IActionResult> UpdateDealItem(string dealId, string itemId, DealItemDto dealItemDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var dealItem = await _context.DealItems
                .FirstOrDefaultAsync(di => di.Id == itemId.ToGuidFromString() && di.DealId == dealId.ToGuidFromString());

            if (dealItem == null)
                return NotFound(Result.Fail($"Deal item with id: {itemId} not found in deal: {dealId}."));

            _mapper.Map(dealItemDto, dealItem);
            _context.Entry(dealItem).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<DealItemDto>.Success(
                    _mapper.Map<DealItemDto>(dealItem), "Deal item updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/deals/5/items/10
        [HttpDelete("{dealId}/items/{itemId}")]
        [Authorize(Policy = Deals.Manage)]
        public async Task<IActionResult> RemoveDealItem(string dealId, string itemId)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var dealItem = await _context.DealItems
                .FirstOrDefaultAsync(di => di.Id == itemId.ToGuidFromString() && di.DealId == dealId.ToGuidFromString());

            if (dealItem == null)
                return NotFound(Result.Fail($"Deal item with id: {itemId} not found in deal: {dealId}."));

            _context.DealItems.Remove(dealItem);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(itemId, "Deal item removed successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}
