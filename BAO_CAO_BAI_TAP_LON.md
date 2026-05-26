# BÁO CÁO BÀI TẬP LỚN eShop

## 1. Mục tiêu
Hoàn thành 3 phần chính của bài tập:

1. AuthZ Policy cho Catalog create/update/delete.
2. Problem Details theo RFC 9457 với `traceId`.
3. Khảo sát gRPC qua `basket.proto` và `grpcurl`.

## 2. Kết quả đã thực hiện

### 2.1 AuthZ Policy cho Catalog
Đã thêm policy `AdminOnly` cho các endpoint thay đổi dữ liệu của Catalog:
- `POST /api/catalog/items`
- `PUT /api/catalog/items`
- `PUT /api/catalog/items/{id}`
- `DELETE /api/catalog/items/{id}`

Luồng xác thực/ủy quyền hiện tại:
- Không có token: trả `401 Unauthorized`
- Token user thường: trả `403 Forbidden`
- Token admin: thực thi thành công `200/201/204` tùy endpoint

Các thay đổi liên quan:
- [src/Catalog.API/Program.cs](src/Catalog.API/Program.cs)
- [src/Catalog.API/Apis/CatalogApi.cs](src/Catalog.API/Apis/CatalogApi.cs)
- [src/eShop.ServiceDefaults/AuthenticationExtensions.cs](src/eShop.ServiceDefaults/AuthenticationExtensions.cs)
- [src/Identity.API/Services/ProfileService.cs](src/Identity.API/Services/ProfileService.cs)
- [src/Identity.API/UsersSeed.cs](src/Identity.API/UsersSeed.cs)
- [src/eShop.AppHost/Program.cs](src/eShop.AppHost/Program.cs)

### 2.2 Problem Details
Đã cập nhật response cho trường hợp không tìm thấy item để trả về Problem Details đúng kiểu RFC 9457, có thêm extension field `traceId`.

Ví dụ:
- `GET /api/catalog/items/99999`
- Response có các field cần thiết: `type`, `title`, `status`, `detail`, `traceId`

Các thay đổi liên quan:
- [src/Catalog.API/Apis/CatalogApi.cs](src/Catalog.API/Apis/CatalogApi.cs)
- [src/Catalog.API/appsettings.json](src/Catalog.API/appsettings.json)
- [src/Catalog.API/Properties/launchSettings.json](src/Catalog.API/Properties/launchSettings.json)

### 2.3 gRPC Exploration
Đã đọc contract của Basket gRPC service trong `basket.proto`:
- `GetBasket(GetBasketRequest) returns (CustomerBasketResponse)`
- `UpdateBasket(UpdateBasketRequest) returns (CustomerBasketResponse)`
- `DeleteBasket(DeleteBasketRequest) returns (DeleteBasketResponse)`

`GetBasket` dùng request rỗng, còn danh tính user được lấy từ auth context của gRPC call. Vì vậy khi dùng `grpcurl`, phần quan trọng là endpoint và metadata/auth, không phải body chứa userId.

Kết quả `grpcurl` thực tế trên Basket API live:

```text
grpcurl -plaintext -proto src\Basket.API\Proto\basket.proto localhost:5221 list
BasketApi.Basket

grpcurl -plaintext -proto src\Basket.API\Proto\basket.proto localhost:5221 describe BasketApi.Basket
service Basket {
	rpc DeleteBasket ( .BasketApi.DeleteBasketRequest ) returns ( .BasketApi.DeleteBasketResponse );
	rpc GetBasket ( .BasketApi.GetBasketRequest ) returns ( .BasketApi.CustomerBasketResponse );
	rpc UpdateBasket ( .BasketApi.UpdateBasketRequest ) returns ( .BasketApi.CustomerBasketResponse );
}

grpcurl -plaintext -proto src\Basket.API\Proto\basket.proto localhost:5221 BasketApi.Basket/GetBasket
{}
```

Hiện tại đây là phần khảo sát contract. Bonus `GetItemCount` chưa được triển khai trong codebase này.

Các file tham khảo:
- [src/Basket.API/Proto/basket.proto](src/Basket.API/Proto/basket.proto)
- [src/ClientApp/Services/Basket/Protos/basket.proto](src/ClientApp/Services/Basket/Protos/basket.proto)
- [src/Basket.API/Grpc/BasketService.cs](src/Basket.API/Grpc/BasketService.cs)

## 3. Giải thích 401 vs 403
- `401 Unauthorized`: request chưa được xác thực. Nghĩa là client chưa gửi token hợp lệ, hoặc không có danh tính đăng nhập.
- `403 Forbidden`: request đã xác thực rồi, nhưng danh tính đó không có quyền truy cập tài nguyên.

Áp dụng vào bài này:
- Không có token -> chưa xác thực -> `401`
- Token user thường -> đã xác thực nhưng không có role `admin` -> `403`
- Token admin -> có role hợp lệ -> cho phép thao tác

## 4. Kết quả kiểm tra
Đã chạy test slice cho `CatalogApiTests` và kết quả cuối cùng là:
- `40 passed, 0 failed`

Test file:
- [tests/Catalog.FunctionalTests/CatalogApiTests.cs](tests/Catalog.FunctionalTests/CatalogApiTests.cs)
- [tests/Catalog.FunctionalTests/TestAuthHandler.cs](tests/Catalog.FunctionalTests/TestAuthHandler.cs)
- [tests/Catalog.FunctionalTests/CatalogApiFixture.cs](tests/Catalog.FunctionalTests/CatalogApiFixture.cs)

## 5. Ảnh chụp màn hình cần đính kèm
> Mục này để AI khác hoặc bạn chèn ảnh chụp màn hình vào báo cáo cuối.

### 5.1 AuthZ
- `artifacts/report/authz-401.png` - POST/PUT/DELETE không có token -> `401`
- `artifacts/report/authz-403.png` - token user thường -> `403`
- `artifacts/report/authz-200.png` - token admin -> thành công

### 5.2 Problem Details
- `artifacts/report/problem-details-404.png` - GET item không tồn tại, có `type/title/status/detail/traceId`

### 5.3 gRPC
- `artifacts/report/grpcurl-list.png` - `grpcurl list`
- `artifacts/report/grpcurl-getbasket.png` - `grpcurl` gọi `GetBasket`
- `artifacts/report/basket-proto.png` - ảnh chụp phần `basket.proto`

### 5.4 Aspire Dashboard
- Screenshot thực tế đã chụp từ resources dashboard sau khi filter `catalog-api`.
- Dùng ảnh này để minh chứng trạng thái service và URL/runtime startup của Catalog API.

## 6. Lệnh gợi ý để tái tạo bằng terminal
### 6.1 Catalog tests
```powershell
dotnet test --project tests\Catalog.FunctionalTests\Catalog.FunctionalTests.csproj --filter-class eShop.Catalog.FunctionalTests.CatalogApiTests --output Normal
```

### 6.2 gRPC exploration
```powershell
grpcurl list localhost:5001
grpcurl localhost:5001 BasketApi.Basket/GetBasket
```

## 7. Ghi chú
- Catalog API hiện đã được nối auth/authorization theo `AdminOnly`.
- Problem Details đã có `traceId` để phục vụ debug và trace log.
- Phần gRPC bonus `GetItemCount` có thể thêm sau nếu cần mở rộng bài tập.
