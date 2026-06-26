export const contentBoardMenuGroups = [
  {
    id: 'dashboard',
    label: 'Dashboard',
    icon: 'LayoutDashboard',
    route: '/content-board/dashboard',
    children: [],
  },
  {
    id: 'noi-dung',
    label: 'Quản lý Nội dung',
    icon: 'BookOpen',
    children: [
      { id: 'subjects', label: 'Môn học', icon: 'BookOpen', route: '/content-board/subjects' },
    ],
  },
  {
    id: 'ca-nhan',
    label: 'Cá nhân',
    icon: 'User',
    children: [
      { id: 'profile', label: 'Hồ sơ', icon: 'UserCircle', route: '/content-board/profile' },
    ],
  },
]

export const mockContentBoard = {
  name: 'Nguyễn Văn Nội Dung',
  staffId: 'CONTENT2024001',
  email: 'content@lms.edu.vn',
  avatar: null,
  initials: 'ND',
  department: 'Hội đồng Quản lý Nội dung',
  campus: 'Cơ sở chính',
}
