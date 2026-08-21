<script setup>
import { ref, computed, watch } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { trainingProgramApi } from '@/services/trainingProgramApi'
import { subjectApi } from '@/services/subjectApi'
import { 
  X, Plus, Trash2, Edit2, BookOpen, 
  Info, RefreshCw, AlertTriangle, Search
} from 'lucide-vue-next'

const props = defineProps({
  isOpen: { type: Boolean, default: false },
  program: { type: Object, default: () => null }
})

const emit = defineEmits(['close', 'updated'])

const popup = usePopupStore()

const loading = ref(false)
const curriculum = ref([])
const availableSubjects = ref([])
const activeSemester = ref(1)

// Modal Thêm / Sửa môn vào học kỳ
const isSubjectModalOpen = ref(false)
const subjectModalMode = ref('add') // 'add' or 'edit'
const subjectForm = ref({
  maMonHoc: null,
  hocKyDuKien: 1,
  soTinChi: 3,
  batBuoc: true,
  thuTu: 1,
  ghiChu: '',
  maMonTienQuyetIds: []
})

// Validation Alerts
const validationWarnings = computed(() => {
  const warnings = []
  if (!props.program) return warnings

  // 1. Check total credits
  const total = curriculum.value.reduce((sum, s) => sum + (s.soTinChi || 0), 0)
  const requiredTotal = props.program.tongTinChiYeuCau || props.program.TongTinChiYeuCau || 120
  if (total < requiredTotal) {
    warnings.push(`Khung hiện có ${total}/${requiredTotal} tín chỉ yêu cầu. Vẫn còn thiếu ${requiredTotal - total} tín chỉ.`)
  } else if (total > requiredTotal + 30) {
    warnings.push(`Tổng tín chỉ trong khung (${total}) vượt quá cao so với mức chuẩn (${requiredTotal}).`)
  }

  // 2. Check prerequisites ordering (Prerequisite subject must be in earlier semester)
  const subjectSemMap = {}
  curriculum.value.forEach(s => {
    subjectSemMap[s.maMonHoc] = s.hocKyDuKien
  })

  curriculum.value.forEach(s => {
    if (s.monTienQuyets && s.monTienQuyets.length > 0) {
      s.monTienQuyets.forEach(p => {
        const prereqSem = subjectSemMap[p.maMonTienQuyet]
        if (prereqSem && prereqSem >= s.hocKyDuKien) {
          warnings.push(`Cảnh báo logic: Môn "${s.tenMonHoc}" (HK${s.hocKyDuKien}) có môn tiên quyết "${p.tenMonTienQuyet}" ở HK${prereqSem} (cần xếp môn tiên quyết vào học kỳ trước).`)
        }
      })
    }
  })

  return warnings
})

const maxSemesters = computed(() => {
  return props.program?.soHocKy || props.program?.SoHocKy || 6
})

const masterSubjectOptions = computed(() => {
  const existingIds = new Set(curriculum.value.map(s => s.maMonHoc || s.MaMonHoc))
  return availableSubjects.value
    .filter(s => !existingIds.has(s.maMonHoc || s.MaMonHoc))
    .map(s => ({
      value: s.maMonHoc || s.MaMonHoc,
      label: `${s.maCodeMonHoc || s.MaCodeMonHoc} - ${s.tenMonHoc || s.TenMonHoc} (${s.soTinChi || s.SoTinChi} TCI)`
    }))
})

const subjectSearchQuery = ref('')
const selectedSubjectIds = ref([])

const normalizeSubjectText = (s) => String(s || '')
  .normalize('NFD')
  .replace(/[\u0300-\u036f]/g, '')
  .toLowerCase()

const filteredMasterSubjects = computed(() => {
  const q = normalizeSubjectText(subjectSearchQuery.value)
  if (!q) return masterSubjectOptions.value
  return masterSubjectOptions.value.filter(o => normalizeSubjectText(o.label).includes(q))
})

const toggleSelectFiltered = () => {
  const visibleIds = filteredMasterSubjects.value.map(o => o.value)
  const allSelected = visibleIds.every(id => selectedSubjectIds.value.includes(id))
  if (allSelected) {
    selectedSubjectIds.value = selectedSubjectIds.value.filter(id => !visibleIds.includes(id))
  } else {
    selectedSubjectIds.value = [...new Set([...selectedSubjectIds.value, ...visibleIds])]
  }
}

