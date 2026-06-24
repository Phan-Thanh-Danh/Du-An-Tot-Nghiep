<script setup>
/**
 * DisciplineView.vue - Super Admin
 * Quản lý kỷ luật sinh viên: danh sách vi phạm, tạo quyết định, lọc theo mức độ/kỳ, xem chi tiết.
 */
import { ref, computed } from 'vue'
import {
  AlertOctagon,
  Plus,
  Search,
  Filter,
  RotateCcw,
  Eye,
  X,
  Save,
  CheckCircle,
  AlertTriangle,
  Info,
  User,
  Calendar,
  FileText,
  ShieldAlert,
  Ban,
  Clock
} from 'lucide-vue-next'

const severityLevels = ref([
  { id: 'warning', name: 'Cảnh cáo', color: 'amber' },
  { id: 'probation', name: 'Quản chế', color: 'orange' },
  { id: 'suspension', name: 'Đình chỉ tạm thời', color: 'rose' },
  { id: 'expulsion', name: 'Buộc thôi học', color: 'red' }
])

const violationTypes = ref([
  'Gian lận thi cử',
  'Vi phạm quy chế học vụ',
  'Vi phạm nội quy ký túc xá',
  'Hành vi không đúng chuẩn mực',
  'Vắng quá quy định không phép',
  'Khác'
])

const disciplines = ref([
  {
    id: 1, studentId: 'HE170300', studentName: 'Vũ Quốc Huy', campus: 'Cơ sở Hòa Lạc', class: 'SE1701',
    violation: 'Gian lận thi cử', description: 'Sử dụng điện thoại trong phòng thi cuối kỳ môn PRN211. Camera giám sát ghi nhận bằng chứng rõ ràng. Đây là lần vi phạm đầu tiên.',
    severity: 'warning', status: 'Đã ban hành', semester: 'Spring 2026',
    decisionDate: '2026-06-08', decisionBy: 'Hội đồng Kỷ luật', effectiveDate: '2026-06-08', endDate: null,
    penalty: 'Cảnh cáo toàn trường + Hủy kết quả thi môn PRN211'
  },
  {
    id: 2, studentId: 'HE170155', studentName: 'Hoàng Đức Hải', campus: 'Cơ sở TP.HCM', class: 'SE1702',
    violation: 'Gian lận thi cử', description: 'Trao đổi bài trong giờ thi giữa kỳ môn SWD392. Giám thị phát hiện 2 bài thi giống nhau > 90%. Sinh viên đã có 1 lần cảnh cáo trước đó.',
    severity: 'probation', status: 'Đã ban hành', semester: 'Spring 2026',
    decisionDate: '2026-06-05', decisionBy: 'Hội đồng Kỷ luật', effectiveDate: '2026-06-05', endDate: '2026-12-05',
    penalty: 'Quản chế 6 tháng + Hủy kết quả thi môn SWD392 + Không được đăng ký quá 15 tín chỉ/kỳ'
  },
  {
    id: 3, studentId: 'HE170078', studentName: 'Vũ Thị Hồng', campus: 'Cơ sở Hòa Lạc', class: 'SE1703',
    violation: 'Vi phạm nội quy ký túc xá', description: 'Vi phạm nội quy KTX lần 3: sử dụng thiết bị điện không được phép (nồi cơm điện) trong phòng, có nguy cơ gây cháy nổ.',
    severity: 'suspension', status: 'Chờ duyệt', semester: 'Spring 2026',
    decisionDate: null, decisionBy: null, effectiveDate: null, endDate: null,
    penalty: 'Dự kiến: Đình chỉ KTX 1 tháng + Phạt tiền theo quy định'
  },
  {
    id: 4, studentId: 'HE170088', studentName: 'Lý Minh Quang', campus: 'Cơ sở TP.HCM', class: 'IA1701',
    violation: 'Vắng quá quy định không phép', description: 'Vắng mặt không phép 12/30 buổi môn PRN221 và 8/20 buổi môn SWE201c. Đã gửi cảnh báo 2 lần qua email nhưng SV không phản hồi.',
    severity: 'warning', status: 'Đã ban hành', semester: 'Spring 2026',
    decisionDate: '2026-06-01', decisionBy: 'Phòng Đào tạo', effectiveDate: '2026-06-01', endDate: null,
    penalty: 'Cảnh cáo học vụ + Cấm thi 2 môn vi phạm + Thông báo phụ huynh'
  },
  {
    id: 5, studentId: 'HE170400', studentName: 'Trần Quốc Bảo', campus: 'Cơ sở Đà Nẵng', class: 'SE1704',
    violation: 'Hành vi không đúng chuẩn mực', description: 'Có hành vi xúc phạm giảng viên trong giờ học trực tuyến. Nhiều sinh viên trong lớp xác nhận sự việc. SV đã xin lỗi nhưng hội đồng cần xem xét mức độ.',
    severity: 'warning', status: 'Chờ duyệt', semester: 'Spring 2026',
    decisionDate: null, decisionBy: null, effectiveDate: null, endDate: null,
    penalty: 'Dự kiến: Cảnh cáo toàn trường + Viết bản kiểm điểm'
  }
])

