# P18B Demo Script & Defense Q&A

## 1. Demo Objective

Mục tiêu demo là chứng minh hệ thống không chỉ có giao diện, mà có backend API, phân quyền, dữ liệu SQL Server thật và tính năng Smart Timetable chạy end-to-end.

## 2. Demo Environment Checklist

| Item | Command / Check | Expected |
| --- | --- | --- |
| Backend | `dotnet run` | Listening on http://localhost:5097 |
| Frontend | `npm run dev` | Vite running |
| SQL Server | `DELL\SQLEXPRESS02` or configured instance | Connected |
| Login | `p12test_staff01@lms.local` | JWT token issued |
| Smart smoke | `node docs/artifacts/p17-smart-feature-demo/p17-api-smoke.mjs` | FINAL STATUS: SUCCESS |

## 3. Recommended Demo Accounts

Use seeded demo account from LargeDemoSeeder.

## 4. 3-Minute Project Pitch

Kính thưa hội đồng, dự án của em là hệ thống quản lý học vụ (LMS) được thiết kế để giải quyết bài toán quản lý phức tạp với nhiều vai trò, khối lượng dữ liệu lớn và dễ xảy ra xung đột lịch học.

Điểm khác biệt của dự án không nằm ở giao diện mà ở luồng xử lý dữ liệu và kiến trúc:
1. **Phân quyền chặt chẽ (Role-based access & JWT)**: Mỗi loại người dùng (Student, Teacher, Parent, BGH, GiaoVu) đều có giao diện và API scope riêng biệt. Backend trực tiếp chặn quyền nếu gọi sai role, chứ không chỉ ẩn menu trên giao diện.
2. **API-backed & SQL Server thật**: Trong các luồng thuộc phạm vi demo/claim, dự án đã rà soát và loại bỏ các màn UI tĩnh, mock/fallback hoặc fake success không có API thật.
3. **Smart Timetable (Xếp lịch thông minh)**: Thay vì ghi đè trực tiếp lịch học dễ gây lỗi dữ liệu, tính năng này sinh ra bản nháp (Draft), chạy thuật toán kiểm tra xung đột trên tập dữ liệu lớn, và chỉ xuất bản (Publish) thông qua Database Transaction an toàn.

Em cũng xin minh bạch về giới hạn của hệ thống: một số thao tác thay đổi dữ liệu cấu trúc (destructive mutation) cần chạy trên tập dữ liệu an toàn (safe seed) để tránh làm hỏng dữ liệu demo diện rộng.

Sau đây, em xin phép demo trực tiếp luồng tính năng của hệ thống.

## 5. 7-Minute Demo Flow

| Minute | Action | Screen | Message |
| --- | --- | --- | --- |
| 0:00-1:00 | Login | Auth | JWT + role dashboard |
| 1:00-2:00 | Role navigation | Sidebar | Role-based screens |
| 2:00-4:30 | Smart Timetable | Schedule | Generate -> Draft -> Conflict -> Publish |
| 4:30-5:30 | API evidence | Network/Smoke JSON | Backend thật |
| 5:30-6:30 | Known limitations | Defense pack | Safe seed explanation |
| 6:30-7:00 | Close | Summary | Demo-ready |

## 6. Detailed Demo Script

### Step 1 — Login
**Action:** Mở trình duyệt, nhập username `p12test_staff01@lms.local` và password, bấm Login. Mở F12 Network tab.
**Say:** Đầu tiên, em thực hiện đăng nhập. Như thầy cô thấy trên Network tab, hệ thống đã gọi API cấp token JWT thành công và điều hướng vào Dashboard theo đúng role Giao Vụ.
**Expected:** Chuyển sang màn hình Dashboard của Giao vụ, sidebar hiển thị đúng chức năng.
**If error:** (Dự phòng) Kiểm tra lại connection string hoặc backend terminal xem có lỗi kết nối DB không.

### Step 2 — Role navigation
**Action:** Click qua lại vài menu (Quản lý lớp, Quản lý khóa học).
**Say:** Giao diện điều hướng thay đổi theo role, và mỗi màn hình đều fetch dữ liệu từ API thật thay vì dùng dữ liệu tĩnh cứng.