const semesterGroups = computed(() => {
  const groups = {}
  for (let i = 1; i <= maxSemesters.value; i++) {
    groups[i] = []
  }
  curriculum.value.forEach(item => {
    const sem = item.hocKyDuKien || 1
    if (!groups[sem]) groups[sem] = []
    groups[sem].push(item)
  })
  return groups
})

const currentSemesterSubjects = computed(() => {
  return semesterGroups.value[activeSemester.value] || []
})

const currentSemesterTotalCredits = computed(() => {
  return currentSemesterSubjects.value.reduce((sum, s) => sum + (s.soTinChi || 0), 0)
})

const totalCurriculumCredits = computed(() => {
  return curriculum.value.reduce((sum, s) => sum + (s.soTinChi || 0), 0)
})

const fetchAllPages = async (params) => {
  const all = []
  const pageSize = 100
  let pageIndex = 1
  while (true) {
    const res = await subjectApi.list({ ...params, pageIndex, pageSize })
    const items = Array.isArray(res) ? res : (res?.items || res?.Items || [])
    all.push(...items)
    if (items.length < pageSize) break
    pageIndex++
  }
  return all
}

const loadAvailableSubjects = async () => {
  const maNganh = props.program?.maNganh || props.program?.MaNganh
  if (!maNganh) return fetchAllPages({})

  const seen = new Set()
  const all = []
  const merge = (items) => {
    for (const item of items) {
      const id = item.maMonHoc ?? item.MaMonHoc
      if (!seen.has(id)) {
        seen.add(id)
        all.push(item)
      }
    }
  }
  const [majorSubjects, genSubjects] = await Promise.all([
    fetchAllPages({ maNganh }),
    fetchAllPages({ keyword: 'GEN' })
  ])
  merge(majorSubjects)
  merge(genSubjects)
  return all
}

const loadData = async () => {
  if (!props.program) return
  loading.value = true
  try {
    const programId = props.program.maChuongTrinh || props.program.MaChuongTrinh
    const [currRes, allSubjects] = await Promise.all([
      trainingProgramApi.getCurriculum(programId),
      loadAvailableSubjects()
    ])
    curriculum.value = currRes?.data || currRes?.Data || (Array.isArray(currRes) ? currRes : [])
    availableSubjects.value = allSubjects
  } catch (err) {
    console.error('Error loading curriculum:', err)
    popup.error('Lỗi', 'Không thể tải chi tiết khung môn học.')
  } finally {
    loading.value = false
  }
}

watch(() => props.isOpen, (newVal) => {
  if (newVal) {
    activeSemester.value = 1
    loadData()
  }
})

const openAddSubjectModal = (sem) => {
  subjectModalMode.value = 'add'
  const semSubs = semesterGroups.value[sem] || []
  subjectForm.value = {
    maMonHoc: null,
    hocKyDuKien: sem,
    soTinChi: 3,
    batBuoc: true,
    thuTu: semSubs.length + 1,
    ghiChu: '',
    maMonTienQuyetIds: []
  }
  selectedSubjectIds.value = []
  subjectSearchQuery.value = ''
  isSubjectModalOpen.value = true
}

const openEditSubjectModal = (sub) => {
  subjectModalMode.value = 'edit'
  subjectForm.value = {
    maMonHoc: sub.maMonHoc,
    hocKyDuKien: sub.hocKyDuKien,
    soTinChi: sub.soTinChi,
    batBuoc: sub.batBuoc,
    thuTu: sub.thuTu,
    ghiChu: sub.ghiChu || '',
    maMonTienQuyetIds: (sub.monTienQuyets || []).map(p => p.maMonTienQuyet)
  }
  isSubjectModalOpen.value = true
}

