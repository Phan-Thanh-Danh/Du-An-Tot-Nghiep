<script setup>
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import {
  ArrowUpDown,
  Award,
  CheckCircle2,
  Download,
  Filter,
  Printer,
  Search,
  TrendingUp,
  Users,
  XCircle,
  Calendar,
  ArrowLeft,
  Eye,
  X,
  FileText,
  RotateCcw,
  BookOpen,
  GraduationCap,
  Layers
} from 'lucide-vue-next'

import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import TableShell from '@/components/ui/TableShell.vue'
import TeacherClassCard from '@/components/GiangVien/TeacherClassCard.vue'
import LmsSelect from '@/components/LmsSelect.vue'
import { teacherApi } from '@/services/teacherApi'

const route = useRoute()
const router = useRouter()

const myClasses = ref([])
const selectedCourseId = ref('')
const selectedClassName = ref('')
const selectedCourseName = ref('')

// ── Search & Cascading Filter States ──
const searchKeyword = ref('')
const selectedMajor = ref('')
const selectedSpecialization = ref('')
const selectedSubject = ref('')
const selectedSemester = ref('')

const majorOptions = computed(() => {
  const map = new Map()
  myClasses.value.forEach(c => {
    const mId = c.maNganh ?? c.MaNganh
    const mName = c.tenNganh ?? c.TenNganh
    if (mId && mName) {
      map.set(String(mId), mName)
    }
  })
  return Array.from(map.entries()).map(([id, name]) => ({ id, name }))
})

const specializationOptions = computed(() => {
  const map = new Map()
  myClasses.value.forEach(c => {
    const mId = c.maNganh ?? c.MaNganh
    if (selectedMajor.value && String(mId) !== String(selectedMajor.value)) return
    const cnId = c.maChuyenNganh ?? c.MaChuyenNganh
    const cnName = c.tenChuyenNganh ?? c.TenChuyenNganh
    if (cnId && cnName) {
      map.set(String(cnId), cnName)
    }
  })
  return Array.from(map.entries()).map(([id, name]) => ({ id, name }))
})

const subjectOptions = computed(() => {
  const map = new Map()
  myClasses.value.forEach(c => {
    const mId = c.maNganh ?? c.MaNganh
    const cnId = c.maChuyenNganh ?? c.MaChuyenNganh
    if (selectedMajor.value && String(mId) !== String(selectedMajor.value)) return
    if (selectedSpecialization.value && String(cnId) !== String(selectedSpecialization.value)) return
    const subId = c.maMonHoc ?? c.MaMonHoc
    const subName = c.tenMonHoc ?? c.TenMonHoc
    const subCode = c.maMonHocCode ?? c.MaMonHocCode
    if (subId && subName) {
      map.set(String(subId), `${subCode ? subCode + ' - ' : ''}${subName}`)
    }
  })
  return Array.from(map.entries()).map(([id, name]) => ({ id, name }))
})

const semesterOptions = computed(() => {
  const map = new Map()
  myClasses.value.forEach(c => {
    const termId = c.maHocKy ?? c.MaHocKy
    const termName = c.tenHocKy ?? c.TenHocKy
    if (termId && termName) {
      map.set(String(termId), termName)
    } else if (termName) {
      map.set(termName, termName)
    }
  })
  return Array.from(map.entries()).map(([id, name]) => ({ id, name }))
})

// Auto-reset dependent filters when parent filter changes
watch(selectedMajor, () => {
  selectedSpecialization.value = ''
  selectedSubject.value = ''
})

watch(selectedSpecialization, () => {
  selectedSubject.value = ''
})

const filteredClasses = computed(() => {
  let list = myClasses.value
  if (selectedMajor.value) {
    list = list.filter(c => String(c.maNganh ?? c.MaNganh) === String(selectedMajor.value))
  }
  if (selectedSpecialization.value) {
    list = list.filter(c => String(c.maChuyenNganh ?? c.MaChuyenNganh) === String(selectedSpecialization.value))
  }
  if (selectedSubject.value) {
    list = list.filter(c => String(c.maMonHoc ?? c.MaMonHoc) === String(selectedSubject.value))
  }
  if (selectedSemester.value) {
    list = list.filter(c => String(c.maHocKy ?? c.MaHocKy) === String(selectedSemester.value) || String(c.tenHocKy ?? c.TenHocKy) === String(selectedSemester.value))
  }
  if (searchKeyword.value.trim()) {
    const q = searchKeyword.value.trim().toLowerCase()
    list = list.filter(c => 
      (c.tenMonHoc || c.TenMonHoc || '').toLowerCase().includes(q) ||
      (c.maMonHocCode || c.MaMonHocCode || '').toLowerCase().includes(q) ||
      (c.tenLop || c.TenLop || '').toLowerCase().includes(q) ||
      (c.tieuDe || c.TieuDe || '').toLowerCase().includes(q)
    )
  }
  return list
})

