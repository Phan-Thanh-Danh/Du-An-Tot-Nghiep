<script setup>
import { ref, computed, onMounted } from 'vue'
import { Search, Plus, X, Pencil, Calendar, Lock, Unlock, Loader2, ShieldAlert, AlertCircle } from 'lucide-vue-next'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassInput from '@/components/ui/GlassInput.vue'
import TableShell from '@/components/ui/TableShell.vue'
import ConfirmActionDialog from '@/components/ui/ConfirmActionDialog.vue'
import LoadingSkeleton from '@/components/ui/LoadingSkeleton.vue'
import EmptyState from '@/components/ui/EmptyState.vue'
import { academicTermApi } from '@/services/academicTermApi'

const loading = ref(true)
const forbidden = ref(false)
const error = ref(null)
const terms = ref([])
const searchQuery = ref('')
const filterStatus = ref('all')

const showFormModal = ref(false); const formMode = ref('create'); const editingId = ref(null); const submitting = ref(false)
let formData = ref({ maCodeHocKy: '', tenHocKy: '', namHoc: '', thuTuTrongNam: 1, ngayBatDau: '', ngayKetThuc: '', ngayKetThucBlock5: '', soTinChiToiDa: null, hanRutMon: '', daKhoa: false })
const formErrors = ref({})
const confirmDelete = ref(null)

onMounted(fetchTerms)
async function fetchTerms() {
  loading.value = true; error.value = null; forbidden.value = false
  try {
    const res = await academicTermApi.list({ PageSize: 200 })
    terms.value = Array.isArray(res) ? res : (res?.items || res?.data || [])
  }
  catch (e) {
    console.error(e)
    if (e?.statusCode === 403) {
      forbidden.value = true
    } else {
      error.value = e.message || 'Không thể tải danh sách học kỳ'
    }
  }
  finally { loading.value = false }
}

const filteredTerms = computed(() => {
  const q = searchQuery.value.trim().toLowerCase()
  return terms.value.filter(t => {
    const ms = !q || t.tenHocKy?.toLowerCase().includes(q) || t.maCodeHocKy?.toLowerCase().includes(q) || t.namHoc?.toLowerCase().includes(q)
    const mst = filterStatus.value === 'all' || (filterStatus.value === 'locked' && t.daKhoa) || (filterStatus.value === 'unlocked' && !t.daKhoa)
    return ms && mst
  })
})

const stats = computed(() => ({ total: terms.value.length, locked: terms.value.filter(t => t.daKhoa).length, unlocked: terms.value.filter(t => !t.daKhoa).length }))

function clearFilters() { searchQuery.value = ''; filterStatus.value = 'all' }

function formatDate(d) { if (!d) return '—'; const p = d.split('T')[0].split('-'); return `${p[2]}/${p[1]}/${p[0]}` }

// ── Form ──
const defaults = () => ({ maCodeHocKy: '', tenHocKy: '', namHoc: '', thuTuTrongNam: 1, ngayBatDau: '', ngayKetThuc: '', ngayKetThucBlock5: '', soTinChiToiDa: null, hanRutMon: '', daKhoa: false })
function resetForm() { formData.value = defaults(); formErrors.value = {} }

function openCreate() { resetForm(); formMode.value = 'create'; editingId.value = null; showFormModal.value = true }
function openEdit(t) {
  formMode.value = 'edit'; editingId.value = t.maHocKy
  formData.value = { maCodeHocKy: t.maCodeHocKy, tenHocKy: t.tenHocKy, namHoc: t.namHoc, thuTuTrongNam: t.thuTuTrongNam, ngayBatDau: t.ngayBatDau || '', ngayKetThuc: t.ngayKetThuc || '', ngayKetThucBlock5: t.ngayKetThucBlock5 || '', soTinChiToiDa: t.soTinChiToiDa, hanRutMon: t.hanRutMon || '', daKhoa: t.daKhoa }
  formErrors.value = {}; showFormModal.value = true
}
function closeForm() { showFormModal.value = false; resetForm() }

function validate() {
  const e = {}
  if (!formData.value.maCodeHocKy.trim()) e.maCodeHocKy = 'Mã học kỳ không được để trống'
  if (!formData.value.tenHocKy.trim()) e.tenHocKy = 'Tên học kỳ không được để trống'
  if (!formData.value.namHoc.trim()) e.namHoc = 'Năm học không được để trống'
  else if (!/^\d{4}-\d{4}$/.test(formData.value.namHoc.trim())) e.namHoc = 'Định dạng: YYYY-YYYY'
  if (!formData.value.ngayBatDau) e.ngayBatDau = 'Ngày bắt đầu không được để trống'
  if (!formData.value.ngayKetThuc) e.ngayKetThuc = 'Ngày kết thúc không được để trống'
  formErrors.value = e; return Object.keys(e).length === 0
}

