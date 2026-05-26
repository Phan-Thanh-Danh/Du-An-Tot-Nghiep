<script setup>
import { ref, computed } from 'vue'
import {
  Search,
  Filter,
  Plus,
  UserPlus,
  ArrowLeftRight,
  Calendar,
  MoreHorizontal,
  ChevronDown,
  UserCheck,
  UserMinus,
  X,
  ChevronLeft,
  ChevronRight
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── State ────────────────────────────────────────────────
const searchQuery = ref('')
const showAddModal = ref(false)
const showFilterPanel = ref(false)
const currentPage = ref(1)

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
  { id: 'PC001', subject: 'Lập trình Java', class: 'SE1601', teacher: 'TS. Nguyễn Minh Khoa', sessions: 3, status: 'assigned', dept: 'CNTT' },
  { id: 'PC002', subject: 'Cấu trúc dữ liệu', class: 'SE1602', teacher: 'ThS. Trần Thị Lan', sessions: 2, status: 'assigned', dept: 'CNTT' },
  { id: 'PC003', subject: 'Lập trình Web', class: 'SE1603', teacher: 'Chưa phân công', sessions: 3, status: 'unassigned', dept: 'CNTT' },
  { id: 'PC004', subject: 'Hệ quản trị CSDL', class: 'SE1604', teacher: 'ThS. Lê Văn Dũng', sessions: 3, status: 'assigned', dept: 'CNTT' },
  { id: 'PC005', subject: 'An toàn thông tin', class: 'SE1605', teacher: 'TS. Phạm Minh Tuấn', sessions: 2, status: 'assigned', dept: 'ATTT' },
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
    case 'assigned': return 'bg-green-100 text-green-700'
    case 'unassigned': return 'bg-red-100 text-red-700'
    default: return 'bg-slate-100 text-slate-700'
  }
}

// ── Functions ────────────────────────────────────────────────
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
</script>

