<script setup>
import { computed, onMounted, ref } from 'vue'
import {
  AlertCircle,
  CheckCircle2,
  Clock,
  History,
  KeyRound,
  Loader2,
  Lock,
  RefreshCw,
  Save,
  ShieldCheck,
  Unlock,
} from 'lucide-vue-next'
import { attendancePolicyApi } from '@/services/attendancePolicyApi'
import GlassButton from '@/components/ui/GlassButton.vue'
import { SkeletonTable, SkeletonCard } from '@/components/common/skeleton'

const loading = ref(true)
const error = ref('')
const policy = ref(null)
const history = ref([])
const saving = ref(false)
const saveError = ref('')
const saveSuccess = ref('')

const form = ref({
  quyVangToiDa: 4,
  tiLeCanhBao: 50,
  heSoVangKhongPhep: 1,
  heSoVangCoPhep: 0,
  heSoDiMuon: 0.5,
  hanGuiPhut: 15,
  hanChinhSuaPhut: 10,
  ghiChu: '',
})

function fillForm(source) {
  if (!source) return
  form.value.quyVangToiDa = source.quyVangToiDa ?? 4
  form.value.tiLeCanhBao = Number(source.tiLeCanhBao ?? 50)
  form.value.heSoVangKhongPhep = Number(source.heSoVangKhongPhep ?? 1)
  form.value.heSoVangCoPhep = Number(source.heSoVangCoPhep ?? 0)
  form.value.heSoDiMuon = Number(source.heSoDiMuon ?? 0.5)
  form.value.hanGuiPhut = source.hanGuiPhut ?? 15
  form.value.hanChinhSuaPhut = source.hanChinhSuaPhut ?? 10
  form.value.ghiChu = source.ghiChu ?? ''
}

async function loadData() {
  loading.value = true
  error.value = ''
  try {
    const [current, historyData] = await Promise.all([
      attendancePolicyApi.getCurrentPolicy(),
      attendancePolicyApi.getPolicyHistory().catch(() => []),
    ])
    policy.value = current
    fillForm(current)
    history.value = Array.isArray(historyData) ? historyData : []
  } catch (e) {
    error.value = e?.message || 'Không lấy được chính sách điểm danh.'
  } finally {
    loading.value = false
  }
}

async function savePolicy() {
  saveError.value = ''
  saveSuccess.value = ''
  if (form.value.quyVangToiDa < 0 || form.value.quyVangToiDa > 1000) {
    saveError.value = 'Quỹ vắng tối đa phải từ 0 đến 1000 buổi.'
    return
  }
  if (form.value.tiLeCanhBao < 0 || form.value.tiLeCanhBao > 100) {
    saveError.value = 'Tỷ lệ cảnh báo phải từ 0 đến 100%.'
    return
  }
  if (form.value.hanGuiPhut < 1 || form.value.hanGuiPhut > 1440) {
    saveError.value = 'Hạn gửi điểm danh phải từ 1 đến 1440 phút.'
    return
  }

  saving.value = true
  try {
    const updated = await attendancePolicyApi.updatePolicy({
      quyVangToiDa: Number(form.value.quyVangToiDa),
      tiLeCanhBao: Number(form.value.tiLeCanhBao),
      heSoVangKhongPhep: Number(form.value.heSoVangKhongPhep),
      heSoVangCoPhep: Number(form.value.heSoVangCoPhep),
      heSoDiMuon: Number(form.value.heSoDiMuon),
      hanGuiPhut: Number(form.value.hanGuiPhut),
      hanChinhSuaPhut: Number(form.value.hanChinhSuaPhut),
      ghiChu: form.value.ghiChu || null,
    })
    policy.value = updated
    saveSuccess.value = 'Đã lưu chính sách điểm danh. Hiệu lực ngay khi lưu.'
    await loadHistoryOnly()
  } catch (e) {
    saveError.value = e?.message || 'Lưu chính sách thất bại.'
  } finally {
    saving.value = false
  }
}

async function loadHistoryOnly() {
  try {
    const historyData = await attendancePolicyApi.getPolicyHistory()
    history.value = Array.isArray(historyData) ? historyData : []
  } catch {
    // giữ nguyên lịch sử cũ nếu lỗi
  }
}

