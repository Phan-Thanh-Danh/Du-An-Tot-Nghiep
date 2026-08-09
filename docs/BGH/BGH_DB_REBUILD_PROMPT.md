# Prompt backup, làm sạch và seed lại dữ liệu toàn LMS

Sao chép toàn bộ nội dung từ phần **PROMPT** bên dưới để giao cho AI khác thực hiện.

## PROMPT

Bạn đang làm việc trong repository:

```text
D:\A\Du-An-Tot-Nghiep
```

Mục tiêu: audit toàn bộ schema và luồng dữ liệu hiện có của LMS, backup database development `LMS`, chỉ xóa **các bản ghi** trong các bảng ứng dụng theo đúng thứ tự khóa ngoại, sau đó seed lại dữ liệu Unicode tiếng Việt đa dạng để kiểm thử toàn LMS, đặc biệt là toàn bộ role BGH. Tổng số bản ghi dữ liệu ứng dụng sau khi seed không được vượt quá **5.000**.

### 1. Giới hạn tuyệt đối

1. Đọc đầy đủ `README.md`, `AGENTS.md`, `CLAUDE.md`, tài liệu trong `docs/**`, `Backend/Data/ApplicationDbContext.cs`, toàn bộ model, seeder và controller trước khi thao tác.
2. Dùng `rg` để lập danh sách bảng, quan hệ PK/FK, unique/index, enum/status và endpoint thật. Không bịa bảng, cột, role, trạng thái hoặc API.
3. Chạy `git status` và bảo toàn mọi thay đổi hiện có. Không dùng `git reset --hard`, `git checkout --` hoặc ghi đè công việc của người dùng.
4. Không sửa `frontend/**`, UI, CSS, hiệu ứng, router hoặc API client.
5. Không sửa controller, DTO, API contract, công thức GPA, Pass/Fail, at-risk, evaluation hoặc business logic.
6. Không thêm dependency, không format toàn repository, không commit/push khi chưa được yêu cầu riêng.
7. Chỉ thao tác database local/development có tên chính xác `LMS`. Tuyệt đối không đụng `LMS_MobileDemo`, staging, production hoặc database dùng chung.

Các lệnh cấm tuyệt đối:

- Không `DROP DATABASE`, detach, rename hoặc tạo lại database `LMS`.
- Không `DROP TABLE`, `TRUNCATE TABLE`, xóa/đổi tên/tạo lại bảng.
- Không xóa, thêm, đổi tên hoặc thay đổi kiểu dữ liệu của bất kỳ cột nào.
- Không thay đổi PK, FK, index, constraint, collation hoặc schema.
- Không xóa, sửa, tạo mới hoặc chạy lại migration; không xóa `ApplicationDbContextModelSnapshot.cs`.
- Không xóa dữ liệu trong `__EFMigrationsHistory` hoặc bảng metadata/framework tương đương.
- Không dùng `EnsureDeleted`, `EnsureCreated` hoặc bất kỳ code nào tái tạo schema.
- Không vô hiệu hóa constraint vĩnh viễn và không dùng `CASCADE` mới để xóa nhanh dữ liệu.

Task này chỉ được phép thay đổi **dữ liệu hàng/bản ghi** trong các bảng ứng dụng hiện có và các file seed/script/report cần thiết để thực hiện việc đó.

### 2. Phạm vi mở rộng toàn LMS

Audit toàn bộ `DbSet` và mapping thật trong `ApplicationDbContext`, sau đó lập inventory cho tất cả bảng ứng dụng theo module, tối thiểu gồm các nhóm nếu chúng thật sự tồn tại:

