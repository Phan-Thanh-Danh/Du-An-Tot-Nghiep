<script setup>
/**
 * AwardsView.vue - Super Admin
 * Quản lý khen thưởng sinh viên: danh sách, tạo mới, lọc theo loại/kỳ, xem chi tiết.
 */
import { ref, computed } from 'vue'
import {
  Award,
  Plus,
  Search,
  Filter,
  RotateCcw,
  Eye,
  Pencil,
  X,
  Save,
  CheckCircle,
  AlertTriangle,
  Info,
  Star,
  Trophy,
  Medal,
  GraduationCap,
  Calendar,
  User
} from 'lucide-vue-next'

const awardTypes = ref([
  { id: 'academic', name: 'Học tập xuất sắc', icon: 'GraduationCap' },
  { id: 'research', name: 'Nghiên cứu khoa học', icon: 'Star' },
  { id: 'extracurricular', name: 'Hoạt động ngoại khóa', icon: 'Trophy' },
  { id: 'competition', name: 'Cuộc thi / Hackathon', icon: 'Medal' },
  { id: 'other', name: 'Khen thưởng khác', icon: 'Award' }
])

const awards = ref([
  { id: 1, studentId: 'HE170001', studentName: 'Nguyễn Văn An', campus: 'Cơ sở Hòa Lạc', type: 'academic', title: 'Sinh viên có GPA cao nhất khoa CNTT kỳ Spring 2026', description: 'GPA 3.95/4.0, đứng đầu toàn khoa với 18 tín chỉ xuất sắc.', semester: 'Spring 2026', awardDate: '2026-06-10', grantedBy: 'Hiệu trưởng', status: 'Đã công bố', reward: 'Giấy khen + 2.000.000đ' },
  { id: 2, studentId: 'HE170045', studentName: 'Trần Thị Bích', campus: 'Cơ sở TP.HCM', type: 'competition', title: 'Giải Nhì cuộc thi FPT Hackathon 2026', description: 'Đội "CodeBreakers" đạt giải Nhì với dự án AI chatbot hỗ trợ học tập.', semester: 'Spring 2026', awardDate: '2026-05-28', grantedBy: 'Ban Giám Hiệu', status: 'Đã công bố', reward: 'Giấy khen + 5.000.000đ' },
  { id: 3, studentId: 'HE170112', studentName: 'Lê Hoàng Long', campus: 'Cơ sở Đà Nẵng', type: 'research', title: 'Bài báo khoa học xuất bản trên tạp chí quốc tế', description: 'Đồng tác giả bài báo "Deep Learning for Vietnamese NLP" trên IEEE Access.', semester: 'Spring 2026', awardDate: '2026-05-15', grantedBy: 'Hội đồng Khoa học', status: 'Đã công bố', reward: 'Giấy khen + 10.000.000đ' },
  { id: 4, studentId: 'HE170203', studentName: 'Phạm Minh Đức', campus: 'Cơ sở Hòa Lạc', type: 'extracurricular', title: 'Thủ lĩnh CLB Lập trình FPT 2025-2026', description: 'Tổ chức thành công 12 workshop kỹ thuật, thu hút 500+ sinh viên tham gia.', semester: 'Spring 2026', awardDate: '2026-06-01', grantedBy: 'Phòng Công tác SV', status: 'Chờ duyệt', reward: 'Giấy khen + Học bổng 50%' },
  { id: 5, studentId: 'HE170089', studentName: 'Ngô Thùy Linh', campus: 'Cơ sở TP.HCM', type: 'academic', title: 'Sinh viên 5 tốt cấp trường năm 2025-2026', description: 'Đạt đủ 5 tiêu chí: Đạo đức tốt, Học tập tốt, Thể lực tốt, Tình nguyện tốt, Hội nhập tốt.', semester: 'Spring 2026', awardDate: '2026-06-05', grantedBy: 'Đoàn Thanh niên', status: 'Đã công bố', reward: 'Bằng khen + 3.000.000đ' }
])

// --- Filter ---
const searchQuery = ref('')
const filterType = ref('all')
const filterStatus = ref('all')

