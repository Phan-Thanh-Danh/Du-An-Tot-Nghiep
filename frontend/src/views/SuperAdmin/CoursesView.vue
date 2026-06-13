<script setup>
/**
 * CoursesView.vue - Super Admin
 * Quản trị tối cao đối với toàn bộ nội dung đào tạo và tiến độ học tập (Module M2).
 * Hỗ trợ phân công lại giảng viên (Teacher Assignment Drawer), lưu trữ khóa học,
 * quản lý trạng thái khóa học (Draft -> Published -> Archived) và ghi Audit Log.
 */
import { ref, computed } from 'vue'
import {
  Search,
  AlertCircle,
  RotateCcw,
  X,
  Save,
  Clock,
  History,
  User,
  UserCheck,
  Users,
  Archive,
  FolderOpen
} from 'lucide-vue-next'

// --- Mock Data ---

// Danh sách Giảng viên hệ thống
const teachers = ref([
  { id: 'GV-001', name: 'Nguyễn Văn A', department: 'CNTT', campus: 'Hòa Lạc', email: 'anv@fpt.edu.vn' },
  { id: 'GV-002', name: 'Trần Thị B', department: 'CNTT', campus: 'Hòa Lạc', email: 'btt@fpt.edu.vn' },
  { id: 'GV-003', name: 'Lê Văn C', department: 'Kinh tế', campus: 'TP.HCM', email: 'clv@fpt.edu.vn' },
  { id: 'GV-004', name: 'Phạm Thị D', department: 'Ngoại ngữ', campus: 'Detech', email: 'dpt@fpt.edu.vn' }
])

// Danh sách Môn học liên kết
const subjects = ref([
  { code: 'PRF192', name: 'Nhập môn lập trình' },
  { code: 'PRO192', name: 'Lập trình hướng đối tượng' },
  { code: 'DBI202', name: 'Cơ sở dữ liệu' },
  { code: 'MKT101', name: 'Nguyên lý Marketing' },
  { code: 'NWC203', name: 'Mạng máy tính' },
  { code: 'SSG104', name: 'Kỹ năng giao tiếp chuyên nghiệp' }
])

// Danh sách Cơ sở (Campus)
const campuses = ref(['Hòa Lạc', 'Detech', 'TP.HCM'])

// Danh sách Khóa học (Course)
const courses = ref([
  {
    code: 'PRO192_SE1901',
    name: 'Lập trình hướng đối tượng - SE1901',
    subjectCode: 'PRO192',
    subjectName: 'Lập trình hướng đối tượng',
    teacherId: 'GV-001',
    teacherName: 'Nguyễn Văn A',
    campus: 'Hòa Lạc',
    chapters: 5,
    lessons: 24,
    studentProgress: 78,
    status: 'Published'
  },
  {
    code: 'PRF192_SE1902',
    name: 'Nhập môn lập trình - SE1902',
    subjectCode: 'PRF192',
    subjectName: 'Nhập môn lập trình',
    teacherId: 'GV-002',
    teacherName: 'Trần Thị B',
    campus: 'Hòa Lạc',
    chapters: 6,
    lessons: 30,
    studentProgress: 92,
    status: 'Published'
  },
  {
    code: 'DBI202_SE1903',
    name: 'Cơ sở dữ liệu - SE1903',
    subjectCode: 'DBI202',
    subjectName: 'Cơ sở dữ liệu',
    teacherId: 'GV-001',
    teacherName: 'Nguyễn Văn A',
    campus: 'Hòa Lạc',
    chapters: 4,
    lessons: 20,
    studentProgress: 45,
    status: 'Published'
  },
  {
    code: 'MKT101_MKT1901',
    name: 'Nguyên lý Marketing - MKT1901',
    subjectCode: 'MKT101',
    subjectName: 'Nguyên lý Marketing',
    teacherId: 'GV-003',
    teacherName: 'Lê Văn C',
    campus: 'TP.HCM',
    chapters: 5,
    lessons: 18,
    studentProgress: 62,
    status: 'Published'
  },
  {
    code: 'NWC203_SE1801',
    name: 'Mạng máy tính - SE1801',
    subjectCode: 'NWC203',
    subjectName: 'Mạng máy tính',
    teacherId: 'GV-004',
    teacherName: 'Phạm Thị D',
    campus: 'Detech',
    chapters: 6,
    lessons: 28,
    studentProgress: 0,
    status: 'Draft'
  },
  {
    code: 'SSG104_GD1701',
    name: 'Kỹ năng giao tiếp - GD1701',
    subjectCode: 'SSG104',
    subjectName: 'Kỹ năng giao tiếp chuyên nghiệp',
    teacherId: 'GV-003',
    teacherName: 'Lê Văn C',
    campus: 'TP.HCM',
    chapters: 4,
    lessons: 12,
    studentProgress: 100,
    status: 'Archived'
  }
])

