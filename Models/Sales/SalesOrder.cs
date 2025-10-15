using Erp.Models.ApplicationUsers;
using Erp.Models.Enum.SaleOrder;

namespace Erp.Models.Sales
{

    //**SalesOrder Relationship Explanation**//
    //*SalesOrder(many) --> (1)ApplicationUser(Each SalesOrder is created by exactly one ApplicationUser)*//
    //*SalesOrder(1) --> (many)SalesOrderItems(Each sale order has multiple items)*//

    public class SalesOrder
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        public Guid CreatedById { get; set; }

        //Enums
        public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Pending;

        //Relationships(foreign key)
        public ApplicationUser CreatedBy { get; set; }

        //Relationships(Navigational Properties)
        public ICollection<SalesOrderItem> Items { get; set; }
    }
}