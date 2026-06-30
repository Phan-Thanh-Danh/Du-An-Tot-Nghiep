<script setup>
import { ref } from 'vue'

import GlassButton from '@/components/ui/GlassButton.vue'
import { Search, Filter, RotateCcw } from 'lucide-vue-next'

const emit = defineEmits(['filter'])
const filters = ref({
  keyword: '',
  status: '',
  type: ''
})

const applyFilters = () => emit('filter', { ...filters.value })
const resetFilters = () => {
  filters.value = { keyword: '', status: '', type: '' }
  applyFilters()
}
</script>

<template>
  <div class="flex flex-col sm:flex-row gap-4 mb-6 p-4 bg-(--surface-card) border border-(--border-card) rounded-xl">
    <div class="flex-1 relative">
      <Search class="w-4 h-4 absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" />
      <input 
        v-model="filters.keyword"
        @keyup.enter="applyFilters"
        class="w-full h-10 pl-9 pr-3 rounded-lg border border-(--border-input) bg-(--surface-input) text-(--text-body) focus:border-(--lg-primary) outline-none"
        placeholder="Tìm theo mã đơn, tiêu đề hoặc MSSV..."
      />
    </div>
    
    <div class="w-full sm:w-48">
      <select v-model="filters.status" @change="applyFilters" class="w-full h-10 px-3 rounded-lg border border-(--border-input) bg-(--surface-input) text-(--text-body)">
        <option value="">Tất cả trạng thái</option>
        <option value="DANG_XEM_XET">Đang xem xét</option>
        <option value="YEU_CAU_BO_SUNG">Cần bổ sung</option>
        <option value="DA_NOP">Đợi tiếp nhận</option>
        <option value="DA_DUYET">Đã duyệt</option>
        <option value="TU_CHOI">Từ chối</option>
      </select>
    </div>

    <GlassButton variant="primary" @click="applyFilters">
      <template #leading><Filter class="w-4 h-4" /></template>
      Lọc
    </GlassButton>
    <GlassButton variant="ghost" @click="resetFilters" aria-label="Reset bộ lọc">
      <RotateCcw class="w-4 h-4" />
    </GlassButton>
  </div>
</template>