### Step 3 — Smart Timetable
**Action:** Mở màn hình Xếp lịch thông minh, nhấn "Tạo bản nháp mới".
**Say:** Đây là tính năng Xếp lịch thông minh. Khi em nhấn tạo, hệ thống gọi API `generate` để sinh ra một bản nháp, thay vì ghi thẳng vào lịch chính thức.
**Expected:** Màn hình hiển thị danh sách bản nháp vừa tạo.
**Action:** Click xem chi tiết bản nháp, sau đó nhấn "Kiểm tra xung đột".
**Say:** Bản nháp sẽ đi qua bộ lọc kiểm tra xung đột để đảm bảo giáo viên, lớp, phòng học không bị trùng ca.
**Action:** Nhấn "Publish" (Xuất bản).
**Say:** Cuối cùng, khi Publish, backend xử lý tạo mới lịch học bằng một Transaction. Nếu có lỗi, toàn bộ thao tác sẽ tự động rollback.

### Step 4 — API evidence
**Action:** Mở file `p17-api-smoke-results.json` hoặc Network Tab.
**Say:** Em chứng minh API thật bằng cách dùng smoke test gọi trực tiếp backend, nhận token JWT, gọi endpoint generate draft, list draft, check conflict và publish. Kết quả được lưu ở `p17-api-smoke-results.json`.

### Step 5 — Mention republish block
**Action:** Cố gắng nhấn Publish lần 2 trên cùng draft đó.
**Say:** Nếu em cố tình publish lại bản nháp này, backend sẽ chặn lại và báo lỗi để bảo vệ tính toàn vẹn của dữ liệu.

### Step 6 — Known limitations and Close
**Action:** Mở file Defense Pack.
**Say:** Một số thao tác destructive như xóa, khóa, reject hoặc payment production cần safe seed/sandbox riêng. Em không chạy bừa các action này trên DB demo để tránh phá dữ liệu, nên phần này được ghi rõ trong evidence pack thay vì nói phóng đại.

## 7. Smart Timetable Explanation

Tính năng này không ghi lịch trực tiếp vào bảng chính. Nó sinh bản nháp trước, kiểm tra xung đột giáo viên/lớp/phòng/ca, sau đó mới publish trong transaction. Nếu có conflict hoặc publish lại draft đã publish, backend chặn để tránh dữ liệu sai.
Ràng buộc kiểm tra bao gồm:
- Giáo viên không trùng ca
- Lớp không trùng ca
- Phòng không trùng ca
- Phòng/ca còn hoạt động
- Lệnh publish chạy trong database transaction nguyên tử
- Republish draft cũ sẽ bị hệ thống block.

## 8. Smart Course Allocation Explanation

Smart Course Allocation hiện là batch assignment API-backed, giúp giáo vụ phân công giáo viên cho nhiều lớp nhanh hơn. Backend kiểm tra scope và duplicate; bản ghi trùng được đưa vào skipped thay vì làm hỏng toàn bộ batch.

## 9. API/Database Evidence Talking Points

Em chứng minh API thật bằng cách dùng smoke test gọi trực tiếp backend, nhận token JWT, gọi endpoint generate draft, list draft, check conflict và publish. Kết quả được lưu ở `p17-api-smoke-results.json`. Các dữ liệu trong luồng demo chính được lấy qua API backend và truy xuất từ SQL Server thật.

## 10. Role-Based Access Talking Points

- Hệ thống dùng **JWT login** tiêu chuẩn.
- Các route và giao diện trên frontend được render theo role.
- Quan trọng nhất, **Backend vẫn enforce role**, không chỉ là frontend ẩn menu. Các API đều có attribute bảo vệ, chặn truy cập nếu tài khoản không đủ thẩm quyền (trả về 403).
- Các role Student/Teacher/Parent/BGH đều bị giới hạn dữ liệu trong scope riêng của họ (không thể tự do truy cập data của trường khác hay sinh viên khác).

## 11. Anti-Mock Explanation

