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
using System;
using static BiteBoard.Data.Constants.Permissions;

namespace BiteBoard.Api.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class CategoriesController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CategoriesController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        [Authorize(Policy = Categories.View)]
        public async Task<IActionResult> GetCategories()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var categories = await _context.Categories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return Ok(Result<List<CategoryDto>>.Success(_mapper.Map<List<CategoryDto>>(categories)));
        }

        [HttpGet("{id}")]
        [Authorize(Policy = Categories.View)]
        public async Task<IActionResult> GetCategory(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id.ToGuidFromString());

            if (category == null)
                return NotFound(Result.Fail($"Category with id: {id} not found."));

            return Ok(Result<CategoryDto>.Success(_mapper.Map<CategoryDto>(category)));
        }

        [HttpPost]
        [Authorize(Policy = Categories.Create)]
        public async Task<IActionResult> CreateCategory(CategoryDto categoryDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            bool alreadyExists = await _context.Categories
                .AnyAsync(c => c.Name == categoryDto.Name);

            if (alreadyExists)
                return Conflict(Result.Fail($"Category name: {categoryDto.Name} already exists."));

            var newCategory = _mapper.Map<Category>(categoryDto);
            await _context.Categories.AddAsync(newCategory);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/categories/{newCategory.Id.ToStringFromGuid()}");
                return Created(uri, Result<CategoryDto>.Success(
                    _mapper.Map<CategoryDto>(newCategory), "Category created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        [HttpPut("{id}")]
        [Authorize(Policy = Categories.Edit)]
        public async Task<IActionResult> UpdateCategory(string id, CategoryDto categoryDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            if (id != categoryDto.Id)
                return BadRequest(Result.Fail($"Invalid id: {id} and category id: {categoryDto.Id}."));

            var categoryInDb = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id.ToGuidFromString());

            if (categoryInDb == null)
                return NotFound(Result.Fail($"Category with id: {id} not found."));

            bool nameExists = await _context.Categories
                .AnyAsync(c => c.Name == categoryDto.Name && c.Id != id.ToGuidFromString());

            if (nameExists)
                return Conflict(Result.Fail($"Category name: {categoryDto.Name} already taken."));

            _mapper.Map(categoryDto, categoryInDb);
            _context.Entry(categoryInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<CategoryDto>.Success(
                    _mapper.Map<CategoryDto>(categoryInDb), "Category updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = Categories.Delete)]
        public async Task<IActionResult> DeleteCategory(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var categoryInDb = await _context.Categories
                .Include(c => c.MenuItems)
                .FirstOrDefaultAsync(c => c.Id == id.ToGuidFromString());

            if (categoryInDb == null)
                return NotFound(Result.Fail($"Category with id: {id} not found."));

            if (categoryInDb.MenuItems?.Any() == true)
                return BadRequest(Result.Fail("Cannot delete category that has menu items."));

            _context.Categories.Remove(categoryInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Category deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}
