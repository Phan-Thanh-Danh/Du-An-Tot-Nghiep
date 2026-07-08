<script setup>
import { ref, onMounted } from 'vue'
import { Search, Loader2, AlertCircle, FileText } from 'lucide-vue-next'
import { registrationApi } from '@/services/registrationApi'

const loading = ref(true)
const apiError = ref('')
const results = ref([])

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const data = await registrationApi.getRegistrationResults()
    results.value = Array.isArray(data)
      ? data.map(item => ({
        id: item.id,
        studentCode: item.studentCode,
        studentName: item.studentName,
        course: item.course,
        group: item.group,
        credits: item.credits,
        status: item.status === 'Enrolled' ? 'registered' : 'waitlisted',
        registeredAt: item.registeredAt?.replace?.('T', ' ').slice(0, 16) || item.registeredAt,
      }))
      : []
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="registration-result max-w-7xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Kết quả đăng ký</h1>
      <p class="text-(--text-body) mt-1">Xem kết quả đăng ký môn học của sinh viên.</p>
    </div>

    <div v-if="loading" class="flex flex-col items-center justify-center py-20 gap-3">
      <Loader2 class="animate-spin text-muted" :size="28" />
      <p class="text-sm text-muted">Đang tải dữ liệu...</p>
    </div>

    <div v-else-if="apiError" class="surface-card border border-card rounded-2xl p-6 flex flex-col items-center justify-center gap-3">
      <AlertCircle :size="32" class="text-(--color-danger-text)" />
      <p class="text-sm font-bold text-heading">Không thể tải dữ liệu</p>
      <p class="text-xs text-muted">{{ apiError }}</p>
      <button @click="loadData" class="lg-button-primary px-4 py-2 text-xs font-bold rounded-xl mt-2">Thử lại</button>
    </div>

    <template v-else>
      <div class="lg-glass-strong p-4 rounded-2xl">
        <label class="flex items-center gap-2 bg-(--surface-input) px-3 py-2 rounded border border-(--border-input) max-w-md">
          <Search :size="16" />
          <input type="text" placeholder="Tìm theo sinh viên hoặc môn học..." class="bg-transparent border-none outline-none w-full" />
        </label>
      </div>

      <div class="lg-table-shell overflow-hidden rounded-2xl">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="surface-solid">
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Mã SV</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Sinh viên</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Môn học</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Tín chỉ</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Trạng thái</th>
              <th class="px-4 py-4 text-[10px] font-semibold text-placeholder uppercase tracking-widest border-b border-default">Thời gian</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-default)">
            <tr v-for="r in results" :key="r.id" class="group hover:bg-(--surface-input) transition-colors">
              <td class="px-4 py-4 text-sm font-semibold text-link">{{ r.studentCode }}</td>
              <td class="px-4 py-4 text-sm font-semibold text-heading">{{ r.studentName }}</td>
              <td class="px-4 py-4">
                <p class="text-sm font-semibold text-heading">{{ r.course }}</p>
                <p class="text-[10px] font-bold text-placeholder mt-0.5">{{ r.group }}</p>
              </td>
              <td class="px-4 py-4 text-sm font-semibold text-heading">{{ r.credits }}</td>
              <td class="px-4 py-4">
                <span :class="['px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest',
                  r.status === 'registered' ? 'bg-(--color-success-bg) text-(--lg-success)' : 'bg-(--color-warning-bg) text-(--lg-warning)']">
                  {{ r.status === 'registered' ? 'Đã đăng ký' : 'Chờ' }}
                </span>
              </td>
              <td class="px-4 py-4 text-sm text-label">{{ r.registeredAt }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="results.length === 0" class="flex flex-col items-center justify-center py-16 text-center">
          <div class="h-14 w-14 rounded-3xl surface-input border border-default flex items-center justify-center mb-4">
            <FileText :size="24" class="text-placeholder" />
          </div>
          <p class="text-base font-semibold text-heading">Chưa có kết quả đăng ký</p>
          <p class="text-sm text-label mt-1">Dữ liệu sẽ xuất hiện khi sinh viên đăng ký môn học.</p>
        </div>
      </div>
    </template>
  </div>
</template>
