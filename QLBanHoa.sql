DROP DATABASE QLBANHOA;
CREATE DATABASE QLBANHOA;
USE QLBANHOA;

CREATE TABLE Role
(
	RoleID nchar(10),
	RoleName nvarchar(30),
	CreatedAt datetime,
	CreatedBy nvarchar(30),
	UpdatedAt datetime,
	UpdatedBy nvarchar(30),
	constraint PK_Role primary key(RoleID)
)

CREATE TABLE Users
(
	UserID nchar(10),
	RoleID nchar(10),
	Name nvarchar(100),
	Email varchar(50),
	Password nvarchar(128),
	Gender nvarchar(10),
	Address nvarchar(100),
	Avatar nvarchar(50),
	CreatedAt datetime,
	CreatedBy nvarchar(30),
	UpdatedAt datetime,
	UpdatedBy nvarchar(30),
	constraint PK_User primary key(UserID),
	constraint FK_Users_RoleID foreign key(RoleID) references Role(RoleID),
)

CREATE TABLE Category
(
	CategoryID nchar(10),
	CategoryName nvarchar(50),
	ParentCategoryID nchar(10)
	constraint PK_Category primary key(CategoryID),
	constraint FK_Category_Parent foreign key(ParentCategoryID) REFERENCES Category(CategoryID)
)
GO
-- Bảng Theme (Chủ đề) - Giữ nguyên
CREATE TABLE Theme
(
	ThemeID nchar(10),
	ThemeName nvarchar(50),
	ParentThemeID nchar(10)
	constraint PK_Theme primary key(ThemeID),
	constraint FK_Theme_Parent foreign key(ParentThemeID) REFERENCES Theme(ThemeID)
)
GO
-- BẢNG PRODUCT (ĐÃ CẬP NHẬT) --
CREATE TABLE Product
(
	ProductID nchar(10),
	CategoryID nchar(10), -- Loại hoa (Vd: Hoa Hồng)
	ThemeID nchar(10),    -- Chủ đề chính (Vd: Hoa Sinh Nhật)
	ProductName nvarchar(30),
	Price money,
	Description nvarchar(1000),
	Image nvarchar(100),
	Quantity int,
	CreatedAt datetime,
	CreatedBy nvarchar(30),
	UpdatedAt datetime,
	UpdatedBy nvarchar(30),
	constraint PK_Product primary key(ProductID),
	constraint FK_Product_Category FOREIGN KEY(CategoryID) REFERENCES Category(CategoryID),
	constraint FK_Product_Theme FOREIGN KEY(ThemeID) REFERENCES Theme(ThemeID) -- Thêm khoá ngoại mới
)

CREATE TABLE Discount
(
	DiscountID nchar(10),
	ProductID nchar(10),
	DiscountName nvarchar(50),
	StartDate datetime,
	EndDate datetime,
	DiscountRate float,
	CreatedAt datetime,
	CreatedBy nvarchar(30),
	UpdatedAt datetime,
	UpdatedBy nvarchar(30),
	constraint PK_Discount primary key(DiscountID),
	constraint FK_Discount_Product foreign key(ProductID) references Product(ProductID)
)

CREATE TABLE Orders
(
	OrderID nchar(10),
	UserID nchar(10),
	OrderDate datetime,
	Address nvarchar(100),
	Status nvarchar(20),
	UserPaymentMethod nvarchar(30),
	CreatedAt datetime,
	CreatedBy nvarchar(30),
	UpdatedAt datetime,
	UpdatedBy nvarchar(30),
	constraint PK_Order primary key(OrderID),
	constraint FK_Orders_UserID foreign key(UserID) references Users(UserID),
)
CREATE TABLE OrderDetail
(
	OrderID nchar(10),
	ProductID nchar(10),
	Quantity int,
	UnitPrice money,
	constraint PK_OrderDetail primary key(OrderID, ProductID),
	constraint FK_OrderDetail_OrderID foreign key(OrderID) references Orders(OrderID),
	constraint FK_OrderDetail_ProductID foreign key(ProductID) references Product(ProductID)
)

CREATE TABLE ShoppingCart
(
	ShoppingCartID nchar(10),
	UserID nchar(10),
	constraint PK_ShoppingCart primary key(ShoppingCartID),
	constraint FK_ShoppingCart_UserID foreign key(UserID) references Users(UserID)
)

