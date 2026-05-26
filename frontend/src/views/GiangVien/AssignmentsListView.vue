<script setup>
import { ref, computed } from 'vue'
import { 
  Plus, Search, ClipboardList, Users, Clock, 
  ChevronRight, FileText, Send,
  X, Upload, Calendar, Award, AlertCircle, 
  CheckCircle2, Trash2, FileCode, Check, BookOpen, SlidersHorizontal
} from 'lucide-vue-next'

const assignments = ref([
  { id: 1, name: 'Assignment 1: HTML/CSS Basic', className: 'SE1601', deadline: '20/05/2026 23:59', status: 'Active', submissionsCount: 42, totalStudents: 45, maxScore: 10, type: 'Assignment' },
  { id: 2, name: 'Assignment 2: JavaScript DOM', className: 'SE1601', deadline: '28/05/2026 23:59', status: 'Active', submissionsCount: 12, totalStudents: 45, maxScore: 10, type: 'Assignment' },
  { id: 3, name: 'Lab 1: UI Design with Figma', className: 'SS1402', deadline: '15/05/2026 23:59', status: 'Completed', submissionsCount: 38, totalStudents: 38, maxScore: 10, type: 'Lab' },
])

const classesList = ref([
  { code: 'SE1601', name: 'Lớp SE1601 - Java', subject: 'Lập trình Java', students: 45 },
  { code: 'SS1402', name: 'Lớp SS1402 - Web', subject: 'Lập trình Web', students: 38 },
  { code: 'SA1709', name: 'Lớp SA1709 - DB', subject: 'Cơ sở dữ liệu', students: 42 }
])

// Filter & Search states
const searchQuery = ref('')
const selectedClassFilter = ref('')

const filteredAssignments = computed(() => {
  return assignments.value.filter(asm => {
    const matchesSearch = asm.name.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchesClass = !selectedClassFilter.value || asm.className === selectedClassFilter.value
    return matchesSearch && matchesClass
  })
})

// Stats computation
const stats = computed(() => {
  const activeCount = assignments.value.filter(a => a.status === 'Active').length
  const draftCount = assignments.value.filter(a => a.status === 'Draft').length
  const totalSubmissions = assignments.value.reduce((acc, curr) => acc + curr.submissionsCount, 0)
  
  return {
    active: activeCount + draftCount,
    submissions: totalSubmissions,
    pendingGrades: 24
  }
})

// Modal & creation states
const showCreateModal = ref(false)
const dragActive = ref(false)
const isSubmitting = ref(false)

const initialForm = {
  name: '',
  classId: '',
  type: 'Assignment',
  deadlineDate: '',
  deadlineTime: '23:59',
  maxScore: 10,
  description: '',
  status: 'Active',
  files: []
}
const form = ref({ ...initialForm })
const errors = ref({})

// File selection
const fileInputRef = ref(null)

// Toast notification state
const toast = ref({
  show: false,
  message: '',
  type: 'success'
})

function triggerToast(msg, type = 'success') {
  toast.value.message = msg
  toast.value.type = type
  toast.value.show = true
  setTimeout(() => {
    toast.value.show = false
  }, 4000)
}

function openCreateModal() {
  form.value = {
    name: '',
    classId: '',
    type: 'Assignment',
    deadlineDate: '',
    deadlineTime: '23:59',
    maxScore: 10,
    description: '',
    status: 'Active',
    files: []
  }
  errors.value = {}
  showCreateModal.value = true
}

function closeCreateModal() {
  showCreateModal.value = false
}

// Drag & drop handlers
function handleDragEnter(e) {
  e.preventDefault()
  dragActive.value = true
}
function handleDragLeave(e) {
  e.preventDefault()
  dragActive.value = false
}
function handleDragOver(e) {
  e.preventDefault()
}
function handleDrop(e) {
  e.preventDefault()
  dragActive.value = false
  if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
    addFiles(e.dataTransfer.files)
  }
}

function triggerFileInput() {
  fileInputRef.value?.click()
}

function handleFileSelect(e) {
  if (e.target.files && e.target.files.length > 0) {
    addFiles(e.target.files)
  }
}