const saveSubject = async () => {
  const programId = props.program.maChuongTrinh || props.program.MaChuongTrinh

  try {
    if (subjectModalMode.value === 'add') {
      const ids = selectedSubjectIds.value
      if (!ids.length) {
        popup.warning('Chưa chọn môn', 'Vui lòng chọn ít nhất một môn học để thêm vào khung.')
        return
      }
      const existingIds = new Set(curriculum.value.map(s => s.maMonHoc || s.MaMonHoc))
      const toAdd = ids.filter(id => !existingIds.has(Number(id)))
      const skipped = ids.filter(id => existingIds.has(Number(id)))
      if (!toAdd.length) {
        popup.warning('Môn đã có trong khung', 'Các môn được chọn đều đã tồn tại trong khung chương trình (ở học kỳ khác). Không cần thêm lại.')
        return
      }
      const semSubs = semesterGroups.value[subjectForm.value.hocKyDuKien] || []
      let added = 0
      for (let i = 0; i < toAdd.length; i++) {
        const id = Number(toAdd[i])
        const found = availableSubjects.value.find(s => (s.maMonHoc || s.MaMonHoc) === id)
        await trainingProgramApi.addSubject(programId, {
          maMonHoc: id,
          hocKyDuKien: Number(subjectForm.value.hocKyDuKien),
          soTinChi: Number(found?.soTinChi || found?.SoTinChi || 3),
          loaiMonHoc: subjectForm.value.batBuoc ? 'Bắt buộc' : 'Tự chọn',
          batBuoc: subjectForm.value.batBuoc,
          thuTu: semSubs.length + i + 1,
          ghiChu: subjectForm.value.ghiChu,
          maMonTienQuyetIds: subjectForm.value.maMonTienQuyetIds
        })
        added++
      }
      popup.success('Thành công', skipped.length
        ? `Đã thêm ${added} môn học vào HK${subjectForm.value.hocKyDuKien}. Bỏ qua ${skipped.length} môn đã có trong khung.`
        : `Đã thêm ${added} môn học vào HK${subjectForm.value.hocKyDuKien}.`)
    } else {
      await trainingProgramApi.updateSubject(programId, subjectForm.value.maMonHoc, {
        hocKyDuKien: Number(subjectForm.value.hocKyDuKien),
        soTinChi: Number(subjectForm.value.soTinChi),
        loaiMonHoc: subjectForm.value.batBuoc ? 'Bắt buộc' : 'Tự chọn',
        batBuoc: subjectForm.value.batBuoc,
        thuTu: Number(subjectForm.value.thuTu),
        ghiChu: subjectForm.value.ghiChu,
        maMonTienQuyetIds: subjectForm.value.maMonTienQuyetIds
      })
      popup.success('Thành công', 'Đã cập nhật môn học trong khung.')
    }
    isSubjectModalOpen.value = false
    await loadData()
    emit('updated')
  } catch (err) {
    console.error('Error saving subject:', err)
    popup.error('Lỗi', err.message || 'Không thể lưu thông tin môn học.')
  }
}

const removeSubject = async (sub) => {
  if (!confirm(`Bạn có chắc chắn muốn gỡ môn "${sub.tenMonHoc}" khỏi HK${sub.hocKyDuKien}?`)) return

  const programId = props.program.maChuongTrinh || props.program.MaChuongTrinh
  try {
    await trainingProgramApi.removeSubject(programId, sub.maMonHoc)
    popup.success('Đã gỡ môn', `Đã xóa môn ${sub.tenMonHoc} khỏi học kỳ.`)
    await loadData()
    emit('updated')
  } catch (err) {
    console.error('Error removing subject:', err)
    popup.error('Lỗi', 'Không thể gỡ môn học khỏi khung.')
  }
}

// Prerequisites options (subjects in earlier semesters)
const possiblePrerequisiteOptions = computed(() => {
  return curriculum.value
    .filter(s => s.maMonHoc !== subjectForm.value.maMonHoc)
    .map(s => ({
      id: s.maMonHoc,
      name: `${s.maCodeMonHoc} - ${s.tenMonHoc} (HK${s.hocKyDuKien})`
    }))
})
</script>