CREATE TABLE ShoppingCartItem
(
	ShoppingCartID nchar(10),
	ProductID nchar(10),
	Quantity int,
	constraint PK_ShoppingCartItem primary key(ShoppingCartID, ProductID),
	constraint FK_ShoppingCartItem_ShoppingCartID foreign key(ShoppingCartID) references ShoppingCart(ShoppingCartID),
	constraint FK_ShoppingCartItem_ProductID foreign key(ProductID) references Product(ProductID)
)
INSERT INTO Role
VALUES
('R001', 'Admin', NULL, NULL, NULL, NULL),
('R002', N'Khách hàng', NULL, NULL, NULL, NULL)

INSERT INTO Users VALUES
('US001', 'R001', N'Trần Văn Đại', 'daitran001@gmail.com', 'vandai123!', N'Nam', N'TP. Hồ Chí Minh', NULL, NULL, NULL, NULL, 1),
('US002', 'R001', N'Nguyễn Thảo Linh', 'nguyenlinh002@gmail.com', 'thaolinh123!', N'Nữ', N'Tây Ninh', NULL, NULL, NULL, NULL, 1),
('US003', 'R001', N'Nguyễn Minh Anh', 'anhnguyen003@gmail.com', 'minhanh123!', N'Nữ', N'TP. Hồ Chí Minh', NULL, NULL, NULL, NULL, 1),
('US004', 'R001', N'Lý Quốc Phong', 'phongly004@gmail.com', 'quocphong123!', N'Nam', N'An Giang', NULL, NULL, NULL, NULL, 1),
('US005', 'R002', N'Đặng Anh Thịnh', 'admin@gmail.com', '123', N'Nam', N'TP. Hồ Chí Minh', NULL, NULL, NULL, NULL, 1)

INSERT INTO Theme VALUES
('TH001', N'Hoa sinh nhật', NULL),
('TH002', N'Hoa sinh nhật sang trọng', 'TH001'),
('TH003', N'Hoa sinh nhật mẹ', 'TH001'),
('TH004', N'Hoa khai trương', NULL),
('TH005', N'Hoa khai trương để bàn', 'TH004'),
('TH006', N'Kệ hoa khai trương', 'TH004'),
('TH007', N'Kệ hoa khai trương hiện đại', 'TH004')
GO
INSERT INTO Category VALUES
('CA001', N'Hoa tươi', NULL),
('CA002', N'Lan hồ điệp', NULL),
('CA003', N'Lan hồ điệp mini', 'CA002'),
('CA004', N'Lan hồ điệp tím', 'CA002'),
('CA005', N'Hoa Hồng', 'CA001'),
('CA006', N'Hoa Baby', 'CA001'),
('CA007', N'Hoa hướng dương', 'CA001'),
('CA008', N'Hoa tulip', 'CA001')
GO

