using Lab1.Data;
using Lab1.DTOs;
using Lab1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderDetailsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrderDetailsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderDetails()
    {
        var orderDetails = await _context.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.Order)
            .ThenInclude(o => o.User)
            .Select(od => new OrderDetailDto
            {
                OrderId = od.OrderId,
                ProductId = od.ProductId,
                Quantity = od.Quantity
            })
            .ToListAsync();

        return Ok(orderDetails);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetailById(int orderId)
    {
        var orderDetail = await _context.OrderDetails
            .Include(od => od.Product)
            .Include(od => od.Order)
            .ThenInclude(o => o.User)
            .Where(od => od.OrderId == orderId)
            .Select(od => new OrderDetailDto
            {
                OrderId = od.OrderId,
                ProductId = od.ProductId,
                Quantity = od.Quantity
            })
            .ToListAsync();

        if (orderDetail == null)
            return NotFound($"Order detail with ID {orderId} not found.");

        return Ok(orderDetail);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderDetail([FromBody] CreateOrderDetailDto dto)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == dto.OrderId);

        if (order == null)
            return BadRequest($"Order with Id {dto.OrderId} does not exist.");

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

        if (product == null)
            return BadRequest($"Product with Id {dto.ProductId} does not exist.");

        var newOrderDetail = new OrderDetail
        {
            OrderId = order.Id,
            ProductId = product.Id,
            Quantity = dto.Quantity
        };

        _context.OrderDetails.Add(newOrderDetail);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Order detail created successfully." });
    }


    [HttpPut("{orderId}/{productId}")]
    public async Task<IActionResult> UpdateOrderDetail(int orderId, int productId, [FromBody] UpdateOrderDetailDto dto)
    {
        if (orderId != dto.OrderId || productId != dto.ProductId)
            return BadRequest("Order or Product ID in the URL does not match the request body.");

        var existingDetail = await _context.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);

        if (existingDetail == null)
            return NotFound($"Order detail for OrderId {orderId} and ProductId {productId} not found.");

        existingDetail.Quantity = dto.Quantity;

        _context.Entry(existingDetail).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{orderId}/{productId}")]
    public async Task<IActionResult> DeleteOrderDetail(int orderId, int productId)
    {
        var detail = await _context.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);

        if (detail == null)
            return NotFound($"Order detail for OrderId {orderId} and ProductId {productId} not found.");

        _context.OrderDetails.Remove(detail);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