function buildPayload() { return { maDonVi: 1, maCodeHocKy: formData.value.maCodeHocKy.trim(), tenHocKy: formData.value.tenHocKy.trim(), namHoc: formData.value.namHoc.trim(), thuTuTrongNam: Number(formData.value.thuTuTrongNam), ngayBatDau: formData.value.ngayBatDau || null, ngayKetThuc: formData.value.ngayKetThuc || null, ngayKetThucBlock5: formData.value.ngayKetThucBlock5 || null, soTinChiToiDa: formData.value.soTinChiToiDa ? Number(formData.value.soTinChiToiDa) : null, hanRutMon: formData.value.hanRutMon || null, daKhoa: formData.value.daKhoa } }

async function submitForm() {
  if (!validate()) return; submitting.value = true
  try {
    if (formMode.value === 'edit') await academicTermApi.update(editingId.value, buildPayload())
    else await academicTermApi.create(buildPayload())
    closeForm(); await fetchTerms()
  } catch (e) { formErrors.value._api = e.message || 'Lỗi khi lưu học kỳ' }
  finally { submitting.value = false }
}

// ── Actions ──
async function toggleLock(t) {
  try { if (t.daKhoa) await academicTermApi.unlock(t.maHocKy); else await academicTermApi.lock(t.maHocKy); await fetchTerms() }
  catch { /* ignore */ }
}

function requestDelete(t) { confirmDelete.value = t }
async function executeDelete() {
  if (!confirmDelete.value) return
  try { await academicTermApi.lock(confirmDelete.value.maHocKy); confirmDelete.value = null; await fetchTerms() }
  catch { confirmDelete.value = null }
}
</script>

