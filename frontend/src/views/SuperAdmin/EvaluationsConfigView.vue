<script setup>
/**
 * EvaluationsConfigView.vue - Super Admin
 * Cấu hình đánh giá giảng viên: tiêu chí, thang điểm, giai đoạn mở đánh giá.
 */
import { ref, computed } from 'vue'
import {
  Star,
  Plus,
  Pencil,
  Trash2,
  Save,
  X,
  CheckCircle,
  AlertTriangle,
  Info,
  Filter,
  RotateCcw,
  Calendar,
  Settings,
  ToggleLeft,
  ToggleRight,
  GripVertical,
  BookOpen,
  Target,
  Clock
} from 'lucide-vue-next'

// --- Mock Data ---
const evaluationPeriods = ref([
  { id: 1, semester: 'Spring 2026', startDate: '2026-06-15', endDate: '2026-06-30', isActive: true, responses: 1250 },
  { id: 2, semester: 'Fall 2025', startDate: '2025-12-15', endDate: '2025-12-30', isActive: false, responses: 2840 },
  { id: 3, semester: 'Summer 2025', startDate: '2025-08-20', endDate: '2025-09-05', isActive: false, responses: 1680 }
])

const evaluationCriteria = ref([
  { id: 1, name: 'Nội dung giảng dạy', description: 'Kiến thức chuyên môn, cập nhật nội dung, liên hệ thực tế', weight: 25, category: 'Chuyên môn', isActive: true },
  { id: 2, name: 'Phương pháp truyền đạt', description: 'Kỹ năng giảng bài, tương tác, sử dụng công nghệ', weight: 20, category: 'Chuyên môn', isActive: true },
  { id: 3, name: 'Quản lý lớp học', description: 'Đúng giờ, tổ chức lớp, xử lý tình huống', weight: 15, category: 'Tổ chức', isActive: true },
  { id: 4, name: 'Tài liệu và bài tập', description: 'Chất lượng slide, bài tập thực hành, tài liệu tham khảo', weight: 15, category: 'Tài liệu', isActive: true },
  { id: 5, name: 'Hỗ trợ sinh viên', description: 'Giải đáp thắc mắc, phản hồi bài tập, nhiệt tình', weight: 15, category: 'Hỗ trợ', isActive: true },
  { id: 6, name: 'Đánh giá công bằng', description: 'Chấm điểm minh bạch, rubric rõ ràng, công bằng', weight: 10, category: 'Đánh giá', isActive: true }
])

const scaleOptions = ref([
  { value: 5, label: 'Thang 5 sao (1-5)' },
  { value: 10, label: 'Thang 10 điểm (1-10)' }
])
const selectedScale = ref(5)

// --- KPI ---
const totalCriteria = computed(() => evaluationCriteria.value.length)
const activeCriteria = computed(() => evaluationCriteria.value.filter(c => c.isActive).length)
const totalWeight = computed(() => evaluationCriteria.value.filter(c => c.isActive).reduce((s, c) => s + c.weight, 0))
const isWeightValid = computed(() => totalWeight.value === 100)

// --- Modal State ---
const isModalOpen = ref(false)
const editingMode = ref('create')
const currentCriterion = ref({ id: null, name: '', description: '', weight: 10, category: 'Chuyên môn', isActive: true })

const isPeriodModalOpen = ref(false)
const currentPeriod = ref({ id: null, semester: '', startDate: '', endDate: '', isActive: false })
const periodEditMode = ref('create')

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const triggerToast = (msg, type = 'success') => {
  toastMessage.value = msg; toastType.value = type; showToast.value = true
  setTimeout(() => { showToast.value = false }, 4000)
}

