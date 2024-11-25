namespace Lab1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }

        // Navigation property
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}