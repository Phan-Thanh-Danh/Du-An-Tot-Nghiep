<script setup>
import { onMounted } from 'vue'
import { RefreshCw } from 'lucide-vue-next'
import { useSubjectFilters } from '@/composables/content-council/useSubjectFilters'

import SubjectFilterBar from '@/components/content-council/subjects/SubjectFilterBar.vue'
import SubjectCard from '@/components/content-council/subjects/SubjectCard.vue'
import SubjectListSkeleton from '@/components/content-council/subjects/SubjectListSkeleton.vue'
import SubjectEmptyState from '@/components/content-council/subjects/SubjectEmptyState.vue'

const {
  isLoading,
  searchQuery,
  statusFilter,
  sortOption,
  filteredSubjects,
  hasActiveFilters,
  fetchSubjects,
  clearFilters,
  subjects
} = useSubjectFilters()

onMounted(() => {
  fetchSubjects()
})
</script>

<template>
  <div class="flex flex-col h-full">
    <!-- Page Header -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
      <div>
        <h1 class="text-2xl sm:text-3xl font-bold text-slate-900 mb-1">Nội dung môn học</h1>
        <p class="text-slate-500">Chọn một môn học để xây dựng chương, bài học, video, tài liệu và Quiz.</p>
      </div>
      <button 
        @click="fetchSubjects"
        :disabled="isLoading"
        class="inline-flex items-center justify-center gap-2 px-4 py-2 bg-white border border-slate-200 rounded-lg text-sm font-medium text-slate-700 hover:bg-slate-50 hover:text-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shrink-0"
      >
        <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': isLoading }" />
        Làm mới
      </button>
    </div>

    <!-- Filter Bar -->
    <SubjectFilterBar 
      v-model:searchQuery="searchQuery"
      v-model:statusFilter="statusFilter"
      v-model:sortOption="sortOption"
      :hasActiveFilters="hasActiveFilters"
      @clearFilters="clearFilters"
    />

    <!-- Main Content Area -->
    <div class="flex-1">
      <!-- Loading State -->
      <div v-if="isLoading" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
        <SubjectListSkeleton v-for="i in 6" :key="i" />
      </div>

      <!-- Content -->
      <template v-else>
        <!-- Data Grid -->
        <div v-if="filteredSubjects.length > 0" class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4 sm:gap-6">
          <SubjectCard 
            v-for="subject in filteredSubjects" 
            :key="subject.id" 
            :subject="subject" 
          />
        </div>

        <!-- Empty States -->
        <SubjectEmptyState 
          v-else-if="subjects.length === 0" 
          type="empty" 
        />
        
        <SubjectEmptyState 
          v-else 
          type="no-results" 
          @clearFilters="clearFilters" 
        />
      </template>
    </div>
  </div>
</template>
