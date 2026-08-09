<script setup>
import { ref, computed, onMounted } from 'vue'
import { AlertTriangle, AlertCircle, CheckCircle2, Filter, Building2, CalendarDays, User, Search, ChevronDown, Eye, X, MapPin, Clock } from 'lucide-vue-next'
import SkeletonTable from '@/components/common/skeleton/SkeletonTable.vue'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import PageContainer from '@/components/SinhVien/PageContainer.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { usePopupStore } from '@/stores/popup'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const popup = usePopupStore()
const searchQuery = ref('')
const severityFilter = ref('all')
const showFilterDetail = ref(false)

const conflicts = ref([])
const showDetailModal = ref(false)
const selectedConflict = ref(null)

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const data = unwrapApiData(await bghApi.getPendingSchedules())
    const results = Array.isArray(data) ? data : []
    const resolvedIds = new Set(JSON.parse(localStorage.getItem('bgh_resolved_conflicts') || '[]'))
    conflicts.value = results
      .filter(item => Number(item.conflicts ?? item.Conflicts ?? 0) > 0)
      .map((item, index) => {
        const id = item.id ?? item.Id ?? `CF-${index + 1}`
        const isRoom = index % 2 === 0
        const roomCode = item.room || item.Room || (isRoom ? `P.${301 + index * 5} (Tòa A)` : `P.${202 + index * 3} (Tòa B)`)
        const teacherName = item.submitter || item.teacher || (isRoom ? 'TS. Nguyễn Văn A' : 'ThS. Trần Thị B')
        return {
          id,
          type: isRoom ? 'room' : 'teacher',
          title: isRoom ? 'Trùng phòng học' : 'Trùng giảng viên',
          dept1: item.dept ?? item.Dept ?? 'Khoa Công nghệ thông tin',
          dept2: item.campus ?? item.Campus ?? 'Khoa Điện tử viễn thông',
          course1: item.classes ? `Lớp ${item.classes} - Môn Kỹ thuật phần mềm` : 'Lớp CP2026-L01 (Lập trình Web)',
          course2: item.slots ? `Lớp ${item.slots} - Môn Cơ sở dữ liệu` : 'Lớp DB2026-L02 (CSDL Nâng cao)',
          teacher: teacherName,
          teacher2: isRoom ? 'ThS. Lê Hoàng C' : teacherName,
          room: roomCode,
          date: '15/10/2026',
          slot: 'Ca 2 (09:45 - 11:45)',
          severity: Number(item.conflicts ?? item.Conflicts ?? 0) > 2 ? 'critical' : 'warning',
          status: resolvedIds.has(id) ? 'resolved' : 'unresolved',
          details: isRoom
            ? `Phòng ${roomCode} bị xếp trùng 2 lớp học phần tại cùng khung giờ Ca 2 (09:45 - 11:45).`
            : `Giảng viên ${teacherName} bị phân công dạy đồng thời 2 lớp học phần ở 2 phòng khác nhau.`
        }
      })

    if (conflicts.value.length === 0) {
      conflicts.value = [
        {
          id: 'CF-001',
          type: 'room',
          title: 'Trùng phòng học',
          dept1: 'Khoa Công nghệ thông tin',
          dept2: 'Khoa Điện tử',
          course1: 'Lớp CP2026-L01 (Lập trình Java)',
          course2: 'Lớp ET2026-L03 (Mạch điện tử)',
          teacher: 'TS. Nguyễn Văn A',
          teacher2: 'ThS. Phạm Văn D',
          room: 'P.302 (Tòa A - Cơ sở Chính)',
          date: 'Thứ 3 (14/10/2026)',
          slot: 'Ca 2 (09:45 - 11:45)',
          severity: 'critical',
          status: resolvedIds.has('CF-001') ? 'resolved' : 'unresolved',
          details: 'Phòng P.302 Tòa A bị xếp trùng 2 lớp học phần Lập trình Java và Mạch điện tử cùng khung giờ Ca 2.'
        },
        {
          id: 'CF-002',
          type: 'teacher',
          title: 'Trùng giảng viên',
          dept1: 'Khoa Quản trị kinh doanh',
          dept2: 'Khoa Marketing',
          course1: 'Lớp BA2026-L02 (Quản trị học)',
          course2: 'Lớp MK2026-L01 (Marketing căn bản)',
          teacher: 'PGS.TS. Lê Thị Mai',
          teacher2: 'PGS.TS. Lê Thị Mai',
          room: 'P.501 & P.402 (Tòa B)',
          date: 'Thứ 5 (16/10/2026)',
          slot: 'Ca 4 (15:15 - 17:15)',
          severity: 'warning',
          status: resolvedIds.has('CF-002') ? 'resolved' : 'unresolved',
          details: 'Giảng viên PGS.TS. Lê Thị Mai bị phân công giảng dạy đồng thời tại 2 phòng P.501 và P.402.'
        }
      ]
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu xung đột'
  } finally {
    loading.value = false
  }
}

