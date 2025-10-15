using Erp.Models.Products;


namespace Erp.Models.Purchase
{

    //**PurchaseOrderItems Relationship Explanation**//
    //*PurchaseOrderItems(many) --> (1)Product(Many PurchaseOrderItems belong to One Product)*//
    //*PurchaseOrderItems(Many) --> (1)PurchaseOrder(Many PurchaseOrdersItems belong to One PurchaseOrder)*//
    public class PurchaseOrderItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        //Relationships(foreign key)
        public Guid PurchaseOrderId { get; set; }
        public Guid ProductId { get; set; }

        //Relationships(Navigational Properties)
        public Product Product { get; set; }
        public PurchaseOrder PurchaseOrder { get; set; }
    }

}