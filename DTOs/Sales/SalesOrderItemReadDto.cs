namespace Erp.DTOs.Sales
{
    public class SalesOrderItemReadDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}