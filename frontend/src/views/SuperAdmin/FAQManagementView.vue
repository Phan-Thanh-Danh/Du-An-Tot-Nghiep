<script setup>
/**
 * FAQManagementView.vue - Super Admin
 * Giao diện quản lý FAQ (Câu hỏi thường gặp).
 * Tạo, sửa, xóa, sắp xếp và phân loại FAQ theo danh mục.
 */
import { ref, computed } from 'vue'
import {
  HelpCircle,
  Plus,
  Pencil,
  Trash2,
  Save,
  X,
  Search,
  Filter,
  RotateCcw,
  ChevronDown,
  ChevronUp,
  Eye,
  EyeOff,
  CheckCircle,
  AlertTriangle,
  Info,
  GripVertical,
  Tag,
  FileQuestion,
  BookOpen,
  Globe,
  Lock
} from 'lucide-vue-next'

// --- Mock Data ---
const faqCategories = ref([
  { id: 1, name: 'Tài khoản & Đăng nhập', color: 'blue' },
  { id: 2, name: 'Học phí & Thanh toán', color: 'emerald' },
  { id: 3, name: 'Đăng ký môn học', color: 'violet' },
  { id: 4, name: 'Thi & Kiểm tra', color: 'amber' },
  { id: 5, name: 'Kỹ thuật & Hỗ trợ', color: 'cyan' }
])

const faqs = ref([
  {
    id: 1,
    categoryId: 1,
    question: 'Làm thế nào để đặt lại mật khẩu LMS?',
    answer: 'Bạn có thể đặt lại mật khẩu bằng cách nhấn vào "Quên mật khẩu" tại trang đăng nhập. Hệ thống sẽ gửi email hướng dẫn đến địa chỉ email đã đăng ký. Nếu không nhận được email, vui lòng liên hệ phòng hỗ trợ kỹ thuật qua ticket.',
    isPublished: true,
    order: 1,
    viewCount: 1520,
    updatedAt: '2026-06-10 09:00',
    isExpanded: false
  },
  {
    id: 2,
    categoryId: 1,
    question: 'Tôi không thể đăng nhập vào hệ thống LMS, phải làm sao?',
    answer: 'Trước tiên hãy kiểm tra xem Caps Lock có đang bật không. Thử xóa cache trình duyệt và đăng nhập lại. Nếu vẫn lỗi, hãy thử dùng trình duyệt Chrome/Firefox phiên bản mới nhất. Trường hợp vẫn không được, hãy gửi ticket kỹ thuật với ảnh chụp lỗi.',
    isPublished: true,
    order: 2,
    viewCount: 980,
    updatedAt: '2026-06-09 14:30',
    isExpanded: false
  },
  {
    id: 3,
    categoryId: 2,
    question: 'Làm sao để kiểm tra công nợ học phí?',
    answer: 'Đăng nhập vào LMS → Menu "Học phí & Thanh toán". Trang này hiển thị chi tiết công nợ từng kỳ, lịch sử thanh toán và phương thức nộp. Nếu phát hiện sai lệch, hãy gửi ticket kèm biên lai.',
    isPublished: true,
    order: 1,
    viewCount: 2150,
    updatedAt: '2026-06-08 11:00',
    isExpanded: false
  },
  {
    id: 4,
    categoryId: 2,
    question: 'Chính sách hoàn học phí khi rút môn như thế nào?',
    answer: 'Sinh viên rút môn trong 2 tuần đầu kỳ sẽ được hoàn 100% học phí. Rút từ tuần 3-4 hoàn 50%. Sau tuần 4 sẽ không được hoàn học phí. Chi tiết xem Quy chế tài chính hoặc liên hệ phòng Tài chính.',
    isPublished: true,
    order: 2,
    viewCount: 876,
    updatedAt: '2026-06-07 16:45',
    isExpanded: false
  },
  {
    id: 5,
    categoryId: 3,
    question: 'Tôi bị trùng lịch khi đăng ký môn, xử lý thế nào?',
    answer: 'Hệ thống tự động kiểm tra trùng lịch. Nếu vẫn bị trùng, hãy chọn nhóm lớp khác có lịch phù hợp. Trường hợp tất cả nhóm đều trùng, liên hệ phòng Giáo vụ để được hỗ trợ sắp xếp.',
    isPublished: true,
    order: 1,
    viewCount: 654,
    updatedAt: '2026-06-06 10:20',
    isExpanded: false
  },
  {
    id: 6,
    categoryId: 4,
    question: 'Bài thi online bị mất kết nối giữa chừng thì sao?',
    answer: 'Nếu mất kết nối dưới 5 phút, bạn có thể đăng nhập lại và tiếp tục làm bài (thời gian vẫn chạy). Nếu quá 5 phút hoặc hệ thống tự đóng bài, hãy gửi ticket khẩn cấp kèm ảnh chụp lỗi để được xem xét cho thi lại.',
    isPublished: true,
    order: 1,
    viewCount: 1890,
    updatedAt: '2026-06-05 08:30',
    isExpanded: false
  },
  {
    id: 7,
    categoryId: 5,
    question: 'Video bài giảng không phát được, phải làm gì?',
    answer: 'Kiểm tra kết nối internet, thử xóa cache trình duyệt. Đảm bảo trình duyệt hỗ trợ HTML5 video. Nếu dùng mạng công ty/trường có tường lửa, thử chuyển sang 4G/WiFi cá nhân. Vẫn lỗi thì gửi ticket kỹ thuật.',
    isPublished: false,
    order: 1,
    viewCount: 0,
    updatedAt: '2026-06-04 14:00',
    isExpanded: false
  }
])