// Audit Logs của hệ thống
const auditLogs = ref([
  {
    id: 1,
    time: '2026-06-08 16:15:20',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    courseCode: 'PRO192_SE1901',
    details: 'Phân công lại giảng viên: Nguyễn Văn A -> Trần Thị B',
    reason: 'Giảng viên cũ xin nghỉ thai sản học kỳ mới'
  },
  {
    id: 2,
    time: '2026-06-04 11:30:15',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    courseCode: 'SSG104_GD1701',
    details: 'Ẩn & Lưu trữ khóa học (Archived)',
    reason: 'Khóa học đã hoàn thành 100% tiến độ, chuyển lưu trữ để giải phóng lớp học active'
  },
  {
    id: 3,
    time: '2026-05-28 09:00:10',
    actor: 'Super Admin (admin@fpt.edu.vn)',
    courseCode: 'NWC203_SE1801',
    details: 'Chuyển trạng thái khóa học: Draft -> Published',
    reason: 'Đã hoàn thành kiểm duyệt nội dung học thuật'
  }
])

// --- State & Filters ---
const searchQuery = ref('')
const selectedTeacher = ref('all')
const selectedSubject = ref('all')
const selectedCampus = ref('all')
const selectedStatus = ref('all')

// --- Lọc danh sách khóa học ---
const filteredCourses = computed(() => {
  return courses.value.filter(c => {
    const matchSearch = c.code.toLowerCase().includes(searchQuery.value.toLowerCase()) || c.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchTeacher = selectedTeacher.value === 'all' || c.teacherId === selectedTeacher.value
    const matchSubject = selectedSubject.value === 'all' || c.subjectCode === selectedSubject.value
    const matchCampus = selectedCampus.value === 'all' || c.campus === selectedCampus.value
    const matchStatus = selectedStatus.value === 'all' || c.status === selectedStatus.value
    return matchSearch && matchTeacher && matchSubject && matchCampus && matchStatus
  })
})

// --- KPI Stats ---
const totalCoursesCount = computed(() => courses.value.length)
const publishedCoursesCount = computed(() => courses.value.filter(c => c.status === 'Published').length)
const archivedCoursesCount = computed(() => courses.value.filter(c => c.status === 'Archived').length)

// Tỷ lệ hoàn thành trung bình toàn hệ thống
const systemAverageProgress = computed(() => {
  const activeCourses = courses.value.filter(c => c.status === 'Published')
  if (activeCourses.length === 0) return 0
  const sum = activeCourses.reduce((acc, curr) => acc + curr.studentProgress, 0)
  return Math.round(sum / activeCourses.length)
})

// --- State Teacher Assignment Drawer ---
const isAssignmentDrawerOpen = ref(false)
const targetAssignmentCourse = ref(null)
const selectedNewTeacherId = ref('')
const assignmentReason = ref('')

// --- State Confirm Archive Modal ---
const isArchiveModalOpen = ref(false)
const targetArchiveCourse = ref(null)
const archiveReason = ref('')
const confirmCheck = ref(false)

// --- Handlers ---

