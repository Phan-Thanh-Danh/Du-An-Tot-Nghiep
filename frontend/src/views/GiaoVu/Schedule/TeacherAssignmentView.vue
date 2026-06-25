<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  Search,
  Filter,
  Plus,
  UserPlus,
  ArrowLeftRight,
  Calendar,
  ChevronDown,
  UserCheck,
  UserMinus,
  X,
  ChevronLeft,
  ChevronRight,
  Users,
  MapPin,
  Pencil,
  Trash2,
  Lightbulb
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── State ────────────────────────────────────────────────
const searchQuery = ref('')
const showAddModal = ref(false)
const showFilterPanel = ref(false)
const currentPage = ref(1)

// Modals State
const showEditModal = ref(false)
const showChangeTeacherModal = ref(false)
const showDeleteModal = ref(false)

const editAssignmentForm = ref({})
const changeTeacherForm = ref({ id: '', teacher: '' })
const itemToDelete = ref(null)

const isSuggested = ref(false)
const suggestedTeacherName = ref('')

// ── Filter State ────────────────────────────────────────────
const filters = ref({
  status: 'all', // all, assigned, unassigned
  department: 'all',
  lecturer: 'all'
})

// ── Form State for Adding Assignment ─────────────────────────
const newAssignmentForm = ref({
  subject: '',
  class: '',
  teacher: '',
  sessions: 1,
  status: 'assigned'
})

// ── Mock Data ────────────────────────────────────────────────
const assignments = ref([
  { id: 'PC001', subject: 'Lập trình Java', class: 'SE1601', teacher: 'TS. Nguyễn Minh Khoa', sessions: 3, status: 'assigned', dept: 'CNTT', schedule: 'T2 (Ca 1-2), T4 (Ca 3)', room: 'Lab 102', students: 30 },
  { id: 'PC002', subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'ThS. Trần Thị Lan', sessions: 2, status: 'assigned', dept: 'CNTT', schedule: 'T3 (Ca 4-5)', room: 'P.305', students: 35 },
  { id: 'PC003', subject: 'Lập trình Web', class: 'SE1603', teacher: 'Chưa phân công', sessions: 3, status: 'unassigned', dept: 'CNTT', schedule: 'T5 (Ca 1-3)', room: 'Lab 201', students: 28 },
  { id: 'PC004', subject: 'Hệ quản trị CSDL', class: 'SE1604', teacher: 'ThS. Lê Văn Dũng', sessions: 3, status: 'assigned', dept: 'CNTT', schedule: 'T6 (Ca 4-6)', room: 'Lab 304', students: 32 },
  { id: 'PC005', subject: 'An toàn thông tin', class: 'SE1605', teacher: 'TS. Phạm Minh Tuấn', sessions: 2, status: 'assigned', dept: 'ATTT', schedule: 'T7 (Ca 1-2)', room: 'P.402', students: 25 },
])

const lecturers = [
  { id: 1, name: 'TS. Nguyễn Minh Khoa', load: '12/15', dept: 'CNTT' },
  { id: 2, name: 'ThS. Trần Thị Lan', load: '8/15', dept: 'CNTT' },
  { id: 3, name: 'TS. Phạm Minh Tuấn', load: '14/15', dept: 'ATTT' },
  { id: 4, name: 'ThS. Lê Văn Dũng', load: '10/15', dept: 'CNTT' },
]

const departments = ['CNTT', 'ATTT', 'Khác']

// ── Computed Properties ──────────────────────────────────────
const filteredAssignments = computed(() => {
  let result = assignments.value

  // Apply search filter
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    result = result.filter(item =>
      item.subject.toLowerCase().includes(query) ||
      item.class.toLowerCase().includes(query) ||
      item.teacher.toLowerCase().includes(query)
    )
  }

  // Apply status filter
  if (filters.value.status !== 'all') {
    result = result.filter(item => item.status === filters.value.status)
  }

  // Apply department filter
  if (filters.value.department !== 'all') {
    result = result.filter(item => item.dept === filters.value.department)
  }

  // Apply lecturer filter
  if (filters.value.lecturer !== 'all') {
    result = result.filter(item => item.teacher === filters.value.lecturer)
  }

  return result
})

