using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class DiscountDTO
    {
        public string DiscountID { get; set; }
        [Required]
        public string ProductID { get; set; }
        public string DiscountName { get; set; }
        public string ProductName {  get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public double DiscountRate { get; set; }
    }
}