-- DỮ LIỆU PRODUCT (ĐÃ CẬP NHẬT) --
-- Giờ mỗi sản phẩm sẽ có (CategoryID, ThemeID) dựa trên dữ liệu gốc
SET DATEFORMAT DMY
-- Cột 2 là CategoryID, Cột 3 là ThemeID
INSERT INTO Product VALUES
('SP001', 'CA005', 'TH002', N'Say ánh mắt', 190000, NULL, 'img_SP001.jpg', 10, NULL, NULL, NULL, NULL),
('SP002', 'CA005', 'TH002', N'Nồng Nàn Tình Yêu',700000, 'img_SP002.jpg', NULL, 50, NULL, NULL, NULL, NULL),
('SP003', 'CA006', 'TH002', N'Mong Manh', 730000, NULL, 'img_SP003.jpg', 20, NULL, NULL, NULL, NULL),
('SP004', 'CA006', NULL, N'Kẹo Bông Gòn',  830000, NULL, 'img_SP004.jpg', 10, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Tươi' -> Chỉ có Category, ko có Theme
('SP005', 'CA005', 'TH003', N'Sound of Love', 1150000, NULL, 'img_SP005.jpg', 10, NULL, NULL, NULL, NULL),
('SP006', 'CA007', NULL, N'Lời Cảm Ơn',  810000, NULL, 'img_SP006.jpg', 10, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Tươi' -> Chỉ có Category, ko có Theme
('SP007', 'CA007', 'TH004', N'May Mắn', 950000, NULL, 'img_SP007.jpg', 10, NULL, NULL, NULL, NULL),
('SP008', 'CA007', 'TH004', N'Tỏa Nắng', 1250000, NULL, 'img_SP008.jpg', 15, NULL, NULL, NULL, NULL),
('SP009', 'CA007', 'TH004', N'Sắc Vàng', 890000, NULL, 'img_SP009.jpg', 20, NULL, NULL, NULL, NULL),
('SP010', 'CA001', 'TH004', N'Sunrise Spirit', 1550000, NULL, 'img_SP010.jpg', 8, NULL, NULL, NULL, NULL),
('SP011', 'CA001', 'TH004', N'Khoe Sắc Thắm', 1380000, NULL, 'img_SP011.jpg', 12, NULL, NULL, NULL, NULL),
('SP012', 'CA003', NULL, N'Chậu Lan Hồ Điệp Phú Quý', 2150000, NULL, 'img_SP012.jpg', 5, NULL, NULL, NULL, NULL), -- Gốc là 'Lan mini'
('SP013', 'CA002', 'TH004', N'Chậu Lan Hồ Điệp Chưng Tết', 2450000, NULL, 'img_SP013.jpg', 6, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa khai trương'
('SP014', 'CA005', NULL, N'Ngỏ Lời', 240000, NULL, 'img_SP014.jpg', 10, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Hồng'
('SP015', 'CA005', NULL, N'Khoe Sắc', 290000, NULL, 'img_SP015.jpg', 12, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Hồng'
('SP016', 'CA005', NULL, N'Mặt Trời Của Anh', 290000, NULL, 'img_SP016.jpg', 15, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Hồng'
('SP017', 'CA006', NULL, N'Baby nhỏ xinh', 390000, NULL, 'img_SP017.jpg', 8, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Baby'
('SP018', 'CA006', NULL, N'Trái tim nhỏ', 400000, NULL, 'img_SP018.jpg', 9, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Baby'
('SP019', 'CA007', NULL, N'Sunny', 250000, NULL, 'img_SP019.jpg', 6, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Hướng Dương'
('SP020', 'CA007', NULL, N'The Hope', 390000, NULL, 'img_SP020.jpg', 5, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Hướng Dương'
('SP021', 'CA008', NULL, N'Simple', 390000, NULL, 'img_SP021.jpg', 7, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Tulip'
('SP022', 'CA008', NULL, N'Đoá hoa tulip - Thiên thanh', 1200000, NULL, 'img_SP022.jpg', 10, NULL, NULL, NULL, NULL), -- Gốc là 'Hoa Tulip'
('SP023', 'CA004', NULL, N'Chậu Lan Hồ Điệp Đại Phát', 8210000, NULL, 'img_SP023.jpg', 7, NULL, NULL, NULL, NULL), -- Gốc là 'Lan tím'
('SP024', 'CA004', NULL, N'Chậu Lan Hồ Điệp- 101', 1940000, NULL, 'img_SP024.jpg', 5, NULL, NULL, NULL, NULL), -- Gốc là 'Lan tím'
('SP025', 'CA003', NULL, N'Chậu Lan Hồ Điệp- Vương Thịnh', 3110000, NULL, 'img_SP025.jpg', 5, NULL, NULL, NULL, NULL), -- Gốc là 'Lan mini'
('SP026', 'CA001', 'TH003', N'Pháo hoa', 400000, NULL, 'img_SP026.jpg', 15, NULL, NULL, NULL, NULL),
('SP027', 'CA001', 'TH003', N'Lời Tri Ân', 390000, NULL, 'img_SP027.jpg', 15, NULL, NULL, NULL, NULL),
('SP028', 'CA001', 'TH003', N'E Ấp', 410000, NULL, 'img_SP028.jpg', 12, NULL, NULL, NULL, NULL),
('SP029', 'CA001', 'TH003', N'Lavender Sắc Hương', 480000, NULL, 'img_SP029.jpg', 11, NULL, NULL, NULL, NULL),
('SP030', 'CA001', 'TH003', N'Vườn ươm trong hộp quà', 19400000, NULL, 'img_SP030.jpg', 15, NULL, NULL, NULL, NULL),
('SP031', 'CA001', 'TH003', N'Trẻ trung', 570000, NULL, 'img_SP031.jpg', 15, NULL, NULL, NULL, NULL),
('SP032', 'CA001', 'TH003', N'Cảm ơn', 750000, NULL, 'img_SP032.jpg', 15, NULL, NULL, NULL, NULL),
('SP033', 'CA001', 'TH003', N'Sen Tinh Tế', 690000, NULL, 'img_SP033.jpg', 12, NULL, NULL, NULL, NULL),
('SP034', 'CA001', 'TH003', N'Nét Quý Phái', 720000, NULL, 'img_SP034.jpg', 11, NULL, NULL, NULL, NULL),
('SP035', 'CA001', 'TH003', N'Kiêu Sa Rực Rỡ', 790000, NULL, 'img_SP035.jpg', 15, NULL, NULL, NULL, NULL)

INSERT INTO Discount VALUES
('GG001', 'SP001', N'Giảm 20%', '10/11/2025', '10/02/2026', 20, NULL, NULL, NULL, NULL),
('GG002', 'SP003', N'Giảm 18%', '10/10/2025', '10/01/2026', 18, NULL, NULL, NULL, NULL),
('GG003', 'SP002', N'Giảm 12%', '12/12/2025', '12/01/2026', 12, NULL, NULL, NULL, NULL),
('GG004', 'SP004', N'Giảm 10%', '10/09/2025', '10/12/2025', 10, NULL, NULL, NULL, NULL);
INSERT INTO Discount VALUES
('GG005', 'SP013', N'Giảm 20%', '1/11/2025', '10/2/2026', 20, NULL, NULL, NULL, NULL),
('GG006', 'SP012', N'Giảm 25%', '3/11/2025', '10/2/2026', 25, NULL, NULL, NULL, NULL);
INSERT INTO Discount VALUES
('GG008', 'SP008', N'Giảm 30%', '2025-11-02', '2026-02-20', 30, NULL, NULL, NULL, NULL),
('GG009', 'SP009', N'Giảm 10%', '2025-11-10', '2026-02-28', 10, NULL, NULL, NULL, NULL),
('GG010', 'SP010', N'Giảm 18%', '2025-11-08', '2026-02-25', 18, NULL, NULL, NULL, NULL),
('GG011', 'SP011', N'Giảm 12%', '2025-11-12', '2026-02-22', 12, NULL, NULL, NULL, NULL),
('GG012', 'SP014', N'Giảm 22%', '2025-11-06', '2026-02-12', 22, NULL, NULL, NULL, NULL),
('GG013', 'SP015', N'Giảm 27%', '2025-11-09', '2026-02-18', 27, NULL, NULL, NULL, NULL),
('GG014', 'SP016', N'Giảm 17%', '2025-11-04', '2026-02-14', 17, NULL, NULL, NULL, NULL),
('GG015', 'SP017', N'Giảm 28%', '2025-11-07', '2026-02-21', 28, NULL, NULL, NULL, NULL);
SET DATEFORMAT DMY;
INSERT INTO Orders VALUES
('OD001', 'US005', '01/10/2025', N'TP. Hồ Chí Minh', N'Đã giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD002', 'US003', '02/10/2025', N'An Giang', N'Đang xử lý', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD003', 'US004', '03/10/2025', N'Tây Ninh', N'Đã giao', N'Chuyển khoản', NULL, NULL, NULL, NULL),
('OD004', 'US002', '04/11/2025', N'TP. Hồ Chí Minh', N'Đã giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD005', 'US001', '05/10/2025', N'TP. Hồ Chí Minh', N'Đã hủy', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD006', 'US002', '06/10/2025', N'Đà Lạt', N'Đã giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD007', 'US003', '07/11/2025', N'Hà Nội', N'Đã giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD008', 'US005', '08/10/2025', N'Tây Ninh', N'Đã giao', N'Chuyển khoản', NULL, NULL, NULL, NULL),
('OD009', 'US004', '09/8/2025', N'Bình Dương', N'Đang giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL),
('OD010', 'US002', '10/9/2025', N'TP. Hồ Chí Minh', N'Đã giao', N'Thanh toán khi nhận hàng', NULL, NULL, NULL, NULL);

INSERT INTO OrderDetail VALUES
('OD001', 'SP001', 1, 190000),
('OD001', 'SP002', 1, 700000),
('OD002', 'SP003', 2, 730000),
('OD003', 'SP001', 1, 190000),
('OD004', 'SP005', 1, 1150000),
('OD005', 'SP004', 3, 830000),
('OD006', 'SP002', 2, 700000),
('OD007', 'SP001', 1, 190000),
('OD008', 'SP003', 2, 730000),
('OD009', 'SP005', 1, 1150000),
('OD010', 'SP004', 1, 830000);

-- Kiểm tra kiểu dữ liệu
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Product' AND COLUMN_NAME = 'Quantity';

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'OrderDate';