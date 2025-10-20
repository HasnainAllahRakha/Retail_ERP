namespace Erp.DTOs.Purchase
{
    public class PurchaseOrderItemUpdateDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
