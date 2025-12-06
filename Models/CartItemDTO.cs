namespace WebBanHoa.Models
{
    public class CartItemDTO
    {
        public string ProductID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountRate { get; set; }
        public int? Quantity { get; set; }
        public string Image { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}