// --- Filter ---
const searchQuery = ref('')
const filterSeverity = ref('all')
const filterStatus = ref('all')

const filteredDisciplines = computed(() => {
  return disciplines.value.filter(d => {
    const matchSearch = searchQuery.value === '' || d.studentName.toLowerCase().includes(searchQuery.value.toLowerCase()) || d.studentId.toLowerCase().includes(searchQuery.value.toLowerCase()) || d.violation.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchSeverity = filterSeverity.value === 'all' || d.severity === filterSeverity.value
    const matchStatus = filterStatus.value === 'all' || d.status === filterStatus.value
    return matchSearch && matchSeverity && matchStatus
  })
})

const resetFilters = () => { searchQuery.value = ''; filterSeverity.value = 'all'; filterStatus.value = 'all' }

// --- KPI ---
const totalDisciplines = computed(() => disciplines.value.length)
const issuedCount = computed(() => disciplines.value.filter(d => d.status === 'Đã ban hành').length)
const pendingCount = computed(() => disciplines.value.filter(d => d.status === 'Chờ duyệt').length)
const severeCount = computed(() => disciplines.value.filter(d => d.severity === 'suspension' || d.severity === 'expulsion').length)

// --- Modal ---
const isModalOpen = ref(false)
const currentDiscipline = ref({ studentId: '', studentName: '', campus: 'Cơ sở Hòa Lạc', class: '', violation: 'Gian lận thi cử', description: '', severity: 'warning', penalty: '' })

// --- Detail ---
const isDetailOpen = ref(false)
const selectedDiscipline = ref(null)

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const triggerToast = (msg, type = 'success') => { toastMessage.value = msg; toastType.value = type; showToast.value = true; setTimeout(() => { showToast.value = false }, 4000) }

const openCreateModal = () => {
  currentDiscipline.value = { studentId: '', studentName: '', campus: 'Cơ sở Hòa Lạc', class: '', violation: 'Gian lận thi cử', description: '', severity: 'warning', penalty: '' }
  isModalOpen.value = true
}

const handleSave = () => {
  if (!currentDiscipline.value.studentName.trim() || !currentDiscipline.value.description.trim()) { triggerToast('Vui lòng điền đầy đủ thông tin.', 'error'); return }
  const newId = disciplines.value.length ? Math.max(...disciplines.value.map(d => d.id)) + 1 : 1
  disciplines.value.unshift({
    ...currentDiscipline.value, id: newId, status: 'Chờ duyệt', semester: 'Spring 2026',
    decisionDate: null, decisionBy: null, effectiveDate: null, endDate: null
  })
  triggerToast('Đã tạo quyết định kỷ luật mới (Chờ duyệt).')
  isModalOpen.value = false
}

const openDetail = (d) => { selectedDiscipline.value = d; isDetailOpen.value = true }

const handleIssue = (d) => {
  d.status = 'Đã ban hành'
  d.decisionDate = new Date().toISOString().slice(0, 10)
  d.effectiveDate = new Date().toISOString().slice(0, 10)
  d.decisionBy = 'Super Admin'
  triggerToast(`Quyết định kỷ luật SV ${d.studentName} đã ban hành.`)
}