const totalFilteredStudents = computed(() => {
  return filteredClasses.value.reduce((acc, c) => acc + (c.siSo || c.studentCount || c.studentsCount || 0), 0)
})

const hasActiveFilters = computed(() => {
  return !!(searchKeyword.value.trim() || selectedMajor.value || selectedSpecialization.value || selectedSubject.value || selectedSemester.value)
})

function resetFilters() {
  searchKeyword.value = ''
  selectedMajor.value = ''
  selectedSpecialization.value = ''
  selectedSubject.value = ''
  selectedSemester.value = ''
}

const gradebook = ref([])
const gradeColumns = ref([])
const loading = ref(false)
const searchQuery = ref('')
const statusFilter = ref('')

// ── Detail modal ──
const showDetailModal = ref(false)
const detailLoading = ref(false)
const detailData = ref(null)
const detailStudentName = ref('')

function formatGrade(value) {
  if (value === null || value === undefined) return '—'
  return Number(value).toFixed(2)
}

const filteredGradebook = computed(() => {
  let list = gradebook.value
  if (statusFilter.value) {
    list = list.filter(sv => sv.status === statusFilter.value)
  }
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.trim().toLowerCase()
    list = list.filter(sv =>
      (sv.name || '').toLowerCase().includes(q) ||
      String(sv.id || '').toLowerCase().includes(q)
    )
  }
  return list
})

// ── Detail modal functions ──
const flatColumns = computed(() => {
  if (!detailData.value) return []
  const gts = detailData.value.gradeTypes ?? detailData.value.GradeTypes ?? []
  const typeGradesMap = detailData.value.typeGrades ?? detailData.value.TypeGrades ?? {}
  
  const cols = []
  gts.forEach(gt => {
    const code = gt.code ?? gt.Code
    const items = gt.items ?? gt.Items ?? []
    const typeWeight = gt.weight ?? gt.Weight ?? 0
    const avgGrade = gt.averageGrade ?? gt.AverageGrade ?? gt.grade ?? gt.Grade ?? typeGradesMap[code] ?? null

    if (items.length === 0) {
      cols.push({
        gtCode: code,
        gtName: gt.name ?? gt.Name,
        weight: typeWeight,
        itemName: '-',
        grade: avgGrade
      })
    } else {
      const itemWeight = (typeWeight / items.length)
      items.forEach(item => {
        cols.push({
          gtCode: code,
          gtName: gt.name ?? gt.Name,
          weight: itemWeight,
          itemName: item.itemName ?? item.ItemName ?? '-',
          grade: item.grade ?? item.Grade ?? null
        })
      })
    }
  })

  const dGK = detailData.value.diemGiuaKy ?? detailData.value.DiemGiuaKy
  const dCK = detailData.value.diemCuoiKy ?? detailData.value.DiemCuoiKy

  if (dGK !== undefined && dGK !== null) {
    cols.push({ gtCode: 'giua_ky', gtName: 'Điểm Giữa Kỳ', weight: 20, itemName: '-', grade: dGK })
  }
  if (dCK !== undefined && dCK !== null) {
    cols.push({ gtCode: 'cuoi_ky', gtName: 'Điểm Cuối Kỳ', weight: 50, itemName: '-', grade: dCK })
  }

  return cols
})

const calculatedDetailGpa = computed(() => {
  if (!detailData.value) return 0
  const apiGpa = detailData.value.gpaMonHoc ?? detailData.value.GpaMonHoc ?? detailData.value.gpa ?? detailData.value.Gpa
  if (apiGpa !== null && apiGpa !== undefined && apiGpa !== '') {
    return Number(apiGpa)
  }
  if (flatColumns.value.length > 0) {
    let weightedSum = 0
    let totalWeight = 0
    flatColumns.value.forEach(col => {
      const g = col.grade ?? col.Grade
      const w = col.weight || 0
      if (g !== null && g !== undefined && g !== '') {
        weightedSum += Number(g) * w
        totalWeight += w
      }
    })
    if (totalWeight > 0) {
      return Number((weightedSum / totalWeight).toFixed(2))
    }
  }
  return 0
})

async function openDetail(sv) {
  detailStudentName.value = sv.name
  detailData.value = null
  showDetailModal.value = true
  detailLoading.value = true
  try {
    const cls = myClasses.value.find(c => String(c.maKhoaHoc) === String(selectedCourseId.value))
    if (!cls) return
    const res = await teacherApi.getStudentGradeDetail(cls.maLop, sv.id, selectedCourseId.value)
    const data = res?.data ?? res?.Data ?? res
    if (data) {
      data.typeGrades = data.typeGrades ?? data.TypeGrades ?? {}
    }
    detailData.value = data
  } catch (error) {
    console.error('Lỗi khi tải chi tiết điểm:', error)
  } finally {
    detailLoading.value = false
  }
}

function closeDetail() {
  showDetailModal.value = false
  detailData.value = null
}

function printGradebook() {
  if (typeof window !== 'undefined') {
    window.print()
  }
}