const formatDateTime = (value) => {
  if (!value) return '—'
  return new Date(value).toLocaleString('vi-VN', {
    dateStyle: 'short',
    timeStyle: 'short',
  })
}

// ===== Tab yêu cầu mở khóa =====
const activeTab = ref('policy')
const unlockLoading = ref(false)
const unlockError = ref('')
const unlockRequests = ref([])
const unlockTotal = ref(0)
const unlockBusyId = ref(null)

const UNLOCK_STATUS = {
  cho_duyet: { label: 'Chờ duyệt', cls: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300' },
  da_duyet: { label: 'Đã duyệt', cls: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-300' },
  da_tu_choi: { label: 'Đã từ chối', cls: 'bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-300' },
}

async function loadUnlockRequests() {
  unlockLoading.value = true
  unlockError.value = ''
  try {
    const result = await attendancePolicyApi.getUnlockRequests({ pageIndex: 1, pageSize: 50 })
    const data = result?.items ?? result?.Items ?? result ?? []
    unlockRequests.value = Array.isArray(data) ? data : []
    unlockTotal.value = result?.totalItems ?? unlockRequests.value.length
  } catch (e) {
    unlockError.value = e?.message || 'Không lấy được danh sách yêu cầu mở khóa.'
  } finally {
    unlockLoading.value = false
  }
}

async function handleUnlockAction(request, action) {
  unlockBusyId.value = request.maYcMoKhoa
  try {
    if (action === 'approve') {
      await attendancePolicyApi.approveUnlockRequest(request.maYcMoKhoa, { ghiChu: null })
    } else {
      const reason = window.prompt('Lý do từ chối yêu cầu mở khóa:', '')
      if (reason === null) return
      if (!reason.trim()) {
        window.alert('Cần nhập lý do từ chối.')
        return
      }
      await attendancePolicyApi.rejectUnlockRequest(request.maYcMoKhoa, { lyDoTuChoi: reason.trim() })
    }
    await loadUnlockRequests()
  } catch (e) {
    window.alert(e?.message || 'Thao tác thất bại.')
  } finally {
    unlockBusyId.value = null
  }
}

const pendingUnlocks = computed(() => unlockRequests.value.filter((r) => r.trangThai === 'cho_duyet'))

function switchTab(tab) {
  activeTab.value = tab
  if (tab === 'unlock' && unlockRequests.value.length === 0) {
    loadUnlockRequests()
  }
}

onMounted(loadData)
</script>

<template>
  <section class="space-y-6">
    <header>
      <p class="text-sm font-semibold text-label">SuperAdmin</p>
      <h1 class="text-2xl font-bold text-heading">Quỹ vắng &amp; Chuyên cần</h1>
      <p class="mt-1 text-sm text-body">
        Cấu hình ngưỡng vắng, hệ số chuyên cần và hạn điểm danh. Thay đổi có hiệu lực ngay và được lưu vào lịch sử.
      </p>
    </header>

    <div v-if="error" class="rounded-xl border border-(--color-danger-text) bg-(--color-danger-bg) p-4 text-sm text-(--color-danger-text)">
      <div class="flex items-start gap-3">
        <AlertCircle class="h-5 w-5 mt-0.5 shrink-0" />
        <div>
          <h3 class="font-bold">Lỗi lấy dữ liệu</h3>
          <p class="mt-1">{{ error }}</p>
        </div>
      </div>
    </div>

    <div v-if="loading" class="space-y-4">
      <div class="rounded-2xl lg-glass-soft p-6"><SkeletonCard /></div>
      <SkeletonTable :rows="4" :columns="6" />
    </div>

    <template v-else>
      <div class="flex items-center gap-2 border-b border-(--border-default) pb-3">
        <button
          class="inline-flex items-center gap-1.5 rounded-xl px-3 py-1.5 text-sm font-bold transition-colors"
          :class="activeTab === 'policy'
            ? 'text-(--lg-primary) bg-(--lg-primary)/10'
            : 'text-label hover:text-heading'"
          @click="switchTab('policy')"
        >
          <ShieldCheck size="15" /> Chính sách
        </button>
        <button
          class="inline-flex items-center gap-1.5 rounded-xl px-3 py-1.5 text-sm font-bold transition-colors"
          :class="activeTab === 'unlock'
            ? 'text-(--lg-primary) bg-(--lg-primary)/10'
            : 'text-label hover:text-heading'"
          @click="switchTab('unlock')"
        >
          <KeyRound size="15" /> Mở khóa điểm danh
          <span
            v-if="pendingUnlocks.length > 0"
            class="inline-flex h-5 min-w-5 items-center justify-center rounded-full bg-amber-500 px-1.5 text-[11px] font-bold text-white"
          >
            {{ pendingUnlocks.length }}
          </span>
        </button>
      </div>

      <!-- ===== Tab: Chính sách ===== -->
      <div v-if="activeTab === 'policy'" class="grid grid-cols-1 xl:grid-cols-5 gap-6">
        <div class="xl:col-span-3 rounded-2xl lg-glass-soft p-6 space-y-5">
          <div class="flex items-center gap-2">
            <ShieldCheck class="h-5 w-5 text-(--lg-primary)" />
            <h2 class="text-base font-bold text-heading">Cấu hình ngưỡng &amp; hệ số</h2>
          </div>

          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-label mb-1">Quỹ vắng tối đa (buổi) *</label>
              <input v-model.number="form.quyVangToiDa" type="number" min="0" max="1000" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              <p class="mt-1 text-[11px] text-placeholder">0 = không giới hạn. Quá ngưỡng sẽ bị cảnh báo rủi ro.</p>
            </div>
            <div>
              <label class="block text-xs font-semibold text-label mb-1">Tỷ lệ cảnh báo (%) *</label>
              <input v-model.number="form.tiLeCanhBao" type="number" min="0" max="100" step="0.5" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              <p class="mt-1 text-[11px] text-placeholder">Khi quỹ vắng đạt % này, hệ thống cảnh báo.</p>
            </div>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase text-label mb-3">Hệ số vắng (trọng số cho điểm chuyên cần)</h3>
            <div class="grid grid-cols-3 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Vắng không phép *</label>
                <input v-model.number="form.heSoVangKhongPhep" type="number" min="0" max="10" step="0.25" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Vắng có phép *</label>
                <input v-model.number="form.heSoVangCoPhep" type="number" min="0" max="10" step="0.25" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Đi muộn *</label>
                <input v-model.number="form.heSoDiMuon" type="number" min="0" max="10" step="0.25" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              </div>
            </div>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase text-label mb-3">Hạn điểm danh (phút)</h3>
            <div class="grid grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Hạn gửi điểm danh *</label>
                <input v-model.number="form.hanGuiPhut" type="number" min="1" max="1440" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-label mb-1">Hạn chỉnh sửa sau khi gửi *</label>
                <input v-model.number="form.hanChinhSuaPhut" type="number" min="0" max="1440" class="h-10 w-full px-3 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
              </div>
            </div>
          </div>

          <div>
            <label class="block text-xs font-semibold text-label mb-1">Ghi chú</label>
            <textarea
              v-model="form.ghiChu"
              rows="2"
              maxlength="500"
              placeholder="Lý do thay đổi, ghi chú cho lần cập nhật này..."
              class="w-full px-3 py-2 rounded-xl bg-(--surface-card) border border-(--border-input) text-sm outline-none focus:ring-2 focus:ring-(--lg-primary) resize-none"
            ></textarea>
          </div>

          <div v-if="saveError" class="rounded-lg border border-(--color-danger-text) bg-(--color-danger-bg) p-3 text-xs text-(--color-danger-text)">
            {{ saveError }}
          </div>
          <div v-if="saveSuccess" class="rounded-lg border border-(--color-success-text) bg-(--color-success-bg) p-3 text-xs text-(--color-success-text) flex items-center gap-1.5">
            <CheckCircle2 size="14" /> {{ saveSuccess }}
          </div>

          <div class="flex items-center justify-between gap-3 pt-1">
            <p class="text-[11px] text-placeholder">
              Hiệu lực mới nhất: {{ formatDateTime(policy?.ngayHieuLuc) }}
              <span v-if="policy?.tenNguoiCapNhat"> · {{ policy.tenNguoiCapNhat }}</span>
            </p>
            <GlassButton variant="primary" size="sm" :disabled="saving" @click="savePolicy">
              <Loader2 v-if="saving" class="h-4 w-4 animate-spin mr-1" />
              <Save v-else size="14" class="mr-1" />
              {{ saving ? 'Đang lưu...' : 'Lưu chính sách' }}
            </GlassButton>
          </div>
        </div>

        <div class="xl:col-span-2 rounded-2xl lg-glass-soft p-6 space-y-4">
          <div class="flex items-center gap-2">
            <History class="h-5 w-5 text-(--lg-primary)" />
            <h2 class="text-base font-bold text-heading">Lịch sử thay đổi</h2>
          </div>

          <p v-if="history.length === 0" class="text-sm text-muted">Chưa có lịch sử thay đổi nào.</p>

          <div v-else class="space-y-3 max-h-[26rem] overflow-y-auto pr-1">
            <div
              v-for="item in history"
              :key="item.maQuyDinh"
              class="rounded-xl border border-(--border-card) bg-(--surface-card) p-3"
            >
              <div class="flex items-center justify-between gap-2">
                <span class="text-xs font-bold text-heading">
                  {{ item.maQuyDinh === policy?.maQuyDinh ? 'Bản hiện hành' : 'Bản trước' }}
                </span>
                <span class="inline-flex items-center gap-1 text-[11px] text-placeholder">
                  <Clock size="11" /> {{ formatDateTime(item.ngayHieuLuc) }}
                </span>
              </div>
              <dl class="mt-2 grid grid-cols-2 gap-x-3 gap-y-1 text-[11px]">
                <div class="flex justify-between"><dt class="text-label">Quỹ vắng</dt><dd class="font-bold text-heading">{{ item.quyVangToiDa }} buổi</dd></div>
                <div class="flex justify-between"><dt class="text-label">Cảnh báo</dt><dd class="font-bold text-heading">{{ item.tiLeCanhBao }}%</dd></div>
                <div class="flex justify-between"><dt class="text-label">Vắng KP</dt><dd class="font-bold text-heading">{{ item.heSoVangKhongPhep }}</dd></div>
                <div class="flex justify-between"><dt class="text-label">Vắng CP</dt><dd class="font-bold text-heading">{{ item.heSoVangCoPhep }}</dd></div>
                <div class="flex justify-between"><dt class="text-label">Đi muộn</dt><dd class="font-bold text-heading">{{ item.heSoDiMuon }}</dd></div>
                <div class="flex justify-between"><dt class="text-label">Hạn gửi</dt><dd class="font-bold text-heading">{{ item.hanGuiPhut }} phút</dd></div>
              </dl>
              <p v-if="item.ghiChu" class="mt-2 text-[11px] text-muted italic">{{ item.ghiChu }}</p>
              <p v-if="item.tenNguoiCapNhat || item.tenNguoiTao" class="mt-1 text-[11px] text-placeholder">
                Bởi: {{ item.tenNguoiCapNhat || item.tenNguoiTao || '—' }}
              </p>
            </div>
          </div>
        </div>
      </div>

      <!-- ===== Tab: Mở khóa điểm danh ===== -->
      <div v-if="activeTab === 'unlock'" class="rounded-2xl lg-glass-soft p-6 space-y-4">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <Unlock class="h-5 w-5 text-(--lg-primary)" />
            <h2 class="text-base font-bold text-heading">Yêu cầu mở khóa điểm danh</h2>
          </div>
          <button
            class="inline-flex items-center gap-1.5 rounded-xl border border-(--border-default) surface-card px-3 py-1.5 text-xs font-bold text-heading hover:bg-(--surface-input) transition-colors"
            @click="loadUnlockRequests"
          >
            <Loader2 v-if="unlockLoading" class="h-3.5 w-3.5 animate-spin text-muted" />
            <RefreshCw v-else class="h-3.5 w-3.5 text-muted" />
            Tải lại
          </button>
        </div>

        <p v-if="unlockError" class="rounded-lg border border-(--color-danger-text) bg-(--color-danger-bg) p-3 text-xs text-(--color-danger-text)">
          {{ unlockError }}
        </p>

        <div v-if="unlockLoading" class="py-4"><SkeletonTable :rows="4" :columns="6" /></div>

        <div v-else-if="unlockRequests.length === 0" class="py-8 text-center">
          <Lock class="mx-auto h-8 w-8 text-muted" />
          <p class="mt-2 text-sm font-semibold text-heading">Không có yêu cầu mở khóa nào</p>
          <p class="mt-1 text-xs text-muted">Giáo viên gửi yêu cầu khi cần sửa điểm danh đã khóa.</p>
        </div>

        <div v-else class="overflow-x-auto rounded-xl border border-(--border-card)">
          <table class="w-full text-left text-sm min-w-[720px]">
            <thead class="bg-slate-50 dark:bg-slate-800/50 text-xs font-bold uppercase text-muted">
              <tr>
                <th class="px-4 py-3 border-b border-(--border-card)">Lớp / Môn</th>
                <th class="px-4 py-3 border-b border-(--border-card)">Buổi học</th>
                <th class="px-4 py-3 border-b border-(--border-card)">Giáo viên yêu cầu</th>
                <th class="px-4 py-3 border-b border-(--border-card)">Lý do</th>
                <th class="px-4 py-3 border-b border-(--border-card)">Trạng thái</th>
                <th class="px-4 py-3 border-b border-(--border-card) text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-(--border-card)">
              <tr v-for="request in unlockRequests" :key="request.maYcMoKhoa" class="hover:bg-slate-50/50 dark:hover:bg-slate-800/20">
                <td class="px-4 py-3">
                  <p class="font-medium text-heading">{{ request.tenLop }}</p>
                  <p class="text-xs text-muted">{{ request.tenMonHoc }}</p>
                </td>
                <td class="px-4 py-3 text-muted">
                  <p>{{ new Date(request.ngayHoc + 'T00:00:00').toLocaleDateString('vi-VN') }}</p>
                  <p class="text-xs">{{ request.tenCa }} · {{ request.tenPhong }}</p>
                </td>
                <td class="px-4 py-3 text-muted">{{ request.tenNguoiYeuCau }}</td>
                <td class="px-4 py-3 text-muted max-w-[220px] truncate" :title="request.lyDo">{{ request.lyDo }}</td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center rounded-full px-2 py-0.5 text-xs font-bold" :class="(UNLOCK_STATUS[request.trangThai] || { cls: 'bg-slate-100 text-slate-600' }).cls">
                    {{ (UNLOCK_STATUS[request.trangThai] || { label: request.trangThai }).label }}
                  </span>
                </td>
                <td class="px-4 py-3">
                  <div class="flex items-center justify-end gap-1.5">
                    <template v-if="request.trangThai === 'cho_duyet'">
                      <button
                        class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-bold text-emerald-700 dark:text-emerald-300 hover:bg-emerald-50 dark:hover:bg-emerald-900/30 transition-colors disabled:opacity-50"
                        :disabled="unlockBusyId === request.maYcMoKhoa"
                        @click="handleUnlockAction(request, 'approve')"
                      >
                        <CheckCircle2 size="13" /> Duyệt
                      </button>
                      <button
                        class="inline-flex items-center gap-1 rounded-lg px-2.5 py-1.5 text-xs font-bold text-rose-700 dark:text-rose-300 hover:bg-rose-50 dark:hover:bg-rose-900/30 transition-colors disabled:opacity-50"
                        :disabled="unlockBusyId === request.maYcMoKhoa"
                        @click="handleUnlockAction(request, 'reject')"
                      >
                        <Lock size="13" /> Từ chối
                      </button>
                    </template>
                    <span v-else class="text-xs text-placeholder">{{ request.ghiChu || request.lyDoTuChoi || '—' }}</span>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </section>
</template>
