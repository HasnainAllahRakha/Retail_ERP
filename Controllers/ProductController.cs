using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Erp.Data;
using Erp.DTOs.Products;
using Erp.Models.Products;
using Erp.Hubs;

namespace Erp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ProductsController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // ✅ GET: api/products
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .Select(p => new ProductReadDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    SKU = p.SKU,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity,
                    ReorderLevel = p.ReorderLevel
                })
                .ToListAsync();

            return Ok(products);
        }

        // ✅ GET: api/products/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            var dto = new ProductReadDto
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ReorderLevel = product.ReorderLevel
            };

            return Ok(dto);
        }

        // ✅ POST: api/products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name = dto.Name,
                SKU = dto.SKU,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                ReorderLevel = dto.ReorderLevel
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 🔔 Send SignalR update to all connected clients
            await _hubContext.Clients.All.SendAsync("ProductAdded", new
            {
                product.Id,
                product.Name,
                product.Price,
                product.StockQuantity
            });

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // ✅ PUT: api/products/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            product.Name = dto.Name;
            product.SKU = dto.SKU;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.StockQuantity = dto.StockQuantity;
            product.ReorderLevel = dto.ReorderLevel;

            await _context.SaveChangesAsync();

            // 🔔 Broadcast update
            await _hubContext.Clients.All.SendAsync("ProductUpdated", new
            {
                product.Id,
                product.Name,
                product.StockQuantity
            });

            return Ok("Product updated successfully.");
        }

        // ✅ DELETE: api/products/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound("Product not found.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            // 🔔 Notify via SignalR
            await _hubContext.Clients.All.SendAsync("ProductDeleted", id);

            return Ok("Product deleted successfully.");
        }
    }
}