const avgGPA = computed(() => {
  if (!gradebook.value.length) return 0
  const sum = gradebook.value.reduce((acc, sv) => acc + Number(sv.gpa || 0), 0)
  return (sum / gradebook.value.length).toFixed(2)
})
const passRate = computed(() => {
  const gradedStudents = gradebook.value.filter(sv => sv.status !== 'Pending')
  if (!gradedStudents.length) return 0
  const passed = gradedStudents.filter((sv) => sv.status === 'Pass').length
  return Math.round((passed / gradedStudents.length) * 100)
})

const summaryStats = computed(() => {
  const passed = gradebook.value.filter((student) => student.status === 'Pass').length
  const failed = gradebook.value.filter((student) => student.status === 'Fail').length
  const totalCredits = gradebook.value.length * 3

  return [
    { label: 'Sinh viên', value: gradebook.value.length, tone: 'primary' },
    { label: 'GPA TB', value: avgGPA.value, tone: 'neutral' },
    { label: 'Đạt', value: passed, tone: 'success' },
    { label: 'Rớt', value: failed, tone: 'danger' },
    { label: 'Tín chỉ', value: totalCredits, tone: 'info' },
    { label: 'Hoàn thành', value: `${passRate.value}%`, tone: 'success' },
  ]
})

function statusVariant(status) {
  if (status === 'Pass') return 'success'
  if (status === 'Fail') return 'danger'
  return 'neutral'
}

function statusLabel(status) {
  if (status === 'Pass') return 'Đạt'
  if (status === 'Fail') return 'Rớt'
  return 'Chưa có điểm'
}

const getClassesList = async () => {
  loading.value = true
  try {
    const classesRes = await teacherApi.getClasses()
    const classesData = classesRes?.data?.items ?? classesRes?.data ?? classesRes?.items ?? classesRes
    if (classesData && Array.isArray(classesData)) {
      myClasses.value = classesData
    }
  } catch (error) {
    console.error("Lỗi tải danh sách khóa học:", error)
  } finally {
    loading.value = false
  }
}

async function loadGrades() {
  const courseId = route.query.courseId
  
  if (!courseId) {
    selectedCourseId.value = ''
    gradebook.value = []
    if (myClasses.value.length === 0) {
      await getClassesList()
    }
    return
  }

  selectedCourseId.value = courseId
  if (myClasses.value.length === 0) {
    await getClassesList()
  }
  const cls = myClasses.value.find(c => String(c.maKhoaHoc) === String(courseId))
  selectedClassName.value = cls ? cls.tenLop : `Lớp của khóa ${courseId}`
  selectedCourseName.value = cls ? cls.tenMonHoc : `Khóa học ${courseId}`

  loading.value = true
  try {
    const res = await teacherApi.getClassGradesV2(cls?.maLop || 0, courseId)
    const data = res?.data ?? res?.Data ?? res
    gradeColumns.value = data?.gradeColumns ?? data?.GradeColumns ?? [
      { code: 'chuyen_can', name: 'Chuyên cần' },
      { code: 'quiz', name: 'Quiz' },
      { code: 'lab', name: 'Lab' },
      { code: 'assignment', name: 'Assignment' }
    ]
    const items = data?.students ?? data?.Students ?? []
    gradebook.value = items.map((sv) => {
      const typeGrades = sv.typeGrades ?? sv.TypeGrades ?? {}
      
      let dQT = sv.diemQuaTrinh ?? sv.DiemQuaTrinh ?? sv.diemQT ?? null
      let dGK = sv.diemGiuaKy ?? sv.DiemGiuaKy ?? sv.diemGK ?? null
      let dCK = sv.diemCuoiKy ?? sv.DiemCuoiKy ?? sv.diemCK ?? null
      let gpaFromDb = sv.gpaMonHoc ?? sv.GpaMonHoc ?? sv.gpa ?? sv.Gpa ?? null
      
      // Calculate dQT from typeGrades if dQT is null
      const validTgValues = Object.values(typeGrades).filter(v => v !== null && v !== undefined && !isNaN(v) && Number(v) > 0)
      if (dQT === null && validTgValues.length > 0) {
        const sum = validTgValues.reduce((acc, v) => acc + Number(v), 0)
        dQT = Number((sum / validTgValues.length).toFixed(2))
      }

      let calculatedGpa = gpaFromDb !== null && gpaFromDb !== undefined ? Number(gpaFromDb) : null
      if (calculatedGpa === null && dQT !== null && dGK !== null && dCK !== null) {
        calculatedGpa = Number((Number(dQT) * 0.3 + Number(dGK) * 0.2 + Number(dCK) * 0.5).toFixed(2))
      }

      const hasGrades = dQT !== null || dGK !== null || dCK !== null || calculatedGpa !== null
      const status = !hasGrades ? 'Pending' : (calculatedGpa !== null && calculatedGpa >= 5.0 ? 'Pass' : 'Fail')
      const trangThai = !hasGrades ? 'Chưa có điểm' : (calculatedGpa !== null && calculatedGpa >= 5.0 ? 'Đạt' : 'Rớt')

      return {
        id: sv.studentId ?? sv.StudentId ?? sv.id,
        name: sv.studentName ?? sv.StudentName ?? sv.name,
        typeGrades: typeGrades,
        diemQuaTrinh: dQT,
        diemGiuaKy: dGK,
        diemCuoiKy: dCK,
        gpa: calculatedGpa,
        status: status,
        trangThai: trangThai,
        daKhoa: sv.daKhoa ?? false,
        credits: 3
      }
    })
  } catch (error) {
    console.error("Lỗi khi tải bảng điểm:", error)
    gradebook.value = []
  } finally {
    loading.value = false
  }
}

