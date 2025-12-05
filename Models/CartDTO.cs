using System.Collections.Generic;

namespace WebBanHoa.Models
{
    public class CartDTO
    {
        public string ShoppingCartID { get; set; }
        public string UserID { get; set; }
        public List<CartItemDTO> Items { get; set; } = new List<CartItemDTO>();
    }
}