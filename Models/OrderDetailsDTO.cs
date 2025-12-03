using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class OrderDetailsDTO
    {
        public string OrderID { get; set; }
        public string ProductID { get; set; }
        public string ProductName { get; set; }

        public string Image { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
    }
}