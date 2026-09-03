<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  ShieldAlert, Search, User, Building, Users, CheckCircle2, Wrench, X, AlertTriangle, Lightbulb, Clock
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import { scheduleApi } from '@/services/scheduleApi'
import academicSchedulingApi from '@/services/academicSchedulingApi'
import { useAuthStore } from '@/stores/auth'

const authStore = useAuthStore()
const userCampusId = computed(() => Number(authStore.user?.campusId || authStore.user?.maDonVi || 0))

const conflicts = ref([])
const selected = ref(null)
const searchQuery = ref('')
const filterLoai = ref('')
const filterMucDo = ref('')
const error = ref('')

const isChecking = ref(false)
const hasChecked = ref(false)
const scannedCount = ref(0)
const activeTerm = ref(null)
const hardConflictCount = ref(0)
const softWarningCount = ref(0)

function unwrapList(response) {
  const data = response?.data ?? response?.Data ?? response
  if (Array.isArray(data)) return data
  if (Array.isArray(data?.items)) return data.items
  if (Array.isArray(data?.Items)) return data.Items
  return []
}

onMounted(async () => {
  try {
    const ctxRes = await academicSchedulingApi.getContext()
    const ctx = ctxRes?.data ?? ctxRes?.Data ?? ctxRes
    activeTerm.value = ctx?.schedulableTerm || ctx?.SchedulableTerm || ctx?.currentTerm || ctx?.CurrentTerm || null
  } catch {
    // Context load optional for initial view
  }
})

// ── Computed ───────────────────────────────────────────────────
const stats = computed(() => ({
  total: conflicts.value.length,
  giangVien: conflicts.value.filter(c => c.loai === 'giang_vien').length,
  phongHoc: conflicts.value.filter(c => c.loai === 'phong_hoc').length,
  lopHoc: conflicts.value.filter(c => c.loai === 'lop_hoc').length,
  chuaXuLy: conflicts.value.filter(c => c.trangThaiXuLy === 'chua_xu_ly').length
}))

const filtered = computed(() => {
  let list = conflicts.value
  if (filterLoai.value) list = list.filter(c => c.loai === filterLoai.value)
  if (filterMucDo.value) list = list.filter(c => c.mucDo === filterMucDo.value)
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.doiTuong.toLowerCase().includes(q) || c.moTa.toLowerCase().includes(q))
  }
  return list
})

const loaiLabel = l => ({ giang_vien: 'Giảng viên', phong_hoc: 'Phòng học', lop_hoc: 'Lớp học', suc_chua: 'Sức chứa' }[l] || l)
const mucDoLabel = m => ({ critical: 'Xung đột cứng', major: 'Cảnh báo', minor: 'Nhẹ' }[m] || m)
const mucDoVariant = m => ({ critical: 'danger', major: 'warning', minor: 'info' }[m] || 'neutral')
const xuLyLabel = s => ({ chua_xu_ly: 'Chưa xử lý', dang_xu_ly: 'Đang xử lý', da_xu_ly: 'Đã xử lý' }[s] || s)
const xuLyVariant = s => ({ chua_xu_ly: 'danger', dang_xu_ly: 'warning', da_xu_ly: 'success' }[s] || 'neutral')
const loaiIcon = l => ({ giang_vien: User, phong_hoc: Building, lop_hoc: Users, suc_chua: Building }[l] || ShieldAlert)

async function performCheck() {
  isChecking.value = true
  hasChecked.value = false
  error.value = ''
  conflicts.value = []
  scannedCount.value = 0
  hardConflictCount.value = 0
  softWarningCount.value = 0

  try {
    const campusId = userCampusId.value || 1
    let termId = activeTerm.value?.maHocKy || activeTerm.value?.MaHocKy

    if (!termId) {
      const ctxRes = await academicSchedulingApi.getContext()
      const ctx = ctxRes?.data ?? ctxRes?.Data ?? ctxRes
      activeTerm.value = ctx?.schedulableTerm || ctx?.SchedulableTerm || ctx?.currentTerm || ctx?.CurrentTerm || null
      termId = activeTerm.value?.maHocKy || activeTerm.value?.MaHocKy
    }

    // Load drafts or published schedules for this campus + term
    const [draftsRes, schedulesRes] = await Promise.all([
      termId ? scheduleApi.listDrafts({ maDonVi: campusId, maHocKy: termId }).catch(() => []) : [],
      scheduleApi.list({ MaDonVi: campusId, MaHocKy: termId || undefined, PageSize: 1000 }).catch(() => [])
    ])

    const drafts = unwrapList(draftsRes)
    const schedules = unwrapList(schedulesRes).filter(r => r.trangThai !== 'da_huy')

    let allItems = []
    if (drafts.length > 0 && drafts[0].items && drafts[0].items.length > 0) {
      allItems = drafts[0].items.map(d => ({
        thuTrongTuan: d.thuTrongTuan ?? d.ThuTrongTuan,
        maCaHoc: d.maCaHoc ?? d.MaCaHoc,
        maGiaoVien: d.maGiaoVien ?? d.MaGiaoVien,
        tenGiaoVien: d.tenGiaoVien ?? d.TenGiaoVien,
        maPhong: d.maPhong ?? d.MaPhong,
        tenPhong: d.tenPhong ?? d.TenPhong,
        maLop: d.maLop ?? d.MaLop,
        tenLop: d.tenLop ?? d.TenLop,
        loi: d.loi ?? d.Loi ?? [],
        canhBao: d.canhBao ?? d.CanhBao ?? []
      }))
    } else {
      allItems = schedules
    }

    scannedCount.value = allItems.length
    conflicts.value = buildConflicts(allItems)

    hardConflictCount.value = conflicts.value.filter(c => c.mucDo === 'critical').length
    softWarningCount.value = conflicts.value.filter(c => c.mucDo !== 'critical').length
    hasChecked.value = true
  } catch (e) {
    error.value = e.message || 'Không thể kiểm tra xung đột thời khóa biểu.'
    conflicts.value = []
    hasChecked.value = true
  } finally {
    isChecking.value = false
  }
}