const paginatedAssignments = computed(() => {
  const itemsPerPage = 5
  const start = (currentPage.value - 1) * itemsPerPage
  return filteredAssignments.value.slice(start, start + itemsPerPage)
})

const totalPages = computed(() => {
  return Math.ceil(filteredAssignments.value.length / 5)
})

const getStatusBadge = (status) => {
  switch (status) {
    case 'assigned': return 'lg-badge lg-badge-success'
    case 'unassigned': return 'lg-badge lg-badge-warning'
    default: return 'lg-badge'
  }
}

// ── Functions ────────────────────────────────────────────────
function openEditModal(item) {
  editAssignmentForm.value = { ...item }
  showEditModal.value = true
}

function handleEditAssignment() {
  if (!editAssignmentForm.value.subject || !editAssignmentForm.value.class) {
    alert('Vui lòng điền đầy đủ thông tin')
    return
  }

  const index = assignments.value.findIndex(a => a.id === editAssignmentForm.value.id)
  if (index !== -1) {
    assignments.value[index] = { ...editAssignmentForm.value }
    // Cập nhật trạng thái và phòng ban nếu giảng viên thay đổi
    if (editAssignmentForm.value.teacher && editAssignmentForm.value.teacher !== 'Chưa phân công') {
      assignments.value[index].status = 'assigned'
      assignments.value[index].dept = lecturers.find(l => l.name === editAssignmentForm.value.teacher)?.dept || assignments.value[index].dept
    } else {
      assignments.value[index].status = 'unassigned'
      assignments.value[index].teacher = 'Chưa phân công'
    }
  }
  showEditModal.value = false
  alert('Cập nhật phân công thành công!')
}

function openChangeTeacherModal(item) {
  changeTeacherForm.value = { id: item.id, teacher: item.teacher === 'Chưa phân công' ? '' : item.teacher }
  showChangeTeacherModal.value = true
}

function handleChangeTeacher() {
  const index = assignments.value.findIndex(a => a.id === changeTeacherForm.value.id)
  if (index !== -1) {
    const newTeacher = changeTeacherForm.value.teacher || 'Chưa phân công'
    assignments.value[index].teacher = newTeacher
    
    if (newTeacher !== 'Chưa phân công') {
      assignments.value[index].status = 'assigned'
      assignments.value[index].dept = lecturers.find(l => l.name === newTeacher)?.dept || assignments.value[index].dept
    } else {
      assignments.value[index].status = 'unassigned'
    }
  }
  closeChangeTeacherModal()
}

function closeChangeTeacherModal() {
  showChangeTeacherModal.value = false
  isSuggested.value = false
  suggestedTeacherName.value = ''
}

function confirmDelete(item) {
  itemToDelete.value = item
  showDeleteModal.value = true
}

function handleDelete() {
  if (itemToDelete.value) {
    assignments.value = assignments.value.filter(a => a.id !== itemToDelete.value.id)
    itemToDelete.value = null
    showDeleteModal.value = false
    
    // Adjust pagination if last item on page is deleted
    if (paginatedAssignments.value.length === 0 && currentPage.value > 1) {
      currentPage.value--
    }
  }
}

function handleAddAssignment() {
  if (!newAssignmentForm.value.subject || !newAssignmentForm.value.class || !newAssignmentForm.value.teacher) {
    alert('Vui lòng điền đầy đủ thông tin')
    return
  }

  const newAssignment = {
    id: `PC${String(assignments.value.length + 1).padStart(3, '0')}`,
    ...newAssignmentForm.value,
    dept: lecturers.find(l => l.name === newAssignmentForm.value.teacher)?.dept || 'CNTT'
  }

  assignments.value.push(newAssignment)

  // Reset form
  newAssignmentForm.value = {
    subject: '',
    class: '',
    teacher: '',
    sessions: 1,
    status: 'assigned'
  }

  showAddModal.value = false
  alert('Thêm phân công thành công!')
}

function clearFilters() {
  filters.value = {
    status: 'all',
    department: 'all',
    lecturer: 'all'
  }
  searchQuery.value = ''
}

function goToPage(page) {
  if (page >= 1 && page <= totalPages.value) {
    currentPage.value = page
  }
}