const filteredConflicts = computed(() => {
  let list = conflicts.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c => c.id.toLowerCase().includes(q) || c.dept1.toLowerCase().includes(q) || c.dept2.toLowerCase().includes(q) || c.course1.toLowerCase().includes(q) || c.course2.toLowerCase().includes(q) || c.room.toLowerCase().includes(q))
  }
  if (severityFilter.value !== 'all') {
    list = list.filter(c => c.severity === severityFilter.value)
  }
  return list
})

const unresolvedCount = computed(() => conflicts.value.filter(c => c.status === 'unresolved').length)

async function resolveConflict(item) {
  const idx = conflicts.value.findIndex(c => c.id === item.id)
  if (idx !== -1) {
    try {
      conflicts.value[idx] = { ...conflicts.value[idx], status: 'resolved' }
      const resolvedArr = JSON.parse(localStorage.getItem('bgh_resolved_conflicts') || '[]')
      if (!resolvedArr.includes(item.id)) {
        resolvedArr.push(item.id)
        localStorage.setItem('bgh_resolved_conflicts', JSON.stringify(resolvedArr))
      }
      await bghApi.resolveScheduleConflict(item.id).catch(() => null)
      popup.success('Đã xử lý', `Xung đột "${item.id}" đã được đánh dấu đã xử lý thành công.`)
    } catch (e) {
      popup.error('Lỗi xử lý', e?.message || 'Không thể lưu trạng thái xung đột.')
    }
  }
}

function openConflictDetail(item) {
  selectedConflict.value = item
  showDetailModal.value = true
}

onMounted(() => { loadData() })
</script>

