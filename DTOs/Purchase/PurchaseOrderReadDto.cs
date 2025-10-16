using Erp.Models.Enum.PurchaseOrder;

namespace Erp.DTOs.Purchase
{
    public class PurchaseOrderReadDto
    {
        public Guid Id { get; set; }
        public string SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public List<PurchaseOrderItemReadDto> Items { get; set; }
    }
}