# Backend Business Audit – LMS Academic Management System

> Tài liệu tổng hợp nhằm làm rõ: hệ thống làm gì, dữ liệu được tổ chức thế nào, luồng nghiệp vụ chạy như thế nào, ai làm gì, và đâu là phần đã triển khai thực tế và đâu là phần còn thiếu/đang chưa hoàn chỉnh.
>
> Cập nhật lần cuối: 2026-07-02

## 1. Tóm tắt kết luận

Hệ thống này là một nền tảng LMS/Academic Management System hướng tới quản trị đào tạo, học vụ và vận hành trường học. Ở mức hiện tại, backend đã có nền tảng khá vững cho các module chính:

- Xác thực và phân quyền
- Tổ chức/cơ sở/chi nhánh
- Đơn từ (applications)
- Thông báo
- Khen thưởng – kỷ luật
- Thời khóa biểu – điểm danh
- Học tập sinh viên: khóa học, bài tập, chương trình đào tạo, học phí
- Báo cáo và audit log

Điểm quan trọng là hệ thống không chỉ là “CRUD cơ bản”, mà đã có các luồng trạng thái, kiểm soát scope theo campus, lịch sử thao tác, audit trail và quy trình phê duyệt. Đây là một hệ thống có cấu trúc nghiệp vụ rõ, dù vẫn ở mức “foundation + workflow core” chứ chưa phải “full production-ready LMS” hoàn chỉnh.

## 2. Mục tiêu nghiệp vụ của hệ thống

Hệ thống nhằm hỗ trợ các vai trò sau trong môi trường đào tạo:

- SuperAdmin / Admin: quản trị toàn hệ thống
- CampusAdmin / SubCampusAdmin: quản trị theo cơ sở/chi nhánh
- AcademicStaff: nhân viên học vụ
- Teacher: giảng viên
- Student: sinh viên/học sinh
- Parent: phụ huynh
- Principal / Chairman: quản lý cấp cao, báo cáo, đánh giá

Nói ngắn gọn, hệ thống làm 3 việc chính:

1. Quản lý người dùng và tổ chức
2. Quản lý các quy trình học vụ và hành chính
3. Theo dõi, báo cáo và audit các thao tác

## 3. Kiến trúc dữ liệu chính

Backend dùng ASP.NET Core + EF Core + SQL Server. DbContext chính là [Backend/Data/ApplicationDbContext.cs](../Backend/Data/ApplicationDbContext.cs), chứa rất nhiều DbSet cho các nhóm dữ liệu.

### 3.1 Nhóm dữ liệu người dùng và phân quyền

- NguoiDung: tài khoản người dùng
- VaiTro: vai trò
- PhanQuyenNguoiDung: gán vai trò cho người dùng
- TokenLamMoi: refresh token

Vai trò này là nền tảng cho login, access control, scope và audit.

### 3.2 Nhóm dữ liệu tổ chức và cơ sở

- DonVi: đơn vị/cơ sở/chi nhánh
- ToaNha, Tang, PhongHoc: cơ sở vật chất

Đây là nền tảng cho việc phân vùng dữ liệu theo campus, cơ sở và phòng học.

### 3.3 Nhóm dữ liệu học tập

- KhoaHoc, Chuong, BaiHoc, TienDoBaiHoc
- BaiTap, BaiNop, NopBaiDanhGia
- ChuongTrinhDaoTao, MonHocTrongChuongTrinh, LopHocPhan, DangKyHocPhan
- DiemSo, DiemDanh

Nhóm này mô tả “học vụ” của sinh viên và giảng viên.

### 3.4 Nhóm dữ liệu thi và kiểm tra

- DeKiemTra, CauHoi, CauHoiDeKiemTra, PhienThiHocSinh
- KyThi, LichThiTong, CaThi, PhanCongGiamThi, ThiSinhCaThi, DiemDanhThi

Đây là phần thi cử, coi thi, giám sát và xử lý vi phạm.

### 3.5 Nhóm dữ liệu đơn từ và workflow

