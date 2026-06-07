<script setup>
import { ref, computed } from 'vue'
import dayjs from 'dayjs'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'

const today = dayjs().format('YYYY-MM-DD')
import {
  Plus,
  Search,
  Calendar,
  Clock,
  Settings,
  MoreVertical,
  CheckCircle2,
  AlertCircle,
  ArrowRight,
  ShieldCheck,
  Edit3,
  X,
  Save,
  SlidersHorizontal,
  FileText,
  Info,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'

// ── Mock Data ────────────────────────────────────────────────
const periods = ref([
  { id: 1, name: 'Đợt 1 - Học kỳ Spring 2026', semester: 'Spring 2026', openDate: '2026-01-15', closeDate: '2026-01-25', withdrawDeadline: '2026-02-10', maxCredits: 24, status: 'open', studentCount: 156, classCount: 12 },
  { id: 2, name: 'Đợt bổ sung - Học kỳ Spring 2026', semester: 'Spring 2026', openDate: '2026-02-01', closeDate: '2026-02-05', withdrawDeadline: '2026-02-15', maxCredits: 12, status: 'draft', studentCount: 0, classCount: 0 },
  { id: 3, name: 'Học kỳ Fall 2025', semester: 'Fall 2025', openDate: '2025-08-10', closeDate: '2025-08-25', withdrawDeadline: '2025-09-10', maxCredits: 24, status: 'closed', studentCount: 342, classCount: 28 },
])

// ── Search ───────────────────────────────────────────────────
const searchQuery = ref('')

const filteredPeriods = computed(() => {
  const q = searchQuery.value.toLowerCase().trim()
  if (!q) return periods.value
  return periods.value.filter(p =>
    p.name.toLowerCase().includes(q) ||
    p.semester.toLowerCase().includes(q)
  )
})

// ── Status helpers ───────────────────────────────────────────
const STATUS_MAP = {
  open:   { label: 'Đang mở',  dot: 'bg-[var(--lg-success)]', badge: 'lg-badge lg-badge-success' },
  draft:  { label: 'Bản nháp', dot: 'bg-[var(--lg-warning)]', badge: 'lg-badge lg-badge-warning' },
  closed: { label: 'Đã đóng',  dot: 'bg-[var(--text-placeholder)]', badge: 'lg-badge surface-solid text-muted border-default' },
}
const getStatusInfo = s => STATUS_MAP[s] || STATUS_MAP.closed

// ── Create Period Modal ──────────────────────────────────────
const showCreateModal = ref(false)
const createForm = ref({ name: '', semester: '', openDate: '', closeDate: '', withdrawDeadline: '', maxCredits: 24 })
const isCreating = ref(false)
useBodyScrollLock(showCreateModal)

function openCreate() {
  createForm.value = { name: '', semester: 'Spring 2026', openDate: '', closeDate: '', withdrawDeadline: '', maxCredits: 24 }
  showCreateModal.value = true
}

function closeCreate() {
  showCreateModal.value = false
}

async function confirmCreate() {
  isCreating.value = true
  await new Promise(r => setTimeout(r, 800))
  periods.value.unshift({
    id: Date.now(),
    ...createForm.value,
    status: 'draft',
    studentCount: 0,
    classCount: 0,
  })
  isCreating.value = false
  closeCreate()
}

// ── Edit Period Modal ────────────────────────────────────────
const showEditModal = ref(false)
const editTarget = ref(null)
const editForm = ref({})
const isEditing = ref(false)
useBodyScrollLock(showEditModal)

function openEdit(period) {
  editTarget.value = period
  editForm.value = { ...period }
  showEditModal.value = true
}

function closeEdit() {
  showEditModal.value = false
  editTarget.value = null
}

async function confirmEdit() {
  isEditing.value = true
  await new Promise(r => setTimeout(r, 800))
  Object.assign(editTarget.value, editForm.value)
  isEditing.value = false
  closeEdit()
}

// ── Open Registration (Draft → Open) ─────────────────────────
const showOpenConfirm = ref(false)
const openTarget = ref(null)
const isOpening = ref(false)
useBodyScrollLock(showOpenConfirm)

function openOpenConfirm(period) {
  openTarget.value = period
  showOpenConfirm.value = true
}

function closeOpenConfirm() {
  showOpenConfirm.value = false
  openTarget.value = null
}

async function confirmOpen() {
  isOpening.value = true
  await new Promise(r => setTimeout(r, 800))
  openTarget.value.status = 'open'
  isOpening.value = false
  closeOpenConfirm()
}

// ── More Actions Dropdown ────────────────────────────────────
const activeMenuId = ref(null)

function toggleMenu(id) {
  activeMenuId.value = activeMenuId.value === id ? null : id
}

function closeMenu() {
  activeMenuId.value = null
}

function handleClosePeriod(period) {
  period.status = 'closed'
  closeMenu()
}

function handleDeleteDraft(period) {
  const idx = periods.value.findIndex(p => p.id === period.id)
  if (idx !== -1) periods.value.splice(idx, 1)
  closeMenu()
}

// ── Stats ────────────────────────────────────────────────────
const stats = computed(() => {
  const all = periods.value
  return {
    total: all.length,
    open: all.filter(p => p.status === 'open').length,
    draft: all.filter(p => p.status === 'draft').length,
    closed: all.filter(p => p.status === 'closed').length,
  }
})
</script>

<template>
  <PageContainer 
    title="Đợt đăng ký môn học" 
    subtitle="Quản lý thời gian, số tín chỉ tối đa và quy trình đăng ký cho sinh viên."
  >
    <template #actions>
      <button class="lg-button-primary px-5 py-2.5 text-sm font-bold" @click="openCreate">
        <Plus :size="18" /> Tạo đợt mới
      </button>
    </template>

    <div class="space-y-4">

      <!-- ── Stats Row ── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div
          v-for="stat in [
            { label: 'Tổng đợt',       value: stats.total, color: 'text-[var(--color-info-text)]', bg: 'bg-[var(--color-info-bg)]' },
            { label: 'Đang mở',        value: stats.open,  color: 'text-[var(--color-success-text)]', bg: 'bg-[var(--color-success-bg)]' },
            { label: 'Bản nháp',       value: stats.draft, color: 'text-[var(--color-warning-text)]', bg: 'bg-[var(--color-warning-bg)]' },
            { label: 'Đã đóng',        value: stats.closed,color: 'text-label',           bg: 'surface-solid' },
          ]"
          :key="stat.label"
          :class="['rounded-2xl p-4 border border-default', stat.bg]"
        >
          <p class="text-[11px] font-black uppercase tracking-widest text-placeholder">{{ stat.label }}</p>
          <p :class="['text-2xl font-black mt-1', stat.color]">{{ stat.value }}</p>
        </div>
      </div>

      <!-- ── Search & Filter ── -->
      <div class="lg-glass-strong p-4 rounded-[24px]">
        <div class="flex-1 min-w-[280px] relative">
          <Search :size="18" class="absolute left-4 top-1/2 -translate-y-1/2 text-placeholder pointer-events-none" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo tên đợt hoặc học kỳ..."
            class="w-full lg-input pl-11 pr-10 py-2.5 text-sm font-medium transition-all"
          />
          <button
            v-if="searchQuery"
            class="absolute right-3 top-1/2 -translate-y-1/2 text-placeholder hover:text-label"
            @click="searchQuery = ''"
          >
            <X :size="14" />
          </button>
        </div>
      </div>

      <!-- ── Result count ── -->
      <div class="flex items-center justify-between px-1">
        <p class="text-sm text-label font-semibold">
          Hiển thị <span class="font-black text-heading">{{ filteredPeriods.length }}</span> / {{ periods.length }} đợt
        </p>
      </div>

      <!-- ── Periods Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Đợt đăng ký</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thời gian</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Max Credits</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-black text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-default)]">
            <tr v-for="period in filteredPeriods" :key="period.id" class="group hover:bg-[var(--surface-input)] transition-colors">
              <td class="px-4 py-4">
                <p class="text-sm font-black text-heading leading-tight">{{ period.name }}</p>
                <p class="text-[11px] font-bold text-[var(--lg-primary)] mt-1 uppercase tracking-tighter">{{ period.semester }}</p>
              </td>
              <td class="px-4 py-4">
                <div class="space-y-1">
                  <div class="flex items-center gap-2 text-xs font-bold text-label">
                    <Clock :size="14" class="text-[var(--lg-success)]" /> {{ period.openDate }} <ArrowRight :size="12" class="text-placeholder" /> {{ period.closeDate }}
                  </div>
                  <div class="flex items-center gap-2 text-[10px] font-medium text-placeholder">
                    <AlertCircle :size="12" /> Hủy môn: {{ period.withdrawDeadline }}
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <span class="text-sm font-black text-heading">{{ period.maxCredits }} TC</span>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-black uppercase tracking-widest', getStatusInfo(period.status).badge]">
                  {{ getStatusInfo(period.status).label }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-[var(--color-info-bg)] hover:text-[var(--lg-primary)] rounded-lg text-placeholder transition-all" title="Chỉnh sửa" @click="openEdit(period)">
                    <Edit3 :size="16" />
                  </button>
                  <button v-if="period.status === 'draft'" class="p-2 hover:bg-[var(--color-success-bg)] hover:text-[var(--lg-success)] rounded-lg text-placeholder transition-all" title="Mở đăng ký" @click="openOpenConfirm(period)">
                    <CheckCircle2 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-[var(--surface-solid)] rounded-lg text-placeholder transition-all relative" @click.stop="toggleMenu(period.id)">
                    <MoreVertical :size="16" />
                  </button>
                </div>

                <!-- Dropdown Menu -->
                <Teleport to="body">
                  <div
                    v-if="activeMenuId === period.id"
                    class="fixed inset-0 z-50"
                    @click="closeMenu"
                  >
                    <div
                      class="absolute right-[48px] mt-2 w-48 surface-modal rounded-2xl shadow-sm border border-default overflow-hidden py-1"
                      :style="{ top: $el?.getBoundingClientRect?.() ? 'auto' : '0px', left: 'auto' }"
                      @click.stop
                    >
                      <button
                        class="w-full px-4 py-2.5 text-sm font-bold text-label hover:bg-[var(--color-info-bg)] flex items-center gap-3 transition-all text-left"
                        @click="openEdit(period)"
                      >
                        <Edit3 :size="15" /> Chỉnh sửa
                      </button>
                      <button
                        v-if="period.status === 'open'"
                        class="w-full px-4 py-2.5 text-sm font-bold text-label hover:bg-[var(--color-warning-bg)] flex items-center gap-3 transition-all text-left"
                        @click="handleClosePeriod(period)"
                      >
                        <X :size="15" /> Đóng đợt
                      </button>
                      <button
                        v-if="period.status === 'draft'"
                        class="w-full px-4 py-2.5 text-sm font-bold text-[var(--lg-danger)] hover:bg-[var(--color-danger-bg)] flex items-center gap-3 transition-all text-left"
                        @click="handleDeleteDraft(period)"
                      >
                        <X :size="15" /> Xóa bản nháp
                      </button>
                    </div>
                  </div>
                </Teleport>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Empty state -->
        <div
          v-if="filteredPeriods.length === 0"
          class="flex flex-col items-center justify-center py-16 text-center"
        >
          <div class="h-14 w-14 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <FileText :size="24" class="text-placeholder" />
          </div>
          <p class="text-base font-black text-heading">Không tìm thấy đợt nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa tìm kiếm.</p>
          <button v-if="searchQuery" class="mt-3 text-sm font-bold text-[var(--lg-primary)] hover:underline" @click="searchQuery = ''">Xóa tìm kiếm</button>
        </div>
      </div>

      <!-- ── Important Rules ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="lg-glass-soft p-5 rounded-[24px] border border-default">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-[var(--color-info-bg)] flex items-center justify-center text-[var(--color-info-text)] shrink-0">
               <ShieldCheck :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-black text-heading">Quy tắc đăng ký</h4>
              <ul class="mt-2 space-y-2">
                <li class="text-xs text-label flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-[var(--lg-primary)] mt-1.5 shrink-0"></div>
                  Sinh viên chỉ có thể đăng ký khi đợt ở trạng thái <strong class="text-heading">Đang mở</strong>.
                </li>
                <li class="text-xs text-label flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-[var(--lg-primary)] mt-1.5 shrink-0"></div>
                  Hệ thống tự động kiểm tra tín chỉ tối đa đã cấu hình.
                </li>
              </ul>
            </div>
          </div>
        </div>

        <div class="lg-glass-soft p-5 rounded-[24px] border border-default">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-[var(--color-warning-bg)] flex items-center justify-center text-[var(--color-warning-text)] shrink-0">
               <Settings :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-black text-heading">Chốt danh sách</h4>
              <p class="text-xs text-label mt-2 leading-relaxed">
                Khi đợt đăng ký kết thúc, hệ thống sẽ tự động chốt danh sách và chuyển các lớp có sĩ số thấp sang trạng thái <strong class="text-heading">PENDING CANCEL</strong> để Giáo vụ xử lý.
              </p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </PageContainer>

  <!-- ════════════════════════════════════════════════════════════
       Create Period Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showCreateModal"
        class="fixed inset-0 z-[100] flex items-center justify-center p-4"
        @click.self="closeCreate"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="modal-header p-6 pb-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input border border-default flex items-center justify-center text-link">
                  <Plus :size="20" />
                </div>
                <div>
                  <h2 class="text-lg font-black text-heading">Tạo đợt đăng ký mới</h2>
                  <p class="text-xs text-muted mt-0.5">Thiết lập thông tin đợt đăng ký môn học</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-muted transition-all"
                @click="closeCreate"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">
                Tên đợt <span class="text-[var(--lg-danger)]">*</span>
              </label>
              <input
                v-model="createForm.name"
                type="text"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                placeholder="VD: Đợt 1 - Học kỳ Spring 2026"
              />
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Học kỳ</label>
              <select
                v-model="createForm.semester"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              >
                <option value="Spring 2026">Spring 2026</option>
                <option value="Fall 2026">Fall 2026</option>
                <option value="Spring 2027">Spring 2027</option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">
                  Ngày mở <span class="text-[var(--lg-danger)]">*</span>
                </label>
                <input
                  v-model="createForm.openDate"
                  type="date"
                  :min="today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
              <div>
                <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">
                  Ngày đóng <span class="text-[var(--lg-danger)]">*</span>
                </label>
                <input
                  v-model="createForm.closeDate"
                  type="date"
                  :min="createForm.openDate || today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Hạn hủy môn</label>
                <input
                  v-model="createForm.withdrawDeadline"
                  type="date"
                  :min="createForm.openDate || today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Max Credits</label>
              <input
                v-model="createForm.maxCredits"
                type="number"
                min="1"
                max="50"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeCreate"
            >Hủy</button>
            <button
              :class="[
                'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
                (!createForm.name || !createForm.openDate || !createForm.closeDate || isCreating) ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="!createForm.name || !createForm.openDate || !createForm.closeDate || isCreating"
              @click="confirmCreate"
            >
              <span v-if="isCreating" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Save v-else :size="16" />
              {{ isCreating ? 'Đang tạo...' : 'Tạo đợt' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Edit Period Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showEditModal && editTarget"
        class="fixed inset-0 z-[110] flex items-center justify-center p-4"
        @click.self="closeEdit"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-lg surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="modal-header p-6 pb-5">
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-3">
                <div class="h-10 w-10 rounded-2xl surface-input border border-default flex items-center justify-center text-link">
                  <Edit3 :size="20" />
                </div>
                <div>
                  <h2 class="text-lg font-black text-heading">Chỉnh sửa đợt đăng ký</h2>
                  <p class="text-xs text-muted mt-0.5">{{ editTarget.name }}</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-[var(--surface-input-focus)] flex items-center justify-center text-muted transition-all"
                @click="closeEdit"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Tên đợt <span class="text-[var(--lg-danger)]">*</span></label>
              <input
                v-model="editForm.name"
                type="text"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Học kỳ</label>
              <select
                v-model="editForm.semester"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              >
                <option value="Spring 2026">Spring 2026</option>
                <option value="Fall 2026">Fall 2026</option>
                <option value="Spring 2027">Spring 2027</option>
              </select>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Ngày mở <span class="text-[var(--lg-danger)]">*</span></label>
                <input
                  v-model="editForm.openDate"
                  type="date"
                  :min="today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
              <div>
                <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Ngày đóng <span class="text-[var(--lg-danger)]">*</span></label>
                <input
                  v-model="editForm.closeDate"
                  type="date"
                  :min="editForm.openDate || today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Hạn hủy môn</label>
              <input
                v-model="editForm.withdrawDeadline"
                type="date"
                :min="editForm.openDate || today"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div>
              <label class="text-[11px] font-black text-label uppercase tracking-widest mb-1.5 block">Max Credits</label>
              <input
                v-model="editForm.maxCredits"
                type="number"
                min="1"
                max="50"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>
          </div>

          <div class="px-6 pb-6 flex items-center justify-end gap-3">
            <button
              class="lg-button-secondary px-5 py-2.5 text-sm font-bold"
              @click="closeEdit"
            >Hủy</button>
            <button
              :class="[
                'lg-button-primary px-6 py-2.5 text-sm font-bold flex items-center gap-2',
                isEditing ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="isEditing"
              @click="confirmEdit"
            >
              <span v-if="isEditing" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <Save v-else :size="16" />
              {{ isEditing ? 'Đang lưu...' : 'Lưu thay đổi' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>

  <!-- ════════════════════════════════════════════════════════════
       Open Registration Confirm Modal
  ════════════════════════════════════════════════════════════ -->
  <Teleport to="body">
    <Transition name="modal">
      <div
        v-if="showOpenConfirm && openTarget"
        class="fixed inset-0 z-[120] flex items-center justify-center p-4"
        @click.self="closeOpenConfirm"
      >
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>

        <div class="relative w-full max-w-md surface-modal rounded-2xl shadow-sm overflow-hidden border border-default">
          <div class="p-6 text-center">
            <div class="h-16 w-16 rounded-3xl bg-[var(--color-success-bg)] flex items-center justify-center text-[var(--color-success-text)] mx-auto mb-4">
              <CheckCircle2 :size="28" />
            </div>
            <h3 class="text-xl font-black text-heading">Mở đợt đăng ký?</h3>
            <p class="text-sm text-label mt-2 leading-relaxed">
              Bạn sắp mở đợt <strong class="text-heading">{{ openTarget.name }}</strong>.
              Sinh viên sẽ có thể đăng ký môn học ngay khi đợt được mở.
            </p>
          </div>
          <div class="px-6 pb-6 flex items-center justify-center gap-3">
            <button
              class="lg-button-secondary px-6 py-2.5 text-sm font-bold"
              @click="closeOpenConfirm"
            >Hủy</button>
            <button
              :class="[
                'px-6 py-2.5 rounded-2xl text-sm font-bold text-white transition-all flex items-center gap-2',
                isOpening
                  ? 'bg-[var(--lg-success)] opacity-60 cursor-not-allowed'
                  : 'bg-[var(--lg-success)] hover:opacity-90'
              ]"
              :disabled="isOpening"
              @click="confirmOpen"
            >
              <span v-if="isOpening" class="h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
              <CheckCircle2 v-else :size="16" />
              {{ isOpening ? 'Đang mở...' : 'Xác nhận mở' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-header {
  background: var(--surface-input);
  border-bottom: 1px solid var(--border-card);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-from .relative.w-full,
.modal-leave-to .relative.w-full {
  transform: scale(0.9) translateY(20px);
}
</style>