Ở các phase P15-P17, em đã rà soát các màn UI tĩnh/mock/fallback. Những màn không có API thật thì hoặc được nối API, hoặc bị loại khỏi phần báo cáo. Vì vậy phần demo hoàn toàn không dựa vào fake success toast hay data giả lập.

## 12. Known Limitations Script

Một số thao tác destructive như xóa, khóa, reject hoặc payment production cần safe seed/sandbox riêng. Em không chạy bừa các action này trên DB demo để tránh phá dữ liệu, nên phần này được ghi rõ trong evidence pack thay vì claim vô lý.

## 13. Defense Q&A Bank

1. **App này có phải chỉ là giao diện không?**
   Dạ không, tất cả màn hình demo đều gọi trực tiếp API thật từ ASP.NET Core backend và thao tác với DB SQL Server.
2. **Làm sao chứng minh frontend gọi backend thật?**
   Dạ có thể F12 mở Network tab để xem request, hoặc xem JSON output của tool API smoke test báo cáo HTTP 200 OK từ server.
3. **Smart Timetable thông minh ở điểm nào?**
   Dạ thông minh ở chỗ nó sinh bản nháp và check xung đột toàn cục theo batch trước khi ghi vào lịch chính thức, tránh vỡ dữ liệu.
4. **Nếu lịch bị trùng thì sao?**
   Hệ thống sẽ báo xung đột (conflict) ngay từ bản nháp và chặn lệnh Publish, không để lọt vào database.
5. **Vì sao không ghi thẳng lịch vào bảng chính?**
   Để bảo vệ database. Nếu ghi thẳng mà lỗi nửa chừng sẽ sinh ra rác dữ liệu. Dùng Draft giúp admin rà soát lại trước khi quyết định.
6. **Publish lỗi giữa chừng thì dữ liệu có bị nửa vời không?**
   Dạ không, quá trình publish được bọc trong một Database Transaction. Nếu một bản ghi lỗi, toàn bộ thay đổi sẽ tự động rollback.
7. **Vì sao không test toàn bộ delete/cancel?**
   Vì đây là dữ liệu liên kết phức tạp. Xóa bừa bãi sẽ hỏng cấu trúc dữ liệu demo lớn. Các action này cần chạy trên tập seed an toàn riêng.
8. **Có dùng mock data không?**
   Dạ không. Hệ thống đã qua đợt rà soát và loại bỏ các màn UI tĩnh, mock/fallback không có API thật trong các luồng chính. Data demo đến từ database seed.
9. **Smart Course Allocation có phải AI không?**
   Dạ đây là batch assignment xử lý theo rule và data hiện có trên backend, không phải AI tự động chọn tối ưu 100%.
10. **Payment đã hoàn thiện trên môi trường thật chưa?**
    Dạ phần tích hợp Payment cần môi trường sandbox hoặc gateway thật để kiểm thử, hiện tại chưa thể xem là dùng được ngay trên production.
11. **Role Student có xem được dữ liệu người khác không?**
    Dạ không. Backend chặn scope theo user id hiện hành từ middleware.
12. **Teacher có sửa điểm lớp không phụ trách được không?**
    Dạ không. Backend có check scope tài khoản giáo viên đối với danh sách lớp được phân công.
13. **BGH có quyền sửa user không?**
    Dạ không, BGH chỉ có quyền read-only đối với danh sách users để xem báo cáo.
14. **Skeleton loading sao chưa làm?**
    Dạ do giới hạn thời gian, em ưu tiên nối API thật và xử lý logic backend vững chắc trước khi làm mượt trải nghiệm UI.
15. **Nếu DB không có khóa học thì thuật toán xử lý thế nào?**
    API sẽ trả về danh sách trống hoặc báo skipped, hệ thống không bị sập.
16. **Điểm khác biệt lớn nhất với CRUD thường là gì?**
    Là các xử lý nghiệp vụ diện rộng (như xếp lịch, assign lớp) dùng mô hình Draft và Transaction.