// --- Filter State ---
const searchQuery = ref('')
const filterCategory = ref('all')
const filterPublished = ref('all')

const filteredFaqs = computed(() => {
  return faqs.value.filter(faq => {
    const matchSearch = searchQuery.value === '' ||
      faq.question.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      faq.answer.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchCategory = filterCategory.value === 'all' || faq.categoryId === Number(filterCategory.value)
    const matchPublished = filterPublished.value === 'all' ||
      (filterPublished.value === 'published' && faq.isPublished) ||
      (filterPublished.value === 'draft' && !faq.isPublished)
    return matchSearch && matchCategory && matchPublished
  })
})

const resetFilters = () => {
  searchQuery.value = ''
  filterCategory.value = 'all'
  filterPublished.value = 'all'
}

// --- KPI ---
const totalFaqs = computed(() => faqs.value.length)
const publishedFaqs = computed(() => faqs.value.filter(f => f.isPublished).length)
const draftFaqs = computed(() => faqs.value.filter(f => !f.isPublished).length)
const totalViews = computed(() => faqs.value.reduce((sum, f) => sum + f.viewCount, 0))

// --- Modal State ---
const isModalOpen = ref(false)
const editingMode = ref('create')
const currentFaq = ref({
  id: null,
  categoryId: 1,
  question: '',
  answer: '',
  isPublished: false
})

const isDeleteModalOpen = ref(false)
const deletingFaq = ref(null)

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')

const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg
  toastType.value = type
  showToast.value = true
  setTimeout(() => { showToast.value = false }, 4000)
}

// --- Handlers ---
const openCreateModal = () => {
  editingMode.value = 'create'
  currentFaq.value = { id: null, categoryId: 1, question: '', answer: '', isPublished: false }
  isModalOpen.value = true
}

const openEditModal = (faq) => {
  editingMode.value = 'edit'
  currentFaq.value = JSON.parse(JSON.stringify(faq))
  isModalOpen.value = true
}