- DonTu: hồ sơ đơn từ của người dùng
- MauDonTu: mẫu đơn
- TepDinhKemDonTu: minh chứng kèm theo
- NhatKyDuyetDon: lịch sử workflow
- NhatKyKiemToan: audit log

Đây là module có cấu trúc nghiệp vụ nhất: có trạng thái, có phân công người xử lý, có ghi nhận lịch sử và có kiểm soát scope.

### 3.6 Nhóm dữ liệu thông báo

- ThongBao, ThongBaoNguoiNhan, MauThongBao, ThongBaoHenGio

Phục vụ cho trung tâm thông báo và gửi thông báo theo nhóm vai trò/cơ sở.

### 3.7 Nhóm dữ liệu khen thưởng – kỷ luật

- DotKhenThuong, KhenThuong, HoSoKyLuat, KhieuNaiKyLuat
- MauBangKhen

Đây là một module nghiệp vụ riêng có luận lý xử lý và chứng nhận.

## 4. Kiến trúc backend

Backend tổ chức theo pattern controller → service → DbContext → SQL Server.

### 4.1 Các lớp chính

- Controllers: tiếp nhận request API
- Services: chứa nghiệp vụ thật
- DTOs: định nghĩa request/response
- Models: entity EF
- Middlewares: xử lý exception, JWT, first-login, campus scope

### 4.2 Middleware và bảo mật

Backend có các middleware quan trọng:

- ExceptionMiddleware: chuẩn hóa lỗi
- JwtMiddleware: đọc JWT và đưa context user vào request
- FirstLoginMiddleware: xử lý trạng thái đổi mật khẩu lần đầu
- CampusScopeMiddleware: hỗ trợ scope theo campus

### 4.3 Authentication flow

Luồng đăng nhập hiện có như sau:

1. Client gọi POST /api/auth/login
2. AuthService kiểm tra thông tin tài khoản
3. Nếu hợp lệ, backend sinh JWT và refresh token
4. Client dùng JWT cho các endpoint protected
5. Current user được lấy từ token và gắn vào request context

## 5. Các module nghiệp vụ chính và luồng vận hành

### 5.1 Xác thực và phân quyền

Đây là nền tảng vận hành hệ thống.

- Login, refresh token, logout, change password đều có API
- Phân quyền theo vai trò và policy
- Scope dữ liệu theo campus/đơn vị được áp dụng ở nhiều module

Vai trò không chỉ quyết định xem được gì mà còn quyết định phạm vi dữ liệu nào được phép truy cập.

### 5.2 Quản trị tổ chức và cơ sở

Module tổ chức cho phép:

- Tạo/sửa/xóa đơn vị
- Xem cây tổ chức
- Xem subtree theo tổ chức cha
- Xóa mềm và xóa cứng

Nghĩa là hệ thống có tư duy “cây tổ chức” và “phạm vi quản lý”, phù hợp với mô hình trường học nhiều cơ sở.

### 5.3 Quản lý học tập sinh viên

Sinh viên có thể xem:

- Dashboard tổng quan
- Khóa học đang học
- Chương trình đào tạo
- Bài tập, bài nộp
- Điểm danh
- Học phí và giao dịch
- Các hồ sơ kỷ luật và khiếu nại

Luồng này khá rõ: sinh viên truy cập dữ liệu của chính mình, backend phải re-query và giới hạn theo MaHocSinh / user hiện tại.

### 5.4 Workflow đơn từ (applications)

Đây là module nghiệp vụ phức tạp nhất và có cấu trúc tốt nhất.

#### Luồng cơ bản

1. Sinh viên tạo đơn nháp
2. Sinh viên chỉnh sửa hoặc nộp đơn
3. Admin/academic staff tiếp nhận và phân công xử lý
4. Người xử lý có thể yêu cầu bổ sung hồ sơ
5. Người xử lý có thể phê duyệt hoặc từ chối
6. Sau khi duyệt, hệ thống có bước xử lý nghiệp vụ sau duyệt
7. Hệ thống ghi lịch sử, audit, và thông báo

#### Trạng thái và chuyển trạng thái

