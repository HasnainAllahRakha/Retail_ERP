namespace Erp.DTOs.Purchase
{
    public class PurchaseOrderCreateDto
    {
        public Guid SupplierId { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; }
    }
}