const goToGrades = (id) => {
  router.push({ query: { ...route.query, courseId: id } })
}

const goBack = () => {
  const q = { ...route.query }
  delete q.courseId
  router.push({ query: q })
}

const exporting = ref(false)
const handleExport = async () => {
  if (!selectedCourseId.value) return
  exporting.value = true
  try {
    const cls = myClasses.value.find(c => String(c.maKhoaHoc) === String(selectedCourseId.value))
    if (!cls) return
    const blob = await teacherApi.exportClassGrades(cls.maLop)
    const url = window.URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    const safeName = (selectedCourseName.value || `KhoaHoc_${selectedCourseId.value}`).replace(/ /g, '_')
    a.download = `BangDiem_${safeName}.xlsx`
    document.body.appendChild(a)
    a.click()
    a.remove()
    window.URL.revokeObjectURL(url)
  } catch (error) {
    console.error("Lỗi khi xuất bảng điểm:", error)
    alert("Không thể xuất bảng điểm lúc này. Vui lòng thử lại sau.")
  } finally {
    exporting.value = false
  }
}

watch(() => route.query.courseId, () => {
  loadGrades()
})

onMounted(loadGrades)
</script>

<template>
  <div class="gradebook-page lg-page-enter">
    <div v-if="loading && !route.query.courseId && myClasses.length === 0">
      <GlassPanel variant="flat" density="compact" class="loading-panel">
        <p>Đang tải danh sách khóa học...</p>
      </GlassPanel>
    </div>
    
    <!-- CARD GRID VIEW (NO CLASS SELECTED) -->
    <template v-else-if="!route.query.courseId">
      <div class="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-5">
        <div>
          <h1 class="text-xl font-bold text-heading tracking-tight">Quản lý điểm khóa học</h1>
          <p class="text-muted text-xs mt-0.5">Chọn khóa học để xem và quản lý kết quả học tập của sinh viên.</p>
        </div>
        <div class="flex items-center gap-3 text-xs">
          <div class="px-3 py-1.5 rounded-xl surface-card border border-card flex items-center gap-2 font-medium text-heading">
            <BookOpen :size="14" class="text-blue-500" />
            <span><strong>{{ filteredClasses.length }}</strong> khóa học</span>
          </div>
          <div class="px-3 py-1.5 rounded-xl surface-card border border-card flex items-center gap-2 font-medium text-heading">
            <Users :size="14" class="text-emerald-500" />
            <span><strong>{{ totalFilteredStudents }}</strong> sinh viên</span>
          </div>
        </div>
      </div>

      <!-- FILTER & SEARCH PANEL -->
      <div class="surface-card border border-card rounded-2xl p-4 mb-6 space-y-3 shadow-xs">
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-3">
          <!-- Search input -->
          <div class="relative lg:col-span-2">
            <Search :size="15" class="absolute left-3 top-1/2 -translate-y-1/2 text-muted" />
            <input
              v-model="searchKeyword"
              type="text"
              placeholder="Tìm theo tên môn, mã môn, tên lớp..."
              class="w-full pl-9 pr-3 py-2 rounded-xl surface-input border border-card text-xs text-heading focus:outline-hidden focus:border-blue-500 transition-colors"
            />
          </div>

          <!-- Major Filter -->
          <div>
            <select
              v-model="selectedMajor"
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-xs text-heading focus:outline-hidden focus:border-blue-500 transition-colors"
            >
              <option value="">Tất cả ngành</option>
              <option v-for="m in majorOptions" :key="m.id" :value="m.id">
                {{ m.name }}
              </option>
            </select>
          </div>

          <!-- Specialization Filter -->
          <div>
            <select
              v-model="selectedSpecialization"
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-xs text-heading focus:outline-hidden focus:border-blue-500 transition-colors"
            >
              <option value="">Tất cả chuyên ngành</option>
              <option v-for="s in specializationOptions" :key="s.id" :value="s.id">
                {{ s.name }}
              </option>
            </select>
          </div>

          <!-- Semester Filter -->
          <div>
            <select
              v-model="selectedSemester"
              class="w-full px-3 py-2 rounded-xl surface-input border border-card text-xs text-heading focus:outline-hidden focus:border-blue-500 transition-colors"
            >
              <option value="">Tất cả học kỳ</option>
              <option v-for="t in semesterOptions" :key="t.id" :value="t.id">
                {{ t.name }}
              </option>
            </select>
          </div>
        </div>

        <div class="flex flex-wrap items-center justify-between gap-3 pt-2 border-t border-card/50 text-xs">
          <!-- Subject Filter inline -->
          <div class="flex items-center gap-2 min-w-0 flex-1">
            <span class="text-muted font-medium shrink-0">Môn học:</span>
            <select
              v-model="selectedSubject"
              class="px-3 py-1.5 rounded-xl surface-input border border-card text-xs text-heading focus:outline-hidden focus:border-blue-500 max-w-xs transition-colors"
            >
              <option value="">Tất cả môn học ({{ subjectOptions.length }})</option>
              <option v-for="sub in subjectOptions" :key="sub.id" :value="sub.id">
                {{ sub.name }}
              </option>
            </select>
          </div>

          <!-- Reset Button -->
          <button
            v-if="hasActiveFilters"
            type="button"
            @click="resetFilters"
            class="px-3 py-1.5 rounded-xl text-xs font-semibold text-rose-600 hover:bg-rose-50 dark:hover:bg-rose-950/30 border border-rose-200 dark:border-rose-800/40 flex items-center gap-1.5 transition-colors cursor-pointer"
          >
            <RotateCcw :size="13" /> Đặt lại bộ lọc
          </button>
        </div>
      </div>
      
      <div v-if="myClasses.length === 0" class="text-center p-12 surface-card rounded-2xl border border-card">
        <Users class="mx-auto h-12 w-12 text-muted mb-3" />
        <p class="text-muted">Bạn chưa được phân công giảng dạy lớp nào.</p>
      </div>

      <div v-else-if="filteredClasses.length === 0" class="text-center p-12 surface-card rounded-2xl border border-card">
        <Search class="mx-auto h-10 w-10 text-muted mb-3" />
        <p class="text-heading font-semibold">Không tìm thấy khóa học nào phù hợp</p>
        <p class="text-muted text-xs mt-1">Hãy thử thay đổi từ khóa tìm kiếm hoặc điều chỉnh bộ lọc ngành/môn học.</p>
        <button
          type="button"
          @click="resetFilters"
          class="mt-3 px-4 py-1.5 rounded-xl text-xs font-bold bg-blue-600 hover:bg-blue-700 text-white transition-colors cursor-pointer"
        >
          Xóa bộ lọc
        </button>
      </div>
      
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <TeacherClassCard
          v-for="cls in filteredClasses"
          :key="cls.maKhoaHoc"
          :title="`${cls.tenMonHoc} - ${cls.tenLop}`"
          :semester="cls.tenHocKy || 'Học kỳ hiện tại'"
          :studentsCount="cls.siSo || cls.studentCount || cls.studentsCount || 0"
        >
          <template #action>
            <button
              @click="goToGrades(cls.maKhoaHoc)"
              class="w-full flex justify-center items-center gap-2 group-hover:bg-(--accent-primary) group-hover:text-inverse transition-all bg-slate-100 dark:bg-slate-800 px-4 py-2.5 rounded-xl text-xs font-bold cursor-pointer"
            >
              Xem sổ điểm
              <ArrowUpDown class="w-4 h-4 rotate-90" />
            </button>
          </template>
        </TeacherClassCard>
      </div>
    </template>
    
    <!-- CLASS GRADEBOOK VIEW (CLASS SELECTED) -->
    <template v-else>
      <div class="flex items-center gap-2 mb-2">
        <GlassButton variant="secondary" size="sm" @click="goBack" class="!px-2">
          <template #leading><ArrowLeft :size="16" /></template>
        </GlassButton>
        <span class="text-sm text-muted">Quay lại danh sách khóa học</span>
      </div>

      <GlassPanel variant="flat" density="compact" class="page-header">
        <div class="header-copy">
          <div class="eyebrow">
            <Users :size="15" />
            {{ selectedClassName }}
          </div>
          <div>
            <h1>Sổ điểm khóa {{ selectedCourseName }}</h1>
            <p>Tổng hợp kết quả học tập và trạng thái hoàn thành môn học của {{ selectedClassName }}.</p>
          </div>
        </div>
  
        <div class="header-actions">
          <GlassButton variant="secondary" size="sm" @click="printGradebook">
            <template #leading>
              <Printer :size="16" />
            </template>
            In bảng điểm
          </GlassButton>
          <GlassButton variant="primary" size="sm" :loading="exporting" @click="handleExport">
            <template #leading v-if="!exporting">
              <Download :size="16" />
            </template>
            Xuất bảng điểm
          </GlassButton>
        </div>
      </GlassPanel>
  
      <GlassPanel variant="flat" density="compact" class="context-panel">
        <div class="metric-strip">
          <div class="metric-card">
            <span class="metric-icon">
              <TrendingUp :size="17" />
            </span>
            <div>
              <p>GPA trung bình</p>
              <strong>{{ avgGPA }}<small>/10.0</small></strong>
              <div class="progress-track" aria-hidden="true">
                <span :style="{ width: `${avgGPA * 10}%` }" />
              </div>
            </div>
          </div>
  
          <div class="metric-card">
            <span class="metric-icon success">
              <CheckCircle2 :size="17" />
            </span>
            <div>
              <p>Tỷ lệ đạt</p>
              <strong>{{ passRate }}<small>% hoàn thành</small></strong>
              <div class="progress-track" aria-hidden="true">
                <span :style="{ width: `${passRate}%` }" />
              </div>
            </div>
          </div>
        </div>
  
        <div class="summary-strip">
          <div v-for="item in summaryStats" :key="item.label" :class="['summary-pill', item.tone]">
            <span>{{ item.label }}</span>
            <strong>{{ item.value }}</strong>
          </div>
        </div>
      </GlassPanel>
  
      <GlassPanel variant="flat" density="compact" class="table-panel">
        <div class="table-toolbar">
          <div>
            <h2>Bảng kết quả chi tiết</h2>
            <p>{{ filteredGradebook.length }} sinh viên · Học kỳ hiện tại</p>
          </div>
          <div class="filters">
            <label class="search-field">
              <Search :size="16" />
              <input v-model="searchQuery" type="text" placeholder="Tìm sinh viên..." />
            </label>
            <LmsSelect v-model="statusFilter" class="w-36">
              <option value="">Tất cả trạng thái</option>
              <option value="Pass">Đạt</option>
              <option value="Fail">Rớt</option>
            </LmsSelect>
          </div>
        </div>
  
        <div v-if="loading" class="py-8 text-center text-muted">
          Đang tải dữ liệu điểm...
        </div>
        <TableShell v-else density="compact">
          <table>
            <thead>
              <tr>
                <th>Sinh viên</th>
                <th v-for="col in gradeColumns" :key="col.code ?? col.Code">
                  <span class="sortable-label">
                    {{ col.name ?? col.Name }}
                    <template v-if="(col.code ?? col.Code) !== 'chuyen_can'">(TB)</template>
                    <ArrowUpDown :size="12" />
                  </span>
                </th>
                <th>Điểm QT</th>
                <th>Giữa kỳ</th>
                <th>Cuối kỳ</th>
                <th>
                  <span class="sortable-label total-label">
                    Tổng kết
                    <ArrowUpDown :size="12" />
                  </span>
                </th>
                <th>Trạng thái</th>
                <th class="text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sv in filteredGradebook" :key="sv.id">
                <td>
                  <div class="student-cell">
                    <span class="student-avatar">{{ sv.name.split(' ').pop()[0] ?? '?' }}</span>
                    <span>
                      <strong>{{ sv.name }}</strong>
                      <small>{{ sv.id }}</small>
                    </span>
                  </div>
                </td>
                
                <td v-for="col in gradeColumns" :key="(col.code ?? col.Code) + '-' + sv.id">
                  <span class="grade-value">{{ formatGrade((sv.typeGrades ?? sv.TypeGrades)?.[col.code ?? col.Code]) }}</span>
                </td>
                
                <td><span class="grade-value">{{ formatGrade(sv.diemQuaTrinh) }}</span></td>
                <td><span class="grade-value">{{ formatGrade(sv.diemGiuaKy) }}</span></td>
                <td><span class="grade-value">{{ formatGrade(sv.diemCuoiKy) }}</span></td>

                <td>
                  <strong :class="['total-score', sv.status === 'Fail' ? 'failed' : sv.status === 'Pass' ? 'passed' : '']">
                    {{ formatGrade(sv.gpa) }}
                  </strong>
                </td>
                <td>
                  <GlassBadge :variant="statusVariant(sv.status)">
                    {{ statusLabel(sv.status) }}
                  </GlassBadge>
                </td>
                <td>
                  <div class="row-actions" style="justify-content: flex-end;">
                    <GlassButton variant="secondary" size="sm" @click="openDetail(sv)">
                      <template #leading>
                        <Eye :size="14" />
                      </template>
                      Xem chi tiết
                    </GlassButton>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </TableShell>
      </GlassPanel>
    </template>
    
    <!-- ═══ MODAL: Xem chi tiết điểm (Glassmorphism Re-design) ═══ -->
    <Teleport to="body">
      <div v-if="showDetailModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-xs" @click.self="closeDetail">
        <div class="w-full max-w-3xl surface-card rounded-2xl shadow-2xl border border-card p-6 space-y-4 max-h-[90vh] overflow-y-auto">
          <div class="flex justify-between items-center border-b border-card pb-3">
            <div>
              <h3 class="text-base font-bold text-heading flex items-center gap-2">
                <FileText :size="18" class="text-link" /> Bảng Điểm Chi Tiết Sinh Viên
              </h3>
              <p class="text-xs text-muted mt-0.5">{{ detailStudentName }}</p>
            </div>
            <button @click="closeDetail" class="text-muted hover:text-heading font-bold text-sm">✕</button>
          </div>

          <div v-if="detailLoading" class="py-8 text-center text-muted text-sm">
            Đang tải dữ liệu điểm chi tiết...
          </div>

          <template v-else-if="detailData">
            <div class="overflow-x-auto rounded-xl border border-card surface-card">
              <table class="w-full text-left text-xs border-collapse">
                <thead>
                  <tr class="bg-(--surface-input) border-b border-card">
                    <th class="py-3 px-4 font-bold text-heading">Thành phần điểm</th>
                    <th class="py-3 px-3 text-center font-bold text-heading">Trọng số</th>
                    <th class="py-3 px-3 text-center font-bold text-heading">Điểm thành phần</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-card">
                  <tr v-for="(col, index) in flatColumns" :key="'col-' + index" class="hover:bg-(--surface-input)/50 transition-colors">
                    <td class="py-2.5 px-4 font-medium text-body">
                      {{ col.gtName }} <span v-if="col.itemName && col.itemName !== '-'" class="text-muted">({{ col.itemName }})</span>
                    </td>
                    <td class="py-2.5 px-3 text-center font-semibold text-muted">
                      {{ (col.weight || 0).toFixed(1).replace('.0', '') }}%
                    </td>
                    <td class="py-2.5 px-3 text-center font-mono font-bold text-heading">
                      {{ (col.grade ?? col.Grade) === null ? '—' : formatGrade(col.grade ?? col.Grade) }}
                    </td>
                  </tr>
                </tbody>
                <tfoot>
                  <tr class="bg-(--accent-primary-soft) border-t border-card">
                    <td class="py-3 px-4 font-bold text-heading">Tổng kết GPA môn học</td>
                    <td class="py-3 px-3 text-center font-bold text-heading">100%</td>
                    <td class="py-3 px-3 text-center font-mono font-black text-lg text-link">
                      {{ formatGrade(calculatedDetailGpa) }}
                    </td>
                  </tr>
                </tfoot>
              </table>
            </div>
          </template>
          <div v-else class="py-8 text-center text-muted text-sm">
            Không thể tải dữ liệu chi tiết điểm.
          </div>

          <div class="flex justify-end pt-2">
            <button @click="closeDetail" class="px-4 py-2 bg-(--lg-primary) text-white text-xs font-bold rounded-xl hover:opacity-90 transition-all">Đóng</button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<style scoped>
