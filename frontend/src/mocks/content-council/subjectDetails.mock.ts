import { ContentCouncilSubjectDetail } from '@/types/content-council/subject'

export const mockSubjectDetails: ContentCouncilSubjectDetail[] = [
  {
    id: 1,
    code: 'COM101',
    name: 'Nhập môn Công nghệ thông tin',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 2 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
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
              { id: 10002, lessonId: 1001, type: 'van_ban', title: 'Tóm tắt khái niệm', order: 2, status: 'published', html: '<p>LMS là Hệ thống quản lý học tập...</p>' }
            ]
          },
          { 
            id: 1002, title: 'Lịch sử phát triển', order: 2, type: 'van_ban', status: 'published', contentCount: 1,
            contents: [
              { id: 10003, lessonId: 1002, type: 'slide_html', title: 'Slide Lịch sử phát triển', order: 1, status: 'published', NoiDungJson: '{"time":1710000000000,"version":"2.28.2","blocks":[{"id":"b1","type":"header","data":{"text":"Lịch sử phát triển","level":2}},{"id":"b2","type":"paragraph","data":{"text":"Máy tính trải qua nhiều thế hệ..."}}]}' }
            ]
          },
          { 
            id: 1003, title: 'Bài tập trắc nghiệm chương 1', order: 3, type: 'trac_nghiem', status: 'active', contentCount: 1,
            contents: [
              { id: 10004, lessonId: 1003, type: 'trac_nghiem', title: 'Quiz chương 1', order: 1, status: 'active', quizId: 1, quizTitle: 'Quiz chương 1', quizQuestionCount: 10, quizDurationMinutes: 15, quizTotalScore: 10, quizCompletionRule: 'pass' }
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
          { id: 1004, title: 'Các thành phần cơ bản', order: 1, type: 'slide_html', status: 'published', contentCount: 3, contents: [] },
          { id: 1005, title: 'CPU và Bộ nhớ', order: 2, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 1006, title: 'Thiết bị ngoại vi', order: 3, type: 'tai_lieu', status: 'inactive', contentCount: 0, contents: [] }
        ]
      },
      {
        id: 103,
        title: 'Phần mềm và Hệ điều hành',
        order: 3,
        hidden: true,
        lessons: [
          { id: 1007, title: 'Phân loại phần mềm', order: 1, type: 'video', status: 'active', contentCount: 1, contents: [] },
          { id: 1008, title: 'Hệ điều hành Windows', order: 2, type: 'slide_html', status: 'active', contentCount: 1, contents: [] }
        ]
      }
    ]
  },
  {
    id: 2,
    code: 'WEB201',
    name: 'Lập trình Web',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 24 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: [
      {
        id: 201,
        title: 'HTML & CSS Cơ bản',
        order: 1,
        hidden: false,
        lessons: [
          { id: 2001, title: 'Giới thiệu HTML', order: 1, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 2002, title: 'CSS Selector', order: 2, type: 'video', status: 'published', contentCount: 2, contents: [] },
          { id: 2003, title: 'Thực hành HTML & CSS', order: 3, type: 'trac_nghiem', status: 'published', contentCount: 1, contents: [] }
        ]
      },
      {
        id: 202,
        title: 'JavaScript căn bản',
        order: 2,
        hidden: false,
        lessons: [
          { id: 2004, title: 'Biến và Kiểu dữ liệu', order: 1, type: 'video', status: 'published', contentCount: 1, contents: [] },
          { id: 2005, title: 'Hàm và Sự kiện', order: 2, type: 'van_ban', status: 'published', contentCount: 2, contents: [] }
        ]
      }
    ]
  },
  {
    id: 3,
    code: 'MOB101',
    name: 'Lập trình Mobile',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'inactive',
    updatedAt: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  },
  {
    id: 4,
    code: 'DBI202',
    name: 'Cơ sở dữ liệu',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 5 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  },
  {
    id: 5,
    code: 'NET102',
    name: 'Mạng máy tính',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  },
  {
    id: 6,
    code: 'PRO192',
    name: 'Lập trình Java',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'inactive',
    updatedAt: new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  },
  {
    id: 7,
    code: 'SOF203',
    name: 'Kiểm thử phần mềm',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 30 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  },
  {
    id: 8,
    code: 'SDN302',
    name: 'Phát triển ứng dụng',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=Thumbnail',
    status: 'active',
    updatedAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(),
    instructorCount: 2,
    studentCount: 150,
    publishedContentRatio: 75,
    createdAt: new Date().toISOString(),
    isPublished: true,
    chapters: []
  }
]
