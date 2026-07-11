<template>
  <div class="space-y-6">
    <div class="flex flex-col md:flex-row md:items-center justify-between gap-4">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Tổng hợp nguyện vọng giảng dạy</h1>
        <p class="text-sm text-slate-500 mt-1">
          Theo dõi tiến độ và xem chi tiết nguyện vọng của giảng viên cho học kỳ sắp tới
        </p>
      </div>
      
      <div v-if="context?.schedulableTerm" class="flex items-center gap-3 bg-white px-4 py-2 border border-slate-200 rounded-lg lg-glass-soft">
        <Calendar class="w-5 h-5 text-indigo-500" />
        <div>
          <div class="text-xs text-slate-500 font-medium">Học kỳ chuẩn bị</div>
          <div class="text-sm font-semibold text-slate-800">{{ context.schedulableTerm.tenHocKy }}</div>
        </div>
        <div class="ml-2 pl-3 border-l border-slate-200">
          <div class="px-2.5 py-1 rounded-md text-xs font-medium"
               :class="context.canPrepareSchedule ? 'bg-emerald-50 text-emerald-700' : 'bg-amber-50 text-amber-700'">
            {{ context.canPrepareSchedule ? 'Đang mở' : 'Đã đóng' }}
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-indigo-600"></div>
    </div>

    <div v-else-if="error" class="bg-red-50 text-red-600 p-4 rounded-lg border border-red-100 flex items-start gap-3">
      <AlertCircle class="w-5 h-5 shrink-0 mt-0.5" />
      <p>{{ error }}</p>
    </div>

    <div v-else-if="!context?.schedulableTerm" class="bg-amber-50 border border-amber-200 rounded-lg p-6 text-center">
      <CalendarX class="w-12 h-12 text-amber-400 mx-auto mb-4" />
      <h3 class="text-lg font-semibold text-amber-800 mb-2">Không có học kỳ nào đang mở</h3>
      <p class="text-amber-600">Hệ thống chưa tìm thấy học kỳ nào ở trạng thái "chuẩn_bị" hoặc sắp bắt đầu.</p>
    </div>

    <div v-else class="space-y-6">
      <!-- Tiến độ tổng quan -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
        <div class="bg-white border border-slate-200 rounded-xl p-5 lg-glass-soft">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-slate-500">Tổng số giảng viên</h3>
            <Users class="w-5 h-5 text-slate-400" />
          </div>
          <div class="text-3xl font-bold text-slate-800">{{ summary.totalTeachers }}</div>
        </div>
        
        <div class="bg-white border border-slate-200 rounded-xl p-5 lg-glass-soft">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-slate-500">Đã chốt (Submitted)</h3>
            <CheckCircle class="w-5 h-5 text-emerald-500" />
          </div>
          <div class="text-3xl font-bold text-emerald-600">{{ summary.submittedCount }}</div>
          <div class="mt-2 text-xs text-emerald-600 font-medium">
            {{ Math.round((summary.submittedCount / Math.max(1, summary.totalTeachers)) * 100) }}% hoàn thành
          </div>
        </div>
        
        <div class="bg-white border border-slate-200 rounded-xl p-5 lg-glass-soft">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-slate-500">Đang nháp (Draft)</h3>
            <FileEdit class="w-5 h-5 text-amber-500" />
          </div>
          <div class="text-3xl font-bold text-amber-600">{{ summary.draftCount }}</div>
        </div>
        
        <div class="bg-white border border-slate-200 rounded-xl p-5 lg-glass-soft">
          <div class="flex items-center justify-between mb-2">
            <h3 class="text-sm font-medium text-slate-500">Chưa bắt đầu</h3>
            <Clock class="w-5 h-5 text-slate-400" />
          </div>
          <div class="text-3xl font-bold text-slate-600">{{ summary.unstartedCount }}</div>
        </div>
      </div>

      <!-- Danh sách chi tiết -->
      <div class="bg-white border border-slate-200 rounded-xl lg-glass-soft overflow-hidden">
        <div class="p-4 border-b border-slate-200 flex flex-wrap gap-4 items-center justify-between bg-slate-50/50">
          <h2 class="font-semibold text-slate-800">Danh sách giảng viên</h2>
          
          <div class="flex gap-2">
            <select v-model="filterStatus" class="px-3 py-1.5 text-sm border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500">
              <option value="all">Tất cả trạng thái</option>
              <option value="submitted">Đã chốt</option>
              <option value="draft">Đang nháp</option>
              <option value="unstarted">Chưa bắt đầu</option>
            </select>
          </div>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full text-left text-sm">
            <thead class="bg-slate-50 text-slate-600 font-medium border-b border-slate-200">
              <tr>
                <th class="p-4">Giảng viên</th>
                <th class="p-4">Khoa/Bộ môn</th>
                <th class="p-4">Trạng thái</th>
                <th class="p-4">Nguyện vọng đặc biệt</th>
                <th class="p-4">Cập nhật lúc</th>
                <th class="p-4 text-center">Thao tác</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-slate-100">
              <tr v-if="filteredTeachers.length === 0">
                <td colspan="6" class="p-8 text-center text-slate-500">
                  Không tìm thấy dữ liệu phù hợp
                </td>
              </tr>
              <tr v-for="t in filteredTeachers" :key="t.maGiaoVien" class="hover:bg-slate-50 transition-colors">
                <td class="p-4">
                  <div class="font-medium text-slate-800">{{ t.tenGiaoVien }}</div>
                  <div class="text-xs text-slate-500">{{ t.email }}</div>
                </td>
                <td class="p-4 text-slate-600">{{ t.tenDonVi }}</td>
                <td class="p-4">
                  <span class="px-2.5 py-1 rounded-full text-xs font-medium"
                        :class="[
                          t.trangThai === 'submitted' ? 'bg-emerald-100 text-emerald-700' :
                          t.trangThai === 'draft' ? 'bg-amber-100 text-amber-700' :
                          'bg-slate-100 text-slate-600'
                        ]">
                    {{ getStatusLabel(t.trangThai) }}
                  </span>
                </td>
                <td class="p-4">
                  <div v-if="t.trangThai !== 'unstarted'" class="text-xs space-y-1">
                    <div v-if="t.soLopToiDaMongMuon" class="text-indigo-600">Tối đa {{ t.soLopToiDaMongMuon }} lớp</div>
                    <div v-if="t.soCaToiDaMoiTuan" class="text-indigo-600">Tối đa {{ t.soCaToiDaMoiTuan }} ca/tuần</div>
                    <div v-if="t.tongSoCaDangKy > 0" class="text-slate-500">Đã đăng ký {{ t.tongSoCaDangKy }} ca ({{ t.tongSoCaUuTien }} ưu tiên)</div>
                    <div v-if="!t.soLopToiDaMongMuon && !t.soCaToiDaMoiTuan && t.tongSoCaDangKy === 0" class="text-slate-400">
                      Không có
                    </div>
                  </div>
                  <div v-else class="text-slate-400 text-xs">-</div>
                </td>
                <td class="p-4 text-slate-500 text-xs">
                  {{ formatDate(t.ngayCapNhat) }}
                </td>
                <td class="p-4 text-center">
                  <button v-if="t.trangThai !== 'unstarted'" @click="viewDetails(t)" class="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg transition-colors" title="Xem chi tiết">
                    <Eye class="w-4 h-4" />
                  </button>
                  <span v-else class="text-slate-300">-</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { staffApi } from '@/services/staffApi'
