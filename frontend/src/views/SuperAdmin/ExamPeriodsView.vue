<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertCircle, CalendarRange, Loader2, Plus, RefreshCw, Eye, X, Lock, Unlock, CheckCircle2 } from 'lucide-vue-next'
import { examApi } from '@/services/examApi'
import { academicTermApi } from '@/services/academicTermApi'
import academicSchedulingApi from '@/services/academicSchedulingApi'
import LmsSelect from '@/components/LmsSelect.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'

const STATUS_LABELS = {
  nhap: 'Bản nháp',
  dang_dien_ra: 'Đang diễn ra',
  da_ket_thuc: 'Đã kết thúc',
}

const STATUS_CLASSES = {
  nhap: 'bg-(--color-warning-bg) text-(--color-warning-text)',
  dang_dien_ra: 'bg-(--color-success-bg) text-(--color-success-text)',
  da_ket_thuc: 'bg-(--color-info-bg) text-(--color-info-text)',
}

const LOAI_LABELS = { giua_ky: 'Giữa kỳ', cuoi_ky: 'Cuối kỳ' }

const loading = ref(false)
const error = ref('')
const examPeriods = ref([])

// Create form
const showCreate = ref(false)
const saving = ref(false)
const formError = ref('')
const form = ref({ tenKyThi: '', maHocKy: null, maNganh: null, loaiKyThi: 'cuoi_ky' })
const terms = ref([])
const majors = ref([])

// Detail
const showDetail = ref(false)
const detail = ref(null)
const detailLichThiTongs = ref([])
const detailLoading = ref(false)

// Action confirm
const actionBusy = ref(null)

const termOptions = computed(() =>
  terms.value.map(t => ({ value: t.maHocKy, label: `${t.tenHocKy} (${t.maCodeHocKy})` }))
)
const majorOptions = computed(() =>
  majors.value.map(m => ({ value: m.maNganh, label: m.tenNganh }))
)

function unwrapList(data) {
  if (Array.isArray(data)) return data
  return data?.items || data?.data || data?.results || []
}

function statusLabel(status) {
  return STATUS_LABELS[status] || status || '—'
}

function statusClass(status) {
  return STATUS_CLASSES[status] || 'bg-(--surface-input) text-label'
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const data = await examApi.getExamPeriods({ pageSize: 100 })
    examPeriods.value = unwrapList(data)
  } catch (err) {
    error.value = err?.message || 'Không tải được cấu hình kỳ thi'
    examPeriods.value = []
  } finally {
    loading.value = false
  }
}

async function loadOptions() {
  try {
    terms.value = await academicTermApi.list({ pageSize: 100 })
  } catch {
    terms.value = []
  }
  try {
    majors.value = await academicSchedulingApi.getMajors()
  } catch {
    majors.value = []
  }
}

function openCreate() {
  formError.value = ''
  form.value = { tenKyThi: '', maHocKy: null, maNganh: null, loaiKyThi: 'cuoi_ky' }
  showCreate.value = true
}

async function submitCreate() {
  formError.value = ''
  if (!form.value.tenKyThi.trim()) {
    formError.value = 'Vui lòng nhập tên kỳ thi.'
    return
  }
  if (!form.value.maHocKy) {
    formError.value = 'Vui lòng chọn học kỳ.'
    return
  }
  saving.value = true
  try {
    const payload = {
      tenKyThi: form.value.tenKyThi.trim(),
      maHocKy: form.value.maHocKy,
      loaiKyThi: form.value.loaiKyThi,
      maNganh: form.value.maNganh || null,
    }
    const created = await examApi.createExamPeriod(payload)
    showCreate.value = false
    await loadData()
    if (created?.maKyThi) {
      await openDetail(created.maKyThi)
    }
  } catch (err) {
    formError.value = err?.message || 'Không tạo được kỳ thi.'
  } finally {
    saving.value = false
  }
}

