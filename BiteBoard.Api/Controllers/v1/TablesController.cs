using Asp.Versioning;
using AutoMapper;
using BiteBoard.Api.DTOs.Requests;
using BiteBoard.API.Controllers.v1;
using BiteBoard.Data.Contexts;
using BiteBoard.Data.Entities;
using BiteBoard.Data.Enums;
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
    public class TablesController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TablesController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/v1/tables
        [HttpGet]
        [Authorize(Policy = Tables.View)]
        public async Task<IActionResult> GetTables()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var tables = await _context.Tables
                .OrderBy(t => t.TableNumber)
                .ToListAsync();

            return Ok(Result<List<TableDto>>.Success(_mapper.Map<List<TableDto>>(tables)));
        }

        // GET: api/v1/tables/5
        [HttpGet("{id}")]
        [Authorize(Policy = Tables.View)]
        public async Task<IActionResult> GetTable(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id.ToGuidFromString());

            if (table == null)
                return NotFound(Result.Fail($"Table with id: {id} not found."));

            return Ok(Result<TableDto>.Success(_mapper.Map<TableDto>(table)));
        }

        // POST: api/v1/tables
        [HttpPost]
        [Authorize(Policy = Tables.Create)]
        public async Task<IActionResult> CreateTable(TableDto tableDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            bool alreadyExists = await _context.Tables
                .AnyAsync(t => t.TableNumber == tableDto.TableNumber);

            if (alreadyExists)
                return Conflict(Result.Fail($"Table number: {tableDto.TableNumber} already exists."));

            var newTable = _mapper.Map<Table>(tableDto);
            await _context.Tables.AddAsync(newTable);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/tables/{newTable.Id.ToStringFromGuid()}");
                return Created(uri, Result<TableDto>.Success(
                    _mapper.Map<TableDto>(newTable), "Table created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/tables/5
        [HttpPut("{id}")]
        [Authorize(Policy = Tables.Edit)]
        public async Task<IActionResult> UpdateTable(string id, TableDto tableDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (id != tableDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and table id: {tableDto.Id}."));

            var tableInDb = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id.ToGuidFromString());

            if (tableInDb == null)
                return NotFound(Result.Fail($"Table with id: {id} not found."));

            bool numberExists = await _context.Tables
                .AnyAsync(t => t.TableNumber == tableDto.TableNumber && t.Id != id.ToGuidFromString());

            if (numberExists)
                return Conflict(Result.Fail($"Table number: {tableDto.TableNumber} already taken."));

            _mapper.Map(tableDto, tableInDb);
            _context.Entry(tableInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<TableDto>.Success(
                    _mapper.Map<TableDto>(tableInDb), "Table updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/tables/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Tables.Delete)]
        public async Task<IActionResult> DeleteTable(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var tableInDb = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id.ToGuidFromString());

            if (tableInDb == null)
                return NotFound(Result.Fail($"Table with id: {id} not found."));

            _context.Tables.Remove(tableInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Table deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/tables/5/status
        [HttpPut("{id}/status")]
        [Authorize(Policy = Tables.Manage)]
        public async Task<IActionResult> UpdateTableStatus(string id, [FromBody] TableStatus status)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var tableInDb = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == id.ToGuidFromString());

            if (tableInDb == null)
                return NotFound(Result.Fail($"Table with id: {id} not found."));

            tableInDb.Status = status;
            _context.Entry(tableInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<TableDto>.Success(
                    _mapper.Map<TableDto>(tableInDb), "Table status updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}