onMounted(() => {
  const route = useRoute()
  const router = useRouter()
  if (route.query.action === 'change-teacher' && route.query.assignmentId) {
    const item = assignments.value.find(a => a.id === route.query.assignmentId)
    if (item) {
      if (route.query.autoApply === 'true') {
        const newTeacher = route.query.suggestedTeacher || ''
        if (newTeacher) {
          const index = assignments.value.findIndex(a => a.id === route.query.assignmentId)
          if (index !== -1) {
            assignments.value[index].teacher = newTeacher
            assignments.value[index].status = 'assigned'
            assignments.value[index].dept = lecturers.find(l => l.name === newTeacher)?.dept || assignments.value[index].dept
          }
        }
        router.replace('/staff/conflicts')
        return
      }
      changeTeacherForm.value = {
        id: item.id,
        teacher: route.query.suggestedTeacher || ''
      }
      isSuggested.value = true
      suggestedTeacherName.value = route.query.suggestedTeacher || ''
      showChangeTeacherModal.value = true
    }
  }
})
</script>

<template>
  <PageContainer
    title="Phân công giảng viên"
    subtitle="Quản lý gán giảng viên cho các lớp học phần trong học kỳ."
  >
    <template #actions>
      <button @click="showAddModal = true" class="lg-button-primary px-5 py-2.5 text-sm font-bold transition-all">
        <Plus :size="18" /> Thêm phân công
      </button>
    </template>

    <div class="space-y-4">
      <!-- ── Stats Header ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="lg-card-glass p-4 flex items-center gap-4">
          <div class="h-10 w-10 rounded-2xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)]">
            <UserPlus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Tổng phân công</p>
            <p class="text-xl font-semibold text-heading">124</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-4">
          <div class="h-10 w-10 rounded-2xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)]">
            <UserMinus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Chưa gán GV</p>
            <p class="text-xl font-semibold text-heading">12</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-4">
          <div class="h-10 w-10 rounded-2xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)]">
            <UserCheck :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Hoàn tất</p>
            <p class="text-xl font-semibold text-heading">90%</p>
          </div>
        </div>
      </div>

      <!-- ── Filter & Search ── -->
      <div class="lg-glass-strong p-4 rounded-2xl flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo môn, lớp hoặc giảng viên..."
            class="w-full lg-input pl-10 pr-4 py-2.5 text-sm font-medium transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button @click="showFilterPanel = !showFilterPanel" class="lg-button-secondary px-4 py-2.5 text-sm font-bold transition-colors">
            <Filter :size="18" /> Bộ lọc
          </button>
          <div class="h-8 w-px bg-[var(--border-default)]"></div>
          <button class="lg-icon-button p-2.5">
            <Calendar :size="20" />
          </button>
        </div>
      </div>

      <!-- ── Assignment Table ── -->
      <div class="lg-table-shell">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-[var(--surface-input)]">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Mã PC</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Lớp & Môn</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Giảng viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Ca</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y border-default">
            <tr v-for="item in paginatedAssignments" :key="item.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <span class="text-xs font-semibold text-heading">{{ item.id }}</span>
              </td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading leading-tight">{{ item.subject }}</p>
                <div class="flex items-center gap-2 mt-1.5">
                   <span class="text-[10px] font-bold text-[var(--lg-primary)] uppercase bg-[var(--lg-primary)]/10 px-1.5 py-0.5 rounded">{{ item.class }}</span>
                   <span class="text-[10px] text-muted flex items-center gap-1"><Users :size="10"/> {{ item.students }} SV</span>
                </div>
                <div class="text-[10px] text-muted mt-1.5 flex items-center gap-1.5">
                   <span class="flex items-center gap-1"><Calendar :size="10" /> {{ item.schedule }}</span>
                   <span class="text-placeholder">•</span>
                   <span class="flex items-center gap-1"><MapPin :size="10"/> {{ item.room }}</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-full bg-[var(--surface-input)] flex items-center justify-center text-[10px] font-semibold text-label">
                    {{ item.teacher !== 'Chưa phân công' ? item.teacher.split(' ').pop().charAt(0) : '?' }}
                  </div>
                  <div>
                    <p :class="['text-sm font-bold', item.teacher === 'Chưa phân công' ? 'text-[var(--lg-danger)] italic' : 'text-heading']">
                      {{ item.teacher }}
                    </p>
                    <p v-if="item.teacher !== 'Chưa phân công'" class="text-[10px] font-medium text-placeholder">Tải dạy: 12/15</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-2">
                  <span class="h-2 w-16 bg-[var(--surface-input)] rounded-full overflow-hidden inline-flex">
                    <span :style="{ width: (item.sessions / 4) * 100 + '%' }" class="bg-[var(--lg-primary)] h-full"></span>
                  </span>
                  <span class="text-xs font-bold text-label">{{ item.sessions }} ca</span>
                </div>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest', getStatusBadge(item.status)]">
                  {{ item.status === 'assigned' ? 'Đã gán' : 'Cần gán' }}
                </span>
              </td>
              <td class="px-4 py-4">
                <div class="flex items-center gap-1.5">
                  <button @click="openChangeTeacherModal(item)" class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--color-info-text)] rounded-lg text-placeholder transition-all" title="Đổi giảng viên">
                    <ArrowLeftRight :size="15" />
                  </button>
                  <button @click="openEditModal(item)" class="p-2 hover:bg-[var(--color-warning-bg)] hover:text-[var(--color-warning-text)] rounded-lg text-placeholder transition-all" title="Chỉnh sửa">
                    <Pencil :size="15" />
                  </button>
                  <button @click="confirmDelete(item)" class="p-2 hover:bg-[var(--color-danger-bg)] hover:text-[var(--color-danger-text)] rounded-lg text-placeholder transition-all" title="Xóa phân công">
                    <Trash2 :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Footer Info ── -->
      <div class="flex items-center justify-between px-4">
         <p class="text-xs font-medium text-label">Hiển thị {{ paginatedAssignments.length }} / {{ filteredAssignments.length }} bản ghi</p>
         <div class="flex items-center gap-1">
            <button @click="goToPage(currentPage - 1)" :disabled="currentPage === 1" class="h-8 w-8 rounded-lg flex items-center justify-center text-label hover:bg-[var(--surface-input)] disabled:opacity-50 transition-all">
              <ChevronLeft :size="16" />
            </button>
            <button v-for="p in totalPages" :key="p" @click="goToPage(p)" :class="['h-8 w-8 rounded-lg flex items-center justify-center text-xs font-bold transition-all', p === currentPage ? 'bg-[var(--lg-primary)] text-white shadow-sm' : 'hover:bg-[var(--surface-input)] text-label']">
              {{ p }}
            </button>
            <button @click="goToPage(currentPage + 1)" :disabled="currentPage === totalPages" class="h-8 w-8 rounded-lg flex items-center justify-center text-label hover:bg-[var(--surface-input)] disabled:opacity-50 transition-all">
              <ChevronRight :size="16" />
            </button>
         </div>
      </div>
    </div>
  </PageContainer>

  <!-- ── Filter Panel ── -->
  <div v-if="showFilterPanel" class="fixed inset-0 z-40 bg-black/20 backdrop-blur-sm" @click="showFilterPanel = false"></div>
  <div v-if="showFilterPanel" class="fixed top-24 right-6 z-50 surface-modal rounded-2xl shadow-sm border border-default p-4 w-72 space-y-4">
    <div>
      <h3 class="text-sm font-bold text-heading mb-3">Trạng thái</h3>
      <div class="space-y-2">
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="all" class="w-4 h-4 rounded-full">
          <span class="text-sm text-label">Tất cả</span>
        </label>
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="assigned" class="w-4 h-4 rounded-full">
          <span class="text-sm text-label">Đã gán</span>
        </label>
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="unassigned" class="w-4 h-4 rounded-full">
          <span class="text-sm text-label">Chưa gán</span>
        </label>
      </div>
    </div>

    <div class="border-t border-default"></div>

    <div>
      <h3 class="text-sm font-bold text-heading mb-3">Phòng ban</h3>
      <select v-model="filters.department" class="w-full lg-input px-3 py-2 text-sm">
        <option value="all">Tất cả</option>
        <option v-for="dept in departments" :key="dept" :value="dept">{{ dept }}</option>
      </select>
    </div>

    <div class="border-t border-default"></div>

    <div>
      <h3 class="text-sm font-bold text-heading mb-3">Giảng viên</h3>
      <select v-model="filters.lecturer" class="w-full lg-input px-3 py-2 text-sm">
        <option value="all">Tất cả</option>
        <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">{{ lecturer.name }}</option>
      </select>
    </div>

    <div class="border-t border-default"></div>

    <div class="flex gap-3">
      <button @click="clearFilters" class="flex-1 lg-button-secondary px-4 py-2 text-sm font-bold">
        Xóa bộ lọc
      </button>
      <button @click="showFilterPanel = false" class="flex-1 lg-button-primary px-4 py-2 text-sm font-bold">
        Áp dụng
      </button>
    </div>
  </div>

  <!-- ── Modal Add Assignment ── -->
  <div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
    <div class="surface-modal rounded-2xl shadow-sm max-w-md w-full p-4 border border-default">
      <!-- Header -->
      <div class="flex items-center justify-between mb-4">
        <h2 class="text-xl font-bold text-heading">Thêm phân công</h2>
        <button @click="showAddModal = false" class="p-1.5 hover:bg-[var(--surface-input)] rounded-lg text-placeholder hover:text-label transition-colors">
          <X :size="20" />
        </button>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleAddAssignment" class="space-y-4">
        <!-- Môn học -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Môn học</label>
          <input
            v-model="newAssignmentForm.subject"
            type="text"
            placeholder="Nhập tên môn học"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Lớp -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Lớp</label>
          <input
            v-model="newAssignmentForm.class"
            type="text"
            placeholder="Nhập mã lớp (vd: SE1601)"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Giảng viên -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Giảng viên</label>
          <select
            v-model="newAssignmentForm.teacher"
            class="w-full lg-input px-4 py-2.5"
          >
            <option value="">-- Chọn giảng viên --</option>
            <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">
              {{ lecturer.name }} ({{ lecturer.load }})
            </option>
          </select>
        </div>

        <!-- Ca -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Ca</label>
          <input
            v-model.number="newAssignmentForm.sessions"
            type="number"
            min="1"
            max="10"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Trạng thái -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Trạng thái</label>
          <select
            v-model="newAssignmentForm.status"
            class="w-full lg-input px-4 py-2.5"
          >
            <option value="assigned">Đã gán</option>
            <option value="unassigned">Chưa gán</option>
          </select>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-default">
          <button
            type="button"
            @click="showAddModal = false"
            class="flex-1 lg-button-secondary px-4 py-2.5 font-bold"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="flex-1 lg-button-primary px-4 py-2.5 font-bold shadow-lg shadow-[var(--lg-primary)]/20"
          >
            Thêm
          </button>
        </div>
      </form>
    </div>
  </div>
  <!-- ── Modals ── -->

  <!-- Edit Assignment Modal -->
  <div v-if="showEditModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
    <div class="bg-[var(--surface-card)] border border-[var(--border-default)] rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">
      <div class="flex items-center justify-between p-4 border-b border-[var(--border-default)]">
        <h3 class="text-lg font-bold text-heading">Chỉnh sửa phân công</h3>
        <button @click="showEditModal = false" class="p-2 text-placeholder hover:text-heading hover:bg-[var(--surface-input)] rounded-xl transition-all">
          <X :size="20" />
        </button>
      </div>
      
      <form @submit.prevent="handleEditAssignment" class="p-4 space-y-4">
        <!-- Môn học -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Môn học</label>
          <input
            v-model="editAssignmentForm.subject"
            type="text"
            placeholder="Nhập tên môn học"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Lớp -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Lớp</label>
          <input
            v-model="editAssignmentForm.class"
            type="text"
            placeholder="Nhập mã lớp (vd: SE1601)"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Giảng viên -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Giảng viên</label>
          <select
            v-model="editAssignmentForm.teacher"
            class="w-full lg-input px-4 py-2.5"
          >
            <option value="Chưa phân công">-- Chưa phân công --</option>
            <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">
              {{ lecturer.name }} ({{ lecturer.load }})
            </option>
          </select>
        </div>

        <!-- Ca -->
        <div>
          <label class="block text-sm font-bold text-label mb-2">Ca</label>
          <input
            v-model.number="editAssignmentForm.sessions"
            type="number"
            min="1"
            max="10"
            class="w-full lg-input px-4 py-2.5"
          />
        </div>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-[var(--border-default)]">
          <button
            type="button"
            @click="showEditModal = false"
            class="flex-1 lg-button-secondary px-4 py-2.5 font-bold"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="flex-1 lg-button-primary px-4 py-2.5 font-bold shadow-lg shadow-[var(--lg-primary)]/20"
          >
            Cập nhật
          </button>
        </div>
      </form>
    </div>
  </div>

  <!-- Change Teacher Modal -->
  <div v-if="showChangeTeacherModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
    <div class="bg-[var(--surface-card)] border border-[var(--border-default)] rounded-2xl shadow-2xl w-full max-w-sm overflow-hidden">
      <div class="flex items-center justify-between p-4 border-b border-[var(--border-default)]">
        <h3 class="text-lg font-bold text-heading">Đổi giảng viên</h3>
        <button @click="closeChangeTeacherModal" class="p-2 text-placeholder hover:text-heading hover:bg-[var(--surface-input)] rounded-xl transition-all">
          <X :size="20" />
        </button>
      </div>

      <!-- Banner gợi ý -->
      <div v-if="isSuggested" class="mx-4 mt-4 p-3 bg-[var(--color-info-bg)] border border-[var(--color-info-text)]/20 rounded-xl flex items-start gap-2.5">
        <Lightbulb :size="16" class="text-[var(--color-info-text)] shrink-0 mt-0.5" />
        <p class="text-xs font-medium text-[var(--color-info-text)]">
          Hệ thống kiểm tra xung đột đề xuất đổi sang giảng viên <strong>{{ suggestedTeacherName }}</strong>.
        </p>
      </div>
      
      <form @submit.prevent="handleChangeTeacher" class="p-4 space-y-4">
        <div>
          <label class="block text-sm font-bold text-label mb-2">Chọn giảng viên mới</label>
          <select
            v-model="changeTeacherForm.teacher"
            class="w-full lg-input px-4 py-2.5"
          >
            <option value="">-- Hủy phân công --</option>
            <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">
              {{ lecturer.name }} ({{ lecturer.load }})
            </option>
          </select>
        </div>

        <div class="flex gap-3 pt-2">
          <button
            type="button"
            @click="closeChangeTeacherModal"
            class="flex-1 lg-button-secondary px-4 py-2.5 font-bold"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="flex-1 px-4 py-2.5 font-bold rounded-xl text-white bg-[var(--lg-primary)] hover:opacity-90 transition-opacity"
          >
            Lưu thay đổi
          </button>
        </div>
      </form>
    </div>
  </div>

  <!-- Delete Confirm Modal -->
  <div v-if="showDeleteModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
    <div class="bg-[var(--surface-card)] border border-[var(--color-danger-bg)] rounded-2xl shadow-2xl w-full max-w-sm overflow-hidden">
      <div class="p-6 text-center">
        <div class="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-[var(--color-danger-bg)] text-[var(--color-danger-text)] mb-4">
          <Trash2 :size="24" />
        </div>
        <h3 class="text-lg font-bold text-heading mb-2">Xóa phân công?</h3>
        <p class="text-sm text-placeholder mb-6">
          Bạn có chắc chắn muốn xóa phân công môn <strong class="text-heading">{{ itemToDelete?.subject }}</strong> lớp <strong class="text-heading">{{ itemToDelete?.class }}</strong> không? Hành động này không thể hoàn tác.
        </p>
        <div class="flex gap-3">
          <button
            @click="showDeleteModal = false"
            class="flex-1 lg-button-secondary px-4 py-2.5 font-bold"
          >
            Hủy
          </button>
          <button
            @click="handleDelete"
            class="flex-1 px-4 py-2.5 font-bold rounded-xl text-white bg-[var(--lg-danger)] hover:opacity-90 transition-opacity"
          >
            Xóa ngay
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