.gradebook-page {
  display: flex;
  flex-direction: column;
  gap: 0.875rem;
  padding-bottom: 2.5rem;
  color: var(--text-body);
}

.page-header,
.context-panel,
.table-toolbar,
.header-actions,
.filters,
.summary-strip,
.metric-strip,
.sortable-label,
.student-cell,
.gpa-cell {
  display: flex;
  align-items: center;
}

.page-header,
.context-panel,
.table-toolbar {
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
}

.header-copy {
  display: flex;
  min-width: 0;
  flex-direction: column;
  gap: 0.5rem;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  width: fit-content;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-input);
  color: var(--text-link);
  padding: 0.25rem 0.6rem;
  font-size: 0.7rem;
  font-weight: 850;
  text-transform: uppercase;
}

.header-copy h1 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1.45rem;
  line-height: 1.15;
  font-weight: 900;
}

.header-copy p,
.table-toolbar p,
.metric-card p,
.summary-pill span,
.student-cell small,
.note-cell,
.student-code {
  color: var(--text-muted);
}

.header-copy p,
.table-toolbar p {
  margin: 0.25rem 0 0;
  font-size: 0.84rem;
}

.header-actions,
.filters,
.summary-strip,
.metric-strip {
  gap: 0.55rem;
  flex-wrap: wrap;
}