const filteredAwards = computed(() => {
  return awards.value.filter(a => {
    const matchSearch = searchQuery.value === '' || a.studentName.toLowerCase().includes(searchQuery.value.toLowerCase()) || a.title.toLowerCase().includes(searchQuery.value.toLowerCase()) || a.studentId.toLowerCase().includes(searchQuery.value.toLowerCase())
    const matchType = filterType.value === 'all' || a.type === filterType.value
    const matchStatus = filterStatus.value === 'all' || a.status === filterStatus.value
    return matchSearch && matchType && matchStatus
  })
})

const resetFilters = () => { searchQuery.value = ''; filterType.value = 'all'; filterStatus.value = 'all' }

// --- KPI ---
const totalAwards = computed(() => awards.value.length)
const publishedAwards = computed(() => awards.value.filter(a => a.status === 'Đã công bố').length)
const pendingAwards = computed(() => awards.value.filter(a => a.status === 'Chờ duyệt').length)
const academicAwards = computed(() => awards.value.filter(a => a.type === 'academic').length)

// --- Modal ---
const isModalOpen = ref(false)
const editingMode = ref('create')
const currentAward = ref({ id: null, studentId: '', studentName: '', campus: 'Cơ sở Hòa Lạc', type: 'academic', title: '', description: '', semester: 'Spring 2026', awardDate: '', grantedBy: '', status: 'Chờ duyệt', reward: '' })

// --- Detail Modal ---
const isDetailOpen = ref(false)
const selectedAward = ref(null)

// --- Toast ---
const showToast = ref(false)
const toastMessage = ref('')
const toastType = ref('success')
const triggerToast = (msg, type = 'success') => { toastMessage.value = msg; toastType.value = type; showToast.value = true; setTimeout(() => { showToast.value = false }, 4000) }

const openCreateModal = () => { editingMode.value = 'create'; currentAward.value = { id: null, studentId: '', studentName: '', campus: 'Cơ sở Hòa Lạc', type: 'academic', title: '', description: '', semester: 'Spring 2026', awardDate: '', grantedBy: '', status: 'Chờ duyệt', reward: '' }; isModalOpen.value = true }

const handleSaveAward = () => {
  if (!currentAward.value.studentName.trim() || !currentAward.value.title.trim()) { triggerToast('Vui lòng điền đầy đủ thông tin.', 'error'); return }
  if (editingMode.value === 'create') {
    const newId = awards.value.length ? Math.max(...awards.value.map(a => a.id)) + 1 : 1
    awards.value.unshift({ ...currentAward.value, id: newId })
    triggerToast('Đã tạo khen thưởng mới.')
  }
  isModalOpen.value = false
}

const openDetail = (award) => { selectedAward.value = award; isDetailOpen.value = true }
const handlePublish = (award) => { award.status = 'Đã công bố'; triggerToast(`Khen thưởng "${award.title.substring(0, 30)}..." đã được công bố.`) }