// --- Handlers ---
const openCreateCriterion = () => {
  editingMode.value = 'create'
  currentCriterion.value = { id: null, name: '', description: '', weight: 10, category: 'Chuyên môn', isActive: true }
  isModalOpen.value = true
}
const openEditCriterion = (c) => {
  editingMode.value = 'edit'
  currentCriterion.value = JSON.parse(JSON.stringify(c))
  isModalOpen.value = true
}
const handleSaveCriterion = () => {
  if (!currentCriterion.value.name.trim()) { triggerToast('Vui lòng nhập tên tiêu chí.', 'error'); return }
  if (editingMode.value === 'create') {
    const newId = evaluationCriteria.value.length ? Math.max(...evaluationCriteria.value.map(c => c.id)) + 1 : 1
    evaluationCriteria.value.push({ ...currentCriterion.value, id: newId })
    triggerToast('Đã thêm tiêu chí đánh giá mới.')
  } else {
    const idx = evaluationCriteria.value.findIndex(c => c.id === currentCriterion.value.id)
    if (idx !== -1) evaluationCriteria.value[idx] = { ...currentCriterion.value }
    triggerToast('Đã cập nhật tiêu chí đánh giá.')
  }
  isModalOpen.value = false
}
const toggleCriterionActive = (c) => {
  c.isActive = !c.isActive
  triggerToast(c.isActive ? `Tiêu chí "${c.name}" đã kích hoạt.` : `Tiêu chí "${c.name}" đã tắt.`, 'info')
}
const deleteCriterion = (c) => {
  evaluationCriteria.value = evaluationCriteria.value.filter(x => x.id !== c.id)
  triggerToast(`Đã xóa tiêu chí "${c.name}".`)
}

const openCreatePeriod = () => {
  periodEditMode.value = 'create'
  currentPeriod.value = { id: null, semester: '', startDate: '', endDate: '', isActive: false }
  isPeriodModalOpen.value = true
}
const handleSavePeriod = () => {
  if (!currentPeriod.value.semester.trim()) { triggerToast('Vui lòng nhập tên học kỳ.', 'error'); return }
  const newId = evaluationPeriods.value.length ? Math.max(...evaluationPeriods.value.map(p => p.id)) + 1 : 1
  evaluationPeriods.value.unshift({ ...currentPeriod.value, id: newId, responses: 0 })
  triggerToast('Đã tạo giai đoạn đánh giá mới.')
  isPeriodModalOpen.value = false
}
const togglePeriodActive = (period) => {
  period.isActive = !period.isActive
  triggerToast(period.isActive ? `Đã mở đánh giá kỳ ${period.semester}.` : `Đã đóng đánh giá kỳ ${period.semester}.`, 'info')
}

const getCategoryColor = (cat) => {
  const colors = { 'Chuyên môn': 'bg-blue-500/10 text-blue-600 dark:text-blue-400', 'Tổ chức': 'bg-violet-500/10 text-violet-600 dark:text-violet-400', 'Tài liệu': 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400', 'Hỗ trợ': 'bg-amber-500/10 text-amber-600 dark:text-amber-400', 'Đánh giá': 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-400' }
  return colors[cat] || colors['Chuyên môn']
}
</script>