function addFiles(fileList) {
  for (let i = 0; i < fileList.length; i++) {
    const file = fileList[i]
    if (form.value.files.some(f => f.name === file.name)) continue
    
    const newFile = ref({
      name: file.name,
      size: (file.size / (1024 * 1024)).toFixed(2) + ' MB',
      progress: 0,
      status: 'uploading'
    })
    form.value.files.push(newFile.value)
    
    let prog = 0
    const interval = setInterval(() => {
      prog += Math.floor(Math.random() * 25) + 15
      if (prog >= 100) {
        newFile.value.progress = 100
        newFile.value.status = 'done'
        clearInterval(interval)
      } else {
        newFile.value.progress = prog
      }
    }, 200)
  }
}

function removeFile(index) {
  form.value.files.splice(index, 1)
}

function validateForm() {
  errors.value = {}
  let isValid = true
  
  if (!form.value.name.trim()) {
    errors.value.name = 'Vui lòng nhập tên bài tập'
    isValid = false
  }
  
  if (!form.value.classId) {
    errors.value.classId = 'Vui lòng chọn lớp học học phần'
    isValid = false
  }
  
  if (!form.value.deadlineDate) {
    errors.value.deadlineDate = 'Vui lòng chọn ngày hạn nộp'
    isValid = false
  }

  if (!form.value.maxScore || form.value.maxScore <= 0 || form.value.maxScore > 100) {
    errors.value.maxScore = 'Thang điểm phải từ 1 đến 100'
    isValid = false
  }
  
  return isValid
}

function submitAssignment() {
  if (!validateForm()) {
    triggerToast('Vui lòng hoàn thiện tất cả thông tin hợp lệ!', 'error')
    return
  }
  
  isSubmitting.value = true
  
  setTimeout(() => {
    const selectedClass = classesList.value.find(c => c.code === form.value.classId)
    const dateParts = form.value.deadlineDate.split('-')
    const formattedDate = `${dateParts[2]}/${dateParts[1]}/${dateParts[0]}`
    
    const newAsm = {
      id: assignments.value.length + 1,
      name: form.value.name,
      className: form.value.classId,
      deadline: `${formattedDate} ${form.value.deadlineTime}`,
      submissionsCount: 0,
      totalStudents: selectedClass ? selectedClass.students : 45,
      status: form.value.status,
      maxScore: form.value.maxScore,
      type: form.value.type
    }
    
    assignments.value.unshift(newAsm)
    isSubmitting.value = false
    showCreateModal.value = false
    
    triggerToast(`Đã tạo thành công bài tập "${form.value.name}"!`, 'success')
  }, 900)
}
</script>

