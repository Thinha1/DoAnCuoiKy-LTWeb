using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models.Payment
{
    public class SePayModel
    {
        public long id { get; set; } // ID giao dịch trên SePay
        public string gateway { get; set; } // Ngân hàng
        public string transactionDate { get; set; }
        public string accountNumber { get; set; }
        public string content { get; set; } // Nội dung CK (Quan trọng: chứa mã đơn)
        public decimal transferAmount { get; set; } // Số tiền
        public string referenceCode { get; set; } // Mã tham chiếu ngân hàng
    }
}