# 🚀 LiteCommerce - Hệ Thống Quản Lý Thương Mại Điện Tử

[![Framework](https://img.shields.io/badge/Framework-.NET%208.0-blueviolet)](https://dotnet.microsoft.com/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-red)](https://www.microsoft.com/en-us/sql-server/)
[![Design](https://img.shields.io/badge/Design-Glassmorphism-cyan)](https://tailwindcss.com/)
[![Status](https://img.shields.io/badge/Status-In%20Development-green)]()

**LiteCommerce** (DUC Shop) là một giải pháp quản lý bán hàng toàn diện được xây dựng trên nền tảng **ASP.NET Core MVC**. Dự án áp dụng kiến trúc đa tầng (Multi-layered Architecture) giúp hệ thống vận hành ổn định, dễ dàng mở rộng và bảo trì.

---

## 🏗 Kiến Trúc Hệ Thống (Architecture)

Dự án được tổ chức theo mô hình phân lớp chuyên nghiệp, tách biệt rõ ràng giữa xử lý dữ liệu và giao diện:

* **`SV22T1020811.Admin`**: Trang quản trị (Back-office) dành cho nhân viên quản lý hàng hóa, đơn hàng và khách hàng.
* **`SV22T1020811.Shop`**: Trang thương mại điện tử (Front-end) dành cho khách hàng trải nghiệm mua sắm.
* **`SV22T1020811.BusinessLayers`**: Tầng logic nghiệp vụ (BLL), điều phối dữ liệu giữa tầng giao diện và tầng dữ liệu.
* **`SV22T1020811.DataLayers`**: Tầng truy cập dữ liệu (DAL), sử dụng **Dapper** và **ADO.NET** để thực thi các câu lệnh SQL tối ưu.
* **`SV22T1020811.Models`**: Chứa các Entity, ViewModel và Common Models dùng chung cho toàn bộ Solution.

---

## 📸 Hình Ảnh Dự Án (Screenshots)

### 🛒 Trang Khách Hàng (Web Shop) - Phong Cách Glassmorphism

| Trang Chủ & Danh Sách Sản Phẩm | Chi Tiết Sản Phẩm & Giỏ Hàng (AJAX) |
| :--- | :--- |
| <img width="1918" height="915" alt="image" src="https://github.com/user-attachments/assets/6c07784e-5b32-4d9b-8211-627324c71071" /> | <img width="1915" height="917" alt="image" src="https://github.com/user-attachments/assets/bb50fb1e-e1bd-4606-a7e4-4b3792b20f3b" />|
| *Giao diện tối giản, responsive, hỗ trợ lọc mượt mà.* | *Tăng giảm số lượng, thông báo Toast báo hiệu tức thì.* |

| Quy Trình Thanh Toán (Checkout) | Lịch Sử Đơn Hàng & Phân Trang |
| :--- | :--- |
| <img width="1919" height="923" alt="image" src="https://github.com/user-attachments/assets/372b5cb2-9585-4cc8-8807-b738578696de" />| <img width="1919" height="919" alt="image" src="https://github.com/user-attachments/assets/b670767e-a548-4758-8cf0-8a4437f8a438" />|
| *Xác nhận thông tin, địa chỉ và phương thức COD.* | *Quản lý đơn cá nhân, phân trang thông minh.* |

### 🛠 Trang Quản Trị (Admin) - Back-office Mạnh Mẽ

### 🛠 Trang Quản Trị (Admin) - Back-office Mạnh Mẽ

| Dashboard & Tổng Quan | Quản Lý Sản Phẩm (Đa Ảnh) |
| :--- | :--- |
| <img width="1919" height="918" alt="image" src="https://github.com/user-attachments/assets/f692ecb6-0895-49ec-a974-5c8f6203a0ff" />| <img width="1919" height="918" alt="image" src="https://github.com/user-attachments/assets/863a32de-ab30-44c4-ac0c-e77858886e6e" />|
| *Giao diện trực quan, thống kê nhanh dữ liệu.* | *Thêm ảnh, thuộc tính, phân loại ...* |

---

## ✨ Tính Năng Nổi Bật

### 🛠 Trang Quản Trị (Admin)
- **Quản lý Sản phẩm:** Hỗ trợ cấu hình đa dạng ảnh (Product Photos) và thuộc tính chi tiết (Product Attributes).
- **Quy trình Đơn hàng:** Xử lý trạng thái đơn hàng chuyên nghiệp (Chờ duyệt -> Đang giao -> Hoàn tất/Hủy).
- **Phân trang & Tìm kiếm:** Lọc dữ liệu thông minh, xử lý phân trang trực tiếp từ SQL Server (`OFFSET...FETCH`).
- **Quản lý Danh mục:** Phân loại hàng hóa theo loại và nhà cung cấp.

### 🛒 Trang Khách Hàng (Web Shop)
- **Giao diện Hiện đại:** Thiết kế theo phong cách **Glassmorphism** (kính mờ), responsive hoàn toàn trên di động.
- **Giỏ hàng (Cart):** Xử lý giỏ hàng qua Session, cho phép tăng giảm số lượng linh hoạt bằng AJAX.
- **Đặt hàng:** Quy trình Checkout nhanh chóng, thông báo Toast báo hiệu trạng thái thời gian thực.

---

## 🛠 Công Nghệ Sử Dụng (Tech Stack)

| Thành phần | Công nghệ |
| :--- | :--- |
| **Backend** | C# / ASP.NET Core 8.0 MVC |
| **Database** | SQL Server (Stored Procedures) |
| **Data Access** | Dapper, ADO.NET |
| **Frontend** | Tailwind CSS, JavaScript (jQuery/Ajax) |
| **Cấu trúc** | Multi-layered Architecture |

---

## ⚙️ Hướng Dẫn Cài Đặt

1. **Clone project:**
   ```bash
   git clone [https://github.com/hoangduc180804-creator/Web-ban-hang.git] (https://github.com/hoangduc180804-creator/Web-ban-hang.git)
