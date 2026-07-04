<script setup>
import { ref, computed, onMounted } from 'vue'
import dayjs from 'dayjs'
import { useBodyScrollLock } from '@/composables/useBodyScrollLock'

const today = dayjs().format('YYYY-MM-DD')
import {
  Plus,
  Search,
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
  FileText,
  Loader2,
} from 'lucide-vue-next'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import { registrationApi } from '@/services/registrationApi'

const loading = ref(true)
const apiError = ref('')

const periods = ref([])

function mapPeriod(period) {
  return {
    ...period,
    openDate: period.openDate?.slice?.(0, 10) || period.openDate,
    closeDate: period.closeDate?.slice?.(0, 10) || period.closeDate,
    withdrawDeadline: period.withdrawDeadline || 'Theo học kỳ',
  }
}

async function initData() {
  loading.value = true
  apiError.value = ''
  try {
    const data = await registrationApi.getPeriods()
    periods.value = Array.isArray(data) ? data.map(mapPeriod) : []
  } catch (err) {
    apiError.value = err?.message || 'Không thể tải đợt đăng ký.'
  } finally {
    loading.value = false
  }
}

onMounted(() => { initData() })

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
  open:   { label: 'Đang mở',  dot: 'bg-(--lg-success)', badge: 'lg-badge lg-badge-success' },
  draft:  { label: 'Bản nháp', dot: 'bg-(--lg-warning)', badge: 'lg-badge lg-badge-warning' },
  closed: { label: 'Đã đóng',  dot: 'bg-(--text-placeholder)', badge: 'lg-badge surface-solid text-muted border-default' },
}
const getStatusInfo = s => STATUS_MAP[s] || STATUS_MAP.closed

// ── Create Period Modal ──────────────────────────────────────
const showCreateModal = ref(false)
const createForm = ref({ name: '', semester: '', maHocKy: null, openDate: '', closeDate: '', withdrawDeadline: '', maxCredits: 24 })
const isCreating = ref(false)
useBodyScrollLock(showCreateModal)

function openCreate() {
  const latestPeriod = periods.value[0]
  createForm.value = {
    name: '',
    semester: latestPeriod?.semester || '',
    maHocKy: latestPeriod?.maHocKy || null,
    openDate: '',
    closeDate: '',
    withdrawDeadline: '',
    maxCredits: latestPeriod?.maxCredits || 24,
  }
  showCreateModal.value = true
}

function closeCreate() {
  showCreateModal.value = false
}

async function confirmCreate() {
  isCreating.value = true
  apiError.value = ''
  try {
    const created = await registrationApi.createPeriod({
      maHocKy: Number(createForm.value.maHocKy),
      openDate: createForm.value.openDate,
      closeDate: createForm.value.closeDate,
      maxCredits: Number(createForm.value.maxCredits),
    })
    periods.value.unshift(mapPeriod(created))
    closeCreate()
  } catch (err) {
    apiError.value = err?.message || 'Không thể tạo đợt đăng ký.'
  } finally {
    isCreating.value = false
  }
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
  apiError.value = ''
  try {
    const updated = await registrationApi.updatePeriod(editTarget.value.id, {
      maHocKy: Number(editForm.value.maHocKy),
      openDate: editForm.value.openDate,
      closeDate: editForm.value.closeDate,
      maxCredits: Number(editForm.value.maxCredits),
    })
    Object.assign(editTarget.value, mapPeriod(updated))
    closeEdit()
  } catch (err) {
    apiError.value = err?.message || 'Không thể cập nhật đợt đăng ký.'
  } finally {
    isEditing.value = false
  }
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
  apiError.value = ''
  try {
    const updated = await registrationApi.openPeriod(openTarget.value.id)
    Object.assign(openTarget.value, mapPeriod(updated))
    closeOpenConfirm()
  } catch (err) {
    apiError.value = err?.message || 'Không thể mở đợt đăng ký.'
  } finally {
    isOpening.value = false
  }
}

// ── More Actions Dropdown ────────────────────────────────────
const activeMenuId = ref(null)

function toggleMenu(id) {
  activeMenuId.value = activeMenuId.value === id ? null : id
}

function closeMenu() {
  activeMenuId.value = null
}

async function handleClosePeriod(period) {
  try {
    const updated = await registrationApi.closePeriod(period.id)
    Object.assign(period, mapPeriod(updated))
  } catch (err) {
    apiError.value = err?.message || 'Không thể đóng đợt đăng ký.'
  }
  closeMenu()
}

