namespace Erp.DTOs.Purchase
{


    public class PurchaseOrderItemReadDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}