import { Calendar, AlertCircle, CalendarX, Users, CheckCircle, FileEdit, Clock, Eye } from 'lucide-vue-next'

const loading = ref(true)
const error = ref(null)

const context = ref(null)
const summary = ref({
  totalTeachers: 0,
  submittedCount: 0,
  draftCount: 0,
  unstartedCount: 0
})
const teachers = ref([])
const filterStatus = ref('all')

const filteredTeachers = computed(() => {
  if (filterStatus.value === 'all') return teachers.value
  return teachers.value.filter(x => x.trangThai === filterStatus.value)
})

const getStatusLabel = (status) => {
  if (status === 'submitted') return 'Đã chốt'
  if (status === 'draft') return 'Đang nháp'
  return 'Chưa làm'
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('vi-VN')
}

const fetchData = async () => {
  try {
    loading.value = true
    error.value = null
    
    const ctx = await staffApi.getSchedulingContext()
    context.value = ctx
    
    if (ctx.schedulableTerm) {
      const termId = ctx.schedulableTerm.maHocKy
      
      const [sumRes, teachersRes] = await Promise.all([
        staffApi.getTeachingPreferenceSummary(termId),
        staffApi.getTeachersPreferenceSummary(termId)
      ])
      
      summary.value = sumRes
      teachers.value = teachersRes || []
    }
  } catch (err) {
    error.value = err.message || 'Lỗi tải dữ liệu tổng hợp nguyện vọng'
  } finally {
    loading.value = false
    
  }
}

const viewDetails = (teacher) => {
  alert(`Tính năng xem chi tiết nguyện vọng của giảng viên ${teacher.tenGiaoVien} đang được phát triển.`)
}

onMounted(() => {
  fetchData()
})
</script>
