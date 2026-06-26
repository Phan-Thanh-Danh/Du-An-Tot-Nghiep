import { ContentCouncilSubject } from '@/types/content-council/subject'

export const mockSubjects: ContentCouncilSubject[] = [
  {
    id: 1,
    code: 'COM101',
    name: 'Nhập môn Công nghệ thông tin',
    thumbnailUrl: 'https://placehold.co/600x400/2563eb/ffffff?text=COM101',
    status: 'active',
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
    thumbnailUrl: 'https://placehold.co/600x400/10b981/ffffff?text=WEB201',
    status: 'active',
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
    thumbnailUrl: 'https://placehold.co/600x400/f59e0b/ffffff?text=MOB101',
    status: 'inactive',
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
    thumbnailUrl: 'https://placehold.co/600x400/8b5cf6/ffffff?text=DBI202',
    status: 'active',
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
    thumbnailUrl: 'https://placehold.co/600x400/ef4444/ffffff?text=NET102',
    status: 'archived',
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
    thumbnailUrl: 'https://placehold.co/600x400/06b6d4/ffffff?text=PRO192',
    status: 'inactive',
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
    thumbnailUrl: 'https://placehold.co/600x400/ec4899/ffffff?text=SOF203',
    status: 'active',
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
    thumbnailUrl: 'https://placehold.co/600x400/14b8a6/ffffff?text=SDN302',
    status: 'active',
    chapterCount: 8,
    lessonCount: 30,
    contentCount: 65,
    quizCount: 12,
    updatedAt: new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString(), // 14 days ago
  }
]