- Nháp → đã nộp → đang xem xét → yêu cầu bổ sung / phê duyệt / từ chối
- Sau duyệt có trạng thái xử lý nghiệp vụ tiếp theo
- Hệ thống có state machine để ngăn chuyển trạng thái sai

#### Các quy tắc quan trọng

- Sinh viên chỉ được truy cập đơn của chính mình
- Admin chỉ xem đơn trong phạm vi campus được phép
- Có row version / concurrency control để tránh ghi đè dữ liệu
- Có lock transaction để tránh race condition
- Các thao tác quan trọng ghi cả timeline và audit log

Đây là module thể hiện rõ “hệ thống có nghiệp vụ thật”, không chỉ là CRUD.

### 5.5 Thông báo và trung tâm thông báo

Module thông báo đã được thiết kế thành hệ thống riêng:

- Gửi thông báo theo người nhận
- Ghi trạng thái đã đọc/chưa đọc
- Hỗ trợ template và thông báo chuyên biệt
- Cho phép preview người nhận trước khi gửi

Nói cách khác, thông báo là một module vận hành độc lập nhưng có thể được kích hoạt từ nhiều workflow khác nhau như đơn từ, reward/discipline, attendance, tuition.

### 5.6 Khen thưởng – kỷ luật

Module này có các bước chính:

- Tạo hồ sơ kỷ luật
- Duyệt/điều chỉnh hiệu lực
- Học sinh xem hồ sơ của mình
- Tạo khiếu nại
- Admin xử lý khiếu nại
- Có thể liên kết với reward campaign và certificate

Đây là một module học thuật và hành chính kết hợp, có cả luồng đánh giá và luồng giải quyết tranh chấp.

### 5.7 Thời khóa biểu và điểm danh

Module này cho phép:

- Quản lý ca học
- Quản lý thời khóa biểu
- Tạo buổi học từ thời khóa biểu
- Điểm danh
- Mở/khóa điểm danh
- Xử lý yêu cầu mở khóa điểm danh

Luồng này khá thực tế cho hệ thống giáo dục: lịch học → tạo buổi học → điểm danh → xử lý phát sinh.

### 5.8 Học phí và thanh toán

Module học phí có phần:

- Cấu hình học phí theo chương trình
- Tạo hóa đơn và giao dịch
- Thanh toán qua PayOS/VietQR
- Theo dõi trạng thái thanh toán

Đây là một module vận hành tài chính, không chỉ là dữ liệu học vụ.

## 6. Luồng vận hành thực tế của hệ thống

### Scenario 1: Sinh viên nộp đơn

1. Sinh viên đăng nhập
2. Hệ thống xác định role = Student
3. Sinh viên tạo đơn nháp
4. Hệ thống gắn mẫu đơn và validate form
5. Sinh viên nộp đơn
6. Đơn đi vào hàng đợi admin
7. Admin tiếp nhận/phân công
8. Quy trình review/approve/reject diễn ra
9. Sau duyệt, bước xử lý nghiệp vụ tiếp tục
10. Hệ thống gửi thông báo và ghi log

### Scenario 2: Admin xử lý đơn

1. Admin đăng nhập
2. Hệ thống giới hạn dữ liệu theo campus scope
3. Admin xem queue đơn từ
4. Admin tiếp nhận / phân công đơn cho người phù hợp
5. Admin có thể yêu cầu bổ sung, phê duyệt, từ chối
6. Hệ thống ghi timeline và audit log
7. Hệ thống tạo thông báo tới sinh viên liên quan

### Scenario 3: Giáo viên/nhân viên học vụ quản lý điểm danh

1. Nhân viên mở lịch học / tạo buổi học
2. Hệ thống sinh các buổi học từ thời khóa biểu
3. Giáo viên/nhân viên thực hiện điểm danh
4. Nếu có sai sót thì có thể mở khóa điểm danh / tạo yêu cầu mở khóa
5. Hệ thống ghi lại thay đổi và trạng thái

### Scenario 4: Xử lý kỷ luật và khiếu nại

1. Hệ thống tạo hồ sơ kỷ luật
2. Sinh viên xem hồ sơ và có thể gửi khiếu nại
3. Admin xem và xử lý khiếu nại
4. Nếu có hiệu lực/không hiệu lực thì cập nhật hồ sơ và log hành động

