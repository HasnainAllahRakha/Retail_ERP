using Erp.Models.Purchase;
using Erp.Models.Sales;

namespace Erp.Models.Products
{
    //**Product Relationship Explanation**//
    //*Product dosent show product details but also the stock of itself(Inventory)*//
    //*Product(1) --> (many)PurchaseOrderItems(One product can appear in many purchase order items)*//
    //*Product(1) --> (many)SalesOrderItems(One product can appear in many sale order items)*//

    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string SKU { get; set; }   // Stock Keeping Unit
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int ReorderLevel { get; set; } = 10; // threshold
        public bool IsActive { get; set; } = true;
    
        //Relationships(Navigational Properties)
        public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; }
        public ICollection<SalesOrderItem> SalesOrderItems { get; set; }
    }

}
