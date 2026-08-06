# Phân tích nghiệp vụ Module Khung Chương Trình Đào Tạo

## Phạm vi

-   Chỉ **Super Admin** được quản lý.
-   Một **Ngành đào tạo** có một **Khung chương trình đào tạo**.
-   Một **Khung chương trình đào tạo** gồm nhiều **Chuyên ngành**.
-   Mỗi chuyên ngành có các **Học kỳ**, **Môn học**, **Điều kiện tiên
    quyết**, **Điều kiện tốt nghiệp**.
-   Chương trình có thể áp dụng cho nhiều **Khóa tuyển sinh** và nhiều
    **Cơ sở**.

## Mô hình

``` text
Ngành đào tạo
    │
    ▼
Khung chương trình đào tạo
    ├── Chuyên ngành A
    │      ├── HK1
    │      ├── HK2
    │      └── ...
    ├── Chuyên ngành B
    └── Chuyên ngành C
```

## Nghiệp vụ chính

### 1. Tạo khung chương trình đào tạo

Tạo khung mới cho một ngành, khai báo tên, số học kỳ, tổng tín chỉ, điều
kiện tốt nghiệp.

### 2. Quản lý chuyên ngành

Thêm, sửa, khóa hoặc xóa chuyên ngành thuộc khung.

### 3. Quản lý học kỳ

Thiết lập số học kỳ cho từng chuyên ngành.

### 4. Quản lý môn học

Phân bổ môn theo từng học kỳ của từng chuyên ngành.

### 5. Thiết lập môn học

-   Tín chỉ
-   Bắt buộc/Tự chọn
-   Tiên quyết
-   Song hành
-   Điều kiện đạt

### 6. Điều kiện tốt nghiệp

-   Đủ tổng tín chỉ
-   Hoàn thành thực tập
-   Hoàn thành đồ án
-   Đạt GPA theo quy định
-   Không còn môn nợ

### 7. Dùng lại chương trình (Reuse)

Không tạo bản mới. Chỉ gán chương trình hiện có cho khóa tuyển sinh mới.

Ví dụ: - CTĐT V1 → K2026 - CTĐT V1 → K2027

### 8. Sao chép chương trình (Clone)

Tạo bản sao độc lập để chỉnh sửa, không ảnh hưởng bản gốc.

### 9. Nâng cấp chương trình (Upgrade)

Tạo Version mới có liên kết lịch sử: V1 → V2 → V3.

### 10. Áp dụng cho khóa tuyển sinh

Một chương trình có thể áp dụng cho nhiều khóa.

### 11. Áp dụng cho cơ sở

-   Tất cả cơ sở.
-   Một số cơ sở được chọn.

### 12. Ngừng áp dụng

Không cho khóa mới sử dụng nhưng vẫn giữ cho các khóa cũ.

### 13. Lưu trữ

Chuyển trạng thái Archive, không xóa dữ liệu.

## Tình huống nghiệp vụ

### TH1: Dùng lại

K2026 và K2027 cùng sử dụng CTĐT V1.

### TH2: Clone

CTĐT V1 → Clone → V2 → chỉnh sửa môn học.

### TH3: Upgrade

Nâng cấp theo quy định mới, lưu lịch sử phiên bản.

### TH4: Áp dụng theo cơ sở

HCM dùng V2, Hà Nội vẫn dùng V1.

### TH5: Áp dụng theo khóa

K2028 dùng V2, K2029 dùng V2.

## Workflow

``` text
Super Admin
    │
    ├── Tạo khung CTĐT
    ├── Quản lý chuyên ngành
    ├── Quản lý học kỳ
    ├── Quản lý môn học
    ├── Thiết lập điều kiện
    ├── Reuse
    ├── Clone
    ├── Upgrade
    ├── Áp dụng cho Khóa tuyển sinh
    ├── Áp dụng cho Cơ sở
    └── Archive
```
