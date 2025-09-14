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
    public class OrdersController : TenantAwaresControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OrdersController(ApplicationDbContext context, IMapper mapper, TenantDbContext tenantContext)
            : base(tenantContext)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/v1/orders
        [HttpGet]
        [Authorize(Policy = Orders.View)]
        public async Task<IActionResult> GetOrders()
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemModifiers)
                .ThenInclude(oim => oim.ModifierOption)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.DeliveryAssignment)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(Result<List<OrderDto>>.Success(_mapper.Map<List<OrderDto>>(orders)));
        }

        // GET: api/v1/orders/5
        [HttpGet("{id}")]
        [Authorize(Policy = Orders.View)]
        public async Task<IActionResult> GetOrder(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.OrderItemModifiers)
                .ThenInclude(oim => oim.ModifierOption)
                .Include(o => o.DeliveryAddress)
                .Include(o => o.DeliveryAssignment)
                .FirstOrDefaultAsync(o => o.Id == id.ToGuidFromString());

            if (order == null)
                return NotFound(Result.Fail($"Order with id: {id} not found."));

            return Ok(Result<OrderDto>.Success(_mapper.Map<OrderDto>(order)));
        }

        // POST: api/v1/orders
        [HttpPost]
        [Authorize(Policy = Orders.Create)]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var newOrder = _mapper.Map<Order>(orderDto);
            newOrder.OrderStatus = OrderStatus.Pending;

            if (newOrder.OrderType == OrderType.Delivery)
                newOrder.DeliveryStatus = DeliveryStatus.Pending;

            await _context.Orders.AddAsync(newOrder);

            int result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var uri = new Uri($"{Request.Scheme}://{Request.Host}/api/v1/orders/{newOrder.Id.ToStringFromGuid()}");
                return Created(uri, Result<OrderDto>.Success(
                    _mapper.Map<OrderDto>(newOrder), "Order created successfully."));
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/orders/5/status
        [HttpPut("{id}/status")]
        [Authorize(Policy = Orders.Manage)]
        public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] OrderStatus status)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var orderInDb = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id.ToGuidFromString());

            if (orderInDb == null)
                return NotFound(Result.Fail($"Order with id: {id} not found."));

            orderInDb.OrderStatus = status;
            _context.Entry(orderInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<OrderDto>.Success(
                    _mapper.Map<OrderDto>(orderInDb), "Order status updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // PUT: api/v1/orders/5/delivery-status
        [HttpPut("{id}/delivery-status")]
        [Authorize(Policy = Orders.Manage)]
        public async Task<IActionResult> UpdateDeliveryStatus(string id, [FromBody] DeliveryStatus status)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var orderInDb = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id.ToGuidFromString());

            if (orderInDb == null)
                return NotFound(Result.Fail($"Order with id: {id} not found."));

            if (orderInDb.OrderType != OrderType.Delivery)
                return BadRequest(Result.Fail("Only delivery orders can have delivery status updated."));

            orderInDb.DeliveryStatus = status;
            _context.Entry(orderInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<OrderDto>.Success(
                    _mapper.Map<OrderDto>(orderInDb), "Delivery status updated successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // POST: api/v1/orders/5/assign-delivery
        [HttpPost("{id}/assign-delivery")]
        [Authorize(Policy = Orders.Manage)]
        public async Task<IActionResult> AssignDeliveryDriver(string id, [FromBody] string driverId)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var orderInDb = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id.ToGuidFromString());

            if (orderInDb == null)
                return NotFound(Result.Fail($"Order with id: {id} not found."));

            if (orderInDb.OrderType != OrderType.Delivery)
                return BadRequest(Result.Fail("Only delivery orders can be assigned to drivers."));

            var assignment = new DeliveryAssignment
            {
                OrderId = orderInDb.Id,
                DriverId = driverId.ToGuidFromString(),
                Status = DeliveryStatus.Assigned
            };

            orderInDb.DeliveryStatus = DeliveryStatus.Assigned;

            await _context.DeliveryAssignments.AddAsync(assignment);
            _context.Entry(orderInDb).State = EntityState.Modified;

            int result = await _context.SaveChangesAsync();
            if (result > 0)
                return Ok(Result<DeliveryAssignmentDto>.Success(
                    _mapper.Map<DeliveryAssignmentDto>(assignment), "Driver assigned successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }

        // DELETE: api/v1/orders/5
        [HttpDelete("{id}")]
        [Authorize(Policy = Orders.Delete)]
        public async Task<IActionResult> DeleteOrder(string id)
        {
            var tenant = GetTenant();
            if (!await ValidateTenant(tenant))
                return NotFound(Result.Fail($"Tenant not specified or invalid."));

            var orderInDb = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id.ToGuidFromString());

            if (orderInDb == null)
                return NotFound(Result.Fail($"Order with id: {id} not found."));

            if (orderInDb.OrderStatus != OrderStatus.Pending)
                return BadRequest(Result.Fail("Cannot delete order that is already in progress."));

            _context.Orders.Remove(orderInDb);
            int result = await _context.SaveChangesAsync();

            if (result > 0)
                return Ok(Result<string>.Success(id, "Order deleted successfully."));

            return StatusCode(StatusCodes.Status500InternalServerError,
                Result.Fail("Something went wrong. Please try again."));
        }
    }
}