- Tổ chức, cơ sở, đơn vị, người dùng, vai trò và phân quyền.
- Ngành đào tạo, chuyên ngành, khóa tuyển sinh, chương trình đào tạo, học kỳ, môn học và môn học trong chương trình.
- Lớp hành chính, lớp học phần/khóa học, phân công giảng dạy và đăng ký học.
- Điểm, thành phần điểm, yêu cầu sửa/mở khóa điểm, học lại, Pass/Fail, GPA và sinh viên có nguy cơ.
- Buổi học, thời khóa biểu, chuyên cần, thay đổi lịch và phê duyệt lịch.
- Tòa nhà, tầng, phòng học, thiết bị/cơ sở vật chất, bảo trì và xung đột phòng nếu schema có.
- Câu hỏi đánh giá, đánh giá giảng viên, đợt đánh giá và dữ liệu tổng hợp liên quan.
- Đơn từ, phê duyệt, thông báo, cảnh báo, nhật ký kiểm toán và các workflow khác đang được API sử dụng.
- Các module Student, Teacher, GiaoVu, BGH, Parent và role khác mà controller hiện có truy vấn.

Không mặc định mọi bảng đều cần nhiều dữ liệu. Phải dựa trên endpoint, FK và màn hình thật để ưu tiên bảng cần thiết, nhưng dữ liệu sau seed phải đủ cho các luồng chính của toàn LMS và đủ tình huống kiểm thử BGH.

### 3. Xác nhận đúng database trước khi xóa dữ liệu

Chạy và lưu kết quả:

```sql
SELECT
    @@SERVERNAME AS ServerName,
    DB_NAME() AS DatabaseName,
    SUSER_SNAME() AS LoginName,
    SERVERPROPERTY('ProductVersion') AS SqlServerVersion,
    SERVERPROPERTY('Edition') AS SqlServerEdition;
```

Điều kiện bắt buộc:

- `DatabaseName` phải bằng chính xác `LMS`.
- Xác định connection string thực tế mà `dotnet run` dùng từ Development config, User Secrets hoặc environment.
- Không hardcode connection string máy cá nhân hoặc secret vào config release.
- Nếu không chứng minh được đây là database local/development hoặc tên không phải `LMS`, dừng ngay.

### 4. Backup bắt buộc trước khi xóa bản ghi

1. Lập số lượng bản ghi trước khi làm cho toàn bộ bảng ứng dụng.
2. Tạo full backup có timestamp:

```text
LMS_before_full_data_reseed_YYYYMMDD_HHmmss.bak
```

3. Ghi đường dẫn tuyệt đối, SQL Server instance, database, thời gian và dung lượng file.
4. Chạy:

```sql
RESTORE VERIFYONLY FROM DISK = N'<absolute-backup-path>';
```

5. Tạo script khôi phục tại:

```text
docs/artifacts/lms-data-reseed/RESTORE_LMS_BACKUP.sql
```

6. Nếu backup hoặc `RESTORE VERIFYONLY` thất bại, dừng ngay và không xóa bất kỳ bản ghi nào.

### 5. Audit Unicode và dữ liệu hiện trạng

Audit mọi cột chuỗi của các bảng ứng dụng để tìm mojibake, tối thiểu gồm:

```text
Ã
Â
Æ
Ä
áº
á»
�
```

Xuất báo cáo gồm bảng, cột, khóa chính, giá trị lỗi và số dòng lỗi. Đồng thời thống kê dữ liệu thiếu FK, orphan, trùng unique, trạng thái ngoài miền hợp lệ và dữ liệu ngày/điểm bất hợp lý.

Quy tắc Unicode:

- Source `.cs`, `.sql`, `.json`, `.md` phải lưu UTF-8.
- Chuỗi tiếng Việt trong SQL tay phải dùng literal `N'...'`.
- Không vá mojibake bằng các lệnh `UPDATE ... LIKE` phỏng đoán hoặc thay nhiều nội dung khác nhau bằng cùng một câu.
- Không thay collation hoặc kiểu cột. Dữ liệu lỗi sẽ được loại bỏ trong bước làm sạch và tạo lại đúng Unicode bằng seed.

### 6. Kế hoạch xóa bản ghi an toàn

Trước khi chạy lệnh xóa, phải tạo và trình bày:

1. Danh sách đầy đủ các bảng ứng dụng và số dòng hiện có.
2. Dependency graph PK/FK thật lấy từ EF model và SQL Server metadata.
3. Thứ tự xóa từ bảng con đến bảng cha.
4. Danh sách bảng được giữ nguyên, gồm `__EFMigrationsHistory` và metadata hệ thống.
5. Script xóa dữ liệu có transaction và cơ chế rollback khi lỗi.

