using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Erp.Data;
using Erp.DTOs.Suppliers;
using Erp.Models.Suppliers;
using Erp.Hubs;
using Erp.Models.ApplicationUsers;

namespace Erp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public SuppliersController(AppDbContext context, IHubContext<NotificationHub> hubContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _userManager = userManager;
        }

        #region Get Suppliers
        // GET: api/suppliers
        [HttpGet]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAll(string? search = null, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            // Base query
            var query = _context.Suppliers.AsQueryable();

            // Optional search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(search) ||
                    s.Email.ToLower().Contains(search) ||
                    s.Phone.ToLower().Contains(search) ||
                    s.Address.ToLower().Contains(search));
            }

            // Count after filtering
            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Apply pagination
            var suppliers = await query
                .OrderBy(s => s.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierReadDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Email = s.Email,
                    Phone = s.Phone,
                    Address = s.Address
                })
                .ToListAsync();

     
            var result = new
            {
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                SearchTerm = search,
                Data = suppliers
            };

            return Ok(result);
        }

        #endregion

        #region Get Suppliers By Id
        //  GET: api/suppliers/{id}
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("Supplier not found.");

            var dto = new SupplierReadDto
            {
                Id = supplier.Id,
                Name = supplier.Name,
                Email = supplier.Email,
                Phone = supplier.Phone,
                Address = supplier.Address
            };

            return Ok(dto);
        }
        #endregion

        #region Add Suppliers
        // POST: api/suppliers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] SupplierCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var supplier = new Supplier
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address
            };

            _context.Suppliers.Add(supplier);
            await _context.SaveChangesAsync();

            // 🔔 SignalR event
            await _hubContext.Clients.All.SendAsync("SupplierAdded", new
            {
                supplier.Id,
                supplier.Name,
                supplier.Email
            });

            return CreatedAtAction(nameof(GetById), new { id = supplier.Id }, supplier);
        }
        #endregion

        #region Update Suppliers
        // PUT: api/suppliers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SupplierUpdateDto dto)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("Supplier not found.");

            supplier.Name = dto.Name;
            supplier.Email = dto.Email;
            supplier.Phone = dto.Phone;
            supplier.Address = dto.Address;

            await _context.SaveChangesAsync();

            // 🔔 Broadcast
            await _hubContext.Clients.All.SendAsync("SupplierUpdated", new
            {
                supplier.Id,
                supplier.Name,
                supplier.Email
            });

            return Ok("Supplier updated successfully.");
        }
        #endregion

        #region Delete Suppliers
        // DELETE: api/suppliers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var supplier = await _context.Suppliers.FindAsync(id);
            if (supplier == null)
                return NotFound("Supplier not found.");

            _context.Suppliers.Remove(supplier);
            await _context.SaveChangesAsync();



            // 🔔 SignalR event
            await _hubContext.Clients.All.SendAsync("SupplierDeleted", id);

            return Ok("Supplier deleted successfully.");
        }
        #endregion

    }
}
