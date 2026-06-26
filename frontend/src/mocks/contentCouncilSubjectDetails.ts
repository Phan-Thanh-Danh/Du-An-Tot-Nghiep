export type ContentStatus = 'draft' | 'published' | 'hidden' | 'empty'
export type LessonType = 'video' | 'pdf' | 'text' | 'quiz' | 'slide' | 'slide_html' | 'document'

export interface ContentCouncilContentBlock {
  id: number
  lessonId: number
  type: 'video' | 'slide_html' | 'document' | 'text' | 'quiz'
  title: string
  order: number
  status: 'draft' | 'published' | 'hidden'
  html?: string
  description?: string
  fileName?: string
  fileUrl?: string
  fileSize?: number
  fileType?: string
  videoUrl?: string
  durationSeconds?: number
  quizId?: number
  quizTitle?: string
  quizQuestionCount?: number
  quizDurationMinutes?: number
  quizTotalScore?: number
  quizCompletionRule?: 'pass' | 'submit'
  NoiDungJson?: string
  NoiDungHtml?: string
}

export interface ContentCouncilLessonOverview {
  id: number
  title: string
  order: number
  type: LessonType | string
  status: ContentStatus | string
  contentCount: number
  contents?: ContentCouncilContentBlock[]
}

export interface ContentCouncilChapterOverview {
  id: number
  title: string
  order: number
  hidden: boolean
  lessons: ContentCouncilLessonOverview[]
}

export interface ContentCouncilSubjectDetail {
  id: number
  code: string
  name: string
  shortDescription: string
  status: 'empty' | 'draft' | 'completed'
  updatedAt: string
  authorName?: string
  chapters: ContentCouncilChapterOverview[]
}

