using Lab1.Data;
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
    public async Task<IActionResult> GetAllOrderDetails()
    {
        var details = await _context.OrderDetails
            .Include(od => od.Order)
            .Include(od => od.Product)
            .ToListAsync();
        return Ok(details);
    }

    [HttpGet("by-order/{orderId}")]
    public async Task<IActionResult> GetOrderDetailsByOrderId(int orderId)
    {
        var details = await _context.OrderDetails
            .Include(od => od.Product)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();

        if (!details.Any())
            return NotFound($"No order details found for OrderId {orderId}");

        return Ok(details);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrderDetail(OrderDetail detail)
    {
        var order = await _context.Orders.FindAsync(detail.OrderId);
        var product = await _context.Products.FindAsync(detail.ProductId);

        if (order == null || product == null)
            return BadRequest("Invalid OrderId or ProductId");

        _context.OrderDetails.Add(detail);
        await _context.SaveChangesAsync();
        return Ok(detail);
    }

    [HttpPut("{orderId}/{productId}")]
    public async Task<IActionResult> UpdateOrderDetail(int orderId, int productId, OrderDetail detail)
    {
        if (orderId != detail.OrderId || productId != detail.ProductId)
            return BadRequest();

        var existingDetail = await _context.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);

        if (existingDetail == null)
            return NotFound();

        existingDetail.Quantity = detail.Quantity;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{orderId}/{productId}")]
    public async Task<IActionResult> DeleteOrderDetail(int orderId, int productId)
    {
        var detail = await _context.OrderDetails
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);

        if (detail == null)
            return NotFound();

        _context.OrderDetails.Remove(detail);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