<template>
  <div class="space-y-4 pb-10 relative">
    
    <!-- Toast Message Component -->
    <Transition name="toast-slide">
      <div v-if="toast.show" 
           :class="['fixed top-4 right-6 z-[100] flex items-center gap-3 px-5 py-4 rounded-2xl shadow-xl border backdrop-blur-md transition-all duration-300', 
                    toast.type === 'success' ? 'bg-emerald-500/90 border-emerald-400 text-white' : 'bg-rose-500/90 border-rose-400 text-white']">
        <CheckCircle2 v-if="toast.type === 'success'" :size="20" />
        <AlertCircle v-else :size="20" />
        <p class="text-sm font-bold tracking-wide">{{ toast.message }}</p>
      </div>
    </Transition>

    <!-- Header -->
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-xl font-black text-slate-800 tracking-tight flex items-center gap-3">
          Danh sách bài tập
        </h1>
        <p class="text-slate-500 mt-1">Quản lý các bài tập về nhà, bài Lab và Assignment cho các lớp học phần.</p>
      </div>
      <button @click="openCreateModal" class="lg-button-primary py-3 px-4 shadow-lg shadow-indigo-100 hover:shadow-xl hover:shadow-indigo-200 transition-all flex items-center gap-2 group active:scale-95" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
        <Plus :size="20" class="group-hover:rotate-90 transition-transform duration-300" /> Tạo bài tập mới
      </button>
    </div>

    <!-- Stats Summary -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
       <div class="lg-card-glass p-4 border-slate-100/80 flex items-center gap-5 bg-white shadow-sm hover:shadow-md transition-all rounded-3xl">
          <div class="h-10 w-10 rounded-2xl bg-indigo-50 text-indigo-600 flex items-center justify-center border border-indigo-100/50">
             <ClipboardList :size="26" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Đang mở</p>
             <p class="text-xl font-black text-slate-850 mt-0.5">{{ stats.active }} bài tập</p>
          </div>
       </div>
       <div class="lg-card-glass p-4 border-slate-100/80 flex items-center gap-5 bg-white shadow-sm hover:shadow-md transition-all rounded-3xl">
          <div class="h-10 w-10 rounded-2xl bg-amber-50 text-amber-600 flex items-center justify-center border border-amber-100/50">
             <Send :size="26" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Tổng bài nộp</p>
             <p class="text-xl font-black text-slate-850 mt-0.5">{{ stats.submissions }} bài</p>
          </div>
       </div>
       <div class="lg-card-glass p-4 border-slate-100/80 flex items-center gap-5 bg-white shadow-sm hover:shadow-md transition-all rounded-3xl">
          <div class="h-10 w-10 rounded-2xl bg-emerald-50 text-emerald-600 flex items-center justify-center border border-emerald-100/50">
             <Clock :size="26" />
          </div>
          <div>
             <p class="text-xs font-bold text-slate-400 uppercase tracking-widest">Cần chấm điểm</p>
             <p class="text-xl font-black text-slate-850 mt-0.5">{{ stats.pendingGrades }} bài</p>
          </div>
       </div>
    </div>

    <!-- Table & Filters -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden transition-all duration-300">
      <div class="p-4 border-b border-slate-50 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <div>
          <h2 class="text-xl font-bold text-slate-800">Chi tiết bài tập</h2>
          <p class="text-xs font-medium text-slate-400 mt-0.5">Sử dụng công cụ tìm kiếm và lọc để tra cứu nhanh.</p>
        </div>
        
        <div class="flex flex-col sm:flex-row gap-3 w-full md:w-auto">
          <!-- Search input -->
          <div class="relative flex-1 sm:w-64">
            <Search :size="16" class="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400" />
            <input 
              v-model="searchQuery"
              type="text" 
              placeholder="Tìm tên bài tập..." 
              class="w-full rounded-2xl border border-slate-200 bg-slate-50/50 pl-10 pr-4 py-2.5 text-xs font-semibold outline-none focus:border-indigo-400 focus:bg-white focus:ring-4 focus:ring-indigo-50 transition-all text-slate-800" 
            />
          </div>
          
          <!-- Class filter dropdown -->
          <div class="relative">
            <select 
              v-model="selectedClassFilter"
              class="appearance-none w-full sm:w-44 rounded-2xl border border-slate-200 bg-slate-50/50 px-4 py-2.5 text-xs font-bold outline-none focus:border-indigo-400 focus:bg-white focus:ring-4 focus:ring-indigo-50 transition-all pr-8 text-slate-800 cursor-pointer"
            >
              <option value="">Tất cả các lớp</option>
              <option v-for="cls in classesList" :key="cls.code" :value="cls.code">
                {{ cls.code }} ({{ cls.subject }})
              </option>
            </select>
            <SlidersHorizontal :size="14" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
          </div>
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/60 text-[11px] font-bold uppercase tracking-wider text-slate-450 border-b border-slate-50">
              <th class="px-5 py-5">Tên bài tập</th>
              <th class="px-4 py-5">Lớp</th>
              <th class="px-4 py-5">Hạn nộp</th>
              <th class="px-4 py-5">Số bài nộp</th>
              <th class="px-4 py-5">Trạng thái</th>
              <th class="px-5 py-5 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50 text-slate-800">
            <tr v-for="asm in filteredAssignments" :key="asm.id" class="group hover:bg-slate-50/40 transition-colors">
              <td class="px-5 py-5">
                <div class="flex items-center gap-3">
                  <div class="h-10 w-10 rounded-xl bg-slate-50 flex items-center justify-center text-slate-400 group-hover:bg-indigo-50 group-hover:text-indigo-600 transition-all border border-slate-100">
                    <FileText :size="20" />
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800 leading-tight">{{ asm.name }}</p>
                    <p class="text-[10px] font-bold uppercase tracking-wider text-slate-400 mt-1">Loại: {{ asm.type || 'Assignment' }}</p>
                  </div>
                </div>
              </td>
              <td class="px-4 py-5">
                <span class="rounded-lg bg-indigo-50 px-2.5 py-1 text-[11px] font-black text-indigo-600 uppercase tracking-wider border border-indigo-100/50">
                  {{ asm.className }}
                </span>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-2 text-sm">
                   <Clock :size="14" class="text-slate-350" />
                   <span class="font-medium text-slate-650">{{ asm.deadline }}</span>
                </div>
              </td>
              <td class="px-4 py-5">
                <div class="flex items-center gap-3">
                   <span class="text-sm font-black text-slate-700">{{ asm.submissionsCount }}/{{ asm.totalStudents }}</span>
                   <div class="h-1.5 w-16 bg-slate-100 rounded-full overflow-hidden">
                      <div class="h-full bg-indigo-500 rounded-full transition-all duration-550" :style="{ width: ((asm.submissionsCount/asm.totalStudents)*100) + '%' }"></div>
                   </div>
                </div>
              </td>
              <td class="px-4 py-5">
                <span v-if="asm.status === 'Active'" class="inline-flex items-center gap-1.5 rounded-full bg-indigo-50 px-2.5 py-0.5 text-[10px] font-bold text-indigo-700 border border-indigo-100">
                  <span class="h-1.5 w-1.5 rounded-full bg-indigo-500 animate-pulse"></span> Đang mở
                </span>
                <span v-else-if="asm.status === 'Draft'" class="inline-flex items-center gap-1.5 rounded-full bg-slate-50 px-2.5 py-0.5 text-[10px] font-bold text-slate-500 border border-slate-200">
                  Nháp
                </span>
                <span v-else class="inline-flex items-center gap-1.5 rounded-full bg-emerald-55/70 px-2.5 py-0.5 text-[10px] font-bold text-emerald-700 border border-emerald-100">
                  <Check :size="10" /> Hoàn thành
                </span>
              </td>
              <td class="px-5 py-5 text-right">
                <router-link to="/teacher/grading" class="inline-flex items-center gap-1.5 rounded-xl bg-slate-50 border border-slate-200/60 px-3.5 py-2 text-xs font-bold text-slate-600 hover:bg-indigo-600 hover:text-white hover:border-indigo-600 hover:shadow-md hover:shadow-indigo-100 transition-all duration-300">
                  Chấm điểm <ChevronRight :size="14" class="group-hover:translate-x-0.5 transition-transform" />
                </router-link>
              </td>
            </tr>
            <tr v-if="filteredAssignments.length === 0">
              <td colspan="6" class="text-center py-10">
                <div class="flex flex-col items-center justify-center text-slate-350">
                  <FileText :size="48" class="text-slate-200 mb-2" />
                  <p class="text-sm font-bold">Không tìm thấy bài tập nào</p>
                  <p class="text-xs">Vui lòng thử lại với từ khóa hoặc bộ lọc khác.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Assignment Modal -->
    <div v-if="showCreateModal" class="fixed inset-0 z-[80] flex items-center justify-center p-4">
      <!-- Backdrop with blur -->
      <div @click="closeCreateModal" class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm transition-opacity duration-300"></div>

      <!-- Modal Content Card -->
      <div class="relative w-full max-w-4xl bg-white rounded-2xl shadow-2xl border border-slate-100 flex flex-col max-h-[90vh] overflow-hidden transform transition-all duration-300 scale-100 animate-modal-in">
        
        <!-- Modal Header -->
        <div class="p-4 md:p-8 border-b border-slate-100 flex justify-between items-center relative overflow-hidden shrink-0">
          <div class="absolute -right-16 -top-16 h-36 w-36 bg-gradient-to-tr from-indigo-50 to-indigo-100/30 rounded-full blur-2xl pointer-events-none" />
          
          <div class="relative z-10 flex items-center gap-4">
            <div class="h-10 w-10 rounded-2xl bg-indigo-50 text-indigo-600 flex items-center justify-center border border-indigo-100/50">
               <Plus :size="24" />
            </div>
            <div>
               <h3 class="text-xl md:text-xl font-black text-slate-850">Tạo Bài Tập Mới</h3>
               <p class="text-xs font-semibold text-slate-400 mt-0.5">Điền các thông tin cần thiết bên dưới để phát hành bài tập cho sinh viên.</p>
            </div>
          </div>
          <button @click="closeCreateModal" class="h-10 w-10 rounded-full bg-slate-50 flex items-center justify-center text-slate-450 hover:bg-rose-50 hover:text-rose-500 hover:rotate-90 transition-all duration-300 relative z-10">
             <X :size="18" />
          </button>
        </div>

        <!-- Modal Body (Scrollable form) -->
        <div class="overflow-y-auto p-4 md:p-8 flex-1 grid grid-cols-1 lg:grid-cols-2 gap-8 bg-slate-50/20">
          
          <!-- Column 1: Details and Configurations -->
          <div class="space-y-4">
            <h4 class="text-[11px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 pb-2">1. Thông tin chung</h4>
            
            <!-- Title -->
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700 flex items-center gap-1.5">
                Tiêu đề bài tập <span class="text-rose-550">*</span>
              </label>
              <div class="relative">
                <input 
                  v-model="form.name"
                  type="text"
                  placeholder="Ví dụ: Assignment 3: Xây dựng giao diện Single Page..."
                  :class="['w-full rounded-2xl border bg-white px-4 py-3 text-sm font-semibold outline-none focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850', 
                           errors.name ? 'border-rose-400 focus:border-rose-450' : 'border-slate-200 focus:border-indigo-400']"
                />
                <p v-if="errors.name" class="text-xs font-bold text-rose-500 mt-1.5 flex items-center gap-1">
                  <AlertCircle :size="12" /> {{ errors.name }}
                </p>
              </div>
            </div>

            <!-- Class & Score Row -->
            <div class="grid grid-cols-2 gap-4">
              <!-- Class selection -->
              <div class="space-y-2">
                <label class="text-xs font-bold text-slate-700 flex items-center gap-1.5">
                  Lớp học phần <span class="text-rose-550">*</span>
                </label>
                <div class="relative">
                  <select 
                    v-model="form.classId"
                    :class="['appearance-none w-full rounded-2xl border bg-white px-4 py-3 text-sm font-semibold outline-none focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850 cursor-pointer pr-10', 
                             errors.classId ? 'border-rose-400 focus:border-rose-450' : 'border-slate-200 focus:border-indigo-400']"
                  >
                    <option value="" disabled selected>Chọn lớp...</option>
                    <option v-for="cls in classesList" :key="cls.code" :value="cls.code">
                      {{ cls.code }} - {{ cls.subject }}
                    </option>
                  </select>
                  <Users :size="14" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <p v-if="errors.classId" class="text-xs font-bold text-rose-500 mt-1.5 flex items-center gap-1">
                    <AlertCircle :size="12" /> {{ errors.classId }}
                  </p>
                </div>
              </div>

              <!-- Max Score -->
              <div class="space-y-2">
                <label class="text-xs font-bold text-slate-700 flex items-center gap-1.5">
                  Thang điểm tối đa <span class="text-rose-550">*</span>
                </label>
                <div class="relative">
                  <input 
                    v-model.number="form.maxScore"
                    type="number"
                    min="1" max="100"
                    placeholder="10"
                    :class="['w-full rounded-2xl border bg-white px-4 py-3 text-sm font-semibold outline-none focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850', 
                             errors.maxScore ? 'border-rose-400 focus:border-rose-450' : 'border-slate-200 focus:border-indigo-400']"
                  />
                  <Award :size="14" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                  <p v-if="errors.maxScore" class="text-xs font-bold text-rose-500 mt-1.5 flex items-center gap-1">
                    <AlertCircle :size="12" /> {{ errors.maxScore }}
                  </p>
                </div>
              </div>
            </div>

            <!-- Type selector (Visual cards) -->
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Loại bài tập</label>
              <div class="grid grid-cols-3 gap-3">
                <button 
                  v-for="t in ['Assignment', 'Lab', 'Homework']" 
                  :key="t"
                  type="button"
                  @click="form.type = t"
                  :class="['p-3 rounded-2xl border text-center transition-all flex flex-col items-center justify-center gap-1.5',
                           form.type === t ? 'border-indigo-500 bg-indigo-50/50 text-indigo-700 ring-2 ring-indigo-500/10' : 'border-slate-200 hover:border-slate-350 hover:bg-slate-50 text-slate-650 bg-white']"
                >
                  <FileText v-if="t === 'Assignment'" :size="18" />
                  <BookOpen v-else-if="t === 'Lab'" :size="18" />
                  <Send v-else :size="18" />
                  <span class="text-xs font-bold">{{ t }}</span>
                </button>
              </div>
            </div>

            <!-- Deadline picker row -->
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700 flex items-center gap-1.5">
                Thời hạn nộp bài (Deadline) <span class="text-rose-550">*</span>
              </label>
              <div class="grid grid-cols-2 gap-4">
                <div class="relative">
                  <input 
                    v-model="form.deadlineDate"
                    type="date"
                    :class="['w-full rounded-2xl border bg-white pl-4 pr-10 py-3 text-sm font-semibold outline-none focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850 cursor-pointer', 
                             errors.deadlineDate ? 'border-rose-400 focus:border-rose-450' : 'border-slate-200 focus:border-indigo-400']"
                  />
                  <Calendar :size="14" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                </div>
                
                <div class="relative">
                  <input 
                    v-model="form.deadlineTime"
                    type="time"
                    class="w-full rounded-2xl border border-slate-200 bg-white pl-4 pr-10 py-3 text-sm font-semibold outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850 cursor-pointer"
                  />
                  <Clock :size="14" class="absolute right-3.5 top-1/2 -translate-y-1/2 text-slate-400 pointer-events-none" />
                </div>
              </div>
              <p v-if="errors.deadlineDate" class="text-xs font-bold text-rose-500 mt-1 flex items-center gap-1">
                <AlertCircle :size="12" /> {{ errors.deadlineDate }}
              </p>
            </div>

            <!-- Switch status (Active / Draft) -->
            <div class="rounded-2xl border border-slate-200/80 bg-white p-4 flex items-center justify-between shadow-sm">
              <div>
                <p class="text-xs font-bold text-slate-800">Kích hoạt bài tập ngay</p>
                <p class="text-[10px] font-semibold text-slate-450 mt-0.5">Sinh viên có thể nhìn thấy và nộp bài ngay sau khi tạo.</p>
              </div>
              <button 
                type="button"
                @click="form.status = form.status === 'Active' ? 'Draft' : 'Active'"
                :class="['relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors duration-200 ease-in-out focus:outline-none',
                         form.status === 'Active' ? 'bg-indigo-600' : 'bg-slate-200']"
              >
                <span 
                  :class="['pointer-events-none inline-block h-5 w-5 transform rounded-full bg-white shadow ring-0 transition duration-200 ease-in-out',
                           form.status === 'Active' ? 'translate-x-5' : 'translate-x-0']"
                />
              </button>
            </div>

          </div>

          <!-- Column 2: Instructions and Attachments -->
          <div class="space-y-4">
            <h4 class="text-[11px] font-black text-slate-400 uppercase tracking-widest border-b border-slate-100 pb-2">2. Hướng dẫn & Tài liệu</h4>
            
            <!-- Description instructions -->
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Mô tả & Hướng dẫn chi tiết</label>
              <textarea 
                v-model="form.description"
                rows="5"
                placeholder="Nhập yêu cầu đề bài, tiêu chí đánh giá, các bước thực hiện chi tiết cho sinh viên..."
                class="w-full rounded-2xl border border-slate-200 bg-white p-4 text-xs font-semibold outline-none focus:border-indigo-400 focus:ring-4 focus:ring-indigo-50 transition-all text-slate-850 resize-none leading-relaxed"
              ></textarea>
            </div>

            <!-- Upload files area (Simulated Drag & Drop) -->
            <div class="space-y-2">
              <label class="text-xs font-bold text-slate-700">Tài liệu đính kèm (Đề bài, Data sample...)</label>
              
              <div 
                @dragenter="handleDragEnter"
                @dragleave="handleDragLeave"
                @dragover="handleDragOver"
                @drop="handleDrop"
                @click="triggerFileInput"
                :class="['border-2 border-dashed rounded-[24px] p-4 text-center cursor-pointer transition-all duration-300 bg-white/50 flex flex-col items-center justify-center min-h-[160px]',
                         dragActive ? 'border-indigo-500 bg-indigo-50/20 scale-[0.99] shadow-inner' : 'border-slate-200 hover:border-indigo-400 hover:bg-white hover:shadow-sm']"
              >
                <input 
                  ref="fileInputRef"
                  type="file" 
                  multiple 
                  class="hidden" 
                  @change="handleFileSelect"
                />
                <div class="h-10 w-10 rounded-full bg-slate-50 text-slate-400 flex items-center justify-center mb-3 shadow-inner group-hover:bg-indigo-50 group-hover:text-indigo-600 transition-colors">
                  <Upload :size="20" />
                </div>
                <p class="text-xs font-bold text-slate-750">
                  <span class="text-indigo-600 underline">Nhấp để tải lên</span> hoặc kéo thả file vào đây
                </p>
                <p class="text-[10px] font-semibold text-slate-400 mt-1">Hỗ trợ các định dạng: PDF, ZIP, RAR, DOCX (Tối đa 15MB)</p>
              </div>

              <!-- Uploading / Uploaded Files List -->
              <div v-if="form.files.length > 0" class="space-y-2 mt-3 animate-fade-in">
                <div 
                  v-for="(file, index) in form.files" 
                  :key="index"
                  class="flex items-center justify-between rounded-xl bg-white border border-slate-100 p-3 hover:border-slate-200 shadow-sm"
                >
                  <div class="flex items-center gap-3 flex-1 min-w-0 mr-3">
                    <div class="h-9 w-9 rounded-lg bg-indigo-50 text-indigo-600 flex items-center justify-center shrink-0">
                      <FileCode :size="18" />
                    </div>
                    <div class="flex-1 min-w-0">
                      <p class="text-xs font-bold text-slate-800 truncate leading-tight">{{ file.name }}</p>
                      
                      <!-- Upload progress bar -->
                      <div class="flex items-center gap-2 mt-1">
                        <span class="text-[9px] font-semibold text-slate-400">{{ file.size }}</span>
                        <div v-if="file.status === 'uploading'" class="flex-1 h-1.5 bg-slate-100 rounded-full overflow-hidden">
                          <div class="h-full bg-indigo-600 transition-all rounded-full" :style="{ width: file.progress + '%' }"></div>
                        </div>
                        <span v-if="file.status === 'uploading'" class="text-[9px] font-bold text-indigo-600">{{ file.progress }}%</span>
                        <span v-else class="text-[9px] font-bold text-emerald-600">Đã tải lên</span>
                      </div>
                    </div>
                  </div>
                  <button 
                    @click.stop="removeFile(index)"
                    class="p-2 text-slate-350 hover:text-rose-500 rounded-lg hover:bg-rose-50 transition-colors"
                  >
                    <Trash2 :size="14" />
                  </button>
                </div>
              </div>

            </div>
          </div>

        </div>

        <!-- Modal Footer -->
        <div class="p-4 border-t border-slate-100 flex justify-end gap-3 bg-white shrink-0">
          <button 
            @click="closeCreateModal"
            class="rounded-xl border border-slate-200 px-5 py-3 text-xs font-bold text-slate-500 hover:bg-slate-50 transition-all active:scale-95"
          >
            Hủy
          </button>
          
          <button 
            @click="submitAssignment"
            :disabled="isSubmitting"
            class="rounded-xl bg-gradient-to-tr from-indigo-600 to-violet-600 px-4 py-3 text-xs font-bold text-white shadow-md shadow-indigo-100 hover:shadow-lg hover:shadow-indigo-200 hover:-translate-y-0.5 transition-all flex items-center gap-2 active:scale-95 disabled:opacity-70 disabled:pointer-events-none"
          >
            <span v-if="isSubmitting" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
            <span v-else>Tạo bài tập</span>
          </button>
        </div>

      </div>
    </div>

  </div>
</template>

<style scoped>
/* Modal slide/fade animations */
@keyframes modal-in {
  from {
    opacity: 0;
    transform: scale(0.95) translateY(15px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}
.animate-modal-in {
  animation: modal-in 0.35s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes fade-in {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}
.animate-fade-in {
  animation: fade-in 0.25s ease-out both;
}

/* Toast animations */
.toast-slide-enter-active,
.toast-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.16, 1, 0.3, 1);
}
.toast-slide-enter-from {
  transform: translateY(-20px) scale(0.9);
  opacity: 0;
}
.toast-slide-leave-to {
  transform: translateY(20px) scale(0.9);
  opacity: 0;
}

.text-slate-855 {
  color: #1e293b;
}

.border-slate-100\/80 {
  border-color: rgba(241, 245, 249, 0.8);
}
</style>
