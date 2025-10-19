namespace Erp.DTOs.Sales
{
    public class SalesOrderUpdateDto
    {
        public string Status { get; set; } = null!; // e.g., Pending, Completed, Cancelled
        public string? Notes { get; set; }
    }
}
