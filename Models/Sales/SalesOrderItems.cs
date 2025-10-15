using Erp.Models.Products;
using Erp.Models.Sales;

namespace Erp.Models.Sales
{
    //**SalesOrderItems Relationship Explanation**//
    //*SalesOrderItems(many) --> (1)Product(Many SalesOrderItems belong to One Product)*//
    //*SalesOrderItems(Many) --> (1)SaleOrder(Many SalesOrderItems belong to One SaleOrder)*//
    public class SalesOrderItem
    {
        public Guid Id { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        //Relationships(foreign key)
        public Guid SalesOrderId { get; set; }
        public Guid ProductId { get; set; }


        //Relationships(Navigational Properties)
        public Product Product { get; set; }
        public SalesOrder SalesOrder { get; set; }
    }
}