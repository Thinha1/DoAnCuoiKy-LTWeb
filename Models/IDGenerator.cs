using System;
using System.Linq;

namespace WebBanHoa.Models
{
    public static class IDGenerator
    {
        private static QLBANHOAEntities db = new QLBANHOAEntities();

        public static string GenerateUserID()
        {
            // THỬ CÁC TÊN ENTITY KHÁC NHAU
            var lastUser = db.Users.OrderByDescending(u => u.UserID).FirstOrDefault();
            // HOẶC: var lastUser = db.User.OrderByDescending(u => u.UserID).FirstOrDefault();

            if (lastUser == null) return "US001";

            string lastID = lastUser.UserID;
            if (lastID.StartsWith("US") && lastID.Length >= 3)
            {
                string numberPart = lastID.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    return $"US{(lastNumber + 1):D3}";
                }
            }

            // Fallback nếu format không đúng
            return "US001";
        }

        public static string GenerateShoppingCartID()
        {

            var lastCart = db.ShoppingCarts.OrderByDescending(s => s.ShoppingCartID).FirstOrDefault();
            // HOẶC: var lastCart = db.ShoppingCart.OrderByDescending(s => s.ShoppingCartID).FirstOrDefault();

            if (lastCart == null) return "GH001";

            string lastID = lastCart.ShoppingCartID;
            if (lastID.StartsWith("GH") && lastID.Length >= 3)
            {
                string numberPart = lastID.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    return $"GH{(lastNumber + 1):D3}";
                }
            }

            // Fallback nếu format không đúng
            return "SC001";
        }

        public static string GenerateProductID()
        {
            var lastProduct = db.Products.OrderByDescending(p => p.ProductID).FirstOrDefault();

            if (lastProduct == null) return "SP001";

            string lastID = lastProduct.ProductID;
            if (lastID.StartsWith("SP") && lastID.Length >= 3)
            {
                string numberPart = lastID.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    return $"SP{(lastNumber + 1):D3}";
                }
            }

            return "SP001";
        }

        public static string GenerateOrderID()
        {
            var lastOrder = db.Orders.OrderByDescending(o => o.OrderID).FirstOrDefault();

            if (lastOrder == null) return "OD001";

            string lastID = lastOrder.OrderID;
            if (lastID.StartsWith("OD") && lastID.Length >= 3)
            {
                string numberPart = lastID.Substring(2);
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    return $"OD{(lastNumber + 1):D3}";
                }
            }

            return "OD001";
        }

    }
}