// Mở Drawer phân công lại giảng viên
const openAssignmentDrawer = (course) => {
  targetAssignmentCourse.value = course
  selectedNewTeacherId.value = course.teacherId
  assignmentReason.value = ''
  isAssignmentDrawerOpen.value = true
}

const saveAssignment = () => {
  if (!selectedNewTeacherId.value || !targetAssignmentCourse.value) return
  if (!assignmentReason.value.trim()) {
    alert('Vui lòng nhập lý do phân công lại giảng viên để ghi nhận Audit Log.')
    return
  }

  const course = courses.value.find(c => c.code === targetAssignmentCourse.value.code)
  const newTeacher = teachers.value.find(t => t.id === selectedNewTeacherId.value)

  if (course && newTeacher) {
    const oldTeacherName = course.teacherName
    course.teacherId = newTeacher.id
    course.teacherName = newTeacher.name

    const logText = `Phân công lại giảng viên: ${oldTeacherName} -> ${newTeacher.name}`
    addAuditLog(course.code, logText, assignmentReason.value)
  }

  isAssignmentDrawerOpen.value = false
  targetAssignmentCourse.value = null
}

// Thay đổi trạng thái nhanh (Draft -> Published)
const publishCourse = (course) => {
  if (confirm(`Bạn có chắc chắn muốn công bố (Publish) khóa học "${course.name}" không?`)) {
    course.status = 'Published'
    addAuditLog(course.code, 'Chuyển trạng thái khóa học sang Published (Đã công bố)', 'Kích hoạt kế hoạch giảng dạy')
  }
}

// Mở Confirm Archive Modal
const openArchiveModal = (course) => {
  targetArchiveCourse.value = course
  archiveReason.value = ''
  confirmCheck.value = false
  isArchiveModalOpen.value = true
}

const confirmArchiveCourse = () => {
  if (!targetArchiveCourse.value) return
  if (!archiveReason.value.trim()) {
    alert('Vui lòng nhập lý do lưu trữ để lưu Audit Log.')
    return
  }
  if (!confirmCheck.value) {
    alert('Vui lòng xác nhận cam kết bảo toàn dữ liệu học vụ.')
    return
  }

  const course = courses.value.find(c => c.code === targetArchiveCourse.value.code)
  if (course) {
    course.status = 'Archived'
    addAuditLog(course.code, 'Lưu trữ khóa học (Archived - is_active = 0)', archiveReason.value)
  }

  isArchiveModalOpen.value = false
  targetArchiveCourse.value = null
}

// Helper thêm Audit Log
const addAuditLog = (courseCode, details, reason) => {
  const now = new Date()
  const timeStr = now.toLocaleString('sv-SE', { timeZone: 'Asia/Ho_Chi_Minh' }).replace('T', ' ')
  auditLogs.value.unshift({
    id: auditLogs.value.length ? Math.max(...auditLogs.value.map(l => l.id)) + 1 : 1,
    time: timeStr,
    actor: 'Super Admin (admin@fpt.edu.vn)',
    courseCode,
    details,
    reason
  })
}

// Trạng thái badge helper
const getStatusBadge = (status) => {
  switch (status) {
    case 'Draft':
      return { class: 'bg-amber-50 text-amber-700 border-amber-200/50 dark:bg-amber-600/10 dark:text-amber-400 dark:border-amber-500/20', label: 'Bản nháp' }
    case 'Published':
      return { class: 'bg-emerald-50 text-emerald-700 border-emerald-200/50 dark:bg-emerald-600/10 dark:text-emerald-400 dark:border-emerald-500/20', label: 'Đã công bố' }
    case 'Archived':
      return { class: 'bg-rose-50 text-rose-700 border-rose-200/50 dark:bg-rose-600/10 dark:text-rose-400 dark:border-rose-500/20', label: 'Đã lưu trữ' }
    default:
      return { class: 'bg-slate-100 text-slate-700', label: status }
  }
}

