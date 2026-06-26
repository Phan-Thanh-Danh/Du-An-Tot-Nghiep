<script setup>
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { Eye, Edit3 } from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()

const props = defineProps({
  subject: {
    type: Object,
    required: true
  }
})

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'empty': return 'bg-slate-100 text-slate-600 border-slate-200'
    case 'draft': return 'bg-blue-50 text-blue-700 border-blue-200'
    case 'completed': return 'bg-green-50 text-green-700 border-green-200'
    default: return 'bg-slate-100 text-slate-600 border-slate-200'
  }
}

const getStatusText = (status) => {
  switch (status) {
    case 'empty': return 'Chưa có nội dung'
    case 'draft': return 'Đang biên soạn'
    case 'completed': return 'Đã hoàn thiện'
    default: return status
  }
}

const formatRelativeTime = (isoString) => {
  if (!isoString) return ''
  const date = new Date(isoString)
  const now = new Date()
  const diffInSeconds = Math.floor((now - date) / 1000)
  
  if (diffInSeconds < 60) return 'Vừa cập nhật'
  const diffInMinutes = Math.floor(diffInSeconds / 60)
  if (diffInMinutes < 60) return `Cập nhật ${diffInMinutes} phút trước`
  const diffInHours = Math.floor(diffInMinutes / 60)
  if (diffInHours < 24) return `Cập nhật ${diffInHours} giờ trước`
  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays < 30) return `Cập nhật ${diffInDays} ngày trước`
  return `Cập nhật ${date.toLocaleDateString('vi-VN')}`
}
</script>

<template>
  <div class="surface-card rounded-t-xl border-x border-t border-card p-3 md:p-4 pb-0 flex flex-col md:flex-row gap-3 md:items-start justify-between">
    <div class="flex-1 min-w-0">
      <div class="flex items-center gap-3 mb-1.5 flex-wrap">
        <span class="text-sm font-semibold tracking-wider text-label uppercase">
          {{ subject.code }}
        </span>
        <span 
          class="text-[11px] font-medium px-2 py-0.5 rounded-full border"
          :class="getStatusBadgeClass(subject.status)"
        >
          {{ getStatusText(subject.status) }}
        </span>
        <span class="text-[11px] text-placeholder font-medium ml-auto md:ml-0">
          {{ formatRelativeTime(subject.updatedAt) }}
        </span>
      </div>
      
      <h1 class="text-xl font-bold text-heading mb-1.5 truncate">
        {{ subject.name }}
      </h1>
      <p class="text-body text-sm max-w-3xl line-clamp-2 md:line-clamp-none mb-3">
        {{ subject.shortDescription }}
      </p>
    </div>

    <!-- Actions -->
    <div class="flex items-center gap-3 shrink-0 mb-4 md:mb-0">
      <button 
        @click="router.push({ name: 'content-council-subject-preview', params: { subjectId: subject.id } })"
        class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-body surface-card border border-card rounded-lg hover:bg-slate-50 focus:outline-none transition-colors"
      >
        <Eye class="w-4 h-4" />
        <span class="hidden sm:inline">Xem như học sinh</span>
      </button>
      
      <button 
        v-if="route.name === 'content-council-subject-editor'"
        @click="router.push({ name: 'content-council-subject-overview', params: { subjectId: subject.id } })"
        class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors shadow-sm"
      >
        <Eye class="w-4 h-4" />
        <span>Tổng quan</span>
      </button>

      <button 
        v-else
        @click="router.push({ name: 'content-council-subject-editor', params: { subjectId: subject.id } })"
        class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-white bg-blue-600 border border-transparent rounded-lg hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 transition-colors shadow-sm"
      >
        <Edit3 class="w-4 h-4" />
        <span>Trình soạn nội dung</span>
      </button>
    </div>
  </div>
</template>
