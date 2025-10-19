namespace Erp.DTOs.Purchase
{
    public class PurchaseOrderUpdateDto
    {
        public string Status { get; set; } = null!; // e.g., Pending, Received
        public DateTime? ReceivedDate { get; set; }
    }
}
