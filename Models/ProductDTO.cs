using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class ProductDTO
    {
        public string ProductID { get; set; }
        public string CategoryID { get; set; }
        public string ThemeID { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> FinalPrice { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public Nullable<double> DiscountRate { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<int> TotalSold { get; set; }
    }
}