const handleSaveFaq = () => {
  if (!currentFaq.value.question.trim() || !currentFaq.value.answer.trim()) {
    triggerToast('Vui lòng điền đầy đủ câu hỏi và câu trả lời.', 'error')
    return
  }

  const timeString = new Date().toLocaleString('vi-VN')

  if (editingMode.value === 'create') {
    const newId = faqs.value.length ? Math.max(...faqs.value.map(f => f.id)) + 1 : 1
    faqs.value.push({
      ...currentFaq.value,
      id: newId,
      order: faqs.value.filter(f => f.categoryId === currentFaq.value.categoryId).length + 1,
      viewCount: 0,
      updatedAt: timeString,
      isExpanded: false
    })
    triggerToast('Đã tạo FAQ mới thành công.')
  } else {
    const index = faqs.value.findIndex(f => f.id === currentFaq.value.id)
    if (index !== -1) {
      faqs.value[index] = { ...faqs.value[index], ...currentFaq.value, updatedAt: timeString }
    }
    triggerToast('Đã cập nhật FAQ thành công.')
  }

  isModalOpen.value = false
}

const openDeleteModal = (faq) => {
  deletingFaq.value = faq
  isDeleteModalOpen.value = true
}

const handleDeleteFaq = () => {
  if (!deletingFaq.value) return
  faqs.value = faqs.value.filter(f => f.id !== deletingFaq.value.id)
  triggerToast(`Đã xóa FAQ "${deletingFaq.value.question.substring(0, 40)}..."`)
  isDeleteModalOpen.value = false
}

const togglePublish = (faq) => {
  faq.isPublished = !faq.isPublished
  faq.updatedAt = new Date().toLocaleString('vi-VN')
  triggerToast(faq.isPublished ? `FAQ đã được xuất bản công khai.` : `FAQ đã chuyển sang bản nháp.`, 'info')
}

const toggleExpand = (faq) => {
  faq.isExpanded = !faq.isExpanded
}