<template>
  <div class="space-y-4">

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Tổng học kỳ</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.total }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-info-bg) flex items-center justify-center">
            <Calendar :size="20" class="text-(--color-info-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Đang mở</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.unlocked }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-success-bg) flex items-center justify-center">
            <Unlock :size="20" class="text-(--color-success-text)" />
          </div>
        </div>
      </div>
      <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-xs font-bold text-muted uppercase tracking-wide">Đã khóa</p>
            <p class="text-3xl font-bold text-heading mt-1">{{ stats.locked }}</p>
          </div>
          <div class="h-10 w-10 rounded-2xl bg-(--color-danger-bg) flex items-center justify-center">
            <Lock :size="20" class="text-(--color-danger-text)" />
          </div>
        </div>
      </div>
    </div>

    <!-- Main Card -->
    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="p-4 border-b border-default bg-(--surface-input)">
        <div class="flex flex-wrap items-center gap-3">
          <div class="flex-1 min-w-[200px]">
            <GlassInput v-model="searchQuery" placeholder="Tìm mã HK, tên, năm học...">
              <template #prefix><Search :size="15" class="text-placeholder" /></template>
            </GlassInput>
          </div>
          <select v-model="filterStatus" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)">
            <option value="all">Tất cả trạng thái</option>
            <option value="unlocked">Đang mở</option>
            <option value="locked">Đã khóa</option>
          </select>
          <button v-if="filterStatus !== 'all' || searchQuery" @click="clearFilters"
            class="h-10 px-3 rounded-xl text-xs font-bold flex items-center gap-1.5 text-(--color-danger-text) hover:bg-(--color-danger-bg) transition-colors shrink-0">
            <X :size="14" /> Xóa lọc
          </button>
          <div class="flex items-center gap-1 ml-auto">
            <GlassButton variant="primary" class="h-10 shrink-0" @click="openCreate">
              <Plus :size="15" class="mr-1" /> Thêm học kỳ
            </GlassButton>
          </div>
        </div>
      </div>

      <!-- 5 States Container -->
      <div v-if="loading" class="p-6"><LoadingSkeleton :lines="6" /></div>

      <!-- Forbidden State -->
      <div v-else-if="forbidden" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
        <ShieldAlert :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
        <h3 class="text-lg font-bold text-(--text-heading)">Không có quyền truy cập</h3>
        <p class="text-sm text-(--text-muted) mt-1">Tài khoản của bạn không được phân quyền quản lý học kỳ.</p>
      </div>

      <!-- Error State with Retry -->
      <div v-else-if="error" class="flex flex-col items-center justify-center py-16 bg-(--surface-card) border border-(--border-default) rounded-xl">
        <AlertCircle :size="48" class="text-(--color-danger-bg, #ef4444) mb-4" />
        <h3 class="text-lg font-bold text-(--text-heading)">Đã xảy ra lỗi</h3>
        <p class="text-sm text-(--text-muted) mt-1 mb-4">{{ error }}</p>
        <GlassButton variant="secondary" @click="fetchTerms">Thử lại</GlassButton>
      </div>

      <!-- Empty State -->
      <div v-else-if="filteredTerms.length === 0" class="p-6">
        <EmptyState title="Không tìm thấy học kỳ nào" description="Thử thay đổi từ khóa hoặc bộ lọc.">
          <GlassButton variant="primary" @click="openCreate"><Plus :size="15" class="mr-1" /> Thêm học kỳ</GlassButton>
        </EmptyState>
      </div>

      <!-- Success State: TableShell -->
      <TableShell v-else>
        <table class="w-full text-left text-sm whitespace-nowrap border-collapse">
          <thead class="bg-(--surface-input) border-b border-default text-muted">
            <tr>
              <th class="px-3 py-4 font-semibold">Mã HK</th>
              <th class="px-3 py-4 font-semibold">Tên học kỳ</th>
              <th class="px-3 py-4 font-semibold">Năm học</th>
              <th class="px-3 py-4 font-semibold text-center">Thứ tự</th>
              <th class="px-3 py-4 font-semibold">Ngày BĐ</th>
              <th class="px-3 py-4 font-semibold">Ngày KT</th>
              <th class="px-3 py-4 font-semibold text-center">TC tối đa</th>
              <th class="px-3 py-4 font-semibold">Trạng thái</th>
              <th class="px-3 py-4 font-semibold text-center">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-for="t in filteredTerms" :key="t.maHocKy" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-3 py-3.5 font-mono text-xs font-bold text-muted">{{ t.maCodeHocKy }}</td>
              <td class="px-3 py-3.5 font-bold text-heading max-w-[200px] truncate">{{ t.tenHocKy }}</td>
              <td class="px-3 py-3.5 text-body">{{ t.namHoc }}</td>
              <td class="px-3 py-3.5 text-center"><span class="inline-flex items-center justify-center h-7 w-7 rounded-lg bg-(--surface-input) text-xs font-bold text-body border border-default">{{ t.thuTuTrongNam }}</span></td>
              <td class="px-3 py-3.5 text-body">{{ formatDate(t.ngayBatDau) }}</td>
              <td class="px-3 py-3.5 text-body">{{ formatDate(t.ngayKetThuc) }}</td>
              <td class="px-3 py-3.5 text-center font-bold text-body">{{ t.soTinChiToiDa || '—' }}</td>
              <td class="px-3 py-3.5">
                <GlassBadge :variant="t.daKhoa ? 'danger' : 'success'" size="sm">
                  {{ t.daKhoa ? 'Đã khóa' : 'Đang mở' }}
                </GlassBadge>
              </td>
              <td class="px-3 py-3.5">
                <div class="flex items-center justify-center gap-1">
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" :title="t.daKhoa ? 'Mở khóa' : 'Khóa'" @click.stop="toggleLock(t)">
                    <component :is="t.daKhoa ? Unlock : Lock" :size="15" />
                  </button>
                  <button class="h-8 w-8 rounded-lg hover:bg-(--accent-primary-soft) flex items-center justify-center text-muted hover:text-(--sidebar-accent) transition-colors" title="Chỉnh sửa" @click.stop="openEdit(t)">
                    <Pencil :size="15" />
                  </button>
                  <button v-if="!t.daKhoa" class="h-8 w-8 rounded-lg hover:bg-(--color-danger-bg) flex items-center justify-center text-muted hover:text-(--color-danger-text) transition-colors" title="Khóa" @click.stop="requestDelete(t)">
                    <X :size="15" />
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </TableShell>
    </div>

    <!-- Create / Edit Modal -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div v-if="showFormModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm" @click.self="closeForm">
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-(--text-heading)">{{ formMode === 'create' ? 'Thêm học kỳ' : 'Chỉnh sửa học kỳ' }}</h3>
              <button @click="closeForm" class="text-(--text-muted) hover:text-(--text-heading) p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors"><X :size="18" /></button>
            </div>
            <div class="px-6 py-5 overflow-y-auto space-y-4" style="max-height: calc(90vh - 140px)">
              <p v-if="formErrors._api" class="text-sm text-(--color-danger-text) font-semibold">{{ formErrors._api }}</p>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Mã HK <span class="text-(--color-danger-text)">*</span></label>
                  <GlassInput v-model="formData.maCodeHocKy" placeholder="VD: HK1-2025-2026" :error="formErrors.maCodeHocKy" class="w-full" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Tên học kỳ <span class="text-(--color-danger-text)">*</span></label>
                  <GlassInput v-model="formData.tenHocKy" placeholder="VD: Học kỳ 1" :error="formErrors.tenHocKy" class="w-full" />
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Năm học <span class="text-(--color-danger-text)">*</span></label>
                  <GlassInput v-model="formData.namHoc" placeholder="VD: 2025-2026" :error="formErrors.namHoc" class="w-full" />
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Thứ tự</label>
                  <select v-model="formData.thuTuTrongNam" class="text-xs bg-(--surface-input) border border-(--border-input) text-(--text-body) rounded-lg w-full h-[38px] px-3 outline-none focus:ring-1 focus:ring-(--border-focus)">
                    <option :value="1">1 - Học kỳ 1</option>
                    <option :value="2">2 - Học kỳ 2</option>
                    <option :value="3">3 - Học kỳ hè</option>
                  </select>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày BĐ <span class="text-(--color-danger-text)">*</span></label>
                  <GlassInput v-slot="scope" :error="formErrors.ngayBatDau" class="w-full">
                    <input v-model="formData.ngayBatDau" v-bind="scope" type="date" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
                  </GlassInput>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Ngày KT <span class="text-(--color-danger-text)">*</span></label>
                  <GlassInput v-slot="scope" :error="formErrors.ngayKetThuc" class="w-full">
                    <input v-model="formData.ngayKetThuc" v-bind="scope" type="date" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
                  </GlassInput>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-3">
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Hạn rút môn</label>
                  <GlassInput v-slot="scope" class="w-full">
                    <input v-model="formData.hanRutMon" v-bind="scope" type="date" class="bg-transparent border-none outline-none w-full text-xs text-(--text-body)" />
                  </GlassInput>
                </div>
                <div>
                  <label class="block text-xs font-semibold text-(--text-muted) mb-1">Số TC tối đa</label>
                  <GlassInput v-model.number="formData.soTinChiToiDa" type="number" min="1" placeholder="VD: 24" class="w-full" />
                </div>
              </div>
              <div class="flex items-center gap-3">
                <label class="text-xs font-semibold text-(--text-muted)">Trạng thái</label>
                <button type="button" class="relative w-10 h-5 rounded-full transition-colors" :class="formData.daKhoa ? 'bg-(--color-danger-text)' : 'bg-emerald-500'" @click="formData.daKhoa = !formData.daKhoa">
                  <span class="absolute top-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform" :class="formData.daKhoa ? 'translate-x-5' : 'translate-x-0.5'" />
                </button>
                <span class="text-xs text-(--text-muted)">{{ formData.daKhoa ? 'Đã khóa' : 'Đang mở' }}</span>
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) bg-(--surface-modal) flex items-center gap-3 justify-end">
              <GlassButton variant="secondary" class="h-10 px-5 text-sm" @click="closeForm">Hủy</GlassButton>
              <GlassButton variant="primary" class="h-10 px-5 text-sm" :disabled="submitting" @click="submitForm">
                <Loader2 v-if="submitting" :size="15" class="mr-1.5 animate-spin" />
                <span v-else>{{ formMode === 'create' ? 'Thêm học kỳ' : 'Cập nhật' }}</span>
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <ConfirmActionDialog v-if="confirmDelete" :show="true" title="Khóa học kỳ"
      :message="`Bạn có chắc muốn khóa &quot;${confirmDelete.tenHocKy}&quot; (${confirmDelete.maCodeHocKy})?`"
      confirm-label="Xác nhận khóa" variant="danger" @confirm="executeDelete" @cancel="confirmDelete = null" />
  </div>
</template>

<style scoped>
.modal-fade-enter-active, .modal-fade-leave-active { transition: opacity 0.2s ease; }
.modal-fade-enter-from, .modal-fade-leave-to { opacity: 0; }
</style>
