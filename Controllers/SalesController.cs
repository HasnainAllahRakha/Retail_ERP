using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Erp.Data;
using Erp.DTOs.Sales;
using Erp.Hubs;
using Erp.Models.Sales;
using Erp.Models.ApplicationUsers;
using Erp.Models.Enum.SaleOrder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace Erp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public SalesController(AppDbContext context, IHubContext<NotificationHub> hubContext, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        // 🟩 CREATE Sale Order (atomic)
        [HttpPost("create")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSale([FromBody] SalesOrderCreateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (request.Items == null || !request.Items.Any())
                    return BadRequest("No sale items provided.");

                var salesOrder = new SalesOrder
                {
                    OrderDate = DateTime.UtcNow,
                    Status = SalesOrderStatus.Pending,
                    Items = new List<SalesOrderItem>(), // ✅ Corrected
                    CreatedById = _userManager.GetUserId(User), // ✅ Set creator if using Identity
                                                                // CreatedByName = User?.Identity?.Name ?? "System" // Only if property exists
                };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        return NotFound($"Product with ID {item.ProductId} not found.");

                    if (product.StockQuantity < item.Quantity)
                        return BadRequest($"Insufficient stock for product: {product.Name}.");

                    product.StockQuantity -= item.Quantity;
                    _context.Products.Update(product);

                    var orderItem = new SalesOrderItem
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = product.Price
                    };

                    salesOrder.Items.Add(orderItem); // ✅ Corrected
                }

                // Optional: only if property exists
                // salesOrder.TotalAmount = salesOrder.Items.Sum(i => i.Quantity * i.UnitPrice);

                _context.SalesOrders.Add(salesOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                    $"New sales order #{salesOrder.Id} created successfully.");

                return Ok(new { message = "Sales order created successfully", salesOrder.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Transaction failed", error = ex.Message });
            }
        }


        // 🟨 GET All Sales Orders
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var salesOrders = await _context.SalesOrders
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Select(s => new SalesOrderReadDto
                {
                    Id = s.Id,
                    OrderDate = s.OrderDate,
                    Status = s.Status,
                    // CreatedByName = s.CreatedByName,
                    Items = s.Items.Select(i => new SalesOrderItemReadDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(salesOrders);
        }

        // 🟦 GET Sale by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sale = await _context.SalesOrders
                .Include(s => s.Items)
                .ThenInclude(i => i.Product)
                .Where(s => s.Id == id)
                .Select(s => new SalesOrderReadDto
                {
                    Id = s.Id,
                    OrderDate = s.OrderDate,
                    Status = s.Status,
                    // CreatedByName = s.CreatedByName,
                    Items = s.Items.Select(i => new SalesOrderItemReadDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (sale == null)
                return NotFound("Sales order not found.");

            return Ok(sale);
        }

        // 🟧 UPDATE Sale Order (status, notes)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SalesOrderUpdateDto request)
        {
            var salesOrder = await _context.SalesOrders.FindAsync(id);
            if (salesOrder == null)
                return NotFound("Sales order not found.");

            if (!Enum.TryParse<SalesOrderStatus>(request.Status, true, out var status))
                return BadRequest("Invalid status.");

            salesOrder.Status = status;


            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                $"Sales order #{salesOrder.Id} updated to {salesOrder.Status}.");

            return Ok("Sales order updated successfully.");
        }

        // 🟥 DELETE Sale Order (only if Pending)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var saleOrder = await _context.SalesOrders
                .Include(s => s.Items)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (saleOrder == null)
                return NotFound("Sales order not found.");

            if (saleOrder.Status != SalesOrderStatus.Pending)
                return BadRequest("Cannot delete a non-pending sales order.");

            // Restore stock
            foreach (var item in saleOrder.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    _context.Products.Update(product);
                }
            }

            _context.SalesOrders.Remove(saleOrder);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                $"Sales order #{saleOrder.Id} deleted.");

            return Ok("Sales order deleted successfully.");
        }
    }
}