Quy tắc thực thi:

- Chỉ dùng `DELETE` có chủ đích trên bảng ứng dụng; không dùng `DROP` hoặc `TRUNCATE`.
- Không xóa theo tên bảng tự đoán. Mọi bảng phải được đối chiếu với database `LMS` thật.
- Không tắt constraint toàn database. Nếu vướng chu trình FK, phải phân tích chính xác và dùng cách cập nhật/xóa bản ghi hợp lệ trong transaction mà không thay schema.
- Không commit transaction cho đến khi mọi lệnh xóa và kiểm tra FK của bước làm sạch đều thành công.
- Không reset identity nếu không thật sự cần. Nếu cần `DBCC CHECKIDENT`, phải ghi rõ bảng, lý do và kết quả; không được thay đổi schema.
- Nếu lỗi, rollback và chứng minh dữ liệu cũ vẫn còn hoặc hướng dẫn restore backup.

### 7. Ngân sách tối đa 5.000 bản ghi

Sau seed, tổng số dòng trong **toàn bộ bảng dữ liệu ứng dụng được task làm sạch/seed lại** không được vượt quá **5.000**, không tính `__EFMigrationsHistory` và metadata framework bắt buộc. Không tạo dữ liệu quy mô 10.000 sinh viên hoặc hàng trăm nghìn điểm.

Phân bổ tham khảo, phải điều chỉnh theo schema thật nhưng không vượt tổng trần:

| Nhóm dữ liệu | Ngân sách tối đa tham khảo |
|---|---:|
| Danh mục, tổ chức, RBAC và cơ sở vật chất | 350 |
| Người dùng và hồ sơ liên quan | 650 |
| Cấu trúc đào tạo, chương trình, học kỳ, môn và lớp | 650 |
| Ghi danh, phân công, buổi học, lịch và chuyên cần | 900 |
| Điểm, thành phần điểm và yêu cầu sửa/mở khóa điểm | 1.400 |
| Đánh giá giảng viên | 450 |
| Đơn từ, cảnh báo, thông báo, phê duyệt và audit | 450 |
| Phần dự phòng cho bảng phụ/FK bắt buộc | 150 |
| **Tổng tối đa** | **5.000** |

Phải tạo truy vấn tổng hợp chứng minh tổng số dòng sau seed không vượt 5.000. Nếu schema có quá nhiều bảng bắt buộc, giảm số lượng ở từng nhóm; không tự nâng trần.

### 8. Tình huống dữ liệu cần bao phủ

Dữ liệu phải deterministic, hợp lệ theo model và đa dạng; không tạo chuỗi/tên vô nghĩa chỉ để đủ số lượng.

#### Tổ chức, tài khoản và phân quyền

- Nhiều cơ sở, khoa/phòng/ban và cây đơn vị nhiều cấp; có active/inactive nếu model hỗ trợ.
- Đủ role thật đang tồn tại như BGH, admin, giáo vụ, giảng viên, sinh viên, phụ huynh và role khác mà hệ thống dùng.
- Người dùng thuộc nhiều đơn vị/campus; có active, inactive, locked và các trạng thái thật khác nếu schema hỗ trợ.
- Email/mã không trùng; password dùng cơ chế hashing hiện tại, không lưu plaintext.
- Giữ hoặc seed lại các tài khoản smoke chuẩn của dự án bằng cơ chế seed hiện có.

#### Đào tạo và chương trình

- Dữ liệu phải đi đúng chuỗi `NganhDaoTao -> ChuyenNganh -> ChuongTrinhDaoTao -> MonHocTrongChuongTrinh`, kết hợp khóa tuyển sinh và học kỳ theo quan hệ thật.
- Có nhiều ngành, chuyên ngành, khóa tuyển sinh, chương trình, học kỳ và môn; không dồn toàn bộ dữ liệu vào một ngành hoặc một học kỳ.
- Có chương trình đang áp dụng, nháp/hết hiệu lực và trạng thái thật khác nếu model hỗ trợ.
- Có môn bắt buộc/tự chọn, nhiều số tín chỉ và học kỳ dự kiến khác nhau nếu schema hỗ trợ.