const getSeverityName = (sev) => severityLevels.value.find(s => s.id === sev)?.name || sev
const getSeverityColor = (sev) => {
  const colors = {
    warning: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/20',
    probation: 'bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-200 dark:border-orange-500/20',
    suspension: 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border-rose-200 dark:border-rose-500/20',
    expulsion: 'bg-red-600/10 text-red-600 dark:text-red-400 border-red-300 dark:border-red-500/20'
  }
  return colors[sev] || colors.warning
}
const getSeverityBadge = (sev) => {
  const map = { warning: 'lg-badge-warning', probation: 'lg-badge-warning', suspension: 'lg-badge-danger', expulsion: 'lg-badge-danger' }
  return map[sev] || 'lg-badge-warning'
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
            <AlertOctagon class="w-8 h-8 text-rose-500" />
            Kỷ Luật Sinh Viên
          </h1>
          <p class="text-sm text-muted mt-1">Quản lý các quyết định kỷ luật, vi phạm và hình thức xử lý sinh viên.</p>
        </div>
        <button @click="openCreateModal" class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2"><Plus class="w-4.5 h-4.5" /> Tạo Quyết Định</button>
      </div>

      <!-- KPI -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500"><AlertOctagon class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng quyết định</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ totalDisciplines }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500"><CheckCircle class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã ban hành</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ issuedCount }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500"><Clock class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Chờ duyệt</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ pendingCount }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-rose-500/10 flex items-center justify-center text-rose-500"><Ban class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Đình chỉ / Thôi học</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ severeCount }}</div></div>
        </div>
      </div>

      <!-- Filter -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2"><Filter class="w-4.5 h-4.5 text-primary" /><h3 class="font-bold text-heading text-sm">Bộ lọc kỷ luật</h3></div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline"><RotateCcw class="w-3.5 h-3.5" /> Xóa lọc</button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="relative"><Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" /><input v-model="searchQuery" type="text" placeholder="Tìm theo tên, MSSV, vi phạm..." class="w-full pl-9 pr-3 lg-control text-sm" /></div>
          <div><select v-model="filterSeverity" class="w-full px-3 lg-control text-sm"><option value="all">Tất cả mức độ</option><option v-for="s in severityLevels" :key="s.id" :value="s.id">{{ s.name }}</option></select></div>
          <div><select v-model="filterStatus" class="w-full px-3 lg-control text-sm"><option value="all">Tất cả trạng thái</option><option value="Đã ban hành">Đã ban hành</option><option value="Chờ duyệt">Chờ duyệt</option></select></div>
        </div>
      </div>

      <!-- Discipline Table -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Sinh viên</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Vi phạm</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Mức độ</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Hình thức xử lý</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Ngày</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredDisciplines.length === 0">
              <td colspan="7" class="px-4 py-12 text-center text-muted"><AlertOctagon class="w-8 h-8 mx-auto mb-2 text-muted" /><span>Không tìm thấy quyết định kỷ luật nào.</span></td>
            </tr>
            <tr v-for="d in filteredDisciplines" :key="d.id" class="transition-colors hover:bg-surface-card-hover">
              <td class="px-4 py-3.5">
                <div class="font-bold text-heading text-xs">{{ d.studentName }}</div>
                <div class="text-[10px] text-muted">{{ d.studentId }} · {{ d.class }} · {{ d.campus }}</div>
              </td>
              <td class="px-4 py-3.5 max-w-[200px]">
                <div class="font-semibold text-heading text-xs">{{ d.violation }}</div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-[10px] font-extrabold px-2 py-0.5 rounded-full border" :class="getSeverityColor(d.severity)">{{ getSeverityName(d.severity) }}</span>
              </td>
              <td class="px-4 py-3.5 max-w-[220px]">
                <div class="text-xs text-body truncate">{{ d.penalty }}</div>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="lg-badge text-[10px]" :class="d.status === 'Đã ban hành' ? 'lg-badge-success' : 'lg-badge-warning'">{{ d.status }}</span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <span class="text-[10px] text-muted">{{ d.decisionDate || 'Chưa có' }}</span>
              </td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-1.5">
                  <button @click="openDetail(d)" class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"><Eye class="w-3.5 h-3.5" /> Xem</button>
                  <button v-if="d.status === 'Chờ duyệt'" @click="handleIssue(d)" class="lg-btn-danger text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"><ShieldAlert class="w-3.5 h-3.5" /> Ban hành</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-2xl lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative max-h-[90vh] overflow-y-auto">
        <button @click="isModalOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"><X class="w-5 h-5" /></button>
        <h2 class="text-xl font-extrabold text-heading mb-5 flex items-center gap-2.5"><AlertOctagon class="w-6 h-6 text-rose-500" /> Tạo Quyết Định Kỷ Luật</h2>

        <div class="lg-alert lg-alert-warning mb-4">
          <div class="flex gap-2"><AlertTriangle class="w-5 h-5 flex-shrink-0 mt-0.5 text-current opacity-90" /><div class="text-xs font-bold leading-relaxed text-current">Quyết định kỷ luật sẽ được lưu ở trạng thái <strong>"Chờ duyệt"</strong> để Hội đồng xem xét trước khi ban hành chính thức.</div></div>
        </div>

        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">MSSV</label><input v-model="currentDiscipline.studentId" type="text" class="w-full px-3 lg-control text-sm" placeholder="HE17xxxx" /></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Tên sinh viên</label><input v-model="currentDiscipline.studentName" type="text" class="w-full px-3 lg-control text-sm" /></div>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Lớp</label><input v-model="currentDiscipline.class" type="text" class="w-full px-3 lg-control text-sm" placeholder="SE17xx" /></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Cơ sở</label>
              <select v-model="currentDiscipline.campus" class="w-full px-3 lg-control text-sm">
                <option>Cơ sở Hòa Lạc</option><option>Cơ sở TP.HCM</option><option>Cơ sở Đà Nẵng</option>
              </select>
            </div>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Loại vi phạm</label>
              <select v-model="currentDiscipline.violation" class="w-full px-3 lg-control text-sm">
                <option v-for="v in violationTypes" :key="v" :value="v">{{ v }}</option>
              </select>
            </div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Mức độ kỷ luật</label>
              <select v-model="currentDiscipline.severity" class="w-full px-3 lg-control text-sm">
                <option v-for="s in severityLevels" :key="s.id" :value="s.id">{{ s.name }}</option>
              </select>
            </div>
          </div>
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Mô tả vi phạm</label><textarea v-model="currentDiscipline.description" rows="3" class="w-full px-3 py-2 lg-control text-sm" placeholder="Mô tả chi tiết hành vi vi phạm..."></textarea></div>
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Hình thức xử lý</label><input v-model="currentDiscipline.penalty" type="text" class="w-full px-3 lg-control text-sm" placeholder="VD: Cảnh cáo + Hủy kết quả thi..." /></div>
        </div>

        <div class="flex justify-end gap-3 pt-5 border-t border-default mt-5">
          <button @click="isModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy</button>
          <button @click="handleSave" class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"><Save class="w-4 h-4" /> Tạo Quyết Định</button>
        </div>
      </div>
    </div>

    <!-- Detail Modal -->
    <div v-if="isDetailOpen && selectedDiscipline" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative max-h-[90vh] overflow-y-auto">
        <button @click="isDetailOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"><X class="w-5 h-5" /></button>
        <div class="flex items-center gap-3 mb-4">
          <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-rose-500 to-red-600 flex items-center justify-center text-white"><AlertOctagon class="w-7 h-7" /></div>
          <div>
            <h2 class="text-lg font-extrabold text-heading">Quyết định kỷ luật</h2>
            <span class="lg-badge text-[10px]" :class="selectedDiscipline.status === 'Đã ban hành' ? 'lg-badge-success' : 'lg-badge-warning'">{{ selectedDiscipline.status }}</span>
          </div>
        </div>
        <div class="space-y-3 text-xs">
          <div class="grid grid-cols-2 gap-3">
            <div><span class="text-muted block">Sinh viên</span><span class="font-bold text-heading">{{ selectedDiscipline.studentName }} ({{ selectedDiscipline.studentId }})</span></div>
            <div><span class="text-muted block">Lớp / Cơ sở</span><span class="font-bold text-heading">{{ selectedDiscipline.class }} · {{ selectedDiscipline.campus }}</span></div>
            <div><span class="text-muted block">Loại vi phạm</span><span class="font-bold text-heading">{{ selectedDiscipline.violation }}</span></div>
            <div><span class="text-muted block">Mức độ</span><span class="text-[10px] font-extrabold px-2 py-0.5 rounded-full border" :class="getSeverityColor(selectedDiscipline.severity)">{{ getSeverityName(selectedDiscipline.severity) }}</span></div>
            <div><span class="text-muted block">Ngày quyết định</span><span class="font-bold text-heading">{{ selectedDiscipline.decisionDate || 'Chưa có' }}</span></div>
            <div><span class="text-muted block">Người ký</span><span class="font-bold text-heading">{{ selectedDiscipline.decisionBy || 'Chưa có' }}</span></div>
          </div>
          <div class="pt-3 border-t border-default/50"><span class="text-muted block mb-1">Mô tả vi phạm</span><p class="text-body leading-relaxed">{{ selectedDiscipline.description }}</p></div>
          <div class="lg-alert" :class="selectedDiscipline.severity === 'suspension' || selectedDiscipline.severity === 'expulsion' ? 'lg-alert-error' : 'lg-alert-warning'">
            <div class="flex gap-2"><ShieldAlert class="w-5 h-5 flex-shrink-0 mt-0.5 text-current opacity-90" /><div class="text-xs font-bold leading-relaxed text-current"><strong>Hình thức xử lý:</strong> {{ selectedDiscipline.penalty }}</div></div>
          </div>
        </div>
        <div class="flex justify-end pt-4 border-t border-default mt-5"><button @click="isDetailOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Đóng</button></div>
      </div>
    </div>
  </div>
</template>