using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class OrderDTO
    {
        public string OrderID { get; set; }

        public string CustomerName { get; set; }

        public string Address {  get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        public string Status { get; set; }
    }
}