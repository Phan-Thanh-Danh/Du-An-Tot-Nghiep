<script setup>
import { computed, ref, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSubjectStore } from '@/stores/content-council/subjectStore'
import SubjectDetailHeader from '@/components/content-council/subjects/SubjectDetailHeader.vue'
import SubjectDetailTabs from '@/components/content-council/subjects/SubjectDetailTabs.vue'
import CurriculumOverviewAccordion from '@/components/content-council/subjects/CurriculumOverviewAccordion.vue'
import SubjectContentSummaryCard from '@/components/content-council/subjects/SubjectContentSummaryCard.vue'
import SubjectOverviewSkeleton from '@/components/content-council/subjects/SubjectOverviewSkeleton.vue'
import SubjectOverviewEmptyState from '@/components/content-council/subjects/SubjectOverviewEmptyState.vue'

const props = defineProps({
  subjectId: {
    type: Number,
    required: true
  }
})

const router = useRouter()
const route = useRoute()
const isLoading = ref(true)

const subjectStore = useSubjectStore()
subjectStore.init()

const subject = computed(() => subjectStore.getSubjectDetail(props.subjectId))

onMounted(() => {
  // Simulate network request
  setTimeout(() => {
    isLoading.value = false
  }, 600)
})

const goBack = () => {
  router.push({ name: 'content-council-subjects' })
}
</script>

<template>
  <div class="h-full flex flex-col">
    <!-- Loading State -->
    <SubjectOverviewSkeleton v-if="isLoading" />

    <!-- Not Found State -->
    <div v-else-if="!subject" class="flex-1 flex flex-col items-center justify-center p-8 text-center surface-card rounded-xl border border-card">
      <div class="w-16 h-16 lg-glass-soft rounded-full flex items-center justify-center mb-4 text-placeholder">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3Z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
      </div>
      <h2 class="text-xl font-bold text-heading mb-2">Không tìm thấy môn học</h2>
      <p class="text-body mb-6 max-w-md">Môn học bạn đang tìm không tồn tại hoặc đã bị xóa khỏi hệ thống.</p>
      <button 
        @click="goBack"
        class="px-4 py-2 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 transition-colors"
      >
        Quay lại danh sách môn học
      </button>
    </div>

    <!-- Content State -->
    <div v-else class="flex-1 flex flex-col">
      <!-- Header -->
      <SubjectDetailHeader :subject="subject" />
      
      <!-- Tabs -->
      <SubjectDetailTabs />

      <!-- Overview Content -->
      <div class="flex-1 flex flex-col gap-4">
        <!-- Top: Summary Card -->
        <SubjectContentSummaryCard :chapters="subject.chapters" />

        <!-- Bottom: Curriculum Accordion -->
        <div v-if="subject.chapters.length === 0" class="surface-card rounded-xl border border-card overflow-hidden">
           <SubjectOverviewEmptyState />
        </div>
        <div v-else class="surface-card rounded-xl border border-card overflow-hidden shadow-sm">
          <CurriculumOverviewAccordion :chapters="subject.chapters" />
        </div>
      </div>
    </div>
  </div>
</template>
