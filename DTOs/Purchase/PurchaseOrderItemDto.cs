namespace Erp.DTOs.Purchase
{
    public class PurchaseOrderItemDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}