.context-panel {
  align-items: stretch;
}

.metric-strip {
  flex: 1;
}

.metric-card {
  display: flex;
  min-width: min(17rem, 100%);
  flex: 1;
  gap: 0.75rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  padding: 0.75rem;
}

.metric-icon {
  display: inline-flex;
  width: 2.1rem;
  height: 2.1rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--accent-primary-soft);
  color: var(--text-link);
}

.metric-icon.success {
  background: var(--color-success-bg);
  color: var(--color-success-text);
}

.metric-card div {
  min-width: 0;
  flex: 1;
}

.metric-card p {
  margin: 0;
  font-size: 0.72rem;
  font-weight: 850;
  text-transform: uppercase;
}

.metric-card strong {
  display: block;
  margin-top: 0.15rem;
  color: var(--text-heading);
  font-size: 1.2rem;
  font-weight: 900;
}

.metric-card small {
  margin-left: 0.25rem;
  color: var(--text-muted);
  font-size: 0.72rem;
  font-weight: 750;
}

.progress-track {
  width: 100%;
  height: 0.45rem;
  margin-top: 0.45rem;
  border: 1px solid var(--border-card);
  border-radius: 999px;
  background: var(--surface-card);
  overflow: hidden;
}

.progress-track span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: var(--accent-primary);
}

