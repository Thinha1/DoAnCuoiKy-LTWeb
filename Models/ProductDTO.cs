using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanHoa.Models
{
    public class ProductDTO
    {
        public string ProductID { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public string CategoryID { get; set; }
        public string CategoryName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn chủ đề")]
        public string ThemeID { get; set; }
        public string ThemeName { get; set; }
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        public string ProductName { get; set; }
        [Required(ErrorMessage = "Giá bán là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> FinalPrice { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string Image { get; set; }
        public Nullable<double> DiscountRate { get; set; }
        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        public Nullable<int> Quantity { get; set; }

        public Nullable<int> IsAvailable { get; set; }
        public Nullable<int> TotalSold { get; set; }

        public Nullable<decimal> TotalRevenue { get; set; }
    }
}