async function handleDeleteDraft(period) {
  try {
    await registrationApi.deletePeriod(period.id)
    const idx = periods.value.findIndex(p => p.id === period.id)
    if (idx !== -1) periods.value.splice(idx, 1)
  } catch (err) {
    apiError.value = err?.message || 'Không thể xóa đợt đăng ký.'
  }
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
      <button v-if="!loading" class="lg-button-primary px-5 py-2.5 text-sm font-bold" @click="openCreate">
        <Plus :size="18" /> Tạo đợt mới
      </button>
    </template>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-muted" :size="28" />
      <p class="text-sm text-muted">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
      <p class="text-xs text-muted">{{ apiError }}</p>
      <button @click="initData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>

    <div class="space-y-4">

      <!-- ── Stats Row ── -->
      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3">
        <div
          v-for="stat in [
            { label: 'Tổng đợt',       value: stats.total, color: 'text-(--color-info-text)', bg: 'bg-(--color-info-bg)' },
            { label: 'Đang mở',        value: stats.open,  color: 'text-(--color-success-text)', bg: 'bg-(--color-success-bg)' },
            { label: 'Bản nháp',       value: stats.draft, color: 'text-(--color-warning-text)', bg: 'bg-(--color-warning-bg)' },
            { label: 'Đã đóng',        value: stats.closed,color: 'text-label',           bg: 'surface-solid' },
          ]"
          :key="stat.label"
          :class="['rounded-2xl p-4 border border-default', stat.bg]"
        >
          <p class="text-[11px] font-semibold uppercase tracking-widest text-placeholder">{{ stat.label }}</p>
          <p :class="['text-2xl font-semibold mt-1', stat.color]">{{ stat.value }}</p>
        </div>
      </div>

      <!-- ── Search & Filter ── -->
      <div class="lg-glass-strong p-4 rounded-2xl">
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
          Hiển thị <span class="font-semibold text-heading">{{ filteredPeriods.length }}</span> / {{ periods.length }} đợt
        </p>
      </div>

      <!-- ── Periods Table ── -->
      <div class="lg-table-shell overflow-hidden">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Đợt đăng ký</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Max Credits</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-default)">
            <tr v-for="period in filteredPeriods" :key="period.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading leading-tight">{{ period.name }}</p>
                <p class="text-[11px] font-bold text-(--lg-primary) mt-1 uppercase tracking-tighter">{{ period.semester }}</p>
              </td>
              <td class="px-4 py-4">
                <div class="space-y-1">
                  <div class="flex items-center gap-2 text-xs font-bold text-label">
                    <Clock :size="14" class="text-(--lg-success)" /> {{ period.openDate }} <ArrowRight :size="12" class="text-placeholder" /> {{ period.closeDate }}
                  </div>
                  <div class="flex items-center gap-2 text-[10px] font-medium text-placeholder">
                    <AlertCircle :size="12" /> Hủy môn: {{ period.withdrawDeadline }}
                  </div>
                </div>
              </td>
              <td class="px-4 py-4">
                <span class="text-sm font-semibold text-heading">{{ period.maxCredits }} TC</span>
              </td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest', getStatusInfo(period.status).badge]">
                  {{ getStatusInfo(period.status).label }}
                </span>
              </td>
              <td class="px-4 py-4 relative">
                <div class="flex items-center gap-1">
                  <button class="p-2 hover:bg-(--color-info-bg) hover:text-(--lg-primary) rounded-lg text-placeholder transition-all" title="Chỉnh sửa" @click="openEdit(period)">
                    <Edit3 :size="16" />
                  </button>
                  <button v-if="period.status === 'draft'" class="p-2 hover:bg-(--color-success-bg) hover:text-(--lg-success) rounded-lg text-placeholder transition-all" title="Mở đăng ký" @click="openOpenConfirm(period)">
                    <CheckCircle2 :size="16" />
                  </button>
                  <button class="p-2 hover:bg-(--surface-solid) rounded-lg text-placeholder transition-all relative" @click.stop="toggleMenu(period.id)">
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
                        class="w-full px-4 py-2.5 text-sm font-bold text-label hover:bg-(--color-info-bg) flex items-center gap-3 transition-all text-left"
                        @click="openEdit(period)"
                      >
                        <Edit3 :size="15" /> Chỉnh sửa
                      </button>
                      <button
                        v-if="period.status === 'open'"
                        class="w-full px-4 py-2.5 text-sm font-bold text-label hover:bg-(--color-warning-bg) flex items-center gap-3 transition-all text-left"
                        @click="handleClosePeriod(period)"
                      >
                        <X :size="15" /> Đóng đợt
                      </button>
                      <button
                        v-if="period.status === 'draft'"
                        class="w-full px-4 py-2.5 text-sm font-bold text-(--lg-danger) hover:bg-(--color-danger-bg) flex items-center gap-3 transition-all text-left"
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
          <p class="text-base font-semibold text-heading">Không tìm thấy đợt nào</p>
          <p class="text-sm text-label mt-1">Thử thay đổi từ khóa tìm kiếm.</p>
          <button v-if="searchQuery" class="mt-3 text-sm font-bold text-(--lg-primary) hover:underline" @click="searchQuery = ''">Xóa tìm kiếm</button>
        </div>
      </div>

      <!-- ── Important Rules ── -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div class="lg-glass-soft p-5 rounded-2xl border border-default">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-(--color-info-bg) flex items-center justify-center text-(--color-info-text) shrink-0">
               <ShieldCheck :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-semibold text-heading">Quy tắc đăng ký</h4>
              <ul class="mt-2 space-y-2">
                <li class="text-xs text-label flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-(--lg-primary) mt-1.5 shrink-0"></div>
                  Sinh viên chỉ có thể đăng ký khi đợt ở trạng thái <strong class="text-heading">Đang mở</strong>.
                </li>
                <li class="text-xs text-label flex items-start gap-2">
                  <div class="h-1 w-1 rounded-full bg-(--lg-primary) mt-1.5 shrink-0"></div>
                  Hệ thống tự động kiểm tra tín chỉ tối đa đã cấu hình.
                </li>
              </ul>
            </div>
          </div>
        </div>

        <div class="lg-glass-soft p-5 rounded-2xl border border-default">
          <div class="flex gap-4">
            <div class="h-10 w-10 rounded-xl bg-(--color-warning-bg) flex items-center justify-center text-(--color-warning-text) shrink-0">
               <Settings :size="20" />
            </div>
            <div>
              <h4 class="text-sm font-semibold text-heading">Chốt danh sách</h4>
              <p class="text-xs text-label mt-2 leading-relaxed">
                Khi đợt đăng ký kết thúc, hệ thống sẽ tự động chốt danh sách và chuyển các lớp có sĩ số thấp sang trạng thái <strong class="text-heading">PENDING CANCEL</strong> để Giáo vụ xử lý.
              </p>
            </div>
          </div>
        </div>
      </div>

    </div>
  </template>
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
                  <h2 class="text-lg font-semibold text-heading">Tạo đợt đăng ký mới</h2>
                  <p class="text-xs text-muted mt-0.5">Thiết lập thông tin đợt đăng ký môn học</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-(--surface-input-focus) flex items-center justify-center text-muted transition-all"
                @click="closeCreate"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">
                Mã học kỳ <span class="text-(--lg-danger)">*</span>
              </label>
              <input
                v-model.number="createForm.maHocKy"
                type="number"
                min="1"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                placeholder="VD: 1"
              />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">
                  Ngày mở <span class="text-(--lg-danger)">*</span>
                </label>
                <input
                  v-model="createForm.openDate"
                  type="date"
                  :min="today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
              <div>
                <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">
                  Ngày đóng <span class="text-(--lg-danger)">*</span>
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
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Hạn hủy môn</label>
                <input
                  v-model="createForm.withdrawDeadline"
                  type="date"
                  :min="createForm.openDate || today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Max Credits</label>
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
                (!createForm.maHocKy || !createForm.openDate || !createForm.closeDate || isCreating) ? 'opacity-45 pointer-events-none' : ''
              ]"
              :disabled="!createForm.maHocKy || !createForm.openDate || !createForm.closeDate || isCreating"
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
                  <h2 class="text-lg font-semibold text-heading">Chỉnh sửa đợt đăng ký</h2>
                  <p class="text-xs text-muted mt-0.5">{{ editTarget.name }}</p>
                </div>
              </div>
              <button
                class="h-8 w-8 rounded-2xl surface-input hover:bg-(--surface-input-focus) flex items-center justify-center text-muted transition-all"
                @click="closeEdit"
              >
                <X :size="16" />
              </button>
            </div>
          </div>

          <div class="p-6 space-y-5">
            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Mã học kỳ <span class="text-(--lg-danger)">*</span></label>
              <input
                v-model.number="editForm.maHocKy"
                type="number"
                min="1"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Ngày mở <span class="text-(--lg-danger)">*</span></label>
                <input
                  v-model="editForm.openDate"
                  type="date"
                  :min="today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
              <div>
                <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Ngày đóng <span class="text-(--lg-danger)">*</span></label>
                <input
                  v-model="editForm.closeDate"
                  type="date"
                  :min="editForm.openDate || today"
                  class="lg-input w-full px-4 py-2.5 text-sm font-bold"
                />
              </div>
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Hạn hủy môn</label>
              <input
                v-model="editForm.withdrawDeadline"
                type="date"
                :min="editForm.openDate || today"
                class="lg-input w-full px-4 py-2.5 text-sm font-bold"
              />
            </div>

            <div>
              <label class="text-[11px] font-semibold text-label uppercase tracking-widest mb-1.5 block">Max Credits</label>
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
            <div class="h-16 w-16 rounded-3xl bg-(--color-success-bg) flex items-center justify-center text-(--color-success-text) mx-auto mb-4">
              <CheckCircle2 :size="28" />
            </div>
            <h3 class="text-xl font-semibold text-heading">Mở đợt đăng ký?</h3>
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
                  ? 'bg-(--lg-success) opacity-60 cursor-not-allowed'
                  : 'bg-(--lg-success) hover:opacity-90'
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
