using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WebBanHoa.Models;

namespace QLBANHOA.Models
{
    public class GioHang
    {
        
        public class ItemGioHang
        {
            public Product SanPham { get; set; }
            public int SoLuong { get; set; }
            public decimal TyLeGiamGia { get; set; }

            // Giá gốc
            public decimal GiaGoc => SanPham?.Price ?? 0;

            // Giá sau giảm
            public decimal GiaSauGiam => GiaGoc * (1 - TyLeGiamGia / 100);

            // Thành tiền
            public decimal ThanhTien => GiaSauGiam * SoLuong;

            // Số tiền được giảm
            public decimal TienGiam => (GiaGoc * TyLeGiamGia / 100) * SoLuong;
        }

        // Danh sách sản phẩm trong giỏ hàng
        private List<ItemGioHang> _items = new List<ItemGioHang>();

        // Property để truy cập items từ bên ngoài
        public List<ItemGioHang> Items => _items;

        // Tổng số lượng sản phẩm
        public int TongSoLuong => _items.Sum(x => x.SoLuong);

        // Tổng tiền hàng
        public decimal TongTien => _items.Sum(x => x.ThanhTien);

        // Tổng tiền giảm giá
        public decimal TongTienGiam => _items.Sum(x => x.TienGiam);

        // Tiền phải thanh toán (tổng tiền hàng)
        public decimal ThanhTien => TongTien;

        // Thêm sản phẩm vào giỏ hàng
        public void ThemSanPham(Product sanPham, decimal tyLeGiamGia, int soLuong = 1)
        {
            var item = _items.FirstOrDefault(x => x.SanPham.ProductID == sanPham.ProductID);

            if (item == null)
            {
                // Thêm mới
                _items.Add(new ItemGioHang
                {
                    SanPham = sanPham,
                    SoLuong = soLuong,
                    TyLeGiamGia = tyLeGiamGia
                });
            }
            else
            {
                // Cập nhật số lượng
                item.SoLuong += soLuong;
                item.TyLeGiamGia = tyLeGiamGia; // Cập nhật tỷ lệ giảm giá mới nhất
            }
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public void XoaSanPham(string maSanPham)
        {
            Debug.WriteLine($"Tìm sản phẩm với ID: {maSanPham}");
            Debug.WriteLine($"Tổng số items: {_items.Count}");

            var item = _items.FirstOrDefault(x => x.SanPham.ProductID == maSanPham);

            if (item != null)
            {
                Debug.WriteLine($"Tìm thấy sản phẩm: {item.SanPham.ProductName}");
                _items.Remove(item);
                Debug.WriteLine($"Đã xóa, số items còn: {_items.Count}");
            }
            else
            {
                Debug.WriteLine("Không tìm thấy sản phẩm");
            }
        }

        // Cập nhật số lượng sản phẩm
        public void CapNhatSoLuong(string maSanPham, int soLuong)
        {
            var item = _items.FirstOrDefault(x => x.SanPham.ProductID == maSanPham);
            if (item != null)
            {
                item.SoLuong = soLuong;
            }
        }

        // Cập nhật tỷ lệ giảm giá
        public void CapNhatGiamGia(string maSanPham, decimal tyLeGiamGia)
        {
            var item = _items.FirstOrDefault(x => x.SanPham.ProductID == maSanPham);
            if (item != null)
            {
                item.TyLeGiamGia = tyLeGiamGia;
            }
        }

        // Kiểm tra sản phẩm có trong giỏ hàng không
        public bool CoSanPham(string maSanPham)
        {
            return _items.Any(x => x.SanPham.ProductID == maSanPham);
        }

        // Lấy số lượng của một sản phẩm
        public int LaySoLuong(string maSanPham)
        {
            var item = _items.FirstOrDefault(x => x.SanPham.ProductID == maSanPham);
            return item?.SoLuong ?? 0;
        }

        // Lấy item theo mã sản phẩm
        public ItemGioHang LayItem(string maSanPham)
        {
            return _items.FirstOrDefault(x => x.SanPham.ProductID == maSanPham);
        }

        // Xóa toàn bộ giỏ hàng
        public void XoaTatCa()
        {
            _items.Clear();
        }

        // Kiểm tra giỏ hàng có trống không
        public bool Trong => _items.Count == 0;
    }
}