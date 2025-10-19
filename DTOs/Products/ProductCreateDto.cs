namespace Erp.DTOs.Products
{
    public class ProductCreateDto
    {
        public string Name { get; set; }
        public string SKU { get; set; }
        public string Description { get; set; }

        public int StockQuantity { get; set; }

        public decimal Price { get; set; }
        public int ReorderLevel { get; set; }
    }
}