17. **Có audit log không?**
    Dạ có. Hệ thống có module audit log ở backend với API xem danh sách và chi tiết nhật ký. Một số nghiệp vụ như Auth/User/RBAC/Organizations/Program Tuition Configs có ghi audit, ngoài ra backend còn có cơ chế ghi log request cho các API đã đăng nhập. Tuy nhiên em không claim audit đã bao phủ 100% mọi field thay đổi của toàn hệ thống.
18. **Có chống fake success toast không?**
    Dạ có. Mọi thông báo thành công trên UI đều dựa vào HTTP status 2xx từ response backend, không hardcode hiện toast.
19. **Nếu republish draft thì sao?**
    Backend lưu trạng thái draft. Nếu đã publish, gọi tiếp API publish sẽ trả lỗi HTTP block ngay lập tức.
20. **Hạn chế lớn nhất hiện tại là gì?**
    Một số action write/delete cần kịch bản an toàn để demo, và UI/UX chưa được trau chuốt (skeleton, anim).
21. **Dự án có mở rộng được không?**
    Dạ được, hệ thống tuân thủ clean architecture đơn giản (Controller/Service) và module hóa theo domain.
22. **Vì sao dùng SQL Server/EF Core?**
    Vì có transaction mạnh, quản lý quan hệ phức tạp và Entity Framework hỗ trợ LINQ tiện lợi, phù hợp quy mô bài toán.
23. **Vì sao dùng JWT?**
    Vì nhẹ, stateless, dễ dàng đính kèm thông tin Role và Campus, giảm tải truy vấn user cho mọi request.
24. **Có phân quyền ở backend hay chỉ frontend?**
    Phân quyền thực sự nằm ở backend với các `[Authorize(Roles = "...")]` và logic kiểm tra scope trong Service.
25. **Nếu được làm tiếp, ưu tiên gì?**
    Em sẽ ưu tiên bổ sung kiểm thử (Unit Test/Integration Test) toàn diện cho các tính năng write và làm đẹp giao diện (Skeleton loading).

## 14. Dangerous Claims To Avoid

- Tránh nói: "100% tất cả action mutation đã được test hoàn toàn".
- Tránh nói: "Hệ thống dùng AI siêu việt hoàn chỉnh để xếp lịch".
- Tránh nói: "Chức năng thanh toán đã ready cho production thật".
- Tránh nói: "Hệ thống hoàn toàn không còn bất cứ lỗi bug nào".
- Tránh nói: "Tất cả các route đều đã được audit action runtime đầy đủ".
- Tránh nói: "Giao diện đã có skeleton và đánh bóng UX 100%".
- Tránh nói: "Có thể đem đi triển khai production cho trường dùng luôn ngay lúc này".

## 15. Final Closing Statement

**Bản 30 giây:**
Dạ, phần trình bày của em đến đây là kết thúc. Em xin tóm tắt lại: hệ thống đã đáp ứng được việc kết nối dữ liệu thật từ FE đến SQL Server, có phân quyền chặt chẽ, và giải quyết an toàn các bài toán học vụ như Xếp lịch bằng cơ chế Bản nháp & Transaction. Dù vẫn còn một vài giới hạn về giao diện và safe seed, nhưng khung kiến trúc cơ bản đã hoàn thiện. Em xin cảm ơn hội đồng đã lắng nghe.

**Bản 60 giây:**
Dạ, phần trình bày của em đến đây là kết thúc. Nhìn lại, giá trị lớn nhất dự án đạt được không phải là một giao diện đẹp tĩnh, mà là một luồng dữ liệu sống kết nối từ Frontend, qua API Backend, xuống đến SQL Server. Tính năng Xếp lịch thông minh và Phân công khóa học đã hoạt động đúng logic thực tế, tuân thủ nguyên tắc an toàn dữ liệu thông qua cơ chế Transaction và Bản nháp. Hệ thống cũng thực thi phân quyền triệt để dựa trên JWT và scope thực tế của từng tài khoản. Mặc dù vẫn còn giới hạn ở các action xóa/sửa sâu cần kịch bản an toàn và UI chưa hoàn hảo 100%, em tin rằng khung kiến trúc này đủ vững chắc để mở rộng. Em xin cảm ơn các thầy cô đã dành thời gian theo dõi.