function slotLabel(item) {
  const days = { 2: 'Thứ Hai', 3: 'Thứ Ba', 4: 'Thứ Tư', 5: 'Thứ Năm', 6: 'Thứ Sáu', 7: 'Thứ Bảy', 8: 'Chủ Nhật' }
  return `${days[item.thuTrongTuan] ?? `Thứ ${item.thuTrongTuan}`} • Ca ${item.maCaHoc}`
}

function buildConflicts(rows) {
  const out = []
  const groups = { giang_vien: new Map(), phong_hoc: new Map(), lop_hoc: new Map() }
  const push = (loai, key, item, label) => {
    const map = groups[loai]
    if (!map.has(key)) map.set(key, [])
    map.get(key).push({ item, label })
  }
  for (const r of rows) {
    if (r.thuTrongTuan && r.maCaHoc) {
      if (r.maGiaoVien) push('giang_vien', `${r.thuTrongTuan}-${r.maCaHoc}-${r.maGiaoVien}`, r, r.tenGiaoVien || `Giáo viên #${r.maGiaoVien}`)
      if (r.maPhong) push('phong_hoc', `${r.thuTrongTuan}-${r.maCaHoc}-${r.maPhong}`, r, r.tenPhong || `Phòng ${r.maPhong}`)
      if (r.maLop) push('lop_hoc', `${r.thuTrongTuan}-${r.maCaHoc}-${r.maLop}`, r, r.tenLop || `Lớp #${r.maLop}`)
    }

    if (Array.isArray(r.loi) && r.loi.length > 0) {
      r.loi.forEach((msg, idx) => {
        out.push({
          id: `err-${r.maKhoaHoc || r.maLop}-${idx}`,
          loai: 'phong_hoc',
          mucDo: 'critical',
          doiTuong: r.tenMonHoc || r.tenLop || 'Khóa học',
          moTa: msg,
          thoiGian: r.thuTrongTuan ? slotLabel(r) : 'Chưa xếp slot',
          trangThaiXuLy: 'chua_xu_ly',
          deXuat: ''
        })
      })
    }
  }

  for (const loai of ['giang_vien', 'phong_hoc', 'lop_hoc']) {
    for (const [key, entries] of groups[loai]) {
      if (entries.length < 2) continue
      const s = entries[0].item
      out.push({
        id: `${loai}-${key}`,
        loai,
        mucDo: 'critical',
        doiTuong: entries[0].label,
        moTa: `${entries.length} buổi trùng ${loai === 'giang_vien' ? 'giảng viên' : loai === 'phong_hoc' ? 'phòng học' : 'lớp học'} tại ${slotLabel(s)}.`,
        thoiGian: slotLabel(s),
        trangThaiXuLy: 'chua_xu_ly',
        deXuat: ''
      })
    }
  }
  return out
}
</script>

