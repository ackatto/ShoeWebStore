namespace ShoeWebStore.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ManufacturerId { get; set; }
        public string ManufacturerName { get; set; }
        public int SupplierId { get; set; }

        public string SupplierName { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public decimal Discount { get; set; }

        public string ImagePath { get; set; }
    }
}
