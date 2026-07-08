<template>
  <div class="space-y-4 pb-10">
    <!-- Loading State -->
    <div v-if="loading" class="flex items-center justify-center py-20">
      <div class="flex flex-col items-center gap-3 text-muted">
        <Loader2 :size="32" class="animate-spin" />
        <p class="text-sm font-medium">Đang tải dữ liệu...</p>
      </div>
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
    <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
      <div>
        <h2 class="sr-only text-xl font-bold text-heading">Khung chương trình</h2>
        <p class="text-xs text-muted mt-1">Chi tiết môn học theo từng học kỳ trong chương trình đào tạo</p>
      </div>
      <select v-model="selectedProgram" class="px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary) min-w-[240px]">
        <option v-for="p in programs" :key="p.maChuongTrinh" :value="p.maChuongTrinh">{{ p.tenChuongTrinh }}</option>
      </select>
    </div>

    <div v-if="currentProgram" class="surface-card border border-card rounded-2xl p-5 shadow-sm">
      <div class="flex items-center justify-between mb-4">
        <div>
          <h2 class="text-lg font-bold text-heading">{{ currentProgram.tenChuongTrinh }}</h2>
          <p class="text-xs text-muted mt-0.5">{{ currentProgram.tenChuyenNganh }} · {{ currentProgram.soHocKy }} học kỳ · {{ totalCredits }} tín chỉ</p>
        </div>
        <span :class="statusBadge(currentProgram.trangThai)" class="inline-flex items-center gap-1 px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider">
          {{ statusLabel(currentProgram.trangThai) }}
        </span>
      </div>
    </div>

    <div v-if="loadingCurriculum" class="flex justify-center py-10">
      <Loader2 class="animate-spin text-muted" :size="24" />
    </div>
    <div v-else-if="curriculumError" class="flex flex-col items-center gap-3 py-10 text-center">
      <AlertCircle :size="24" class="text-(--color-danger-text)" />
      <p class="text-sm text-(--color-danger-text) font-medium">{{ curriculumError }}</p>
    </div>
    <template v-else>
      <div v-for="semester in semesters" :key="semester.maChuongTrinhHocKy" class="surface-card border border-card rounded-2xl overflow-hidden shadow-sm">
        <div class="px-5 py-3 bg-(--surface-input)/30 flex items-center justify-between">
          <h3 class="text-sm font-bold text-heading flex items-center gap-2">
            <span class="h-6 w-6 rounded-lg bg-(--lg-primary) text-white flex items-center justify-center text-[11px] font-bold">{{ semester.thuTuHocKy }}</span>
            Học kỳ {{ semester.thuTuHocKy }}
          </h3>
          <span class="text-xs text-muted">{{ semester.subjects.length }} môn · {{ semester.totalCredits }} tín chỉ</span>
        </div>
        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm text-body whitespace-nowrap">
            <thead class="bg-(--surface-card)">
              <tr>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Mã môn</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Tên môn học</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Số tín chỉ</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Loại môn</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Bắt buộc</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Thứ tự</th>
                <th class="px-4 py-2.5 font-bold text-heading text-[11px] uppercase tracking-wider">Trạng thái</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="sub in semester.subjects" :key="sub.maChuongTrinhMonHoc" class="hover:bg-(--surface-input)/50 transition-colors">
                <td class="px-4 py-2.5">
                  <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ sub.maCodeMonHoc }}</span>
                </td>
                <td class="px-4 py-2.5 font-semibold text-heading">{{ sub.tenMonHoc }}</td>
                <td class="px-4 py-2.5">
                  <span class="font-bold text-heading">{{ sub.soTinChi }}</span>
                </td>
                <td class="px-4 py-2.5">
                  <span class="text-xs px-2 py-0.5 rounded bg-(--surface-input) text-muted border border-default">{{ sub.loaiMonHoc }}</span>
                </td>
                <td class="px-4 py-2.5">
                  <span v-if="sub.batBuoc" class="text-(--color-success-text) bg-(--color-success-bg) px-2 py-0.5 rounded text-xs font-bold">Bắt buộc</span>
                  <span v-else class="text-muted bg-(--surface-input) px-2 py-0.5 rounded text-xs">Tự chọn</span>
                </td>
                <td class="px-4 py-2.5 text-muted">{{ sub.thuTu }}</td>
                <td class="px-4 py-2.5">
                  <span :class="sub.conHoatDong ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="text-xs font-bold">
                    {{ sub.conHoatDong ? 'Hoạt động' : 'Ngừng' }}
                  </span>
                </td>
              </tr>
            </tbody>
            <tfoot v-if="semester.ghiChu">
              <tr>
                <td colspan="7" class="px-4 py-2 text-xs text-muted italic">
                  📝 {{ semester.ghiChu }}
                </td>
              </tr>
            </tfoot>
          </table>
        </div>
      </div>

      <div v-if="!selectedProgram" class="text-center py-12 text-muted">
        <Library :size="40" class="mx-auto mb-3 opacity-50" />
        <p>Vui lòng chọn chương trình đào tạo để xem khung chương trình.</p>
      </div>
    </template>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { Library, CheckCircle2, FileText, AlertCircle, Eye, Archive, Loader2 } from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { apiRequest, unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)