const resetFilters = () => {
  searchQuery.value = ''
  selectedTeacher.value = 'all'
  selectedSubject.value = 'all'
  selectedCampus.value = 'all'
  selectedStatus.value = 'all'
}
</script>

<template>
  <div class="courses-page pb-12 space-y-6">
    <!-- Header -->
    <header class="page-header flex flex-col md:flex-row md:items-center justify-between gap-4 border-b border-default pb-4">
      <div>
        <h1 class="text-2xl font-bold text-heading">Quản lý Khóa học Hệ thống (Module M2)</h1>
        <p class="text-sm text-label mt-1">Điều phối phân công giảng dạy, kiểm soát nội dung bài học, lưu trữ và theo dõi tiến độ hoàn thành khóa học.</p>
      </div>
      <div class="flex items-center gap-3">
        <span class="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-blue-50/60 dark:bg-blue-950/20 border border-blue-200/50 text-xs font-bold text-blue-700 dark:text-blue-400 select-none">
          <Users :size="14" /> Super Admin Portal
        </span>
      </div>
    </header>

    <!-- KPI Mini Panel -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Tổng số khóa học</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-heading">{{ totalCoursesCount }}</span>
          <span class="text-[10px] text-placeholder">Lớp khóa học</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-emerald-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Đã công bố (Published)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-emerald-600 dark:text-emerald-400">
            {{ publishedCoursesCount }}
          </span>
          <span class="text-[10px] text-placeholder">Đang hoạt động</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-rose-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Lưu trữ (Archived)</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-rose-600 dark:text-rose-400">
            {{ archivedCoursesCount }}
          </span>
          <span class="text-[10px] text-placeholder">Khóa học ngưng mở</span>
        </div>
      </div>
      <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 p-4 shadow-sm border-l-4 border-l-blue-500">
        <p class="text-xs font-bold text-label uppercase tracking-wide">Tiến độ sinh viên trung bình</p>
        <div class="flex items-baseline gap-2 mt-1">
          <span class="text-3xl font-extrabold text-blue-600 dark:text-blue-400">
            {{ systemAverageProgress }}%
          </span>
          <span class="text-[10px] text-placeholder">Toàn hệ thống</span>
        </div>
      </div>
    </div>

    <!-- Filters Bar -->
    <div class="lg-glass-soft p-4 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm flex flex-col lg:flex-row lg:items-center justify-between gap-4">
      <div class="flex flex-wrap items-center gap-3 flex-1 min-w-0">
        <!-- Search -->
        <div class="relative w-full md:w-64">
          <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm mã hoặc tên khóa học..."
            class="glass-input pl-9 w-full"
          />
        </div>

        <!-- Teacher filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Giảng viên:</span>
          <select v-model="selectedTeacher" class="glass-select w-full sm:w-40">
            <option value="all">Tất cả giảng viên</option>
            <option v-for="t in teachers" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
        </div>

        <!-- Subject filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Môn học:</span>
          <select v-model="selectedSubject" class="glass-select w-full sm:w-44">
            <option value="all">Tất cả các môn</option>
            <option v-for="s in subjects" :key="s.code" :value="s.code">{{ s.name }}</option>
          </select>
        </div>

        <!-- Campus filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Cơ sở:</span>
          <select v-model="selectedCampus" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả cơ sở</option>
            <option v-for="c in campuses" :key="c" :value="c">{{ c }}</option>
          </select>
        </div>

        <!-- Status filter -->
        <div class="flex items-center gap-1.5 w-full sm:w-auto">
          <span class="text-xs text-label font-bold whitespace-nowrap hidden lg:inline">Trạng thái:</span>
          <select v-model="selectedStatus" class="glass-select w-full sm:w-36">
            <option value="all">Tất cả trạng thái</option>
            <option value="Draft">Bản nháp</option>
            <option value="Published">Đã công bố</option>
            <option value="Archived">Đã lưu trữ</option>
          </select>
        </div>
      </div>

      <button @click="resetFilters" class="glass-btn secondary shrink-0 self-end lg:self-auto justify-center">
        <RotateCcw :size="14" /> Xóa bộ lọc
      </button>
    </div>

    <!-- Courses Table -->
    <div class="lg-glass-soft rounded-2xl border border-white/60 dark:border-white/10 shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse text-sm">
          <thead>
            <tr class="bg-slate-50/50 dark:bg-white/5 border-b border-default text-[11px] font-bold text-label uppercase tracking-widest">
              <th class="p-4">Khóa học / Lớp</th>
              <th class="p-4">Giảng viên phụ trách</th>
              <th class="p-4">Cơ sở (Campus)</th>
              <th class="p-4">Môn học liên kết</th>
              <th class="p-4 text-center">Nội dung bài học</th>
              <th class="p-4">Tiến độ sinh viên trung bình</th>
              <th class="p-4 text-center">Trạng thái</th>
              <th class="p-4 text-right">Thao tác quản trị</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredCourses.length === 0">
              <td colspan="8" class="p-8 text-center text-placeholder">
                Không tìm thấy khóa học nào phù hợp với bộ lọc.
              </td>
            </tr>
            <tr
              v-for="course in filteredCourses"
              :key="course.code"
              class="hover:bg-white/40 dark:hover:bg-white/5 transition-all group"
            >
              <!-- Khóa học / Lớp -->
              <td class="p-4 font-bold text-heading">
                <div class="flex items-center gap-2">
                  <FolderOpen :size="16" class="text-blue-500 shrink-0" />
                  <div class="flex flex-col">
                    <span>{{ course.name }}</span>
                    <span class="text-[10px] text-placeholder font-mono leading-none mt-0.5">{{ course.code }}</span>
                  </div>
                </div>
              </td>
              <!-- Giảng viên phụ trách -->
              <td class="p-4 font-semibold text-body">
                <div class="flex items-center gap-2">
                  <div class="w-7 h-7 rounded-full bg-slate-100 dark:bg-white/10 flex items-center justify-center text-xs text-placeholder">
                    <User :size="12" />
                  </div>
                  {{ course.teacherName }}
                </div>
              </td>
              <!-- Cơ sở -->
              <td class="p-4">
                <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-lg bg-slate-100 dark:bg-white/10 font-bold text-xs text-heading">
                  {{ course.campus }}
                </span>
              </td>
              <!-- Môn liên kết -->
              <td class="p-4 text-body font-medium">
                {{ course.subjectCode }} - {{ course.subjectName }}
              </td>
              <!-- Số chương / bài -->
              <td class="p-4 text-center">
                <div class="flex flex-col items-center">
                  <span class="font-bold text-heading">{{ course.chapters }} chương</span>
                  <span class="text-[10px] text-placeholder mt-0.5">{{ course.lessons }} bài học</span>
                </div>
              </td>
              <!-- Tiến độ sinh viên -->
              <td class="p-4">
                <div class="space-y-1.5 max-w-[150px]">
                  <div class="flex items-center justify-between text-xs font-bold text-body">
                    <span>Tiến độ:</span>
                    <span>{{ course.studentProgress }}%</span>
                  </div>
                  <div class="h-2 w-full rounded-full bg-slate-100 dark:bg-slate-800 overflow-hidden">
                    <div
                      class="h-full rounded-full transition-all duration-300"
                      :class="course.studentProgress === 100 ? 'bg-emerald-500' : 'bg-blue-500'"
                      :style="`width: ${course.studentProgress}%`"
                    ></div>
                  </div>
                </div>
              </td>
              <!-- Trạng thái -->
              <td class="p-4 text-center">
                <span :class="['px-2.5 py-0.5 rounded-full text-xs font-bold border inline-block', getStatusBadge(course.status).class]">
                  {{ getStatusBadge(course.status).label }}
                </span>
              </td>
              <!-- Hành động -->
              <td class="p-4 text-right">
                <div class="flex items-center justify-end gap-1">
                  <!-- Phân công lại giảng viên -->
                  <button
                    @click="openAssignmentDrawer(course)"
                    class="p-1.5 text-blue-600 hover:bg-blue-50 dark:hover:bg-blue-900/20 rounded transition-all font-semibold text-xs flex items-center gap-1"
                    title="Gán giảng viên phụ trách"
                    :disabled="course.status === 'Archived'"
                  >
                    <UserCheck :size="14" /> Gán GV
                  </button>

                  <!-- Công bố nhanh từ Draft -->
                  <button
                    v-if="course.status === 'Draft'"
                    @click="publishCourse(course)"
                    class="p-1.5 text-emerald-600 hover:bg-emerald-50 dark:hover:bg-emerald-900/20 rounded transition-all font-semibold text-xs"
                    title="Công bố khóa học rộng rãi"
                  >
                    Công bố
                  </button>

                  <!-- Lưu trữ (Archive) -->
                  <button
                    v-if="course.status !== 'Archived'"
                    @click="openArchiveModal(course)"
                    class="p-1.5 text-rose-500 hover:bg-rose-50 dark:hover:bg-rose-900/20 rounded transition-all"
                    title="Lưu trữ khóa học"
                  >
                    <Archive :size="14" />
                  </button>

                  <span v-else class="text-xs text-placeholder italic font-medium select-none pr-2">
                    Không thao tác
                  </span>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Audit Logs Section -->
    <div class="lg-glass-soft p-5 rounded-2xl border border-white/60 dark:border-white/10 shadow-sm space-y-4">
      <div class="flex items-center justify-between border-b border-default pb-3">
        <h3 class="font-bold text-heading flex items-center gap-2">
          <History :size="18" class="text-violet-500" />
          Nhật ký điều phối khóa học (Audit Log)
        </h3>
        <span class="text-[10px] uppercase font-bold text-label bg-slate-100 dark:bg-white/10 px-2 py-0.5 rounded-md">Truy vết & Quản trị</span>
      </div>

      <div class="space-y-3 max-h-[250px] overflow-y-auto pr-2">
        <div
          v-for="log in auditLogs"
          :key="log.id"
          class="flex items-start gap-3 p-3 rounded-xl bg-white/40 dark:bg-white/5 border border-default text-xs"
        >
          <div class="p-2 bg-violet-50 dark:bg-violet-950 text-violet-600 dark:text-violet-400 rounded-lg mt-0.5">
            <Clock :size="14" />
          </div>
          <div class="flex-1 min-w-0">
            <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-1">
              <span class="font-bold text-heading">Khóa học: {{ log.courseCode }}</span>
              <span class="text-[10px] text-placeholder font-mono">{{ log.time }}</span>
            </div>
            <p class="text-body font-semibold mt-1">{{ log.details }}</p>
            <p class="text-placeholder mt-0.5">Lý do điều phối: <span class="italic text-body font-medium">"{{ log.reason }}"</span></p>
            <p class="text-[10px] text-placeholder mt-1">Người vận hành: <span class="font-mono">{{ log.actor }}</span></p>
          </div>
        </div>
      </div>
    </div>

    <!-- Teacher Assignment Drawer (Trượt từ phải sang) -->
    <div v-if="isAssignmentDrawerOpen" class="drawer-overlay" @click.self="isAssignmentDrawerOpen = false">
      <div class="drawer-content h-full max-w-md w-full bg-white dark:bg-slate-900 border-l border-default shadow-2xl flex flex-col">
        <!-- Header -->
        <div class="p-5 border-b border-default flex items-center justify-between">
          <div>
            <h3 class="font-bold text-heading text-base flex items-center gap-1.5">
              <UserCheck :size="18" class="text-blue-500" />
              Gán giảng viên giảng dạy
            </h3>
            <p class="text-xs text-label mt-0.5">Lớp khóa học: <strong class="text-heading font-mono">{{ targetAssignmentCourse?.code }}</strong></p>
          </div>
          <button @click="isAssignmentDrawerOpen = false" class="p-1.5 hover:bg-slate-100 dark:hover:bg-white/10 rounded text-placeholder hover:text-heading">
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-5 space-y-5">
          <div class="p-3.5 bg-slate-50 dark:bg-white/5 border border-default rounded-xl space-y-1.5 text-xs text-body">
            <div>Giảng viên hiện tại: <strong class="text-heading">{{ targetAssignmentCourse?.teacherName }}</strong></div>
            <div>Môn học liên kết: <strong class="text-heading">{{ targetAssignmentCourse?.subjectName }}</strong></div>
            <div>Cơ sở học thuật: <strong class="text-heading">{{ targetAssignmentCourse?.campus }}</strong></div>
          </div>

          <!-- Select New Teacher -->
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Chọn Giảng viên thay thế *</label>
            <select v-model="selectedNewTeacherId" class="glass-select w-full">
              <option v-for="t in teachers" :key="t.id" :value="t.id">
                {{ t.name }} (Khoa {{ t.department }} - {{ t.campus }})
              </option>
            </select>
          </div>

          <!-- Assignment Reason -->
          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Lý do phân công lại giảng viên *</label>
            <textarea
              v-model="assignmentReason"
              rows="3"
              class="glass-input w-full"
              placeholder="Nhập lý do chi tiết..."
            ></textarea>
          </div>
        </div>

        <!-- Footer -->
        <div class="p-4 border-t border-default bg-slate-50/80 dark:bg-slate-900 flex gap-3">
          <button @click="isAssignmentDrawerOpen = false" class="glass-btn secondary flex-1 justify-center">Hủy</button>
          <button
            @click="saveAssignment"
            class="glass-btn primary flex-1 justify-center"
            :disabled="!selectedNewTeacherId || !assignmentReason.trim()"
            :class="{'opacity-50 cursor-not-allowed': !selectedNewTeacherId || !assignmentReason.trim()}"
          >
            <Save :size="14" /> Phân công giảng dạy
          </button>
        </div>
      </div>
    </div>

    <!-- Confirm Archive Modal -->
    <div v-if="isArchiveModalOpen" class="modal-overlay">
      <div class="modal-content max-w-md w-full border border-rose-200 dark:border-rose-500/30">
        <!-- Header -->
        <div class="flex items-center gap-3 mb-4">
          <div class="flex items-center justify-center w-10 h-10 rounded-full bg-rose-100 text-rose-600 shrink-0">
            <Archive :size="20" />
          </div>
          <div>
            <h3 class="text-lg font-bold text-heading">Xác nhận LƯU TRỮ Khóa học</h3>
            <p class="text-xs text-rose-600 font-bold uppercase tracking-widest mt-0.5">Xác nhận bảo mật học vụ</p>
          </div>
        </div>

        <!-- Body -->
        <div class="space-y-4">
          <!-- Điều kiện kiểm tra tiến độ sinh viên -->
          <div class="p-3 bg-rose-50 dark:bg-rose-950/30 text-rose-800 dark:text-rose-400 rounded-xl text-xs space-y-2 border border-rose-100 dark:border-rose-500/20 leading-relaxed font-semibold">
            <div class="flex items-center gap-1.5 font-bold text-sm">
              <AlertCircle :size="15" />
              Chính sách bảo toàn dữ liệu học vụ:
            </div>
            <!-- Cảnh báo tiến độ sinh viên > 0 -->
            <p v-if="targetArchiveCourse?.studentProgress > 0">
              Khóa học <strong class="text-rose-700 dark:text-rose-400">"{{ targetArchiveCourse?.name }}"</strong> hiện đã phát sinh tiến độ học tập của sinh viên (<strong class="text-heading">{{ targetArchiveCourse?.studentProgress }}%</strong>).
            </p>
            <p v-else>
              Khóa học <strong class="text-rose-700 dark:text-rose-400">"{{ targetArchiveCourse?.name }}"</strong> chưa có tiến độ sinh viên.
            </p>
            <p class="text-xs leading-normal">
              Hệ thống tuyệt đối **không cho phép xóa vật lý** bất kỳ bài học hoặc khóa học nào khi có sinh viên tham gia học. Mọi dữ liệu chỉ được ẩn và lưu trữ bằng cách chuyển trạng thái (`is_active = 0`) nhằm bảo vệ lịch sử học tập.
            </p>
          </div>

          <div class="form-group">
            <label class="block text-xs font-bold text-label mb-1.5">Lý do chuyển lưu trữ (Bắt buộc ghi Log) *</label>
            <textarea
              v-model="archiveReason"
              rows="3"
              class="glass-input w-full"
              placeholder="Nhập lý do đóng lớp khóa học..."
            ></textarea>
          </div>

          <label class="flex items-start gap-2.5 p-3 bg-slate-50 dark:bg-white/5 border border-default rounded-xl cursor-pointer">
            <input
              v-model="confirmCheck"
              type="checkbox"
              class="mt-1 accent-rose-600 rounded"
            />
            <div class="text-xs text-body font-semibold select-none leading-normal">
              Tôi xác nhận đóng băng khóa học này và chịu trách nhiệm về việc ẩn cấu trúc nội dung lớp học trên giao diện sinh viên.
            </div>
          </label>
        </div>

        <!-- Footer -->
        <div class="flex gap-3 justify-end mt-6 border-t border-default pt-4">
          <button @click="isArchiveModalOpen = false" class="glass-btn secondary">Hủy thao tác</button>
          <button
            @click="confirmArchiveCourse"
            class="glass-btn primary !bg-rose-600 hover:!bg-rose-700 !text-white"
            :disabled="!archiveReason.trim() || !confirmCheck"
            :class="{'opacity-50 cursor-not-allowed': !archiveReason.trim() || !confirmCheck}"
          >
            <Archive :size="14" /> Xác nhận Lưu trữ (Ghi Log)
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.courses-page {
  font-family: inherit;
}

