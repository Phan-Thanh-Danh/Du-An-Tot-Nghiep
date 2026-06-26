<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useSubjectStore } from '@/stores/content-council/subjectStore'
import { ChevronRight } from 'lucide-vue-next'

const route = useRoute()
const subjectId = computed(() => Number(route.params.subjectId))

const store = useSubjectStore()
store.init()

const subject = computed(() => store.getSubjectById(subjectId.value) || null)
</script>

<template>
  <div class="mx-auto max-w-7xl w-full flex flex-col h-full">
    <!-- Breadcrumb -->
    <nav class="mb-4 flex items-center text-sm font-medium text-label">
      <router-link 
        :to="{ name: 'content-council-subjects' }"
        class="hover:text-blue-500 transition-colors"
      >
        Nội dung môn học
      </router-link>
      <ChevronRight class="w-4 h-4 mx-2 opacity-50" />
      <span class="text-body font-semibold truncate max-w-[200px] sm:max-w-xs md:max-w-md">
        {{ subject ? `${subject.code} - ${subject.name}` : 'Chi tiết môn học' }}
      </span>
    </nav>

    <!-- Router View cho nội dung con (Overview, Editor, Settings) -->
    <div class="flex-1 relative">
      <router-view v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" :subjectId="subjectId" />
        </transition>
      </router-view>
    </div>
  </div>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
