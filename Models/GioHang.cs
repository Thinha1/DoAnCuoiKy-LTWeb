using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanHoa.Models
{
    public class GioHang
    {
        private List<GioHangItem> items = new List<GioHangItem>();

        public List<GioHangItem> Items => items;

        public int TongSoLuong => items.Sum(x => x.SoLuong);

        public decimal TongTien => items.Sum(x => x.ThanhTien);

        public void ThemSanPham(Product sanPham, int soLuong = 1)
        {
            var item = items.FirstOrDefault(x => x.SanPham.ProductID == sanPham.ProductID);
            if (item == null)
            {
                items.Add(new GioHangItem
                {
                    SanPham = sanPham,
                    SoLuong = soLuong
                });
            }
            else
            {
                item.SoLuong += soLuong;
            }
        }

        public void XoaSanPham(string productId)
        {
            var item = items.FirstOrDefault(x => x.SanPham.ProductID == productId);
            if (item != null)
            {
                items.Remove(item);
            }
        }

        public void CapNhatSoLuong(string productId, int soLuong)
        {
            var item = items.FirstOrDefault(x => x.SanPham.ProductID == productId);
            if (item != null)
            {
                item.SoLuong = soLuong;
            }
        }

        public void XoaTatCa()
        {
            items.Clear();
        }

        public bool ContainsProduct(string productId)
        {
            return items.Any(x => x.SanPham.ProductID == productId);
        }

        public int GetSoLuong(string productId)
        {
            var item = items.FirstOrDefault(x => x.SanPham.ProductID == productId);
            return item?.SoLuong ?? 0;
        }
    }

    public class GioHangItem
    {
        public Product SanPham { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien => (SanPham.Price ?? 0) * SoLuong;
    }
}