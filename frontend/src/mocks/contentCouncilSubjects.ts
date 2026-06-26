export interface ContentCouncilSubject {
  id: number
  code: string
  name: string
  shortDescription: string
  status: 'empty' | 'draft' | 'completed'
  chapterCount: number
  lessonCount: number
  contentCount: number
  quizCount: number
  updatedAt: string
}

export const mockSubjects: ContentCouncilSubject[] = [
  {
    id: 1,
    code: 'COM101',
    name: 'Nhập môn Công nghệ thông tin',
    shortDescription: 'Cung cấp kiến thức nền tảng về máy tính, mạng và tư duy lập trình.',
    status: 'draft',
    chapterCount: 4,
    lessonCount: 18,
    contentCount: 35,
    quizCount: 5,
    updatedAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(), // 2 hours ago
  },
  {
    id: 2,
    code: 'WEB201',
    name: 'Lập trình Web',
    shortDescription: 'HTML, CSS, JavaScript cơ bản và cách xây dựng giao diện web tương tác.',
    status: 'completed',
    chapterCount: 6,
    lessonCount: 24,
    contentCount: 50,
    quizCount: 10,
    updatedAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(), // 1 day ago
  },
  {
    id: 3,
    code: 'MOB101',
    name: 'Lập trình Mobile',
    shortDescription: 'Phát triển ứng dụng di động đa nền tảng với React Native hoặc Flutter.',
    status: 'empty',
    chapterCount: 0,
    lessonCount: 0,
    contentCount: 0,
    quizCount: 0,
    updatedAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(), // 7 days ago
  },
  {
    id: 4,
    code: 'DBI202',
    name: 'Cơ sở dữ liệu',
    shortDescription: 'Thiết kế, truy vấn và quản lý dữ liệu với SQL Server.',
    status: 'draft',
    chapterCount: 5,
    lessonCount: 15,
    contentCount: 20,
    quizCount: 3,
    updatedAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(), // 5 hours ago
  },
  {
    id: 5,
    code: 'NET102',
    name: 'Mạng máy tính',
    shortDescription: 'Tìm hiểu về mô hình OSI, TCP/IP và cấu hình mạng cơ bản.',
    status: 'completed',
    chapterCount: 7,
    lessonCount: 21,
    contentCount: 42,
    quizCount: 7,
    updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(), // 2 days ago
  },
  {
    id: 6,
    code: 'PRO192',
    name: 'Lập trình Java',
    shortDescription: 'Lập trình hướng đối tượng cơ bản với ngôn ngữ Java.',
    status: 'empty',
    chapterCount: 0,
    lessonCount: 0,
    contentCount: 0,
    quizCount: 0,
    updatedAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(), // 10 days ago
  },
  {
    id: 7,
    code: 'SOF203',
    name: 'Kiểm thử phần mềm',
    shortDescription: 'Các kỹ thuật, quy trình và công cụ kiểm thử phần mềm tự động hóa.',
    status: 'draft',
    chapterCount: 4,
    lessonCount: 12,
    contentCount: 25,
    quizCount: 4,
    updatedAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(), // 30 minutes ago
  },
  {
    id: 8,
    code: 'SDN302',
    name: 'Phát triển ứng dụng',
    shortDescription: 'Xây dựng ứng dụng doanh nghiệp đầy đủ tính năng.',
    status: 'completed',
    chapterCount: 8,
    lessonCount: 30,
    contentCount: 65,
    quizCount: 12,
    updatedAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(), // 14 days ago
  }
]
