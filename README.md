<<<<<<< HEAD
# 💎 MarketService - Premium Marketplace Platform

[![Build Status](https://img.shields.io/badge/Build-Succeeded-success.svg)](#)
[![Tech Stack](https://img.shields.io/badge/Stack-.NET%209%20%7C%20EF%20Core%20%7C%20SQL%20Server-blue.svg)](#)

MarketService là một nền tảng thương mại điện tử hiện đại, tập trung vào trải nghiệm người dùng tối giản và giao diện cao cấp. Dự án được xây dựng trên nền tảng .NET 9 với kiến trúc mạnh mẽ và giao diện Glassmorphism tinh tế.

---

## ✨ Điểm Nổi Bật (Key Features)

- **🚀 Glassmorphism UI**: Giao diện mang phong cách tương lai với hiệu ứng kính mờ, gradient động và animation mượt mà.
- **🔐 Hệ Thống Auth Toàn Diện**: 
  - Đăng nhập bằng Email/Username.
  - Luồng Quên mật khẩu & Reset Password với Token bảo mật.
  - Giao diện đăng ký/đặng nhập thiết kế riêng.
- **💸 Marketplace & Orders**: 
  - Quản lý sản phẩm, danh mục động.
  - Hệ thống giỏ hàng, đặt hàng và theo dõi vận chuyển (Shipping).
- **💬 Real-time Chat**: Trao đổi trực tiếp giữa người mua và người bán qua SignalR.
- **👤 Profile Cá Nhân Hóa**: Trang cá nhân với ảnh bìa tùy chỉnh, thông số hoạt động và quản lý đơn hàng.

---

## 📸 Giao Diện Dự Án (Showcase)

> [!TIP]
> **Cách chụp ảnh đẹp**: Anh hãy mở trình duyệt ở chế độ Toàn màn hình (F11) hoặc chụp các thành phần quan trọng như Card Login, Hero Section ở trang Home để đưa vào README.

| Đăng Nhập (Glassmorphism) | Trang Cá Nhân (Profile) |
| :---: | :---: |
| ![Login Screenshot](docs/screenshots/login.png) | ![Profile Screenshot](docs/screenshots/profile.png) |<img width="739" height="57" alt="image" src="https://github.com/user-attachments/assets/ba1247ae-77ea-4349-bc21-3d671bae12a5" />


---

## 🛠️ Công Nghệ Sử Dụng (Tech Stack)

- **Backend**: C# 13, ASP.NET Core 9.0 (MVC & API)
- **Database**: SQL Server, Entity Framework Core 9.0
- **Frontend**: Vanilla CSS (Custom UI Framework), JavaScript (ES6+), SignalR
- **Security**: JWT Authentication, BCrypt Password Hashing

---

## ⚙️ Hướng Dẫn Cài Đặt (Installation)

1. **Clone repository**:
   ```bash
   git clone https://github.com/Loc2261/MarketService.git
   cd MarketService
   ```

2. **Cấu hình Database**:
   Cập nhật chuỗi kết nối trong `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=YOUR_SERVER;Database=MarketServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
   }
   ```

3. **Chạy Migration**:
   ```bash
   dotnet ef database update
   ```

4. **Khởi chạy**:
   ```bash
   dotnet run
   ```

---

## 👤 Tác Giả
- GitHub: [@Loc2261](https://github.com/Loc2261)
- Project: MarketService
=======
# market-server
1 trang mua bán và trao đổi
>>>>>>> b6aaaf9fceee58579c04a3c56285711c8a832024