<template>
  <PageContainer
    title="Phân công giảng viên"
    subtitle="Quản lý gán giảng viên cho các lớp học phần trong học kỳ."
  >
    <template #actions>
      <button @click="showAddModal = true" class="lg-button-primary px-5 py-2.5 text-sm font-bold shadow-lg shadow-blue-500/20 hover:shadow-xl transition-all">
        <Plus :size="18" /> Thêm phân công
      </button>
    </template>

    <div class="space-y-6">
      <!-- ── Stats Header ── -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-blue-50 flex items-center justify-center text-blue-600">
            <UserPlus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Tổng phân công</p>
            <p class="text-2xl font-black text-slate-800">124</p>
          </div>
        </div>
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-orange-50 flex items-center justify-center text-orange-600">
            <UserMinus :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Chưa gán GV</p>
            <p class="text-2xl font-black text-slate-800">12</p>
          </div>
        </div>
        <div class="lg-card-glass p-6 flex items-center gap-4">
          <div class="h-12 w-12 rounded-2xl bg-green-50 flex items-center justify-center text-green-600">
            <UserCheck :size="24" />
          </div>
          <div>
            <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest">Hoàn tất</p>
            <p class="text-2xl font-black text-slate-800">90%</p>
          </div>
        </div>
      </div>

      <!-- ── Filter & Search ── -->
      <div class="lg-glass-strong p-4 rounded-[24px] flex flex-wrap items-center justify-between gap-4">
        <div class="flex-1 min-w-[300px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-slate-400" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo môn, lớp hoặc giảng viên..."
            class="w-full bg-white border border-slate-100 rounded-xl pl-11 pr-4 py-2.5 text-sm font-medium outline-none focus:ring-4 focus:ring-blue-500/10 transition-all"
          >
        </div>
        <div class="flex items-center gap-3">
          <button @click="showFilterPanel = !showFilterPanel" class="lg-button-secondary px-4 py-2.5 text-sm font-bold hover:bg-white transition-colors">
            <Filter :size="18" /> Bộ lọc
          </button>
          <div class="h-8 w-px bg-slate-200"></div>
          <button class="lg-icon-button bg-white border border-slate-100 p-2.5 text-slate-500 hover:bg-slate-50">
            <Calendar :size="20" />
          </button>
        </div>
      </div>

      <!-- ── Assignment Table ── -->
      <div class="lg-table-shell">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-slate-50/50">
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Mã PC</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Lớp & Môn</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Giảng viên</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Tiết/Tuần</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Trạng thái</th>
              <th class="px-6 py-4 text-[10px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="item in paginatedAssignments" :key="item.id" class="group hover:bg-white/50 transition-colors">
              <td class="px-6 py-4">
                <span class="text-xs font-black text-slate-800">{{ item.id }}</span>
              </td>
              <td class="px-6 py-4">
                <p class="text-sm font-black text-slate-800 leading-tight">{{ item.subject }}</p>
                <p class="text-[11px] font-bold text-blue-600 mt-1 uppercase">{{ item.class }}</p>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-8 w-8 rounded-full bg-slate-100 flex items-center justify-center text-[10px] font-black text-slate-500">
                    {{ item.teacher !== 'Chưa phân công' ? item.teacher.split(' ').pop().charAt(0) : '?' }}
                  </div>
                  <div>
                    <p :class="['text-sm font-bold', item.teacher === 'Chưa phân công' ? 'text-red-500 italic' : 'text-slate-700']">
                      {{ item.teacher }}
                    </p>
                    <p v-if="item.teacher !== 'Chưa phân công'" class="text-[10px] font-medium text-slate-400">Tải dạy: 12/15</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <span class="h-2 w-16 bg-slate-100 rounded-full overflow-hidden inline-flex">
                    <span :style="{ width: (item.sessions / 4) * 100 + '%' }" class="bg-blue-500 h-full"></span>
                  </span>
                  <span class="text-xs font-bold text-slate-700">{{ item.sessions }} tiết</span>
                </div>
              </td>
              <td class="px-6 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest', getStatusBadge(item.status)]">
                  {{ item.status === 'assigned' ? 'Đã gán' : 'Cần gán' }}
                </span>
              </td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-2">
                  <button class="p-2 hover:bg-blue-50 hover:text-blue-600 rounded-lg text-slate-400 transition-all" title="Đổi giảng viên">
                    <ArrowLeftRight :size="16" />
                  </button>
                  <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 transition-all">
                    <MoreHorizontal :size="16" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- ── Footer Info ── -->
      <div class="flex items-center justify-between px-4">
         <p class="text-xs font-medium text-slate-500">Hiển thị {{ paginatedAssignments.length }} / {{ filteredAssignments.length }} bản ghi</p>
         <div class="flex items-center gap-1">
            <button @click="goToPage(currentPage - 1)" :disabled="currentPage === 1" class="h-8 w-8 rounded-lg flex items-center justify-center text-slate-500 hover:bg-white disabled:opacity-50 transition-all">
              <ChevronLeft :size="16" />
            </button>
            <button v-for="p in totalPages" :key="p" @click="goToPage(p)" :class="['h-8 w-8 rounded-lg flex items-center justify-center text-xs font-bold transition-all', p === currentPage ? 'bg-blue-600 text-white shadow-md' : 'hover:bg-white text-slate-500']">
              {{ p }}
            </button>
            <button @click="goToPage(currentPage + 1)" :disabled="currentPage === totalPages" class="h-8 w-8 rounded-lg flex items-center justify-center text-slate-500 hover:bg-white disabled:opacity-50 transition-all">
              <ChevronRight :size="16" />
            </button>
         </div>
      </div>
    </div>
  </PageContainer>

  <!-- ── Filter Panel ── -->
  <div v-if="showFilterPanel" class="fixed inset-0 z-40 bg-black/20 backdrop-blur-sm" @click="showFilterPanel = false"></div>
  <div v-if="showFilterPanel" class="fixed top-24 right-6 z-50 bg-white rounded-[20px] shadow-2xl border border-slate-100 p-6 w-72 space-y-6">
    <div>
      <h3 class="text-sm font-bold text-slate-700 mb-3">Trạng thái</h3>
      <div class="space-y-2">
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="all" class="w-4 h-4 rounded-full">
          <span class="text-sm text-slate-700">Tất cả</span>
        </label>
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="assigned" class="w-4 h-4 rounded-full">
          <span class="text-sm text-slate-700">Đã gán</span>
        </label>
        <label class="flex items-center gap-3 cursor-pointer">
          <input v-model="filters.status" type="radio" value="unassigned" class="w-4 h-4 rounded-full">
          <span class="text-sm text-slate-700">Chưa gán</span>
        </label>
      </div>
    </div>

    <div class="border-t border-slate-100"></div>

    <div>
      <h3 class="text-sm font-bold text-slate-700 mb-3">Phòng ban</h3>
      <select v-model="filters.department" class="w-full px-3 py-2 rounded-lg border border-slate-200 text-sm outline-none focus:ring-2 focus:ring-blue-500/20">
        <option value="all">Tất cả</option>
        <option v-for="dept in departments" :key="dept" :value="dept">{{ dept }}</option>
      </select>
    </div>

    <div class="border-t border-slate-100"></div>

    <div>
      <h3 class="text-sm font-bold text-slate-700 mb-3">Giảng viên</h3>
      <select v-model="filters.lecturer" class="w-full px-3 py-2 rounded-lg border border-slate-200 text-sm outline-none focus:ring-2 focus:ring-blue-500/20">
        <option value="all">Tất cả</option>
        <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">{{ lecturer.name }}</option>
      </select>
    </div>

    <div class="border-t border-slate-100"></div>

    <div class="flex gap-3">
      <button @click="clearFilters" class="flex-1 px-4 py-2 rounded-lg border border-slate-200 text-slate-700 text-sm font-bold hover:bg-slate-50 transition-colors">
        Xóa bộ lọc
      </button>
      <button @click="showFilterPanel = false" class="flex-1 px-4 py-2 rounded-lg bg-blue-600 text-white text-sm font-bold hover:bg-blue-700 transition-colors">
        Áp dụng
      </button>
    </div>
  </div>

  <!-- ── Modal Add Assignment ── -->
  <div v-if="showAddModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
    <div class="bg-white rounded-[24px] shadow-2xl max-w-md w-full p-6 border border-slate-100">
      <!-- Header -->
      <div class="flex items-center justify-between mb-6">
        <h2 class="text-xl font-bold text-slate-900">Thêm phân công</h2>
        <button @click="showAddModal = false" class="p-2 hover:bg-slate-100 rounded-lg text-slate-400 hover:text-slate-600 transition-colors">
          <X :size="20" />
        </button>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleAddAssignment" class="space-y-4">
        <!-- Môn học -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Môn học</label>
          <input
            v-model="newAssignmentForm.subject"
            type="text"
            placeholder="Nhập tên môn học"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Lớp -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Lớp</label>
          <input
            v-model="newAssignmentForm.class"
            type="text"
            placeholder="Nhập mã lớp (vd: SE1601)"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Giảng viên -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Giảng viên</label>
          <select
            v-model="newAssignmentForm.teacher"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option value="">-- Chọn giảng viên --</option>
            <option v-for="lecturer in lecturers" :key="lecturer.id" :value="lecturer.name">
              {{ lecturer.name }} ({{ lecturer.load }})
            </option>
          </select>
        </div>

        <!-- Tiết/Tuần -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Tiết/Tuần</label>
          <input
            v-model.number="newAssignmentForm.sessions"
            type="number"
            min="1"
            max="10"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          />
        </div>

        <!-- Trạng thái -->
        <div>
          <label class="block text-sm font-bold text-slate-700 mb-2">Trạng thái</label>
          <select
            v-model="newAssignmentForm.status"
            class="w-full px-4 py-2.5 rounded-lg border border-slate-200 focus:outline-none focus:ring-2 focus:ring-blue-500/20 focus:border-blue-500 transition-colors"
          >
            <option value="assigned">Đã gán</option>
            <option value="unassigned">Chưa gán</option>
          </select>
        </div>

        <!-- Actions -->
        <div class="flex gap-3 pt-4 border-t border-slate-200">
          <button
            type="button"
            @click="showAddModal = false"
            class="flex-1 px-4 py-2.5 rounded-lg border border-slate-200 text-slate-700 font-bold hover:bg-slate-50 transition-colors"
          >
            Hủy
          </button>
          <button
            type="submit"
            class="flex-1 px-4 py-2.5 rounded-lg bg-blue-600 text-white font-bold hover:bg-blue-700 transition-colors shadow-lg shadow-blue-500/20"
          >
            Thêm
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