/* Glass panel */
.lg-glass-soft {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  backdrop-filter: blur(12px);
}

/* Table styling */
table th {
  color: var(--text-label);
  border-bottom: 1px solid var(--border-default);
}
table td {
  border-bottom: 1px solid var(--border-default);
  color: var(--text-body);
}

/* Button & input styling */
.glass-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  border: 1px solid transparent;
}
.glass-btn.primary {
  background: var(--text-link);
  color: white;
}
.glass-btn.primary:hover {
  background: #1d4ed8;
  transform: translateY(-1px);
}
.glass-btn.primary:disabled {
  background: #93c5fd;
  transform: none;
}
.glass-btn.secondary {
  background: var(--surface-input);
  border-color: var(--border-input);
  color: var(--text-heading);
}
.glass-btn.secondary:hover {
  background: var(--surface-input-focus);
}

.glass-input, .glass-select {
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  padding: 0.55rem 0.75rem;
  border-radius: 10px;
  color: var(--text-heading);
  font-size: 0.8rem;
  outline: none;
  transition: all 0.2s;
}
.glass-input:focus, .glass-select:focus {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
  background: var(--surface-input-focus);
}

/* Drawer styling */
.drawer-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.3);
  backdrop-filter: blur(4px);
  display: flex;
  justify-content: flex-end;
  z-index: 50;
}
.drawer-content {
  animation: slideIn 0.3s ease-out;
}

/* Modal styling */
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.4);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 50;
  padding: 1rem;
}
.modal-content {
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: 20px;
  padding: 1.5rem;
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 10px 10px -5px rgba(0, 0, 0, 0.04);
  backdrop-filter: blur(16px);
  animation: modalScale 0.25s ease-out;
}

@keyframes modalScale {
  from {
    opacity: 0;
    transform: scale(0.95);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes slideIn {
  from {
    transform: translateX(100%);
  }
  to {
    transform: translateX(0);
  }
}
</style>
