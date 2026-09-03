# Tài liệu Môn COM102 — Lập trình Hướng Đối tượng với Java
> **Mã môn học:** COM102  
> **Số tín chỉ:** 3 TC  
> **Học kỳ:** Học kỳ 2 (sau khi đạt COM101)  
> **Môn tiên quyết:** COM101 — Nhập môn lập trình (điểm C trở lên)  
> **Giảng viên phụ trách:** Bộ môn Khoa học Máy tính — AET Academy

---

## 1. Mô tả môn học

COM102 — **Lập trình Hướng Đối tượng (Object-Oriented Programming)** là môn học bắt buộc trong chương trình đào tạo ngành Kỹ thuật Phần mềm (SE). Môn học trang bị cho sinh viên kiến thức và kỹ năng lập trình theo mô hình hướng đối tượng sử dụng ngôn ngữ **Java**.

### Mục tiêu môn học (Course Learning Outcomes — CLO)
| CLO | Nội dung | Mức độ |
|---|---|---|
| CLO1 | Hiểu và áp dụng 4 tính chất OOP: Đóng gói, Kế thừa, Đa hình, Trừu tượng | Áp dụng |
| CLO2 | Thiết kế class, interface, abstract class phù hợp | Phân tích |
| CLO3 | Xử lý ngoại lệ (Exception Handling) trong Java | Áp dụng |
| CLO4 | Sử dụng Java Collections Framework | Áp dụng |
| CLO5 | Đọc hiểu UML Class Diagram cơ bản | Hiểu |
| CLO6 | Viết code sạch, có comment, đặt tên đúng chuẩn Java | Tổng hợp |

---

## 2. Nội dung chương trình học (Syllabus)

### Chương 1: Ôn tập Java căn bản và giới thiệu OOP (2 tuần)
- Review: Biến, kiểu dữ liệu, vòng lặp, mảng từ COM101
- Khái niệm Object (Đối tượng) và Class (Lớp)
- Sự khác nhau giữa lập trình thủ tục (procedural) và OOP
- Cài đặt môi trường: JDK 17, IntelliJ IDEA / VS Code

### Chương 2: Class và Object — Tính Đóng gói (3 tuần)
- Định nghĩa class, thuộc tính (field), phương thức (method)
- Constructor (hàm khởi tạo): Default, parameterized, copy constructor
- Access modifiers: `public`, `private`, `protected`, `default`
- **Encapsulation (Đóng gói):** Getter, Setter, `this` keyword
- Static fields và static methods
- **Bài tập thực hành:** Xây dựng class `BankAccount`, `Student`, `Product`

```java
// Ví dụ: Class Student với đóng gói
public class Student {
    private String studentId;
    private String fullName;
    private double gpa;
    
    public Student(String studentId, String fullName) {
        this.studentId = studentId;
        this.fullName = fullName;
        this.gpa = 0.0;
    }
    
    public String getStudentId() { return studentId; }
    public String getFullName() { return fullName; }
    public double getGpa() { return gpa; }
    
    public void setGpa(double gpa) {
        if (gpa >= 0.0 && gpa <= 10.0) {
            this.gpa = gpa;
        }
    }
    
    @Override
    public String toString() {
        return "Student[" + studentId + ": " + fullName + ", GPA=" + gpa + "]";
    }
}
```

### Chương 3: Tính Kế thừa — Inheritance (2 tuần)
- Từ khóa `extends` và quan hệ IS-A
- `super` keyword: Gọi constructor cha, phương thức cha
- Method Overriding (Ghi đè phương thức)
- `final` class và `final` method
- Chuỗi kế thừa (Inheritance chain)
- **Bài tập:** Xây dựng hierarchy `Person → Employee → Teacher/Student`

### Chương 4: Tính Đa hình — Polymorphism (2 tuần)
- **Compile-time polymorphism:** Method Overloading (Nạp chồng)
- **Runtime polymorphism:** Method Overriding + Upcasting
- Interface: Định nghĩa, implements, default methods (Java 8+)
- Abstract class vs Interface — Khi nào dùng cái nào?
- `instanceof` operator
- **Bài tập:** Hệ thống tính diện tích các hình học (Shape → Circle, Rectangle, Triangle)

```java
// Ví dụ: Đa hình với Interface
public interface Printable {
    void print();
    default void printWithBorder() {
        System.out.println("=".repeat(50));
        print();
        System.out.println("=".repeat(50));
    }
}

public abstract class Shape implements Printable {
    protected String color;
    public abstract double getArea();
    
    @Override
    public void print() {
        System.out.println(getClass().getSimpleName() + " | Color: " + color + " | Area: " + getArea());
    }
}
```

### Chương 5: Exception Handling — Xử lý ngoại lệ (1 tuần)
- Hệ thống Exception trong Java: `Throwable → Error / Exception`
- Checked vs Unchecked Exception
- `try-catch-finally`, `try-with-resources`
- `throw` và `throws`
- Tạo Custom Exception
- **Bài tập:** Xử lý ngoại lệ trong class BankAccount (số dư âm, rút quá số dư)

