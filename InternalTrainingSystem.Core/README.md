# Internal Training System API

Đây là phần API của hệ thống đào tạo nội bộ, được xây dựng với ASP.NET Core 8.0.

## 📋 Mục lục

- [Yêu cầu hệ thống](#yêu-cầu-hệ-thống)
- [Cài đặt](#cài-đặt)
- [Cấu hình](#cấu-hình)
- [Chạy ứng dụng](#chạy-ứng-dụng)
- [API Documentation](#api-documentation)
- [Cấu trúc thư mục](#cấu-trúc-thư-mục)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

## 🔧 Yêu cầu hệ thống

- .NET 8.0 SDK hoặc mới hơn
- SQL Server 2019 hoặc mới hơn (hoặc SQL Server LocalDB)
- Visual Studio 2022 hoặc VS Code
- Git

## 🚀 Cài đặt

### 1. Clone repository

```bash
git clone <repository-url>
cd InternalTrainingSystem.Core
```

### 2. Restore packages

```bash
dotnet restore
```

### 3. Cấu hình Environment Variables

1. Copy file `.env.example` thành `.env`:
   ```bash
   copy .env.example .env
   ```

2. Mở file `.env` và cập nhật các giá trị theo môi trường của bạn:
   ```env
   CONNECTION_STRING=Server=localhost;Database=InternalTrainingSystemDB;Trusted_Connection=true;TrustServerCertificate=true;
   JWT_SECRET_KEY=your-actual-secret-key-at-least-32-characters
   # ... các cấu hình khác
   ```

### 4. Database Setup

```bash
# Tạo database migration (nếu cần)
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

## ⚙️ Cấu hình

### Environment Variables

File `.env` chứa các cấu hình quan trọng:

| Variable | Mô tả | Ví dụ |
|----------|-------|-------|
| `CONNECTION_STRING` | Chuỗi kết nối database | `Server=localhost;Database=...` |
| `JWT_SECRET_KEY` | Secret key cho JWT token | `your-32-character-secret-key` |
| `JWT_EXPIRE_HOURS` | Thời gian hết hạn JWT (giờ) | `24` |
| `GOOGLE_API_KEY` | API key cho Google services | `your-google-api-key` |
| `SMTP_SERVER` | SMTP server cho email | `smtp.gmail.com` |
| `MAX_FILE_SIZE_MB` | Kích thước file tối đa | `10` |

### appsettings.json

File này chứa cấu hình mặc định và sẽ được override bởi environment variables.

## 🏃‍♂️ Chạy ứng dụng

### Development Mode

```bash
dotnet run
```

Hoặc với hot reload:

```bash
dotnet watch run
```

### Production Mode

```bash
dotnet run --environment Production
```

API sẽ chạy tại:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## 📚 API Documentation

### Swagger UI

Khi chạy ở Development mode, truy cập Swagger UI tại:
```
https://localhost:5001/swagger
```

### Endpoints chính

#### Authentication
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/refresh` - Refresh token
- `POST /api/auth/logout` - Đăng xuất

#### Users
- `GET /api/users` - Lấy danh sách users
- `GET /api/users/{id}` - Lấy thông tin user
- `PUT /api/users/{id}` - Cập nhật user
- `DELETE /api/users/{id}` - Xóa user

#### Courses
- `GET /api/courses` - Lấy danh sách khóa học
- `POST /api/courses` - Tạo khóa học mới
- `GET /api/courses/{id}` - Lấy thông tin khóa học
- `PUT /api/courses/{id}` - Cập nhật khóa học
- `DELETE /api/courses/{id}` - Xóa khóa học

### Authentication

API sử dụng JWT Bearer token. Thêm header sau vào request:

```
Authorization: Bearer <your-jwt-token>
```

## 📁 Cấu trúc thư mục

```
InternalTrainingSystem.Core/
├── Controllers/          # API Controllers
├── Models/              # Data models
├── Services/            # Business logic
├── Data/               # Database context & repositories
├── DTOs/               # Data Transfer Objects
├── Middleware/         # Custom middleware
├── Helpers/            # Utility classes
├── Properties/         # Launch settings
├── wwwroot/           # Static files
├── .env               # Environment variables (không commit)
├── .env.example       # Template cho .env
├── .gitignore         # Git ignore rules
├── appsettings.json   # App configuration
├── Program.cs         # Entry point
└── README.md          # Tài liệu này
```

## 🚀 Deployment

### Docker

1. Tạo file `Dockerfile`:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InternalTrainingSystem.Core.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "InternalTrainingSystem.Core.dll"]
```

2. Build và run:
```bash
docker build -t internal-training-api .
docker run -p 8080:80 internal-training-api
```

### Azure App Service

1. Publish profile:
```bash
dotnet publish -c Release
```

2. Deploy lên Azure App Service
3. Cấu hình Environment Variables trong Azure Portal

## 🔧 Troubleshooting

### Lỗi thường gặp

#### 1. Connection String không đúng
```
Error: Cannot open database "InternalTrainingSystemDB"
```
**Giải pháp:** Kiểm tra `CONNECTION_STRING` trong file `.env`

#### 2. JWT Secret Key quá ngắn
```
Error: IDX10720: Unable to create KeyedHashAlgorithm
```
**Giải pháp:** Đảm bảo `JWT_SECRET_KEY` có ít nhất 32 ký tự

#### 3. CORS Error
```
Error: CORS policy blocked
```
**Giải pháp:** Cập nhật `ALLOWED_ORIGINS` trong file `.env`

### Debug Mode

Để bật debug logging, cập nhật `LOG_LEVEL=Debug` trong file `.env`

### Performance Monitoring

Sử dụng Application Insights hoặc tools tương tự để monitor performance.

## 🤝 Contributing

1. Fork repository
2. Tạo feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📞 Support

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra [Troubleshooting](#troubleshooting)
2. Tạo issue trên GitHub
3. Liên hệ team development

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Lưu ý quan trọng:**
- ⚠️ **Không bao giờ commit file `.env` lên git**
- 🔒 **Luôn sử dụng HTTPS trong production**
- 🔑 **Thay đổi JWT_SECRET_KEY trong production**
- 💾 **Backup database thường xuyên**