#### Lớp, lịch học và cơ sở vật chất

- Có nhiều lớp hành chính/lớp học phần, giảng viên phụ trách, sinh viên đăng ký và sĩ số khác nhau.
- Có lịch đã duyệt, chờ duyệt, thay đổi/hủy và xung đột hợp lệ để kiểm thử cảnh báo nếu model hỗ trợ.
- Có phòng lý thuyết, lab, studio/hội trường; sức chứa khác nhau; hoạt động, bảo trì và ngừng dùng nếu có trạng thái tương ứng.
- Không tạo lịch ngoài học kỳ, phòng không tồn tại hoặc buổi học orphan.

#### Điểm và phân tích học tập

- Có Pass, Fail, sát ngưỡng, điểm 0, điểm tối đa, thiếu thành phần, chưa có điểm, học lại/cải thiện và yêu cầu sửa/mở khóa điểm nếu nghiệp vụ hỗ trợ.
- Phân bố qua nhiều ngành, chuyên ngành, môn, lớp và học kỳ để lọc tổng thể hoặc từng cấp đều có kết quả khác nhau.
- Có sinh viên GPA cao, trung bình, thấp và nguy cơ rớt; không hardcode kết quả thống kê.
- Điểm và trọng số phải nằm trong miền hợp lệ; tổng trọng số đúng theo business rule hiện tại.

#### Đánh giá giảng viên

- Có giáo viên chưa được đánh giá, ít đánh giá, nhiều đánh giá và trường hợp đồng hạng.
- Có mức rất tích cực, tích cực, trung lập, tiêu cực, rất tiêu cực; nhận xét null/rỗng hợp lệ và nhận xét tiếng Việt chi tiết.
- Có nhiều chủ đề nhận xét để endpoint tổng hợp/AI analysis có dữ liệu đa dạng.

#### Workflow, cảnh báo và nhật ký

- Có đơn/yêu cầu ở các trạng thái chờ duyệt, đã duyệt, từ chối, hủy hoặc trạng thái thật tương ứng.
- Có thông báo đã đọc/chưa đọc, cảnh báo nhiều mức, nhật ký audit từ nhiều hành động và nhiều thời điểm.
- Mọi bản ghi workflow phải trỏ đến người dùng và đối tượng nghiệp vụ thật.

### 9. Seed deterministic và idempotent

- Dùng fixed random seed nếu cần sinh số liệu.
- Dùng mã/email tự nhiên ổn định để nhận diện dữ liệu seed.
- Chạy seed lần hai không tạo trùng và không làm tổng số dòng vượt 5.000.
- Không dùng `DateTime.Now` làm khóa duy nhất; thời gian kiểm thử nên cố định và phủ nhiều khoảng hợp lý.
- Tôn trọng base seed hiện tại và thứ tự FK. Có thể chỉnh/tách seeder, nhưng không sửa business logic hoặc schema.

### 10. Kiểm tra toàn vẹn toàn LMS

Xác minh tối thiểu:

- Không orphan FK và không vi phạm unique/index hiện có.
- Không trùng email, mã người dùng, mã môn, mã chương trình, mã lớp, mã tòa/phòng hoặc khóa nghiệp vụ tương đương.
- Không có người dùng trỏ tới role/đơn vị không tồn tại.
- Không có điểm, đánh giá, lịch, đăng ký, đơn từ hoặc audit trỏ đến đối tượng không tồn tại.
- Dữ liệu tenant/campus được phân bố và scope đúng theo `MaDonVi`.
- Không còn chuỗi mojibake trong các bảng ứng dụng.
- Tổng số dòng sau seed không vượt 5.000.
- Seed lần hai không làm thay đổi số lượng ngoài các cập nhật idempotent dự kiến.
- Schema, bảng, cột, constraint và lịch sử migration trước/sau phải giống nhau.

### 11. Verify API và endpoint