<template>
  <div class="min-h-screen lg-app-bg text-heading font-sans relative pb-12">
    <div class="lg-shell-orbs"><div class="lg-shell-orb lg-shell-orb-primary"></div><div class="lg-shell-orb lg-shell-orb-secondary"></div></div>

    <!-- Toast -->
    <div v-if="showToast" class="fixed bottom-5 right-5 z-[110] p-4 rounded-xl shadow-xl border flex items-center gap-3 animate-in fade-in slide-in-from-bottom duration-300" :class="{ 'bg-emerald-500 text-white border-emerald-400': toastType === 'success', 'bg-rose-500 text-white border-rose-400': toastType === 'error', 'bg-sky-500 text-white border-sky-400': toastType === 'info' }">
      <CheckCircle v-if="toastType === 'success'" class="w-5 h-5 flex-shrink-0" /><AlertTriangle v-else-if="toastType === 'error'" class="w-5 h-5 flex-shrink-0" /><Info v-else class="w-5 h-5 flex-shrink-0" />
      <span class="text-sm font-bold">{{ toastMessage }}</span>
    </div>

    <div class="lg-shell-content mx-auto relative z-10">
      <!-- Header -->
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-6">
        <div>
          <h1 class="text-2xl md:text-3xl font-extrabold tracking-tight text-heading flex items-center gap-3">
            <Star class="w-8 h-8 text-primary" />
            Cấu Hình Đánh Giá Giảng Viên
          </h1>
          <p class="text-sm text-muted mt-1">Thiết lập tiêu chí, trọng số, thang điểm và giai đoạn mở đánh giá giảng viên.</p>
        </div>
      </div>

      <!-- KPI -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500"><Target class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng tiêu chí</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ totalCriteria }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500"><CheckCircle class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Đang kích hoạt</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ activeCriteria }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl flex items-center justify-center" :class="isWeightValid ? 'bg-emerald-500/10 text-emerald-500' : 'bg-rose-500/10 text-rose-500'"><Star class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng trọng số</div><div class="text-2xl font-bold mt-0.5" :class="isWeightValid ? 'text-emerald-600 dark:text-emerald-400' : 'text-rose-500'">{{ totalWeight }}%</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500"><Settings class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Thang điểm</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ selectedScale }} sao</div></div>
        </div>
      </div>

      <!-- Evaluation Periods -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2"><Calendar class="w-4.5 h-4.5 text-primary" /><h3 class="font-bold text-heading text-sm">Giai đoạn đánh giá</h3></div>
          <button @click="openCreatePeriod" class="lg-btn-primary text-xs px-3 py-1.5 font-bold flex items-center gap-1"><Plus class="w-3.5 h-3.5" /> Tạo giai đoạn</button>
        </div>
        <div class="space-y-2.5">
          <div v-for="period in evaluationPeriods" :key="period.id" class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3 rounded-lg bg-surface-card border border-default/30">
            <div class="flex items-center gap-3">
              <button @click="togglePeriodActive(period)" class="flex-shrink-0" :title="period.isActive ? 'Đóng đánh giá' : 'Mở đánh giá'">
                <component :is="period.isActive ? ToggleRight : ToggleLeft" class="w-8 h-8" :class="period.isActive ? 'text-emerald-500' : 'text-muted'" />
              </button>
              <div>
                <div class="font-bold text-heading text-sm flex items-center gap-2">
                  {{ period.semester }}
                  <span class="lg-badge text-[9px]" :class="period.isActive ? 'lg-badge-success' : 'lg-badge-warning'">{{ period.isActive ? 'Đang mở' : 'Đã đóng' }}</span>
                </div>
                <div class="text-[10px] text-muted">{{ period.startDate }} → {{ period.endDate }} · {{ period.responses.toLocaleString() }} phản hồi</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Criteria List -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2"><Target class="w-4.5 h-4.5 text-primary" /><h3 class="font-bold text-heading text-sm">Tiêu chí đánh giá</h3></div>
          <div class="flex items-center gap-2">
            <select v-model.number="selectedScale" class="px-3 lg-control text-xs">
              <option v-for="s in scaleOptions" :key="s.value" :value="s.value">{{ s.label }}</option>
            </select>
            <button @click="openCreateCriterion" class="lg-btn-primary text-xs px-3 py-1.5 font-bold flex items-center gap-1"><Plus class="w-3.5 h-3.5" /> Thêm tiêu chí</button>
          </div>
        </div>

        <!-- Weight validation bar -->
        <div class="mb-4">
          <div class="flex justify-between text-xs mb-1.5">
            <span class="font-bold text-label">Tổng trọng số tiêu chí đang kích hoạt:</span>
            <span class="font-extrabold text-sm" :class="isWeightValid ? 'text-emerald-500' : 'text-rose-500'">{{ totalWeight }}% / 100%</span>
          </div>
          <div class="lg-progress-track w-full h-3">
            <div class="lg-progress-fill transition-all duration-300" :class="isWeightValid ? 'bg-emerald-500' : totalWeight > 100 ? 'bg-rose-600' : 'bg-amber-500'" :style="{ width: Math.min(totalWeight, 100) + '%' }"></div>
          </div>
          <p v-if="!isWeightValid" class="text-[11px] font-semibold text-rose-500 mt-1.5 flex items-center gap-1"><AlertTriangle class="w-3.5 h-3.5" /> {{ totalWeight > 100 ? `Thừa ${totalWeight - 100}%` : `Thiếu ${100 - totalWeight}%` }}. Tổng trọng số phải bằng 100%.</p>
        </div>

        <div class="space-y-2.5">
          <div v-for="criterion in evaluationCriteria" :key="criterion.id" class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 p-3 rounded-lg border transition-all" :class="criterion.isActive ? 'bg-surface-card border-default/30' : 'bg-surface-card/50 border-default/10 opacity-60'">
            <div class="flex items-center gap-3 flex-1 min-w-0">
              <button @click="toggleCriterionActive(criterion)" class="flex-shrink-0">
                <component :is="criterion.isActive ? ToggleRight : ToggleLeft" class="w-7 h-7" :class="criterion.isActive ? 'text-emerald-500' : 'text-muted'" />
              </button>
              <div class="min-w-0">
                <div class="font-bold text-heading text-sm flex items-center gap-2">
                  {{ criterion.name }}
                  <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" :class="getCategoryColor(criterion.category)">{{ criterion.category }}</span>
                </div>
                <div class="text-[10px] text-muted truncate">{{ criterion.description }}</div>
              </div>
            </div>
            <div class="flex items-center gap-3 flex-shrink-0">
              <span class="text-sm font-extrabold text-primary">{{ criterion.weight }}%</span>
              <button @click="openEditCriterion(criterion)" class="lg-icon-button p-1.5 text-muted hover:text-heading"><Pencil class="w-4 h-4" /></button>
              <button @click="deleteCriterion(criterion)" class="lg-icon-button p-1.5 text-muted hover:text-rose-500"><Trash2 class="w-4 h-4" /></button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Criterion Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative">
        <button @click="isModalOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"><X class="w-5 h-5" /></button>
        <h2 class="text-lg font-extrabold text-heading mb-4 flex items-center gap-2"><Target class="w-5 h-5 text-primary" /> {{ editingMode === 'create' ? 'Thêm Tiêu Chí' : 'Sửa Tiêu Chí' }}</h2>
        <div class="space-y-4">
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Tên tiêu chí</label><input v-model="currentCriterion.name" type="text" class="w-full px-3 lg-control text-sm" placeholder="VD: Nội dung giảng dạy" /></div>
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Mô tả</label><textarea v-model="currentCriterion.description" rows="3" class="w-full px-3 py-2 lg-control text-sm" placeholder="Mô tả chi tiết tiêu chí..."></textarea></div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Nhóm</label>
              <select v-model="currentCriterion.category" class="w-full px-3 lg-control text-sm">
                <option>Chuyên môn</option><option>Tổ chức</option><option>Tài liệu</option><option>Hỗ trợ</option><option>Đánh giá</option>
              </select>
            </div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Trọng số (%)</label><input v-model.number="currentCriterion.weight" type="number" min="0" max="100" class="w-full px-3 lg-control text-sm font-bold" /></div>
          </div>
        </div>
        <div class="flex justify-end gap-3 pt-5 border-t border-default mt-5">
          <button @click="isModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy</button>
          <button @click="handleSaveCriterion" class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"><Save class="w-4 h-4" /> Lưu</button>
        </div>
      </div>
    </div>

    <!-- Period Modal -->
    <div v-if="isPeriodModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative">
        <button @click="isPeriodModalOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"><X class="w-5 h-5" /></button>
        <h2 class="text-lg font-extrabold text-heading mb-4 flex items-center gap-2"><Calendar class="w-5 h-5 text-primary" /> Tạo Giai Đoạn Đánh Giá</h2>
        <div class="space-y-4">
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Học kỳ</label><input v-model="currentPeriod.semester" type="text" class="w-full px-3 lg-control text-sm" placeholder="VD: Summer 2026" /></div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Ngày bắt đầu</label><input v-model="currentPeriod.startDate" type="date" class="w-full px-3 lg-control text-sm" /></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Ngày kết thúc</label><input v-model="currentPeriod.endDate" type="date" class="w-full px-3 lg-control text-sm" /></div>
          </div>
          <label class="flex items-center gap-3 cursor-pointer"><input type="checkbox" v-model="currentPeriod.isActive" class="rounded text-primary focus:ring-primary border-default" /><span class="text-sm font-bold text-heading">Kích hoạt ngay sau khi tạo</span></label>
        </div>
        <div class="flex justify-end gap-3 pt-5 border-t border-default mt-5">
          <button @click="isPeriodModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy</button>
          <button @click="handleSavePeriod" class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"><Save class="w-4 h-4" /> Tạo</button>
        </div>
      </div>
    </div>
  </div>
</template>