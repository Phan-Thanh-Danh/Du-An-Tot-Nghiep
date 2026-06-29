<script setup>
import StudentDisciplineCard from './StudentDisciplineCard.vue'

defineProps({
  records: { type: Array, default: () => [] }
})
const emit = defineEmits(['select'])
</script>

<template>
  <div>
    <h3 class="text-lg font-bold text-[var(--text-heading)] mb-4">Hồ sơ kỷ luật</h3>
    
    <div v-if="records.length === 0" class="text-center py-12 bg-[var(--surface-card)] rounded-xl border border-[var(--border-card)]">
      <div class="text-[var(--text-muted)]">Không có hồ sơ kỷ luật nào. Chúc bạn luôn giữ vững phong độ!</div>
    </div>

    <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      <StudentDisciplineCard 
        v-for="r in records" 
        :key="r.id" 
        :record="r" 
        class="cursor-pointer"
        @click="emit('select', r)"
      />
    </div>
  </div>
</template>