// --- Helpers ---
const getCategoryName = (id) => faqCategories.value.find(c => c.id === id)?.name || ''
const getCategoryColor = (id) => {
  const colors = {
    blue: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-200 dark:border-blue-500/20',
    emerald: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-200 dark:border-emerald-500/20',
    violet: 'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-200 dark:border-violet-500/20',
    amber: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/20',
    cyan: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 border-cyan-200 dark:border-cyan-500/20'
  }
  const cat = faqCategories.value.find(c => c.id === id)
  return colors[cat?.color] || colors.blue
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs">
      <div class="lg-shell-orb lg-shell-orb-primary"></div>
      <div class="lg-shell-orb lg-shell-orb-secondary"></div>
    </div>

    <!-- Toast -->
    <div
      v-if="showToast"
      class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300"
      :class="{
        'bg-emerald-500 text-white border-emerald-400': toastType === 'success',
        'bg-rose-500 text-white border-rose-400': toastType === 'error',
        'bg-sky-500 text-white border-sky-400': toastType === 'info'
      }"
    >
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" />
      <AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" />
      <Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Page Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <FileQuestion class="w-8 h-8 text-primary" />
            Quản Lý FAQ
          </h1>
          <p class="text-sm text-muted mt-1">
            Tạo và quản lý các câu hỏi thường gặp giúp sinh viên tự giải đáp thắc mắc phổ biến.
          </p>
        </div>
        <button @click="openCreateModal" class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2">
          <Plus class="w-4.5 h-4.5" />
          Thêm FAQ Mới
        </button>
      </div>

      <!-- KPI Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500">
            <HelpCircle class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng FAQ</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalFaqs }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500">
            <Globe class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã xuất bản</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ publishedFaqs }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500">
            <Lock class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Bản nháp</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ draftFaqs }}</div>
          </div>
        </div>

        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500">
            <Eye class="w-6 h-6" />
          </div>
          <div>
            <div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng lượt xem</div>
            <div class="text-2xl font-bold mt-0.5 text-heading">{{ totalViews.toLocaleString() }}</div>
          </div>
        </div>
      </div>

      <!-- Filter Panel -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2">
            <Filter class="w-4.5 h-4.5 text-primary" />
            <h3 class="font-bold text-heading text-sm">Bộ lọc FAQ</h3>
          </div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline">
            <RotateCcw class="w-3.5 h-3.5" />
            Xóa bộ lọc
          </button>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" />
            <input
              v-model="searchQuery"
              type="text"
              placeholder="Tìm theo câu hỏi hoặc câu trả lời..."
              class="w-full pl-9 pr-3 lg-control text-sm"
            />
          </div>

          <div>
            <select v-model="filterCategory" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả danh mục</option>
              <option v-for="cat in faqCategories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </div>

          <div>
            <select v-model="filterPublished" class="w-full px-3 lg-control text-sm">
              <option value="all">Tất cả trạng thái</option>
              <option value="published">Đã xuất bản</option>
              <option value="draft">Bản nháp</option>
            </select>
          </div>
        </div>
      </div>

      <!-- FAQ Accordion List -->
      <div class="space-y-3 mb-8">
        <div v-if="filteredFaqs.length === 0" class="lg-glass-soft lg-card lg-density-spacious text-center">
          <FileQuestion class="w-10 h-10 text-muted mx-auto mb-3" />
          <p class="text-sm text-muted font-semibold">Không tìm thấy FAQ nào phù hợp với bộ lọc.</p>
        </div>

        <div
          v-for="faq in filteredFaqs"
          :key="faq.id"
          class="lg-glass-soft lg-card overflow-visible transition-all duration-200"
          :class="faq.isExpanded ? 'ring-1 ring-primary/20' : ''"
        >
          <!-- FAQ Header (Collapsed) -->
          <div
            class="flex items-center gap-3 px-4 py-3.5 cursor-pointer select-none hover:bg-surface-card-hover transition-colors rounded-xl"
            @click="toggleExpand(faq)"
          >
            <div class="flex-shrink-0">
              <component
                :is="faq.isExpanded ? ChevronUp : ChevronDown"
                class="w-5 h-5 text-primary transition-transform duration-200"
              />
            </div>

            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1 flex-wrap">
                <span
                  class="text-[10px] font-bold px-2 py-0.5 rounded-full border"
                  :class="getCategoryColor(faq.categoryId)"
                >
                  {{ getCategoryName(faq.categoryId) }}
                </span>
                <span
                  class="lg-badge text-[9px]"
                  :class="faq.isPublished ? 'lg-badge-success' : 'lg-badge-warning'"
                >
                  {{ faq.isPublished ? 'Công khai' : 'Nháp' }}
                </span>
              </div>
              <h4 class="font-bold text-heading text-sm truncate">{{ faq.question }}</h4>
            </div>

            <div class="flex items-center gap-2 flex-shrink-0">
              <span class="text-[10px] text-muted font-medium hidden sm:inline">
                {{ faq.viewCount.toLocaleString() }} lượt xem
              </span>

              <!-- Actions -->
              <div class="flex items-center gap-1" @click.stop>
                <button
                  @click="togglePublish(faq)"
                  class="lg-icon-button p-1.5 text-muted hover:text-heading"
                  :title="faq.isPublished ? 'Ẩn khỏi công khai' : 'Xuất bản công khai'"
                >
                  <component :is="faq.isPublished ? EyeOff : Eye" class="w-4 h-4" />
                </button>
                <button
                  @click="openEditModal(faq)"
                  class="lg-icon-button p-1.5 text-muted hover:text-heading"
                  title="Chỉnh sửa FAQ"
                >
                  <Pencil class="w-4 h-4" />
                </button>
                <button
                  @click="openDeleteModal(faq)"
                  class="lg-icon-button p-1.5 text-muted hover:text-rose-500"
                  title="Xóa FAQ"
                >
                  <Trash2 class="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>

          <!-- FAQ Body (Expanded) -->
          <div v-if="faq.isExpanded" class="px-4 pb-4 pt-0">
            <div class="ml-8 pl-4 border-l-2 border-primary/20">
              <p class="text-xs text-body leading-relaxed whitespace-pre-line">{{ faq.answer }}</p>
              <div class="flex items-center gap-4 mt-3 text-[10px] text-muted">
                <span class="flex items-center gap-1">
                  <Eye class="w-3 h-3" /> {{ faq.viewCount.toLocaleString() }} lượt xem
                </span>
                <span>Cập nhật: {{ faq.updatedAt }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Create/Edit Modal -->
    <div
      v-if="isModalOpen"
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-2xl lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative max-h-[90vh] overflow-y-auto">
        <button
          @click="isModalOpen = false"
          class="absolute top-4 right-4 text-muted hover:text-heading transition-colors lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"
        >
          <X class="w-5 h-5" />
        </button>

        <div class="mb-5 pb-4 border-b border-default">
          <h2 class="text-xl font-extrabold text-heading flex items-center gap-2.5">
            <FileQuestion class="w-6 h-6 text-primary" />
            {{ editingMode === 'create' ? 'Thêm FAQ Mới' : 'Chỉnh Sửa FAQ' }}
          </h2>
        </div>

        <div class="space-y-4">
          <!-- Category -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Danh mục</label>
            <select v-model="currentFaq.categoryId" class="w-full px-3 lg-control text-sm">
              <option v-for="cat in faqCategories" :key="cat.id" :value="cat.id">{{ cat.name }}</option>
            </select>
          </div>

          <!-- Question -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Câu hỏi</label>
            <input
              v-model="currentFaq.question"
              type="text"
              placeholder="Nhập câu hỏi thường gặp..."
              class="w-full px-3 lg-control text-sm"
            />
          </div>

          <!-- Answer -->
          <div>
            <label class="block text-xs font-bold text-label mb-1.5 uppercase">Câu trả lời</label>
            <textarea
              v-model="currentFaq.answer"
              rows="6"
              placeholder="Nhập câu trả lời chi tiết..."
              class="w-full px-3 py-2 lg-control text-sm"
            ></textarea>
          </div>

          <!-- Publish toggle -->
          <label class="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              v-model="currentFaq.isPublished"
              class="rounded text-primary focus:ring-primary border-default"
            />
            <div>
              <span class="text-sm font-bold text-heading">Xuất bản công khai ngay</span>
              <span class="block text-[10px] text-muted mt-0.5">FAQ sẽ hiển thị cho tất cả sinh viên và giảng viên</span>
            </div>
          </label>
        </div>

        <!-- Footer -->
        <div class="flex items-center justify-end gap-3 pt-5 border-t border-default mt-5">
          <button @click="isModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy bỏ</button>
          <button
            @click="handleSaveFaq"
            class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"
          >
            <Save class="w-4 h-4" />
            {{ editingMode === 'create' ? 'Tạo FAQ' : 'Lưu thay đổi' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Delete Confirm Modal -->
    <div
      v-if="isDeleteModalOpen"
      class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200"
    >
      <div class="w-full max-w-md lg-glass-strong lg-density-spacious rounded-xl shadow-2xl relative">
        <div class="flex items-start gap-3.5 mb-4">
          <div class="w-10 h-10 rounded-full bg-rose-500/10 flex items-center justify-center text-rose-500 flex-shrink-0">
            <Trash2 class="w-5 h-5" />
          </div>
          <div>
            <h3 class="text-lg font-extrabold text-rose-600 dark:text-rose-400">Xác nhận xóa FAQ</h3>
            <p class="text-xs text-muted mt-1">Hành động này không thể hoàn tác.</p>
          </div>
        </div>

        <div class="lg-alert lg-alert-warning mb-4">
          <div class="flex gap-2">
            <AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5 text-current opacity-90" />
            <div class="text-xs font-bold leading-relaxed text-current">
              Bạn có chắc chắn muốn xóa FAQ "<strong>{{ deletingFaq?.question }}</strong>"? FAQ đã xuất bản sẽ bị gỡ khỏi trang công khai ngay lập tức.
            </div>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2.5">
          <button @click="isDeleteModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy bỏ</button>
          <button
            @click="handleDeleteFaq"
            class="lg-btn-danger px-5 py-2 text-sm font-bold flex items-center gap-1.5"
          >
            <Trash2 class="w-4 h-4" />
            Xóa FAQ
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
