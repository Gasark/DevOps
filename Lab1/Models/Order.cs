namespace Lab1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }

        // Foreign key
        public int UserId { get; set; }

        // Navigation properties
        public User User { get; set; } = null!;
        public List<OrderDetail> OrderDetails { get; set; } = new();
    }
}
