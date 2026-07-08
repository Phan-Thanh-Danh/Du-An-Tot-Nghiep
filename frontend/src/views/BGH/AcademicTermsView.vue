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
        <h2 class="sr-only text-xl font-bold text-heading">Học kỳ & Khóa</h2>
        <p class="text-xs text-muted mt-1">Danh sách các học kỳ, khóa tuyển sinh trên toàn hệ thống</p>
      </div>
      <div class="flex gap-2">
        <select v-model="yearFilter" @change="filterData" class="px-3 py-2 bg-(--surface-input) border border-input rounded-lg text-sm text-body focus:outline-none focus:border-(--lg-primary)">
          <option value="">Tất cả năm học</option>
          <option v-for="y in academicYears" :key="y" :value="y">{{ y }}</option>
        </select>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-left text-sm text-body whitespace-nowrap">
          <thead class="bg-(--surface-card)">
            <tr>
              <th class="px-4 py-3 font-bold text-heading">Mã học kỳ</th>
              <th class="px-4 py-3 font-bold text-heading">Tên học kỳ</th>
              <th class="px-4 py-3 font-bold text-heading">Năm học</th>
              <th class="px-4 py-3 font-bold text-heading">Ngày BĐ</th>
              <th class="px-4 py-3 font-bold text-heading">Ngày KT</th>
              <th class="px-4 py-3 font-bold text-heading">Thứ tự</th>
              <th class="px-4 py-3 font-bold text-heading">Tín chỉ tối đa</th>
              <th class="px-4 py-3 font-bold text-heading">Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="filteredTerms.length === 0" class="bg-transparent">
              <td colspan="8" class="py-12 text-center text-muted">
                <CalendarDays :size="32" class="mx-auto mb-2 opacity-50" />
                <p>Không có học kỳ nào.</p>
              </td>
            </tr>
            <tr v-for="term in filteredTerms" :key="term.maHocKy" class="hover:bg-(--surface-input)/50 transition-colors">
              <td class="px-4 py-3">
                <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-bold bg-(--surface-input) text-heading border border-default">{{ term.maCodeHocKy }}</span>
              </td>
              <td class="px-4 py-3 font-bold text-heading">{{ term.tenHocKy }}</td>
              <td class="px-4 py-3">{{ term.namHoc }}</td>
              <td class="px-4 py-3">{{ term.ngayBatDau }}</td>
              <td class="px-4 py-3">{{ term.ngayKetThuc }}</td>
              <td class="px-4 py-3">Kỳ {{ term.thuTuTrongNam }}</td>
              <td class="px-4 py-3">{{ term.soTinChiToiDa || '—' }}</td>
              <td class="px-4 py-3">
                <span :class="term.daKhoa ? 'bg-(--color-danger-bg) text-(--color-danger-text)' : 'bg-(--color-success-bg) text-(--color-success-text)'" class="inline-flex items-center gap-1 px-2 py-1 rounded-md text-[10px] font-bold uppercase tracking-wider">
                  <Lock v-if="term.daKhoa" :size="12" /><Unlock v-else :size="12" />
                  {{ term.daKhoa ? 'Đã khóa' : 'Đang mở' }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <div class="surface-card border border-card rounded-2xl p-6 shadow-sm">
      <h2 class="text-lg font-bold text-heading mb-4">Khóa tuyển sinh</h2>
      <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
        <div v-for="cohort in cohorts" :key="cohort.maKhoaTuyenSinh" class="p-4 rounded-2xl border border-default hover:border-(--border-input-focus) transition-all">
          <div class="flex items-center gap-3">
            <div class="h-10 w-10 rounded-xl bg-(--color-info-bg) text-(--color-info-text) flex items-center justify-center font-bold">
              {{ cohort.maCodeKhoa }}
            </div>
            <div>
              <p class="text-sm font-bold text-heading">{{ cohort.tenKhoa }}</p>
              <p class="text-[10px] text-muted">{{ cohort.namBatDau }} - {{ cohort.namKetThucDuKien || 'Chưa xác định' }}</p>
            </div>
          </div>
          <div class="mt-3 flex items-center justify-between text-xs">
            <span class="text-muted">{{ cohort.moTa || 'Không có mô tả' }}</span>
            <span :class="cohort.conHoatDong ? 'text-(--color-success-text)' : 'text-(--color-danger-text)'" class="font-bold">
              {{ cohort.conHoatDong ? 'Đang hoạt động' : 'Ngừng' }}
            </span>
          </div>
        </div>
      </div>
    </div>
    </template>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { CalendarDays, Lock, Unlock, AlertCircle, Loader2 } from 'lucide-vue-next'
import { bghApi } from '@/services/bghApi'
import { unwrapApiData } from '@/services/apiClient'

const loading = ref(false)
const error = ref(null)

const yearFilter = ref('')

const terms = ref([])

async function loadData() {
  loading.value = true
  error.value = null
  try {
    const res = await bghApi.getAcademicTerms()
    terms.value = unwrapApiData(res) || []
  } catch (e) {
    error.value = e?.message || 'Lỗi tải dữ liệu học kỳ'
  } finally {
    loading.value = false
  }
}

const academicYears = computed(() => {
  const years = new Set(terms.value.map(t => t.namHoc))
  return [...years].sort()
})

const cohorts = [
  { maKhoaTuyenSinh: 1, maCodeKhoa: 'K18', tenKhoa: 'Khóa 18 (2023-2026)', namBatDau: 2023, namKetThucDuKien: 2026, moTa: 'Tuyển sinh năm 2023 - hệ Cao đẳng chính quy', conHoatDong: true },
  { maKhoaTuyenSinh: 2, maCodeKhoa: 'K19', tenKhoa: 'Khóa 19 (2024-2027)', namBatDau: 2024, namKetThucDuKien: 2027, moTa: 'Tuyển sinh năm 2024 - hệ Cao đẳng chính quy', conHoatDong: true },
  { maKhoaTuyenSinh: 3, maCodeKhoa: 'K20', tenKhoa: 'Khóa 20 (2025-2028)', namBatDau: 2025, namKetThucDuKien: 2028, moTa: 'Tuyển sinh năm 2025 - hệ Cao đẳng chính quy', conHoatDong: true },
  { maKhoaTuyenSinh: 4, maCodeKhoa: 'K21', tenKhoa: 'Khóa 21 (2026-2029)', namBatDau: 2026, namKetThucDuKien: 2029, moTa: 'Tuyển sinh năm 2026 - hệ Cao đẳng chính quy', conHoatDong: false },
]

const filteredTerms = computed(() => {
  if (!yearFilter.value) return terms.value
  return terms.value.filter(t => t.namHoc === yearFilter.value)
})

function filterData() {}

onMounted(() => { loadData() })
</script>
