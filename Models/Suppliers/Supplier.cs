using Erp.Models.Purchase;

namespace Erp.Models.Suppliers
{
    //**Supplier Relationship Explanation**//
    //*Supplier(1) --> (many)PurchaseOrder*//(One supplier can have many purchase orders)*//

    public class Supplier
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ContactPerson { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        // Relationships(Navigational Properties)
        public ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}