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


        #region Get Products
        //      GET: api/products?pageNumber=1&pageSize=10
        //          api/products?search=keyboard
        //          api/products?search=12345&pageNumber=2
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll(string? search = null, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Start with base query
            var query = _context.Products.AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    p.SKU.ToLower().Contains(search));
            }

            // Count after filtering
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Apply pagination
            var products = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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

            var result = new
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                SearchTerm = search,
                Data = products
            };

            return Ok(result);
        }
        #endregion


        #region Get Products By Id
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
        #endregion

        #region Add Products
        // POST: api/products
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
        #endregion

        #region Update Products
        // PUT: api/products/{id}
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
        #endregion

        #region Delete Product
        //  DELETE: api/products/{id}
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
        #endregion
    }

}
