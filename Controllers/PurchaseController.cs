using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Erp.Data;
using Erp.DTOs.Purchase;
using Erp.Hubs;
using Erp.Models.Purchase;
using Erp.Models.Enum.PurchaseOrder;

namespace Erp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public PurchaseController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // 🟩 CREATE Purchase Order (atomic)
        [HttpPost("create")]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseOrderCreateDto request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var supplier = await _context.Suppliers.FindAsync(request.SupplierId);
                if (supplier == null)
                    return NotFound("Supplier not found.");

                var purchaseOrder = new PurchaseOrder
                {
                    SupplierId = request.SupplierId,
                    OrderDate = DateTime.UtcNow,
                    Status = PurchaseOrderStatus.Pending,
                    Items = new List<PurchaseOrderItem>() // ✅ Corrected property name and type
                };

                foreach (var item in request.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                        return NotFound($"Product with ID {item.ProductId} not found.");

                    var orderItem = new PurchaseOrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };

                    purchaseOrder.Items.Add(orderItem); // ✅ Fixed collection reference

                    // Optional: instantly update stock
                    product.StockQuantity += item.Quantity;
                    _context.Products.Update(product);
                }

                _context.PurchaseOrders.Add(purchaseOrder);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                    $"Purchase order #{purchaseOrder.Id} created for supplier {supplier.Name}.");

                return Ok(new { message = "Purchase order created successfully", purchaseOrder.Id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(new { message = "Transaction failed", error = ex.Message });
            }
        }


        // 🟨 GET All Purchase Orders
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var purchases = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                    .ThenInclude(i => i.Product)
                .Select(p => new PurchaseOrderReadDto
                {
                    Id = p.Id,
                    SupplierName = p.Supplier.Name,
                    OrderDate = p.OrderDate,
                    ReceivedDate = p.ReceivedDate,
                    Status = p.Status,
                    Items = p.Items.Select(i => new PurchaseOrderItemReadDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(purchases);
        }


        // 🟦 GET Purchase Order by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var purchase = await _context.PurchaseOrders
                .Include(p => p.Supplier)
                .Include(p => p.Items)
                .ThenInclude(i => i.Product)
                .Where(p => p.Id == id)
                .Select(p => new PurchaseOrderReadDto
                {
                    Id = p.Id,
                    SupplierName = p.Supplier.Name,
                    OrderDate = p.OrderDate,
                    ReceivedDate = p.ReceivedDate,
                    Status = p.Status,
                    Items = p.Items.Select(i => new PurchaseOrderItemReadDto
                    {
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (purchase == null)
                return NotFound("Purchase order not found.");

            return Ok(purchase);
        }

        // 🟧 UPDATE Purchase Order (status, received date, etc.)
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PurchaseOrderUpdateDto request)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(id);
            if (purchaseOrder == null)
                return NotFound("Purchase order not found.");

            if (!Enum.TryParse<PurchaseOrderStatus>(request.Status, true, out var status))
                return BadRequest("Invalid status.");

            purchaseOrder.Status = status;
            purchaseOrder.ReceivedDate = request.ReceivedDate ?? DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                $"Purchase order #{purchaseOrder.Id} updated to {purchaseOrder.Status}.");

            return Ok("Purchase order updated successfully.");
        }

        // 🟥 DELETE Purchase Order (only if Pending)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var purchaseOrder = await _context.PurchaseOrders
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchaseOrder == null)
                return NotFound("Purchase order not found.");

            if (purchaseOrder.Status != PurchaseOrderStatus.Pending)
                return BadRequest("Cannot delete a non-pending purchase order.");

            _context.PurchaseOrders.Remove(purchaseOrder);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveNotification",
                $"Purchase order #{purchaseOrder.Id} deleted.");

            return Ok("Purchase order deleted successfully.");
        }
    }
}
