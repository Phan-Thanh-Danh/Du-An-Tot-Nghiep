<script setup>
import GlassPanel from '@/components/ui/GlassPanel.vue'
import { Users, AlertCircle } from 'lucide-vue-next'

defineProps({
  previewData: {
    type: Object,
    default: null
  },
  loading: Boolean
})
</script>

<template>
  <GlassPanel variant="soft" class="h-full">
    <template #header>
      <h3 class="text-md font-semibold text-[var(--text-heading)] flex items-center gap-2">
        <Users class="w-5 h-5 text-[var(--lg-primary)]" />
        Kết quả Preview
      </h3>
    </template>

    <div v-if="loading" class="flex justify-center items-center h-40">
      <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-[var(--lg-primary)]"></div>
    </div>
    
    <div v-else-if="previewData" class="space-y-4">
      <div class="p-4 rounded-lg bg-[var(--surface-hover)]">
        <div class="text-sm text-[var(--text-muted)] mb-1">Tổng số người nhận dự kiến</div>
        <div class="text-3xl font-bold text-[var(--lg-primary)]">{{ previewData.tongNguoiNhan }}</div>
      </div>
      
      <div v-if="previewData.tongNguoiNhan === 0" class="flex items-start gap-2 p-3 rounded-lg bg-[var(--color-warning-bg)] text-[var(--color-warning-text)] text-sm">
        <AlertCircle class="w-4 h-4 mt-0.5 shrink-0" />
        <p>Cảnh báo: Không tìm thấy người nhận nào khớp với phạm vi đã chọn. Nút Gửi sẽ bị vô hiệu hóa.</p>
      </div>
      
      <div v-if="previewData.danhSachMau?.length > 0">
        <h4 class="text-sm font-medium text-[var(--text-heading)] mb-2">Danh sách mẫu (tối đa 10):</h4>
        <ul class="space-y-2">
          <li v-for="user in previewData.danhSachMau" :key="user.id" class="text-sm p-2 rounded bg-[var(--surface-card)] border border-[var(--border-card)]">
            <div class="font-medium">{{ user.hoTen }}</div>
            <div class="text-xs text-[var(--text-muted)]">{{ user.email }} • {{ user.maNguoiDung }}</div>
          </li>
        </ul>
      </div>
    </div>
    
    <div v-else class="flex flex-col items-center justify-center h-40 text-[var(--text-muted)] text-sm text-center">
      <Users class="w-8 h-8 mb-2 opacity-50" />
      <p>Nhấn "Preview người nhận" để xem danh sách dự kiến</p>
    </div>
  </GlassPanel>
</template>
