using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class GoodsReceiptNoteDTO
    {
        public string GoodsReceiptNoteID { get; set; }

        public string SupplierID { get; set; }

        public string SupplierName { get; set; }

        public DateTime ReceiptDate { get; set; }

        public int IsDeleted {  get; set; }

        public List<GoodsReceiptNoteDetailDTO> GoodsReceiptNoteDetails { get; set; }

        public decimal TotalPrice {  get; set; }
    }
}