## 7. Những phần đã triển khai tốt

### Đã có nền tảng mạnh

- API auth và authorization rõ ràng
- Phân quyền dựa trên role/policy
- Module organizations có tư duy scope
- Module applications có state machine và audit trail
- Module notifications có thiết kế riêng và đầy đủ
- Module reward/discipline có workflow thực tế
- Module schedule/attendance có model và controller rõ

### Có tính “workflow-driven”

Không phải chỉ là danh sách dữ liệu; hệ thống đã bắt đầu xây dựng luồng nghiệp vụ thật:

- draft → submit → review → approve/reject
- pending → process → record result
- create → appeal → resolve
- schedule → session → attendance

## 8. Những phần còn thiếu hoặc đang ở mức partial

### 8.1 Frontend chưa đồng bộ đầy đủ

- Nhiều view vẫn dùng mock data hoặc inline demo data
- Một số role vẫn chưa kết nối thật API
- Một số endpoint backend có nhưng frontend chưa dùng

### 8.2 Một số module có backend nhưng chưa full integration

- Teacher/staff/BGH/parent portal còn thiếu hoặc chưa hoàn thiện
- Một số dashboard/route vẫn là mock hoặc placeholder

### 8.3 Một số nghiệp vụ vẫn cần triển khai sâu hơn

- Notification trigger tự động cho một số module chưa đầy đủ
- Một số reports/charts/analytics vẫn chưa hoàn thiện
- Production deployment, env config và operational playbook cần chuẩn hóa

## 9. Mức độ maturity hiện tại

| Module | Trạng thái | Nhận định |
|---|---|---|
| Auth & access | Good | Đã có nền tảng rõ ràng |
| Organizations | Good | Có model và API rõ |
| Applications | Strong | Là module có nghiệp vụ phong phú nhất |
| Notifications | Good | Có module riêng, có thể dùng độc lập |
| Reward & Discipline | Good | Có workflow và API rõ |
| Schedule & Attendance | Good | Có model, controller, workflow thực tế |
| Student learning modules | Partial | Có nhiều endpoint nhưng chưa phải toàn bộ LMS đầy đủ |
| Frontend role integration | Partial | Còn nhiều mock/placeholder |
| Production readiness | Partial | Còn cần chuẩn hóa môi trường, test và deployment |

## 10. Kết luận cuối cùng

Nếu nhìn theo góc độ “hệ thống có nghiệp vụ thật”, thì repo này đã đi khá xa. Nó không chỉ là một demo CRUD; nó có:

- cấu trúc dữ liệu học thuật đủ rộng,
- backend API có tầng service riêng,
- nhiều workflow có trạng thái và audit,
- phân quyền và scope rõ,
- đủ cơ sở để mở rộng thành một LMS trường học đáng tin cậy.

Tuy nhiên, nếu nhìn theo góc độ “sẵn sàng dùng thật trong vận hành trường học”, thì hệ thống vẫn còn ở mức “foundation + workflow core”, chưa phải “production-ready hoàn chỉnh” vì còn cần:

- frontend role-by-role integration,
- test tự động và integration test đầy đủ,
- chuẩn hóa dữ liệu và environment,
- hoàn thiện một số module còn thiếu hoặc chưa nối thật.

## 11. Khuyến nghị ưu tiên tiếp theo

1. Hoàn thiện frontend cho các role đã có backend thật
2. Tập trung vào student/teacher/staff workflows đã có endpoint
3. Bổ sung test cho schedule/attendance và các workflow quan trọng
4. Chuẩn hóa báo cáo và dashboard thực tế
5. Cập nhật API contract và documentation cho các module mới
6. Đẩy mạnh notification trigger từ workflow nghiệp vụ sang người dùng thật

---

Nếu cần, bước tiếp theo có thể là: chuyển tài liệu này thành một “business process map” theo từng vai trò (Student / Admin / Teacher / Staff / BGH) với sơ đồ luồng cụ thể từng màn hình và từng API.
