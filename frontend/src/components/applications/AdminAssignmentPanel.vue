<script setup>
import { ref } from 'vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import { UserPlus, UserCheck } from 'lucide-vue-next'

defineProps({
  application: { type: Object, required: true },
  assignableUsers: { type: Array, default: () => [] }
})
const emit = defineEmits(['assign', 'receive'])

const selectedUserId = ref('')


const handleAssign = () => {
  if (selectedUserId.value) emit('assign', { nguoiXuLyId: selectedUserId.value })
}
</script>

<template>
  <GlassPanel padding="normal" class="space-y-4">
    <h3 class="font-semibold text-[var(--text-heading)] flex items-center gap-2">
      <UserPlus class="w-4 h-4" /> Phân công xử lý
    </h3>

    <div v-if="application.nguoiXuLyId" class="p-3 bg-[var(--surface-hover)] border border-[var(--lg-primary)] border-opacity-30 rounded-lg flex justify-between items-center">
      <div>
        <div class="text-xs text-[var(--text-muted)]">Đang phụ trách</div>
        <div class="font-medium text-[var(--text-body)] flex items-center gap-1 mt-0.5">
          <UserCheck class="w-3 h-3 text-[var(--color-success-text)]" />
          {{ application.tenNguoiXuLy }}
        </div>
      </div>
    </div>

    <div v-if="application.trangThai !== 'DA_DUYET' && application.trangThai !== 'TU_CHOI'" class="space-y-3 pt-2">
      <div>
        <label class="block text-xs font-medium text-[var(--text-muted)] mb-1">Chuyển người xử lý</label>
        <select v-model="selectedUserId" class="w-full h-9 px-2 rounded border border-[var(--border-input)] bg-[var(--surface-input)] text-sm text-[var(--text-body)]">
          <option value="">-- Chọn nhân sự --</option>
          <option v-for="u in assignableUsers" :key="u.id" :value="u.id">{{ u.hoTen }} ({{ u.maNhanVien }})</option>
        </select>
      </div>
      
      <div class="flex gap-2">
        <GlassButton variant="secondary" size="sm" class="flex-1" @click="emit('receive')">Tiếp nhận ngay</GlassButton>
        <GlassButton variant="primary" size="sm" class="flex-1" :disabled="!selectedUserId" @click="handleAssign">Phân công</GlassButton>
      </div>
    </div>
  </GlassPanel>
</template>
