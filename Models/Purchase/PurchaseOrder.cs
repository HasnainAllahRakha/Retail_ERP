using Erp.Models.Enum.PurchaseOrder;
using Erp.Models.Suppliers;

namespace Erp.Models.Purchase
{
    //**PurchaseOrder Relationship Explanation**//
    //*PurchaseOrder(1) --> (many)PurchaseOrderItems(Each purchase order has multiple items)*//
    //*PurchaseOrder(Many) --> (1)Supplier(Many PurchaseOrders belong to One Supplier)*//
    public class PurchaseOrder
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ReceivedDate { get; set; }
        
        //Enum types
        public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Pending;

        //Relationships(foreign key)
        public Guid SupplierId { get; set; }

        //Relationships(Navigational Properties)
        public Supplier Supplier { get; set; }
        public ICollection<PurchaseOrderItem> Items { get; set; }
    }
}