const getTypeName = (type) => awardTypes.value.find(t => t.id === type)?.name || type
const getTypeColor = (type) => {
  const colors = { academic: 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-200 dark:border-blue-500/20', research: 'bg-violet-500/10 text-violet-600 dark:text-violet-400 border-violet-200 dark:border-violet-500/20', extracurricular: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-200 dark:border-emerald-500/20', competition: 'bg-amber-500/10 text-amber-600 dark:text-amber-400 border-amber-200 dark:border-amber-500/20', other: 'bg-cyan-500/10 text-cyan-600 dark:text-cyan-400 border-cyan-200 dark:border-cyan-500/20' }
  return colors[type] || colors.other
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
            <Award class="w-8 h-8 text-primary" />
            Khen Thưởng Sinh Viên
          </h1>
          <p class="text-sm text-muted mt-1">Quản lý và công bố các quyết định khen thưởng sinh viên xuất sắc.</p>
        </div>
        <button @click="openCreateModal" class="lg-btn-primary px-4 py-2.5 text-sm font-bold flex items-center gap-2"><Plus class="w-4.5 h-4.5" /> Tạo Khen Thưởng</button>
      </div>

      <!-- KPI -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-sky-500/10 flex items-center justify-center text-sky-500"><Award class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Tổng khen thưởng</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ totalAwards }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-emerald-500/10 flex items-center justify-center text-emerald-500"><CheckCircle class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Đã công bố</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ publishedAwards }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500"><AlertTriangle class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Chờ duyệt</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ pendingAwards }}</div></div>
        </div>
        <div class="lg-glass-soft lg-card lg-card-hover lg-density-normal flex items-center gap-4">
          <div class="w-12 h-12 rounded-xl bg-violet-500/10 flex items-center justify-center text-violet-500"><GraduationCap class="w-6 h-6" /></div>
          <div><div class="text-xs font-semibold text-muted tracking-wider uppercase">Học tập xuất sắc</div><div class="text-2xl font-bold mt-0.5 text-heading">{{ academicAwards }}</div></div>
        </div>
      </div>

      <!-- Filter -->
      <div class="lg-glass-soft lg-card lg-density-normal mb-6">
        <div class="flex items-center justify-between mb-4 pb-3 border-b border-default">
          <div class="flex items-center gap-2"><Filter class="w-4.5 h-4.5 text-primary" /><h3 class="font-bold text-heading text-sm">Bộ lọc khen thưởng</h3></div>
          <button @click="resetFilters" class="text-xs text-link font-bold flex items-center gap-1 hover:underline"><RotateCcw class="w-3.5 h-3.5" /> Xóa lọc</button>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <div class="relative"><Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted" /><input v-model="searchQuery" type="text" placeholder="Tìm theo tên SV, MSSV, tiêu đề..." class="w-full pl-9 pr-3 lg-control text-sm" /></div>
          <div><select v-model="filterType" class="w-full px-3 lg-control text-sm"><option value="all">Tất cả loại</option><option v-for="t in awardTypes" :key="t.id" :value="t.id">{{ t.name }}</option></select></div>
          <div><select v-model="filterStatus" class="w-full px-3 lg-control text-sm"><option value="all">Tất cả trạng thái</option><option value="Đã công bố">Đã công bố</option><option value="Chờ duyệt">Chờ duyệt</option></select></div>
        </div>
      </div>

      <!-- Awards Table -->
      <div class="lg-table-shell overflow-x-auto mb-8">
        <table class="min-w-full divide-y divide-default text-sm">
          <thead>
            <tr class="surface-table-header">
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Sinh viên</th>
              <th class="px-4 py-3 text-left text-xs font-bold text-label uppercase">Khen thưởng</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Loại</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Phần thưởng</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Trạng thái</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Ngày</th>
              <th class="px-4 py-3 text-center text-xs font-bold text-label uppercase">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-default">
            <tr v-if="filteredAwards.length === 0">
              <td colspan="7" class="px-4 py-12 text-center text-muted"><Award class="w-8 h-8 mx-auto mb-2 text-muted" /><span>Không tìm thấy khen thưởng nào.</span></td>
            </tr>
            <tr v-for="award in filteredAwards" :key="award.id" class="transition-colors hover:bg-surface-card-hover">
              <td class="px-4 py-3.5">
                <div class="font-bold text-heading text-xs">{{ award.studentName }}</div>
                <div class="text-[10px] text-muted">{{ award.studentId }} · {{ award.campus }}</div>
              </td>
              <td class="px-4 py-3.5 max-w-[240px]"><div class="font-semibold text-heading text-xs truncate">{{ award.title }}</div></td>
              <td class="px-4 py-3.5 text-center"><span class="text-[10px] font-bold px-2 py-0.5 rounded-full border" :class="getTypeColor(award.type)">{{ getTypeName(award.type) }}</span></td>
              <td class="px-4 py-3.5 text-center"><span class="text-xs font-semibold text-heading">{{ award.reward }}</span></td>
              <td class="px-4 py-3.5 text-center"><span class="lg-badge text-[10px]" :class="award.status === 'Đã công bố' ? 'lg-badge-success' : 'lg-badge-warning'">{{ award.status }}</span></td>
              <td class="px-4 py-3.5 text-center"><span class="text-[10px] text-muted">{{ award.awardDate }}</span></td>
              <td class="px-4 py-3.5 text-center">
                <div class="flex items-center justify-center gap-1.5">
                  <button @click="openDetail(award)" class="lg-btn-secondary text-xs px-2.5 py-1.5 flex items-center gap-1"><Eye class="w-3.5 h-3.5" /> Xem</button>
                  <button v-if="award.status === 'Chờ duyệt'" @click="handlePublish(award)" class="lg-btn-primary text-xs px-2.5 py-1.5 flex items-center gap-1 font-bold"><CheckCircle class="w-3.5 h-3.5" /> Công bố</button>
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
        <h2 class="text-xl font-extrabold text-heading mb-5 flex items-center gap-2.5"><Award class="w-6 h-6 text-primary" /> Tạo Khen Thưởng Mới</h2>
        <div class="space-y-4">
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">MSSV</label><input v-model="currentAward.studentId" type="text" class="w-full px-3 lg-control text-sm" placeholder="HE17xxxx" /></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Tên sinh viên</label><input v-model="currentAward.studentName" type="text" class="w-full px-3 lg-control text-sm" /></div>
          </div>
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Tiêu đề khen thưởng</label><input v-model="currentAward.title" type="text" class="w-full px-3 lg-control text-sm" /></div>
          <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Mô tả chi tiết</label><textarea v-model="currentAward.description" rows="3" class="w-full px-3 py-2 lg-control text-sm"></textarea></div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Loại khen thưởng</label><select v-model="currentAward.type" class="w-full px-3 lg-control text-sm"><option v-for="t in awardTypes" :key="t.id" :value="t.id">{{ t.name }}</option></select></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Phần thưởng</label><input v-model="currentAward.reward" type="text" class="w-full px-3 lg-control text-sm" placeholder="Giấy khen + ..." /></div>
          </div>
          <div class="grid grid-cols-2 gap-3">
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Ngày khen thưởng</label><input v-model="currentAward.awardDate" type="date" class="w-full px-3 lg-control text-sm" /></div>
            <div><label class="block text-xs font-bold text-label mb-1.5 uppercase">Người ký / Đơn vị</label><input v-model="currentAward.grantedBy" type="text" class="w-full px-3 lg-control text-sm" /></div>
          </div>
        </div>
        <div class="flex justify-end gap-3 pt-5 border-t border-default mt-5">
          <button @click="isModalOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Hủy</button>
          <button @click="handleSaveAward" class="lg-btn-primary px-5 py-2 text-sm font-bold flex items-center gap-1.5"><Save class="w-4 h-4" /> Tạo Khen Thưởng</button>
        </div>
      </div>
    </div>

    <!-- Detail Modal -->
    <div v-if="isDetailOpen && selectedAward" class="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-slate-950/40 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="w-full max-w-lg lg-glass-strong lg-density-spacious rounded-2xl shadow-2xl relative">
        <button @click="isDetailOpen = false" class="absolute top-4 right-4 text-muted hover:text-heading lg-icon-button p-1.5 bg-surface-card rounded-lg border border-default"><X class="w-5 h-5" /></button>
        <div class="flex items-center gap-3 mb-4">
          <div class="w-12 h-12 rounded-xl bg-gradient-to-br from-amber-400 to-orange-500 flex items-center justify-center text-white"><Award class="w-7 h-7" /></div>
          <div>
            <h2 class="text-lg font-extrabold text-heading">{{ selectedAward.title }}</h2>
            <span class="lg-badge text-[10px]" :class="selectedAward.status === 'Đã công bố' ? 'lg-badge-success' : 'lg-badge-warning'">{{ selectedAward.status }}</span>
          </div>
        </div>
        <div class="space-y-3 text-xs">
          <div class="grid grid-cols-2 gap-3">
            <div><span class="text-muted block">Sinh viên</span><span class="font-bold text-heading">{{ selectedAward.studentName }} ({{ selectedAward.studentId }})</span></div>
            <div><span class="text-muted block">Cơ sở</span><span class="font-bold text-heading">{{ selectedAward.campus }}</span></div>
            <div><span class="text-muted block">Loại</span><span class="font-bold text-heading">{{ getTypeName(selectedAward.type) }}</span></div>
            <div><span class="text-muted block">Ngày khen</span><span class="font-bold text-heading">{{ selectedAward.awardDate }}</span></div>
            <div><span class="text-muted block">Phần thưởng</span><span class="font-bold text-heading">{{ selectedAward.reward }}</span></div>
            <div><span class="text-muted block">Người ký</span><span class="font-bold text-heading">{{ selectedAward.grantedBy }}</span></div>
          </div>
          <div class="pt-3 border-t border-default/50"><span class="text-muted block mb-1">Mô tả</span><p class="text-body leading-relaxed">{{ selectedAward.description }}</p></div>
        </div>
        <div class="flex justify-end pt-4 border-t border-default mt-5"><button @click="isDetailOpen = false" class="lg-btn-secondary px-4 py-2 text-sm font-bold">Đóng</button></div>
      </div>
    </div>
  </div>
</template>