export const mockSubjectDetails: ContentCouncilSubjectDetail[] = [
  {
    id: 1,
    code: 'COM101',
    name: 'Nhập môn Công nghệ thông tin',
    shortDescription: 'Cung cấp kiến thức nền tảng về máy tính, mạng và tư duy lập trình.',
    status: 'draft',
    updatedAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    authorName: 'Nguyễn Văn A',
    chapters: [
      {
        id: 101,
        title: 'Tổng quan CNTT',
        order: 1,
        hidden: false,
        lessons: [
          { 
            id: 1001, title: 'Hệ thống máy tính là gì?', order: 1, type: 'video', status: 'published', contentCount: 2,
            contents: [
              { id: 10001, lessonId: 1001, type: 'video', title: 'Giới thiệu hệ thống LMS', order: 1, status: 'published', videoUrl: 'https://example.com/video1.mp4', durationSeconds: 510, description: 'Video giới thiệu ngắn gọn' },
              { id: 10002, lessonId: 1001, type: 'text', title: 'Tóm tắt khái niệm', order: 2, status: 'published', html: '<p>LMS là Hệ thống quản lý học tập...</p>' }
            ]
          },
          { 
            id: 1002, title: 'Lịch sử phát triển', order: 2, type: 'text', status: 'published', contentCount: 1,
            contents: [
              { id: 10003, lessonId: 1002, type: 'slide_html', title: 'Slide Lịch sử phát triển', order: 1, status: 'published', NoiDungJson: '{"time":1710000000000,"version":"2.28.2","blocks":[{"id":"b1","type":"header","data":{"text":"Lịch sử phát triển","level":2}},{"id":"b2","type":"paragraph","data":{"text":"Máy tính trải qua nhiều thế hệ..."}}]}' }
            ]
          },
          { 
            id: 1003, title: 'Bài tập trắc nghiệm chương 1', order: 3, type: 'quiz', status: 'draft', contentCount: 1,
            contents: [
              { id: 10004, lessonId: 1003, type: 'quiz', title: 'Quiz chương 1', order: 1, status: 'draft', quizId: 1, quizTitle: 'Quiz chương 1', quizQuestionCount: 10, quizDurationMinutes: 15, quizTotalScore: 10, quizCompletionRule: 'pass' }
            ]
          }
        ]
      },
      {
        id: 102,
        title: 'Phần cứng máy tính',
        order: 2,
        hidden: false,
        lessons: [
          { id: 1004, title: 'Các thành phần cơ bản', order: 1, type: 'slide', status: 'published', contentCount: 3, contents: [] },
          { id: 1005, title: 'CPU và Bộ nhớ', order: 2, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 1006, title: 'Thiết bị ngoại vi', order: 3, type: 'pdf', status: 'empty', contentCount: 0, contents: [] }
        ]
      },
      {
        id: 103,
        title: 'Phần mềm và Hệ điều hành',
        order: 3,
        hidden: true,
        lessons: [
          { id: 1007, title: 'Phân loại phần mềm', order: 1, type: 'video', status: 'draft', contentCount: 1, contents: [] },
          { id: 1008, title: 'Hệ điều hành Windows', order: 2, type: 'slide', status: 'draft', contentCount: 1, contents: [] }
        ]
      }
    ]
  },
  {
    id: 2,
    code: 'WEB201',
    name: 'Lập trình Web',
    shortDescription: 'HTML, CSS, JavaScript cơ bản và cách xây dựng giao diện web tương tác.',
    status: 'completed',
    updatedAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    authorName: 'Trần Thị B',
    chapters: [
      {
        id: 201,
        title: 'HTML & CSS Cơ bản',
        order: 1,
        hidden: false,
        lessons: [
          { id: 2001, title: 'Giới thiệu HTML', order: 1, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 2002, title: 'CSS Selector', order: 2, type: 'video', status: 'published', contentCount: 2, contents: [] },
          { id: 2003, title: 'Thực hành HTML & CSS', order: 3, type: 'quiz', status: 'published', contentCount: 1, contents: [] }
        ]
      },
      {
        id: 202,
        title: 'JavaScript căn bản',
        order: 2,
        hidden: false,
        lessons: [
          { id: 2004, title: 'Biến và Kiểu dữ liệu', order: 1, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 2005, title: 'Hàm và Sự kiện', order: 2, type: 'text', status: 'published', contentCount: 2, contents: [] }
        ]
      }
    ]
  },
  {
    id: 3,
    code: 'MOB101',
    name: 'Lập trình Mobile',
    shortDescription: 'Phát triển ứng dụng di động đa nền tảng với React Native hoặc Flutter.',
    status: 'empty',
    updatedAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    authorName: 'Lê Văn C',
    chapters: []
  },
  {
    id: 4,
    code: 'DBI202',
    name: 'Cơ sở dữ liệu',
    shortDescription: 'Thiết kế, truy vấn và quản lý dữ liệu với SQL Server.',
    status: 'draft',
    updatedAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
    authorName: 'Phạm Văn D',
    chapters: []
  },
  {
    id: 5,
    code: 'NET102',
    name: 'Mạng máy tính',
    shortDescription: 'Tìm hiểu về mô hình OSI, TCP/IP và cấu hình mạng cơ bản.',
    status: 'completed',
    updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    authorName: 'Nguyễn Thị E',
    chapters: []
  },
  {
    id: 6,
    code: 'PRO192',
    name: 'Lập trình Java',
    shortDescription: 'Lập trình hướng đối tượng cơ bản với ngôn ngữ Java.',
    status: 'empty',
    updatedAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
    authorName: 'Trần Văn F',
    chapters: []
  },
  {
    id: 7,
    code: 'SOF203',
    name: 'Kiểm thử phần mềm',
    shortDescription: 'Các kỹ thuật, quy trình và công cụ kiểm thử phần mềm tự động hóa.',
    status: 'draft',
    updatedAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    authorName: 'Lê Thị G',
    chapters: []
  },
  {
    id: 8,
    code: 'SDN302',
    name: 'Phát triển ứng dụng',
    shortDescription: 'Xây dựng ứng dụng doanh nghiệp đầy đủ tính năng.',
    status: 'completed',
    updatedAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(),
    authorName: 'Hoàng Văn H',
    chapters: []
  }
]