### Chương 6: Java Collections Framework (2 tuần)
- `List`: `ArrayList`, `LinkedList` — Khi nào dùng cái nào?
- `Set`: `HashSet`, `TreeSet` — Phần tử không trùng lặp
- `Map`: `HashMap`, `TreeMap` — Key-Value pairs
- `Iterator` và `for-each`
- Generics (Generic types): `List<String>`, `Map<Integer, Student>`
- Sắp xếp: `Comparable`, `Comparator`
- **Bài tập:** Quản lý danh sách sinh viên với CRUD đầy đủ

### Chương 7: File I/O và Serialization (1 tuần)
- Đọc/Ghi file text: `FileReader`, `BufferedReader`, `PrintWriter`
- Serialization: Lưu object vào file nhị phân
- **Bài tập:** Lưu và nạp danh sách sinh viên từ file

### Chương 8: Giới thiệu UML và Design Patterns cơ bản (1 tuần)
- UML Class Diagram: Quan hệ Association, Aggregation, Composition, Inheritance
- Design Pattern: Singleton, Factory (giới thiệu)
- **Dự án cuối kỳ:** Hệ thống quản lý thư viện sách (Library Management System)

---

## 3. Cấu trúc điểm số môn COM102

| Thành phần | Trọng số | Chi tiết |
|---|---|---|
| Điểm quá trình (Assignments) | 30% | 4 bài tập lập trình, mỗi bài 7.5% |
| Điểm giữa kỳ | 20% | Bài kiểm tra viết tay 60 phút (tuần 8) |
| Dự án cuối kỳ | 20% | Hệ thống OOP hoàn chỉnh (tuần 14–15) |
| Thi cuối kỳ | 30% | Bài thi tự luận + viết code 90 phút |
| **Tổng** | **100%** | Điểm tổng kết thang 10 |

**Điều kiện đạt môn:** Điểm tổng kết ≥ 4.0 / 10

---

## 4. Tài liệu tham khảo

### Giáo trình chính
1. **Head First Java, 3rd Edition** — Kathy Sierra & Bert Bates (O'Reilly, 2022)
2. **Effective Java, 3rd Edition** — Joshua Bloch (Addison-Wesley, 2018)

### Tài liệu bổ sung
- Oracle Java Documentation: https://docs.oracle.com/en/java/
- Java Tutorial by Programiz: https://www.programiz.com/java-programming
- Baeldung Java Guides: https://www.baeldung.com

### Công cụ phát triển
- **JDK:** OpenJDK 17 LTS (miễn phí) hoặc Oracle JDK 17
- **IDE:** IntelliJ IDEA Community Edition (khuyến nghị) hoặc VS Code + Extension Pack for Java
- **Kiểm tra code:** Checkstyle plugin (code style) + JUnit 5 (unit testing cơ bản)

---

## 5. Các câu hỏi và khái niệm thường gặp

### OOP là gì?
OOP (Object-Oriented Programming — Lập trình Hướng Đối tượng) là mô hình lập trình tổ chức code xung quanh "đối tượng" thay vì "hàm và logic". Mỗi đối tượng chứa **dữ liệu (thuộc tính)** và **hành vi (phương thức)**.

### 4 tính chất OOP
1. **Encapsulation (Đóng gói):** Ẩn chi tiết cài đặt, chỉ lộ interface cần thiết qua getter/setter.
2. **Inheritance (Kế thừa):** Class con tái sử dụng code từ class cha qua `extends`.
3. **Polymorphism (Đa hình):** Cùng một method hoạt động khác nhau với các loại đối tượng khác nhau.
4. **Abstraction (Trừu tượng):** Ẩn sự phức tạp, chỉ show những gì cần thiết qua abstract class/interface.

### Sự khác nhau giữa Abstract Class và Interface
| Tiêu chí | Abstract Class | Interface |
|---|---|---|
| Từ khóa | `abstract class` | `interface` |
| Kế thừa | `extends` (chỉ 1) | `implements` (nhiều) |
| Constructor | Có | Không |
| Field | Có (mọi loại) | Chỉ `public static final` |
| Method | Có thể có/không abstract | Mặc định `abstract` (Java 8+ có `default`) |
| Khi dùng | Chia sẻ code giữa các class liên quan | Định nghĩa contract cho các class không liên quan |

---

## 6. Dự án cuối kỳ — Library Management System

### Yêu cầu bắt buộc:
- Thiết kế ít nhất **5 classes** có quan hệ kế thừa/interface
- Sử dụng **Collections** (ArrayList hoặc HashMap) để lưu dữ liệu
- **Exception Handling** đầy đủ cho các trường hợp lỗi
- **File I/O:** Lưu/nạp dữ liệu từ file
- **UML Class Diagram** (vẽ bằng draw.io hoặc PlantUML)
- Code clean, có Javadoc comment

### Chức năng tối thiểu:
1. Quản lý sách (Thêm, Xóa, Sửa, Tìm kiếm)
2. Quản lý độc giả (Đăng ký thành viên)
3. Mượn/Trả sách (ghi nhận ngày, tính phí phạt trễ hạn)
4. Báo cáo: Sách đang mượn, lịch sử mượn trả

### Tiêu chí chấm:
- Đúng yêu cầu chức năng: 40%
- Chất lượng thiết kế OOP: 30%
- Code sạch và có test: 20%
- Báo cáo + UML: 10%