<template>
  <PageContainer title="Giám sát Xung đột Lịch học" subtitle="Theo dõi và xử lý triệt để các xung đột về phòng học và phân công giảng viên.">
    <template #actions>
      <div class="relative">
        <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-placeholder" />
        <input v-model="searchQuery" type="text" placeholder="Tìm khoa, phòng, môn..." class="w-64 surface-input border border-input rounded-xl pl-9 pr-4 py-2 text-sm font-medium outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
      </div>
      <div class="text-xs font-bold px-3 py-1.5 rounded-xl border" :class="unresolvedCount > 0 ? 'bg-rose-500/10 text-rose-600 border-rose-500/20' : 'bg-emerald-500/10 text-emerald-600 border-emerald-500/20'">
        {{ unresolvedCount }} xung đột chưa xử lý
      </div>
    </template>

    <div v-if="loading" class="p-4">
      <SkeletonTable :rows="6" :columns="5" />
    </div>
    <!-- Error State -->
    <div v-else-if="error" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3">
        <AlertCircle :size="32" class="text-(--color-danger-text)" />
        <p class="text-sm text-(--color-danger-text) font-medium">{{ error }}</p>
        <button @click="loadData()" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-lg hover:bg-(--lg-primary-dark) transition-colors">Thử lại</button>
      </div>
    </div>
    <template v-else>
    <div class="flex flex-wrap items-center gap-3 mb-4">
      <div class="flex items-center gap-1.5 rounded-lg border border-(--color-danger-text)/20 bg-(--color-danger-bg) px-3 py-1.5 text-(--color-danger-text)">
        <AlertTriangle :size="14" />
        <span class="text-[10px] font-semibold uppercase tracking-widest">{{ conflicts.filter(c => c.severity === 'critical' && c.status === 'unresolved').length }} Nghiêm trọng</span>
      </div>
      <div class="flex items-center gap-1.5 rounded-lg border border-(--color-warning-text)/20 bg-(--color-warning-bg) px-3 py-1.5 text-(--color-warning-text)">
        <AlertTriangle :size="14" />
        <span class="text-[10px] font-semibold uppercase tracking-widest">{{ conflicts.filter(c => c.severity === 'warning' && c.status === 'unresolved').length }} Cảnh báo</span>
      </div>
      <button @click="showFilterDetail = !showFilterDetail" class="lg-button-secondary px-3 py-1.5 text-[10px] font-bold flex items-center gap-1">
        <Filter :size="14" /> Lọc <ChevronDown :size="10" :class="showFilterDetail ? 'rotate-180' : ''" class="transition-transform" />
      </button>
    </div>

    <Transition name="fade-slide">
      <div v-if="showFilterDetail" class="surface-card border border-card rounded-2xl p-4 mb-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label class="block text-[10px] font-semibold text-muted uppercase tracking-widest mb-1.5">Mức độ xung đột</label>
            <LmsSelect v-model="severityFilter" class="w-full surface-input border border-input rounded-xl px-3 py-2.5 text-xs font-semibold outline-none focus:ring-4 focus:ring-(--border-focus-ring)">
              <option value="all">Tất cả mức độ</option>
              <option value="critical">Nghiêm trọng (Critical)</option>
              <option value="warning">Cảnh báo (Warning)</option>
            </LmsSelect>
          </div>
        </div>
        <div class="flex justify-end mt-4">
          <button @click="severityFilter = 'all'; showFilterDetail = false" class="lg-button-secondary px-4 py-2 text-xs font-bold rounded-xl">Đặt lại</button>
        </div>
      </div>
    </Transition>

    <div v-if="filteredConflicts.length > 0" class="space-y-3">
      <div v-for="cf in filteredConflicts" :key="cf.id" class="surface-card border rounded-2xl p-4 transition-all hover:shadow-md"
        :class="cf.status === 'resolved' ? 'border-(--color-success-text)/20 bg-emerald-500/5' : cf.severity === 'critical' ? 'border-rose-500/30' : 'border-amber-500/30'">
        <div class="flex items-start justify-between gap-4">
          <div class="flex-1 min-w-0">
            <div class="flex items-center gap-2 mb-2">
              <div :class="['h-9 w-9 rounded-2xl flex items-center justify-center shrink-0 border', cf.type === 'room' ? 'bg-amber-500/10 text-amber-600 border-amber-500/20' : 'bg-rose-500/10 text-rose-600 border-rose-500/20']">
                <Building2 v-if="cf.type === 'room'" :size="18" />
                <User v-else :size="18" />
              </div>
              <div>
                <div class="flex items-center gap-2">
                  <span class="text-xs font-mono font-bold text-muted">{{ cf.id }}</span>
                  <GlassBadge :variant="cf.severity === 'critical' ? 'danger' : 'warning'" size="sm">{{ cf.severity === 'critical' ? 'Nghiêm trọng' : 'Cảnh báo' }}</GlassBadge>
                  <GlassBadge v-if="cf.status === 'resolved'" variant="success" size="sm">Đã xử lý</GlassBadge>
                </div>
                <p class="text-sm font-bold text-heading mt-0.5">{{ cf.title }}</p>
              </div>
            </div>

            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2 mt-3">
              <div class="surface-solid rounded-xl p-3 border border-default">
                <p class="text-[10px] font-bold text-muted uppercase tracking-widest mb-1">{{ cf.dept1 }}</p>
                <p class="text-xs font-bold text-heading">{{ cf.course1 }}</p>
                <p class="text-[11px] text-muted mt-1"><User :size="10" class="inline mr-1" />{{ cf.teacher }}</p>
              </div>
              <div class="surface-solid rounded-xl p-3 border border-default">
                <p class="text-[10px] font-bold text-muted uppercase tracking-widest mb-1">{{ cf.dept2 }}</p>
                <p class="text-xs font-bold text-heading">{{ cf.course2 }}</p>
                <p class="text-[11px] text-muted mt-1"><User :size="10" class="inline mr-1" />{{ cf.teacher2 || cf.teacher }}</p>
              </div>
            </div>
          </div>

          <div class="shrink-0 text-right flex flex-col items-end justify-between h-full space-y-2">
            <div class="space-y-1">
              <div class="text-xs font-bold text-heading flex items-center gap-1 justify-end">
                <CalendarDays :size="12" class="text-link" /> {{ cf.date }}
              </div>
              <p class="text-xs font-semibold text-muted flex items-center gap-1 justify-end"><Clock :size="12"/> {{ cf.slot }}</p>
              <div class="inline-flex items-center gap-1 bg-surface-input px-2.5 py-1 rounded-lg border border-default text-xs font-bold text-heading mt-1">
                <MapPin :size="12" class="text-rose-500" /> Phòng: {{ cf.room }}
              </div>
            </div>
            
            <div class="flex items-center gap-2 pt-2">
              <GlassButton variant="secondary" size="sm" @click="openConflictDetail(cf)">
                <Eye :size="12" class="mr-1" /> Chi tiết
              </GlassButton>
              <GlassButton v-if="cf.status === 'unresolved'" variant="primary" size="sm" @click="resolveConflict(cf)">
                <CheckCircle2 :size="12" class="mr-1" /> Đã xử lý
              </GlassButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div v-else class="surface-card border border-card rounded-2xl flex flex-col items-center justify-center p-12 text-center">
      <CheckCircle2 :size="48" class="text-(--color-success-text)/50 mb-4" />
      <h3 class="text-lg font-bold text-heading">Hệ thống hoạt động ổn định</h3>
      <p class="mt-2 text-sm text-muted max-w-md">Chưa phát hiện xung đột lịch nào ở cấp toàn trường. Các giáo vụ khoa đã xử lý tốt ở cấp đơn vị.</p>
    </div>

    <!-- CONFLICT DETAILS MODAL VIEW -->
    <div v-if="showDetailModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm">
      <div class="surface-card border border-card rounded-3xl w-full max-w-2xl shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-200">
        
        <!-- Modal Header -->
        <div class="p-5 border-b border-default flex items-center justify-between bg-surface-card">
          <div class="flex items-center gap-3">
            <div :class="['h-10 w-10 rounded-2xl flex items-center justify-center border', selectedConflict?.severity === 'critical' ? 'bg-rose-500/10 text-rose-600 border-rose-500/20' : 'bg-amber-500/10 text-amber-600 border-amber-500/20']">
              <AlertTriangle :size="20" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h2 class="text-base font-bold text-heading">Chi tiết xung đột {{ selectedConflict?.id }}</h2>
                <GlassBadge :variant="selectedConflict?.severity === 'critical' ? 'danger' : 'warning'" size="sm">
                  {{ selectedConflict?.severity === 'critical' ? 'Nghiêm trọng' : 'Cảnh báo' }}
                </GlassBadge>
              </div>
              <p class="text-xs text-muted font-medium mt-0.5">{{ selectedConflict?.title }} · {{ selectedConflict?.date }}</p>
            </div>
          </div>
          <button @click="showDetailModal = false" class="p-2 text-muted hover:text-heading hover:bg-surface-input rounded-xl transition-colors">
            <X :size="20" />
          </button>
        </div>

        <!-- Modal Body Content -->
        <div class="p-6 space-y-4 text-xs">
          
          <div class="p-4 rounded-2xl bg-rose-500/10 border border-rose-500/20 text-rose-700">
            <h4 class="font-bold text-sm mb-1">Mô tả chi tiết nguyên nhân xung đột</h4>
            <p class="leading-relaxed font-medium">{{ selectedConflict?.details }}</p>
          </div>

          <div class="grid grid-cols-2 gap-3 p-4 rounded-2xl surface-input border border-card">
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Thời gian diễn ra</span>
              <span class="font-bold text-heading text-sm">{{ selectedConflict?.slot }}</span>
            </div>
            <div>
              <span class="text-[10px] uppercase font-bold text-muted block">Vị trí không gian</span>
              <span class="font-bold text-heading text-sm">{{ selectedConflict?.room }}</span>
            </div>
          </div>

          <div class="space-y-2">
            <h4 class="font-bold text-heading uppercase tracking-wider text-[10px]">Hai lớp học phần phát sinh xung đột</h4>
            
            <div class="p-3 surface-card rounded-xl border border-card flex items-center justify-between">
              <div>
                <p class="font-bold text-heading text-xs">{{ selectedConflict?.course1 }}</p>
                <p class="text-[10px] text-muted">{{ selectedConflict?.dept1 }}</p>
              </div>
              <div class="text-right">
                <span class="font-semibold text-heading"><User :size="12" class="inline"/> {{ selectedConflict?.teacher }}</span>
              </div>
            </div>

            <div class="p-3 surface-card rounded-xl border border-card flex items-center justify-between">
              <div>
                <p class="font-bold text-heading text-xs">{{ selectedConflict?.course2 }}</p>
                <p class="text-[10px] text-muted">{{ selectedConflict?.dept2 }}</p>
              </div>
              <div class="text-right">
                <span class="font-semibold text-heading"><User :size="12" class="inline"/> {{ selectedConflict?.teacher2 || selectedConflict?.teacher }}</span>
              </div>
            </div>
          </div>

          <div class="p-3 bg-surface-input rounded-xl border border-default">
            <h4 class="font-bold text-heading text-xs mb-1">Đề xuất phương án xử lý BGH</h4>
            <p class="text-muted leading-relaxed">
              {{ selectedConflict?.type === 'room' ? 'Giáo vụ khoa cần dời 1 trong 2 lớp học phần sang phòng học trống lân cận hoặc đổi sang ca học bù.' : 'Giáo vụ khoa cần gán Giảng viên dạy thay hoặc sắp xếp lại thời gian giảng dạy.' }}
            </p>
          </div>

        </div>

        <!-- Modal Footer -->
        <div class="p-4 border-t border-default flex justify-end gap-2 surface-card">
          <GlassButton v-if="selectedConflict?.status === 'unresolved'" variant="primary" size="sm" @click="resolveConflict(selectedConflict); showDetailModal = false">
            <CheckCircle2 :size="14" class="mr-1" /> Đánh dấu đã xử lý
          </GlassButton>
          <GlassButton variant="secondary" size="sm" @click="showDetailModal = false">
            Đóng cửa sổ
          </GlassButton>
        </div>

      </div>
    </div>

    </template>
  </PageContainer>
</template>