<template>
  <div class="h-full flex flex-col space-y-4">
    <!-- Header -->
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <ShieldAlert class="text-amber-500" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Kiểm tra xung đột</h1>
        </div>
        <div class="flex items-center gap-3 text-xs text-(--text-muted) mt-1 ml-8 flex-wrap">
          <span v-if="userCampusId" class="font-semibold text-(--text-body)">Cơ sở: {{ userCampusId }}</span>
          <span v-if="activeTerm" class="font-semibold text-(--text-body)">Học kỳ: {{ activeTerm.tenHocKy || activeTerm.TenHocKy }}</span>
          <span v-if="hasChecked">Đã quét: <strong class="text-(--text-heading)">{{ scannedCount }}</strong> mục</span>
          <span v-if="hasChecked">Xung đột cứng: <strong class="text-(--color-danger-text)">{{ hardConflictCount }}</strong></span>
          <span v-if="hasChecked">Cảnh báo: <strong class="text-(--color-warning-text)">{{ softWarningCount }}</strong></span>
        </div>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="primary" @click="performCheck" :disabled="isChecking">
          <Wrench v-if="!isChecking" :size="15" class="mr-1" />
          <span v-if="isChecking" class="w-3 h-3 rounded-full border-2 border-white border-t-transparent animate-spin mr-2"></span>
          {{ isChecking ? 'Đang kiểm tra...' : 'Kiểm tra toàn hệ thống' }}
        </GlassButton>
      </div>
    </div>
    <div v-if="error" class="rounded-xl border border-(--color-danger-border) bg-(--color-danger-bg) p-3 text-sm text-(--color-danger-text)">
      {{ error }}
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 h-24">
      <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Tổng xung đột</p>
          <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.total }}</p>
        </div>
        <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--color-danger-bg) text-(--color-danger-text)">
          <ShieldAlert :size="20" />
        </div>
      </div>
      <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Giảng viên</p>
          <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.giangVien }}</p>
        </div>
        <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--accent-primary-soft) text-(--lg-primary)">
          <User :size="20" />
        </div>
      </div>
      <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Phòng học</p>
          <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.phongHoc }}</p>
        </div>
        <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--color-success-bg) text-(--color-success-text)">
          <Building :size="20" />
        </div>
      </div>
      <div class="surface-card border border-(--border-card) rounded-2xl p-4 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">Lớp học</p>
          <p class="text-2xl font-bold text-(--text-heading) mt-1">{{ stats.lopHoc }}</p>
        </div>
        <div class="w-10 h-10 rounded-full flex items-center justify-center bg-(--accent-violet-soft) text-(--accent-violet)">
          <Users :size="20" />
        </div>
      </div>
    </div>

    <!-- Main Content -->
    <div class="flex gap-4 flex-1 min-h-0 flex-col lg:flex-row">

      <!-- List -->
      <div class="flex-1 surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col min-w-0 overflow-hidden">
        <div class="p-3 border-b border-(--border-default) flex items-center justify-between bg-(--surface-input)">
          <div class="flex gap-2">
            <select v-model="filterLoai" class="bg-(--surface-card) border border-(--border-card) rounded-lg px-2 py-1.5 text-xs text-(--text-body) outline-none focus:border-(--lg-primary)">
              <option value="">Tất cả loại</option>
              <option value="giang_vien">Giảng viên</option>
              <option value="phong_hoc">Phòng học</option>
              <option value="lop_hoc">Lớp học</option>
            </select>
            <select v-model="filterMucDo" class="bg-(--surface-card) border border-(--border-card) rounded-lg px-2 py-1.5 text-xs text-(--text-body) outline-none focus:border-(--lg-primary)">
              <option value="">Mọi mức độ</option>
              <option value="critical">Nghiêm trọng</option>
              <option value="major">Trung bình</option>
            </select>
          </div>
          <div class="relative">
            <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="14" />
            <input v-model="searchQuery" type="text" placeholder="Tìm xung đột..." class="pl-8 pr-3 h-8 bg-(--surface-card) border border-(--border-input) rounded-lg text-xs text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus) w-48" />
          </div>
        </div>

        <div class="flex-1 overflow-auto p-4 space-y-3 bg-transparent">
          <div v-for="c in filtered" :key="c.id"
               @click="selected = c"
               class="surface-card border rounded-xl p-4 cursor-pointer transition-all hover:shadow-md relative overflow-hidden"
               :class="selected?.id === c.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary)' : 'border-(--border-card)'">

               <div class="absolute left-0 top-0 bottom-0 w-1"
                    :class="c.mucDo === 'critical' ? 'bg-red-500' : c.mucDo === 'major' ? 'bg-amber-500' : 'bg-blue-500'"></div>

               <div class="pl-2 flex justify-between items-start">
                  <div>
                    <div class="flex items-center gap-2 mb-1">
                      <component :is="loaiIcon(c.loai)" :size="14" class="text-(--text-muted)" />
                      <span class="text-xs font-mono font-bold text-(--text-heading)">{{ loaiLabel(c.loai) }}</span>
                      <GlassBadge :variant="mucDoVariant(c.mucDo)" size="sm">{{ mucDoLabel(c.mucDo) }}</GlassBadge>
                    </div>
                    <h3 class="font-bold text-(--text-heading) text-base mt-1">{{ c.doiTuong }}</h3>
                    <p class="text-sm text-(--color-danger-text) font-medium mt-1">{{ c.moTa }}</p>
                  </div>

                  <div class="text-right flex flex-col items-end gap-2">
                    <GlassBadge :variant="xuLyVariant(c.trangThaiXuLy)" size="sm">{{ xuLyLabel(c.trangThaiXuLy) }}</GlassBadge>
                    <p class="text-xs font-medium text-(--text-muted) bg-(--surface-input) px-2 py-1 rounded">{{ c.thoiGian }}</p>
                  </div>
               </div>
          </div>

          <div v-if="isChecking" class="p-4">
            <ListSkeleton :items="4" />
          </div>

          <div v-else-if="!hasChecked" class="flex flex-col items-center justify-center p-12 text-(--text-muted)">
            <Clock :size="48" class="opacity-20 mb-3" />
            <p class="font-semibold text-sm">Chưa kiểm tra xung đột</p>
            <p class="text-xs mt-1">Nhấn nút "Kiểm tra toàn hệ thống" phía trên để quét toàn bộ lịch của cơ sở và học kỳ.</p>
          </div>

          <div v-else-if="filtered.length === 0" class="flex flex-col items-center justify-center p-12 text-(--text-muted)">
            <CheckCircle2 :size="48" class="text-emerald-500 mb-3 opacity-90" />
            <p class="font-bold text-sm text-(--text-heading)">Không phát hiện xung đột</p>
            <p class="text-xs mt-1">Đã quét {{ scannedCount }} mục trong học kỳ. Toàn bộ giảng viên, phòng học và lớp học đều hợp lệ.</p>
          </div>
        </div>
      </div>

      <!-- Detail Panel -->
      <div v-if="selected" class="w-full lg:w-80 shrink-0 flex flex-col gap-3">
        <div class="surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col h-full overflow-hidden">
          <div class="p-4 border-b border-(--border-default) flex justify-between items-center bg-(--surface-input)">
            <h3 class="font-bold text-(--text-heading)">Gợi ý xử lý</h3>
            <button class="text-(--text-muted) hover:text-(--text-heading)" @click="selected = null"><X :size="16" /></button>
          </div>

          <div class="p-4 flex-1 overflow-auto space-y-5">
            <div>
              <p class="text-xs text-(--text-muted) uppercase tracking-wider font-bold mb-1">Phân tích xung đột</p>
              <div class="space-y-2 text-sm text-(--text-body)">
                <div class="flex justify-between"><span class="text-(--text-muted)">Đối tượng:</span> <span class="font-medium text-right">{{ selected.doiTuong }}</span></div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Mức độ:</span>
                  <GlassBadge :variant="mucDoVariant(selected.mucDo)" size="sm">{{ mucDoLabel(selected.mucDo) }}</GlassBadge>
                </div>
                <div class="flex justify-between"><span class="text-(--text-muted)">Thời gian:</span> <span class="font-medium">{{ selected.thoiGian }}</span></div>
              </div>
            </div>

            <div class="p-3 bg-(--color-danger-bg) border border-(--color-danger-border) rounded-xl">
              <p class="text-xs font-bold text-(--color-danger-text) flex items-center gap-1 mb-1"><AlertTriangle :size="14"/> Mô tả lỗi</p>
              <p class="text-sm text-(--color-danger-text) opacity-90">{{ selected.moTa }}</p>
            </div>

            <div v-if="selected.deXuat" class="p-3 bg-(--color-success-bg) border border-(--color-success-border) rounded-xl">
              <p class="text-xs font-bold text-(--color-success-text) flex items-center gap-1 mb-1"><Lightbulb :size="14"/> Đề xuất từ hệ thống</p>
              <p class="text-sm text-(--color-success-text) opacity-90 font-medium">{{ selected.deXuat }}</p>
            </div>

            <div v-if="!selected.deXuat" class="p-3 bg-(--surface-input) rounded-xl text-center">
              <p class="text-sm text-(--text-muted)">Hệ thống không có đề xuất tự động. Cần xử lý thủ công.</p>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-else class="w-full lg:w-80 shrink-0 hidden lg:flex flex-col items-center justify-center p-6 text-center border-2 border-dashed border-(--border-card) rounded-2xl">
        <div class="w-12 h-12 rounded-full bg-(--surface-input) flex items-center justify-center text-(--text-muted) mb-3">
          <Wrench :size="24" />
        </div>
        <p class="text-sm font-medium text-(--text-heading)">Chọn một xung đột</p>
        <p class="text-xs text-(--text-muted) mt-1">Chọn một dòng bên trái để xem nguyên nhân và gợi ý xử lý tự động.</p>
      </div>

    </div>
  </div>
</template>
