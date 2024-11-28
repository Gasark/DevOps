using Lab1.Data;
using Lab1.DTOs;
using Lab1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lab1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var products = await _context.Products
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .ToListAsync();

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(int id)
    {
        var product = await _context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price
            })
            .FirstOrDefaultAsync();

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] ProductDto productDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            Name = productDto.Name,
            Price = productDto.Price
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price
        });
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto productDto)
    {
        if (id != productDto.Id)
            return BadRequest("Product ID in the URL does not match the ID in the request body.");

        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        product.Name = productDto.Name;
        product.Price = productDto.Price;

        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}