.summary-strip {
  max-width: 28rem;
  justify-content: flex-end;
}

.summary-pill {
  display: grid;
  min-width: 5rem;
  gap: 0.05rem;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0.45rem 0.6rem;
}

.summary-pill strong {
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.summary-pill span {
  font-size: 0.68rem;
  font-weight: 800;
}

.summary-pill.primary,
.summary-pill.info {
  background: var(--accent-primary-soft);
}

.summary-pill.success {
  background: var(--color-success-bg);
}

.summary-pill.danger {
  background: var(--color-danger-bg);
}

.table-panel {
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
}

.table-toolbar {
  border-bottom: 1px solid var(--border-card);
  padding-bottom: 0.75rem;
}

.table-toolbar h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 1rem;
  font-weight: 900;
}

.search-field {
  display: inline-flex;
  align-items: center;
  min-height: 2.25rem;
  width: min(18rem, 100%);
  gap: 0.45rem;
  border: 1px solid var(--border-input);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  padding: 0 0.7rem;
  color: var(--text-placeholder);
  transition:
    border-color 0.2s ease,
    background 0.2s ease,
    box-shadow 0.2s ease;
}

.search-field:focus-within {
  border-color: var(--border-input-focus);
  background: var(--surface-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.search-field input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-heading);
  font-size: 0.82rem;
  font-weight: 750;
}

