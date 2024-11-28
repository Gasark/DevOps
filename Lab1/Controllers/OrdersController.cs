using Lab1.Data;
using Lab1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab1.DTOs;

namespace Lab1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _context.Orders
        .Include(o => o.User) // Підтягуємо користувача
        .Include(o => o.OrderDetails)
        .ThenInclude(od => od.Product) // Підтягуємо продукти
        .Select(o => new OrderDto
        {
            Id = o.Id,
            OrderDate = o.OrderDate,
            UserId = o.User.Id,
            //OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
            //{
            //    ProductId = od.ProductId,
            //    Quantity = od.Quantity
            //}).ToList()
        })
        .ToListAsync();

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .Where(o => o.Id == id)
            .Select(o => new OrderDto
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                UserId = o.User.Id,
            })
            .FirstOrDefaultAsync();

        if (order == null) return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] OrderCreateDto orderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = new Order
        {
            OrderDate = orderDto.OrderDate,
            UserId = orderDto.UserId
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, new OrderDto
        {
            Id = order.Id,
            OrderDate = order.OrderDate,
            UserId = order.UserId
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateOrder(int id, [FromBody] OrderUpdateDto orderDto)
    {
        if (id != orderDto.Id)
            return BadRequest("Order ID in the URL does not match the ID in the request body.");

        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        order.OrderDate = orderDto.OrderDate;
        order.UserId = orderDto.UserId;

        _context.Entry(order).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return NotFound();

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