1. Build backend, chạy backend bằng database `LMS`, đăng nhập bằng tài khoản test thật và smoke các API hiện có.
2. Ưu tiên kiểm tra toàn bộ endpoint BGH trong các controller `Bgh*.cs`, gồm dashboard, users, academic overview/GPA/at-risk/reports/pass-fail, evaluations, schedule, approvals và master data nếu tồn tại.
3. Kiểm tra chuỗi filter BGH trả dữ liệu DB thật theo thứ tự ngành -> chuyên ngành -> môn trong chương trình -> học kỳ, bao gồm lựa chọn tổng thể nếu contract hiện tại hỗ trợ.
4. Smoke thêm endpoint chính của Student, Teacher, GiaoVu, Parent và role khác để chứng minh dữ liệu toàn LMS không orphan.
5. Với route detail, lấy ID thật từ list API; không dùng ID giả và không thêm mock/fallback.
6. Không sửa controller để ép smoke pass. Nếu API lỗi do logic hiện tại, báo endpoint, status code, response và bằng chứng liên quan.

### 12. Build và kiểm tra cuối

Chạy tối thiểu:

```powershell
cd Backend
dotnet restore
dotnet build
dotnet ef migrations list
```

Không chạy `dotnet ef migrations add`, `migrations remove`, `database drop` hoặc lệnh có thể thay đổi schema. Chỉ chạy `dotnet ef database update` nếu đã chứng minh không có migration pending và lệnh là no-op; mặc định không cần chạy.

Sau đó:

1. Chạy test backend/API hiện có.
2. Chạy seed lần một và lần hai.
3. Scan lại Unicode toàn bộ bảng ứng dụng; số lỗi mojibake phải bằng 0.
4. Thống kê số dòng từng bảng và tổng cộng.
5. So sánh schema, bảng, cột, PK/FK/index và migration history trước/sau; phải không đổi.
6. Xác minh kết nối vẫn là `LMS`, không phải `LMS_MobileDemo`.

### 13. Báo cáo bàn giao

Tạo:

```text
docs/artifacts/lms-data-reseed/LMS_DATA_RESEED_REPORT.md
```

Báo cáo phải ghi:

- Git status trước/sau và mọi file đã sửa/tạo.
- Server/database đã thao tác.
- Đường dẫn backup và kết quả `RESTORE VERIFYONLY`.
- Inventory bảng, dependency graph và số dòng trước/sau.
- Thứ tự `DELETE`, phạm vi transaction và kết quả rollback test.
- Bằng chứng không drop DB/bảng/cột, không thay schema và không thay migration.
- Kết quả scan Unicode trước/sau.
- Phân bổ dữ liệu theo module và tổng số dòng, chứng minh không vượt 5.000.
- Kết quả seed idempotency lần một/lần hai.
- Kết quả build, test và API smoke theo role.
- Tài khoản kiểm thử và hướng dẫn restore backup.
- Lỗi, endpoint thiếu dữ liệu hoặc giới hạn còn lại; không che giấu bằng mock.

### 14. Điều kiện hoàn thành

Chỉ báo hoàn thành khi:

- Backup `LMS` đã được `RESTORE VERIFYONLY` thành công trước khi xóa bản ghi.
- Database `LMS`, toàn bộ bảng, cột, constraint và migration history vẫn tồn tại, không đổi schema.
- Chỉ dữ liệu hàng trong các bảng ứng dụng được làm sạch và seed lại.
- Dữ liệu phủ các module chính của toàn LMS và đủ tình huống kiểm thử role BGH.
- Tổng số dòng dữ liệu ứng dụng sau seed không vượt 5.000.
- Seed chạy hai lần không tạo trùng.
- Không còn mojibake trong dữ liệu seed.
- Backend build thành công và endpoint trả dữ liệu thật từ `LMS`.
- Không có frontend hoặc business logic bị sửa.

Trước lệnh `DELETE` đầu tiên, hãy báo ngắn gọn: database mục tiêu, đường dẫn backup, kết quả verify backup, số bảng sẽ làm sạch, thứ tự xóa theo FK, các bảng được giữ nguyên, file seed/script sẽ sửa và tổng ngân sách dữ liệu dự kiến. Chỉ tiếp tục khi toàn bộ điều kiện an toàn phía trên đều đạt.
