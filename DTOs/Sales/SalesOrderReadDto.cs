using Erp.Models.Enum.SaleOrder;

namespace Erp.DTOs.Sales
{
    public class SalesOrderReadDto
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; }
        public SalesOrderStatus Status { get; set; }
        public string CreatedByName { get; set; }
        public List<SalesOrderItemReadDto> Items { get; set; }
    }
}