.search-field input::placeholder {
  color: var(--text-placeholder);
}

.sortable-label {
  gap: 0.35rem;
  color: var(--text-link);
  white-space: nowrap;
}

.student-cell {
  min-width: 13rem;
  gap: 0.65rem;
}

.student-avatar {
  display: inline-flex;
  width: 2rem;
  height: 2rem;
  flex: none;
  align-items: center;
  justify-content: center;
  border: 1px solid var(--border-card);
  border-radius: var(--radius-md);
  background: var(--surface-input);
  color: var(--text-link);
  font-size: 0.75rem;
  font-weight: 900;
}

.student-cell strong {
  display: block;
  color: var(--text-heading);
  font-size: 0.86rem;
}

.student-cell small {
  display: block;
  margin-top: 0.1rem;
  font-size: 0.72rem;
  font-weight: 750;
}

.student-code,
.credits-cell,
.note-cell {
  font-size: 0.8rem;
  font-weight: 750;
}

.credits-cell {
  color: var(--text-heading);
}

.gpa-cell {
  min-width: 4.5rem;
  gap: 0.4rem;
  color: var(--text-link);
}

.gpa-cell strong {
  font-size: 0.95rem;
  font-weight: 900;
}

.gpa-cell strong.passed {
  color: var(--text-heading);
}

.gpa-cell strong.failed {
  color: var(--color-danger-text);
}

.note-cell {
  text-align: right;
  white-space: nowrap;
}

@media (max-width: 1024px) {
  .page-header,
  .context-panel,
  .table-toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .summary-strip {
    max-width: none;
    justify-content: flex-start;
  }
}

@media (max-width: 640px) {
  .header-actions,
  .filters,
  .summary-strip,
  .metric-strip {
    display: grid;
    grid-template-columns: 1fr;
  }

  .search-field,
  .summary-pill {
    width: 100%;
  }

  .note-cell {
    text-align: left;
  }
}
.detail-types-table-wrapper {
  border: 1px solid var(--border-card, #e2e8f0);
  border-radius: 8px;
  background-color: var(--surface-card, #ffffff);
}
.detail-table {
  width: 100%;
  border-collapse: collapse;
}
.detail-table th, .detail-table td {
  padding: 1rem 1.25rem;
  border-right: 1px solid var(--border-card, #e2e8f0);
  border-bottom: 1px solid var(--border-card, #e2e8f0);
  white-space: nowrap;
}
.detail-table th.sticky-col, .detail-table td.sticky-col {
  position: sticky;
  left: 0;
  background-color: var(--surface-card, #f8fafc);
  z-index: 10;
  border-right: 2px solid var(--border-card, #e2e8f0);
  box-shadow: 2px 0 5px rgba(0,0,0,0.02);
}
.detail-table thead th {
  background-color: var(--surface-card, #f8fafc);
  font-weight: 600;
  border-bottom: 2px solid var(--border-card, #e2e8f0);
}
.detail-modal {
  width: 95vw;
  max-width: 1600px;
  max-height: 90vh;
  overflow-y: auto;
  border-radius: 16px;
  padding: 2rem;
}
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.6);
  backdrop-filter: blur(8px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}
.detail-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 2rem;
}
.detail-header h2 {
  margin: 0;
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--text-heading);
}
.detail-header p {
  margin: 0.25rem 0 0;
  color: var(--text-muted);
  font-size: 0.9rem;
}
.text-success { color: var(--color-success-text, #059669); }
.text-danger { color: var(--color-danger-text, #dc2626); }
.text-center { text-align: center; }
</style>