<template>
  <Teleport to="body">
    <div v-if="isOpen" class="modal-overlay" @click.self="emit('close')">
      <div class="modal-content surface-card border border-card p-6 rounded-2xl max-w-5xl w-full max-h-[92vh] overflow-y-auto flex flex-col gap-5">
        
        <!-- Header -->
        <div class="flex items-center justify-between border-b border-slate-500/10 pb-4">
          <div>
            <div class="flex items-center gap-2">
              <span class="px-2.5 py-0.5 rounded text-xs font-bold bg-teal-500/15 text-teal-600 dark:text-teal-300 border border-teal-300">
                {{ program?.maCodeChuongTrinh || program?.MaCodeChuongTrinh }}
              </span>
              <h2 class="text-xl font-bold text-heading">
                {{ program?.tenChuongTrinh || program?.TenChuongTrinh }}
              </h2>
            </div>
            <p class="text-xs text-label mt-1">
              Chuyên ngành: <strong>{{ program?.tenChuyenNganh || program?.TenChuyenNganh }}</strong> | 
              Số học kỳ chuẩn: <strong>{{ maxSemesters }} kỳ</strong> | 
              Tổng tín chỉ tích lũy: <strong class="text-teal-600 dark:text-teal-400">{{ totalCurriculumCredits }} / {{ program?.tongTinChiYeuCau || program?.TongTinChiYeuCau || 120 }} TCI</strong>
            </p>
          </div>

          <div class="flex items-center gap-2">
            <button @click="loadData" class="glass-btn secondary p-2" title="Tải lại chi tiết">
              <RefreshCw :size="16" :class="{ 'animate-spin': loading }" />
            </button>
            <button @click="emit('close')" class="text-label hover:text-heading p-1.5 rounded-lg hover:bg-slate-500/10">
              <X :size="20" />
            </button>
          </div>
        </div>

        <!-- Validation Warnings (Rule Engine Alert) -->
        <div v-if="validationWarnings.length > 0" class="bg-amber-500/10 border border-amber-300/40 rounded-xl p-3 text-xs text-amber-700 dark:text-amber-300 flex flex-col gap-1">
          <div class="flex items-center gap-1.5 font-bold">
            <AlertTriangle :size="15" class="text-amber-500 shrink-0" />
            <span>Cảnh báo Kiểm tra Quy tắc (Rule Engine Validation):</span>
          </div>
          <ul class="list-disc pl-5 space-y-0.5">
            <li v-for="(warn, idx) in validationWarnings" :key="idx">{{ warn }}</li>
          </ul>
        </div>

        <!-- Semester Tabs Header -->
        <div class="flex items-center gap-2 border-b border-slate-500/10 pb-2 overflow-x-auto">
          <button 
            v-for="sem in maxSemesters" 
            :key="sem"
            @click="activeSemester = sem"
            class="px-4 py-2 rounded-xl text-xs font-bold transition flex items-center gap-2 whitespace-nowrap"
            :class="activeSemester === sem 
              ? 'bg-teal-600 text-white shadow-sm' 
              : 'surface-card text-label hover:text-heading border border-card'"
          >
            <span>Học kỳ {{ sem }}</span>
            <span 
              class="px-1.5 py-0.5 rounded-full text-[10px]"
              :class="activeSemester === sem ? 'bg-white/20 text-white' : 'bg-slate-500/10 text-label'"
            >
              {{ (semesterGroups[sem] || []).length }} môn ({{ (semesterGroups[sem] || []).reduce((sum, s) => sum + s.soTinChi, 0) }} TCI)
            </span>
          </button>
        </div>

        <!-- Current Semester Subject Table -->
        <div class="flex flex-col gap-3">
          <div class="flex items-center justify-between">
            <h3 class="text-sm font-bold text-heading flex items-center gap-2">
              <BookOpen :size="16" class="text-teal-600" />
              Danh sách môn học thuộc Học kỳ {{ activeSemester }}
              <span class="text-xs font-normal text-label">({{ currentSemesterSubjects.length }} môn · {{ currentSemesterTotalCredits }} tín chỉ)</span>
            </h3>
            <button @click="openAddSubjectModal(activeSemester)" class="glass-btn primary py-1.5 px-3 text-xs inline-flex items-center gap-1.5">
              <Plus :size="14" /> Thêm môn vào HK{{ activeSemester }}
            </button>
          </div>

          <div v-if="loading" class="py-12 flex justify-center text-label text-sm items-center gap-2">
            <RefreshCw :size="18" class="animate-spin" /> Đang tải danh sách môn học...
          </div>

          <div v-else-if="currentSemesterSubjects.length === 0" class="py-12 surface-card border border-card rounded-2xl text-center text-label text-sm flex flex-col items-center gap-2">
            <Info :size="24" class="text-slate-400" />
            <p>Học kỳ {{ activeSemester }} hiện chưa có môn học nào trong khung.</p>
            <button @click="openAddSubjectModal(activeSemester)" class="glass-btn secondary text-xs mt-1">
              + Thêm môn học đầu tiên cho HK{{ activeSemester }}
            </button>
          </div>

          <div v-else class="overflow-x-auto overflow-y-auto max-h-[46vh] border border-slate-500/10 rounded-xl surface-card">
            <table class="w-full text-left text-sm text-body whitespace-nowrap">
              <thead class="sticky top-0 z-10 bg-(--surface-card) text-xs text-label font-bold uppercase border-b border-slate-500/10">
                <tr>
                  <th class="px-4 py-3">STT</th>
                  <th class="px-4 py-3">Mã môn</th>
                  <th class="px-4 py-3">Tên môn học</th>
                  <th class="px-4 py-3">Số TCI</th>
                  <th class="px-4 py-3">Phân loại</th>
                  <th class="px-4 py-3">Môn tiên quyết</th>
                  <th class="px-4 py-3 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-500/10">
                <tr v-for="(sub, index) in currentSemesterSubjects" :key="sub.maMonHoc" class="hover:bg-slate-500/5 transition">
                  <td class="px-4 py-3 text-xs text-label font-medium">{{ sub.thuTu || index + 1 }}</td>
                  <td class="px-4 py-3">
                    <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-slate-500/10 text-heading border border-card">
                      {{ sub.maCodeMonHoc }}
                    </span>
                  </td>
                  <td class="px-4 py-3 font-semibold text-heading">{{ sub.tenMonHoc }}</td>
                  <td class="px-4 py-3 font-bold text-teal-600 dark:text-teal-400">{{ sub.soTinChi }} TCI</td>
                  <td class="px-4 py-3">
                    <span 
                      class="px-2 py-0.5 rounded text-[11px] font-bold"
                      :class="sub.batBuoc ? 'bg-emerald-500/15 text-emerald-600 border border-emerald-300' : 'bg-slate-500/15 text-slate-600 border border-slate-300'"
                    >
                      {{ sub.batBuoc ? 'Bắt buộc' : 'Tự chọn' }}
                    </span>
                  </td>
                  <td class="px-4 py-3 text-xs">
                    <div v-if="sub.monTienQuyets && sub.monTienQuyets.length > 0" class="flex flex-wrap gap-1">
                      <span 
                        v-for="p in sub.monTienQuyets" 
                        :key="p.maMonTienQuyet"
                        class="px-2 py-0.5 bg-amber-500/15 text-amber-700 dark:text-amber-300 border border-amber-300 rounded text-[11px] font-medium"
                      >
                        {{ p.maCodeMonTienQuyet }} (≥{{ p.diemToiThieu || 5 }})
                      </span>
                    </div>
                    <span v-else class="text-label italic text-[11px]">Không có</span>
                  </td>
                  <td class="px-4 py-3 text-right">
                    <div class="flex items-center justify-end gap-1">
                      <button @click="openEditSubjectModal(sub)" class="action-btn text-teal-600 hover:bg-teal-500/10" title="Chỉnh sửa thông số môn">
                        <Edit2 :size="15" />
                      </button>
                      <button @click="removeSubject(sub)" class="action-btn text-rose-600 hover:bg-rose-500/10" title="Gỡ khỏi khung">
                        <Trash2 :size="15" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Footer -->
        <div class="flex justify-end pt-3 border-t border-slate-500/10">
          <button @click="emit('close')" class="glass-btn secondary px-5">Đóng</button>
        </div>

        <!-- Inner Modal: Add / Edit Subject -->
        <div v-if="isSubjectModalOpen" class="modal-overlay" @click.self="isSubjectModalOpen = false">
          <div class="modal-content surface-card border border-card p-6 rounded-2xl max-w-md w-full flex flex-col gap-4">
            <div class="flex items-center justify-between pb-3 border-b border-slate-500/10">
              <h4 class="font-bold text-heading text-base">
                {{ subjectModalMode === 'add' ? `Thêm môn vào HK${subjectForm.hocKyDuKien}` : 'Chỉnh sửa môn trong khung' }}
              </h4>
              <button @click="isSubjectModalOpen = false" class="text-label hover:text-heading"><X :size="18" /></button>
            </div>

            <div class="flex flex-col gap-3">
              <div v-if="subjectModalMode === 'add'" class="form-group">
                <div class="flex items-center justify-between mb-1">
                  <label class="block text-xs font-bold text-label">Chọn Môn học (chọn được nhiều môn) *</label>
                  <button v-if="filteredMasterSubjects.length" @click="toggleSelectFiltered" class="text-[11px] font-bold text-teal-600 hover:underline">
                    {{ filteredMasterSubjects.every(o => selectedSubjectIds.includes(o.value)) ? 'Bỏ chọn tất cả' : `Chọn tất cả (${filteredMasterSubjects.length})` }}
                  </button>
                </div>
                <div class="relative mb-2">
                  <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-label" />
                  <input
                    v-model="subjectSearchQuery"
                    type="text"
                    placeholder="Nhập mã / tên môn để tìm..."
                    class="glass-input w-full text-xs pl-8"
                  />
                </div>
                <div class="max-h-56 overflow-y-auto border border-card rounded-lg p-2 flex flex-col gap-1 text-xs surface-input">
                  <label
                    v-for="opt in filteredMasterSubjects"
                    :key="opt.value"
                    class="flex items-center gap-2 cursor-pointer hover:bg-slate-500/10 p-1.5 rounded"
                  >
                    <input
                      type="checkbox"
                      :value="opt.value"
                      v-model="selectedSubjectIds"
                      class="text-teal-600 rounded"
                    />
                    <span class="text-heading font-medium">{{ opt.label }}</span>
                  </label>
                  <span v-if="filteredMasterSubjects.length === 0" class="text-label italic">
                    {{ subjectSearchQuery ? 'Không tìm thấy môn phù hợp.' : 'Không còn môn nào để thêm (đã có trong khung).' }}
                  </span>
                </div>
                <p class="text-[11px] text-teal-600 dark:text-teal-400 font-bold mt-1.5">
                  Đã chọn {{ selectedSubjectIds.length }} môn
                </p>
              </div>

              <div class="grid grid-cols-2 gap-3">
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Học kỳ dự kiến</label>
                  <input v-model="subjectForm.hocKyDuKien" type="number" min="1" :max="maxSemesters" class="glass-input w-full text-xs" />
                </div>
                <div class="form-group">
                  <label class="block text-xs font-bold text-label mb-1">Số tín chỉ</label>
                  <input v-model="subjectForm.soTinChi" type="number" min="1" max="10" class="glass-input w-full text-xs" />
                </div>
              </div>

              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Loại môn học</label>
                <div class="flex items-center gap-4 mt-1 text-xs">
                  <label class="flex items-center gap-1.5 cursor-pointer text-heading">
                    <input type="radio" :value="true" v-model="subjectForm.batBuoc" class="text-teal-600" /> Bắt buộc
                  </label>
                  <label class="flex items-center gap-1.5 cursor-pointer text-heading">
                    <input type="radio" :value="false" v-model="subjectForm.batBuoc" class="text-teal-600" /> Tự chọn
                  </label>
                </div>
              </div>

              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Chọn Môn Tiên Quyết (Nếu có)</label>
                <div class="max-h-32 overflow-y-auto border border-card rounded-lg p-2 flex flex-col gap-1 text-xs surface-input">
                  <label 
                    v-for="opt in possiblePrerequisiteOptions" 
                    :key="opt.id"
                    class="flex items-center gap-2 cursor-pointer hover:bg-slate-500/10 p-1 rounded"
                  >
                    <input 
                      type="checkbox" 
                      :value="opt.id" 
                      v-model="subjectForm.maMonTienQuyetIds"
                      class="text-teal-600 rounded" 
                    />
                    <span class="text-heading font-medium">{{ opt.name }}</span>
                  </label>
                  <span v-if="possiblePrerequisiteOptions.length === 0" class="text-label italic">
                    Chưa có môn khác trong khung để chọn tiên quyết.
                  </span>
                </div>
              </div>

              <div class="form-group">
                <label class="block text-xs font-bold text-label mb-1">Ghi chú</label>
                <input v-model="subjectForm.ghiChu" type="text" class="glass-input w-full text-xs" placeholder="Ghi chú thêm về môn trong khung..." />
              </div>
            </div>

            <div class="flex gap-2 pt-2 border-t border-slate-500/10">
              <button @click="isSubjectModalOpen = false" class="glass-btn secondary flex-1 text-xs justify-center">Hủy</button>
              <button @click="saveSubject" class="glass-btn primary flex-1 text-xs justify-center">Lưu môn học</button>
            </div>
          </div>
        </div>

      </div>
    </div>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.7);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 1rem;
}
</style>
