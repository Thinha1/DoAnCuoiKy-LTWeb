using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class GoodsReceiptNoteDetailDTO
    {

        public string GoodsReceiptNoteID {  get; set; }

        public string ProductID { get; set; }

        public string ProductName { get; set; }

        public string Image {  get; set; }

        public decimal UnitPrice {  get; set; }

        public int Quantity {  get; set; }
    }
}