const loadingCurriculum = ref(false)
const curriculumError = ref(null)

const selectedProgram = ref(null)

const programs = ref([])
const programTerms = ref([])
const programSubjects = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const progRes = await bghApi.getPrograms()
    programs.value = unwrapApiData(progRes) || []
    if (programs.value.length > 0) {
      selectedProgram.value = programs.value[0].maChuongTrinh
    }
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu chương trình đào tạo'
  } finally {
    loading.value = false
  }
}

watch(selectedProgram, async (newVal) => {
  if (!newVal) {
    programTerms.value = []
    programSubjects.value = []
    return
  }
  loadingCurriculum.value = true
  curriculumError.value = null
  try {
    const [termsRes, subjectsRes] = await Promise.all([
      apiRequest(`/api/master-data/training-programs/${newVal}/terms`),
      apiRequest(`/api/master-data/training-programs/${newVal}/subjects`),
    ])
    programTerms.value = unwrapApiData(termsRes) || []
    programSubjects.value = unwrapApiData(subjectsRes) || []
  } catch (e) {
    curriculumError.value = e?.message || 'Lỗi tải dữ liệu khung chương trình'
    programTerms.value = []
    programSubjects.value = []
  } finally {
    loadingCurriculum.value = false
  }
})

const currentProgram = computed(() => programs.value.find(p => p.maChuongTrinh === selectedProgram.value))

const totalCredits = computed(() => {
  let total = 0
  for (const sem of semesters.value) {
    for (const sub of sem.subjects) {
      total += sub.soTinChi
    }
  }
  return total
})

const semesters = computed(() => {
  const terms = programTerms.value
  const subjects = programSubjects.value
  return terms.map(term => {
    const termSubjects = subjects.filter(s => s.hocKyDuKien === term.thuTuHocKy)
    return {
      maChuongTrinhHocKy: term.maChuongTrinhHocKy,
      thuTuHocKy: term.thuTuHocKy,
      subjects: termSubjects.map(s => ({
        maChuongTrinhMonHoc: s.maChuongTrinhMonHoc,
        maCodeMonHoc: s.maCodeMonHoc,
        tenMonHoc: s.tenMonHoc,
        soTinChi: s.soTinChi,
        loaiMonHoc: s.loaiMonHoc,
        batBuoc: s.batBuoc,
        thuTu: s.thuTu,
        conHoatDong: s.conHoatDong,
      })),
      totalCredits: termSubjects.reduce((sum, s) => sum + s.soTinChi, 0),
      ghiChu: null,
    }
  })
})

function statusBadge(status) {
  switch (status) {
    case 'active': return 'bg-(--color-success-bg) text-(--color-success-text)'
    case 'approved': return 'bg-(--color-info-bg) text-(--color-info-text)'
    default: return 'bg-(--surface-input) text-muted'
  }
}

function statusLabel(status) {
  switch (status) {
    case 'active': return 'Đang áp dụng'
    case 'approved': return 'Đã duyệt'
    default: return status
  }
}

onMounted(() => { loadData() })
</script>