async function openDetail(id) {
  showDetail.value = true
  detail.value = null
  detailLichThiTongs.value = []
  detailLoading.value = true
  try {
    detail.value = await examApi.getExamPeriod(id)
    detailLichThiTongs.value = unwrapList(await examApi.getExamPeriodLichThiTongs(id))
  } catch (err) {
    detail.value = { error: err?.message || 'Không tải được chi tiết kỳ thi.' }
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  showDetail.value = false
  detail.value = null
  detailLichThiTongs.value = []
}

async function runAction(kyThi, action) {
  actionBusy.value = kyThi.maKyThi
  try {
    if (action === 'publish') {
      await examApi.publishExamPeriod(kyThi.maKyThi)
    } else {
      await examApi.closeExamPeriod(kyThi.maKyThi)
    }
    await loadData()
  } catch (err) {
    error.value = err?.message || 'Thao tác thất bại.'
  } finally {
    actionBusy.value = null
  }
}

onMounted(() => {
  loadData()
  loadOptions()
})
</script>

<template>
  <div class="space-y-4 pb-10">
    <div class="surface-card border border-card rounded-2xl p-5 shadow-sm">
      <div class="flex items-start justify-between gap-4 flex-wrap">
        <div>
          <h2 class="text-lg font-bold text-heading">Cấu hình kỳ thi</h2>
          <p class="mt-1 max-w-3xl text-xs text-muted">
            Tạo kỳ thi theo học kỳ (cho tất cả ngành hoặc từng ngành). Hệ thống tự động gắn đề thi có sẵn cho từng môn; môn chưa có đề sẽ được đánh dấu "Chưa có đề thi".
          </p>
        </div>
        <div class="flex items-center gap-2">
          <button
            class="inline-flex items-center gap-2 rounded-xl border border-default surface-card px-3 py-2 text-xs font-bold text-heading hover:bg-slate-50 dark:hover:bg-slate-800 transition-colors"
            @click="loadData"
            :disabled="loading"
          >
            <Loader2 v-if="loading" class="h-4 w-4 animate-spin text-muted" />
            <RefreshCw v-else class="h-4 w-4 text-muted" />
            Tải lại
          </button>
          <GlassButton variant="primary" class="px-3.5 py-2 text-xs" @click="openCreate">
            <Plus size="14" class="mr-1" /> Tạo kỳ thi
          </GlassButton>
        </div>
      </div>

      <div v-if="error" class="mt-4 rounded-xl border border-red-200 bg-red-50 p-4 dark:border-red-900/50 dark:bg-red-900/20">
        <div class="flex items-start gap-3">
          <AlertCircle class="h-5 w-5 text-red-600 dark:text-red-400 mt-0.5" />
          <div>
            <h3 class="text-sm font-bold text-red-800 dark:text-red-200">Lỗi lấy dữ liệu</h3>
            <p class="mt-1 text-sm text-red-600 dark:text-red-300">{{ error }}</p>
          </div>
        </div>
      </div>

      <SkeletonTable v-else-if="loading && examPeriods.length === 0" :rows="5" :columns="6" class="mt-8" />

      <div v-else-if="examPeriods.length === 0" class="mt-8 flex flex-col items-center justify-center py-10 border border-dashed border-card rounded-xl">
        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-indigo-50 dark:bg-indigo-900/20">
          <CalendarRange class="h-6 w-6 text-indigo-600 dark:text-indigo-400" />
        </div>
        <h3 class="mt-4 text-sm font-bold text-heading">Chưa có kỳ thi nào</h3>
        <p class="mt-1 text-xs text-muted max-w-sm text-center">Bấm "Tạo kỳ thi" để bắt đầu cấu hình giai đoạn thi.</p>
      </div>

      <div v-else class="mt-6 overflow-hidden rounded-xl border border-card shadow-sm">
        <table class="w-full text-left text-sm">
          <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
            <tr>
              <th scope="col" class="px-4 py-3 border-b border-card">Tên kỳ thi</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Học kỳ</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Loại</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Ngành</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Môn có đề / Tổng</th>
              <th scope="col" class="px-4 py-3 border-b border-card">Trạng thái</th>
              <th scope="col" class="px-4 py-3 border-b border-card text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-card">
            <tr v-for="kyThi in examPeriods" :key="kyThi.maKyThi" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
              <td class="px-4 py-3 font-medium text-heading">{{ kyThi.tenKyThi }}</td>
              <td class="px-4 py-3 text-muted">{{ kyThi.tenHocKy || `Học kỳ ${kyThi.maHocKy}` }}</td>
              <td class="px-4 py-3 text-muted">{{ LOAI_LABELS[kyThi.loaiKyThi] || kyThi.loaiKyThi }}</td>
              <td class="px-4 py-3 text-muted">{{ kyThi.tenNganh || 'Tất cả ngành' }}</td>
              <td class="px-4 py-3 text-muted">
                <span class="inline-flex items-center gap-1.5">
                  <span v-if="kyThi.soMonChuaCoDeThi > 0" class="text-amber-600 dark:text-amber-400">{{ kyThi.soMonCoDeThi }}</span>
                  <span v-else class="text-emerald-600 dark:text-emerald-400">{{ kyThi.soMonCoDeThi }}</span>
                  <span class="text-label">/ {{ kyThi.soLichThiTong }}</span>
                </span>
              </td>
              <td class="px-4 py-3">
                <span class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold" :class="statusClass(kyThi.trangThai)">
                  {{ statusLabel(kyThi.trangThai) }}
                </span>
              </td>
              <td class="px-4 py-3">
                <div class="flex items-center justify-end gap-1.5">
                  <button
                    class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-bold text-indigo-600 dark:text-indigo-400 hover:bg-indigo-50 dark:hover:bg-indigo-900/30 transition-colors"
                    @click="openDetail(kyThi.maKyThi)"
                  >
                    <Eye size="13" /> Chi tiết
                  </button>
                  <button
                    v-if="kyThi.trangThai === 'nhap'"
                    class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-bold text-emerald-600 dark:text-emerald-400 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 transition-colors disabled:opacity-50"
                    :disabled="actionBusy === kyThi.maKyThi"
                    @click="runAction(kyThi, 'publish')"
                  >
                    <Loader2 v-if="actionBusy === kyThi.maKyThi" class="h-3.5 w-3.5 animate-spin" />
                    <Unlock v-else size="13" /> Mở
                  </button>
                  <button
                    v-if="kyThi.trangThai === 'dang_dien_ra'"
                    class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-bold text-rose-600 dark:text-rose-400 hover:bg-rose-50 dark:hover:bg-rose-900/30 transition-colors disabled:opacity-50"
                    :disabled="actionBusy === kyThi.maKyThi"
                    @click="runAction(kyThi, 'close')"
                  >
                    <Loader2 v-if="actionBusy === kyThi.maKyThi" class="h-3.5 w-3.5 animate-spin" />
                    <Lock v-else size="13" /> Đóng
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Tạo kỳ thi -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div
          v-if="showCreate"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4"
          @click.self="showCreate = false"
        >
          <div class="w-full max-w-lg lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-heading">Tạo kỳ thi mới</h3>
              <button @click="showCreate = false" class="text-muted hover:text-heading p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X size="18" />
              </button>
            </div>
            <div class="px-6 py-5 space-y-4">
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Tên kỳ thi *</label>
                <input
                  v-model="form.tenKyThi"
                  type="text"
                  placeholder="VD: Kỳ thi cuối kỳ HK3 2026"
                  class="w-full h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)"
                />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Học kỳ *</label>
                <LmsSelect v-model="form.maHocKy" :options="termOptions" placeholder="Chọn học kỳ" searchable />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Loại kỳ thi *</label>
                <LmsSelect
                  v-model="form.loaiKyThi"
                  :options="[
                    { value: 'giua_ky', label: 'Giữa kỳ' },
                    { value: 'cuoi_ky', label: 'Cuối kỳ' },
                  ]"
                />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Phạm vi ngành</label>
                <LmsSelect
                  v-model="form.maNganh"
                  :options="[{ value: null, label: 'Tất cả ngành' }, ...majorOptions]"
                  placeholder="Tất cả ngành"
                  searchable
                />
              </div>
              <p class="text-xs text-muted">
                Khi tạo, hệ thống sẽ tự sinh lịch thi tổng cho từng môn thuộc phạm vi và tự gắn đề thi có sẵn theo mã môn. Môn chưa có đề sẽ được báo "Chưa có đề thi".
              </p>
              <div v-if="formError" class="rounded-lg border border-(--color-danger-text) bg-(--color-danger-bg) p-3 text-xs text-(--color-danger-text)">
                {{ formError }}
              </div>
            </div>
            <div class="px-6 py-4 border-t border-(--border-default) flex justify-end gap-2">
              <GlassButton variant="secondary" size="sm" @click="showCreate = false">Hủy</GlassButton>
              <GlassButton variant="primary" size="sm" :disabled="saving" @click="submitCreate">
                <Loader2 v-if="saving" class="h-4 w-4 animate-spin mr-1" />
                {{ saving ? 'Đang tạo...' : 'Tạo kỳ thi' }}
              </GlassButton>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>

    <!-- Chi tiết kỳ thi -->
    <Teleport to="body">
      <transition name="modal-fade">
        <div
          v-if="showDetail"
          class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4"
          @click.self="closeDetail"
        >
          <div class="w-full max-w-4xl lg-glass-strong rounded-2xl shadow-2xl border border-(--border-card) overflow-hidden" style="max-height: 90vh">
            <div class="px-6 py-4 border-b border-(--border-default) flex items-center justify-between">
              <h3 class="text-lg font-bold text-heading">Chi tiết kỳ thi</h3>
              <button @click="closeDetail" class="text-muted hover:text-heading p-1.5 rounded-lg hover:bg-(--surface-input) transition-colors">
                <X size="18" />
              </button>
            </div>

            <div v-if="detailLoading" class="px-6 py-8">
              <SkeletonTable :rows="5" :columns="4" />
            </div>

            <div v-else-if="detail" class="px-6 py-5 overflow-y-auto space-y-5" style="max-height: calc(90vh - 140px)">
              <div v-if="detail.error" class="rounded-lg border border-red-200 bg-red-50 p-4 dark:border-red-900/50 dark:bg-red-900/20 text-sm text-red-700 dark:text-red-300">
                {{ detail.error }}
              </div>

              <template v-else>
                <div class="flex flex-col gap-3">
                  <div class="flex items-center gap-2 flex-wrap">
                    <h4 class="text-base font-bold text-heading">{{ detail.tenKyThi }}</h4>
                    <span class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold" :class="statusClass(detail.trangThai)">
                      {{ statusLabel(detail.trangThai) }}
                    </span>
                  </div>
                  <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 text-sm">
                    <div class="surface-card border border-card rounded-xl p-3">
                      <p class="text-[11px] font-semibold text-label uppercase">Học kỳ</p>
                      <p class="text-sm font-bold text-heading mt-1">{{ detail.tenHocKy || '—' }}</p>
                    </div>
                    <div class="surface-card border border-card rounded-xl p-3">
                      <p class="text-[11px] font-semibold text-label uppercase">Loại</p>
                      <p class="text-sm font-bold text-heading mt-1">{{ LOAI_LABELS[detail.loaiKyThi] || detail.loaiKyThi }}</p>
                    </div>
                    <div class="surface-card border border-card rounded-xl p-3">
                      <p class="text-[11px] font-semibold text-label uppercase">Phạm vi</p>
                      <p class="text-sm font-bold text-heading mt-1">{{ detail.tenNganh || 'Tất cả ngành' }}</p>
                    </div>
                    <div class="surface-card border border-card rounded-xl p-3">
                      <p class="text-[11px] font-semibold text-label uppercase">Môn có đề / Tổng</p>
                      <p class="text-sm font-bold text-heading mt-1">
                        {{ detail.soMonCoDeThi }} / {{ detail.soLichThiTong }}
                        <span v-if="detail.soMonChuaCoDeThi > 0" class="text-xs font-normal text-amber-600 dark:text-amber-400 block mt-0.5">
                          {{ detail.soMonChuaCoDeThi }} môn chưa có đề thi
                        </span>
                      </p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2">
                    <GlassButton
                      v-if="detail.trangThai === 'nhap'"
                      variant="primary"
                      size="sm"
                      :disabled="actionBusy === detail.maKyThi"
                      @click="runAction(detail, 'publish').then(() => openDetail(detail.maKyThi))"
                    >
                      <Unlock size="13" class="mr-1" /> Mở giai đoạn thi
                    </GlassButton>
                    <GlassButton
                      v-if="detail.trangThai === 'dang_dien_ra'"
                      variant="danger"
                      size="sm"
                      :disabled="actionBusy === detail.maKyThi"
                      @click="runAction(detail, 'close').then(() => openDetail(detail.maKyThi))"
                    >
                      <Lock size="13" class="mr-1" /> Đóng giai đoạn thi
                    </GlassButton>
                    <span v-if="detail.trangThai === 'da_ket_thuc'" class="inline-flex items-center gap-1 text-xs font-bold text-muted">
                      <CheckCircle2 size="14" /> Giai đoạn thi đã kết thúc
                    </span>
                  </div>
                </div>

                <div>
                  <p class="text-sm font-bold text-heading mb-2">Danh sách môn thi & đề thi</p>
                  <div class="overflow-hidden rounded-xl border border-card">
                    <table class="w-full text-left text-sm">
                      <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
                        <tr>
                          <th scope="col" class="px-4 py-3 border-b border-card">Môn học</th>
                          <th scope="col" class="px-4 py-3 border-b border-card">Đề thi</th>
                          <th scope="col" class="px-4 py-3 border-b border-card">Hình thức</th>
                          <th scope="col" class="px-4 py-3 border-b border-card">Ngày thi dự kiến</th>
                          <th scope="col" class="px-4 py-3 border-b border-card">Ca thi</th>
                        </tr>
                      </thead>
                      <tbody class="divide-y divide-card">
                        <tr v-for="ltt in detailLichThiTongs" :key="ltt.maLichThiTong" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
                          <td class="px-4 py-3 font-medium text-heading">{{ ltt.tenMonHoc || `Môn #${ltt.maMonHoc}` }}</td>
                          <td class="px-4 py-3">
                            <span v-if="ltt.maDeKiemTra" class="inline-flex items-center gap-1.5 text-emerald-700 dark:text-emerald-300">
                              <CheckCircle2 size="14" class="shrink-0" />
                              {{ ltt.tenDeKiemTra || `Đề #${ltt.maDeKiemTra}` }}
                            </span>
                            <span v-else class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300">
                              Chưa có đề thi
                            </span>
                          </td>
                          <td class="px-4 py-3 text-muted">{{ ltt.hinhThucThi }}</td>
                          <td class="px-4 py-3 text-muted">{{ ltt.ngayThiDuKien ? new Date(ltt.ngayThiDuKien).toLocaleDateString('vi-VN') : '—' }}</td>
                          <td class="px-4 py-3 text-muted">{{ ltt.soCaThi }}</td>
                        </tr>
                        <tr v-if="detailLichThiTongs.length === 0">
                          <td colspan="5" class="px-4 py-6 text-center text-xs text-muted">Kỳ thi chưa có môn thi nào.</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                </div>
              </template>
            </div>
          </div>
        </div>
      </transition>
    </Teleport>
  </div>
</template>

<style scoped>
.modal-fade-enter-active,
.modal-fade-leave-active {
  transition: opacity 0.2s ease;
}
.modal-fade-enter-from,
.modal-fade-leave-to {
  opacity: 0;
}
</style>
