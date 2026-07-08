<script setup>
import { ref, onMounted } from 'vue'
import { Loader2, AlertCircle, Calendar, Clock, BookOpen } from 'lucide-vue-next'
import { staffApi } from '@/services/staffApi'

const loading = ref(true)
const apiError = ref('')
const period = ref(null)

async function loadData() {
  loading.value = true
  apiError.value = ''
  try {
    const res = await staffApi.getRegistrationPeriod()
    period.value = res ?? null
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

onMounted(() => { loadData() })
</script>

<template>
  <div class="registration-period max-w-4xl mx-auto space-y-6">
    <div class="lg-glass-soft p-6 rounded-2xl">
      <h1 class="text-2xl font-bold text-(--text-heading)">Đợt đăng ký hiện tại</h1>
      <p class="text-(--text-body) mt-1">Thông tin chi tiết về đợt đăng ký môn học.</p>
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

    <template v-else-if="period">
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Calendar :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Ngày mở</p>
            <p class="text-base font-semibold text-heading">{{ period.openDate }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <Clock :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Ngày đóng</p>
            <p class="text-base font-semibold text-heading">{{ period.closeDate }}</p>
          </div>
        </div>
        <div class="lg-card-glass p-4 flex items-center gap-3">
          <div class="h-10 w-10 rounded-xl lg-glass-soft text-link flex items-center justify-center">
            <BookOpen :size="20" />
          </div>
          <div>
            <p class="text-[9px] font-semibold text-placeholder uppercase tracking-widest">Max Credits</p>
            <p class="text-base font-semibold text-heading">{{ period.maxCredits }} TC</p>
          </div>
        </div>
      </div>

      <div class="lg-card-glass p-6 rounded-2xl space-y-4">
        <h3 class="text-lg font-semibold text-heading">{{ period.name }}</h3>
        <div class="grid grid-cols-2 gap-4">
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Học kỳ</p>
            <p class="text-sm font-bold text-heading mt-1">{{ period.semester }}</p>
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Trạng thái</p>
            <span class="mt-1 inline-block px-2.5 py-1 rounded-full text-[10px] font-semibold uppercase tracking-widest bg-(--color-success-bg) text-(--lg-success)">
              Đang mở
            </span>
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Sinh viên đã đăng ký</p>
            <p class="text-sm font-bold text-heading mt-1">{{ period.studentCount }}</p>
          </div>
          <div>
            <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Lớp học phần</p>
            <p class="text-sm font-bold text-heading mt-1">{{ period.classCount }}</p>
          </div>
        </div>
        <div class="pt-4 border-t border-default">
          <p class="text-[10px] font-semibold text-placeholder uppercase tracking-widest">Hạn hủy môn</p>
          <p class="text-sm font-bold text-heading mt-1">{{ period.withdrawDeadline }}</p>
        </div>
      </div>
    </template>
  </div>
</template>
