<script setup>
import { defineProps } from 'vue'

import { useRouter } from 'vue-router'

const props = defineProps({
  subject: {
    type: Object,
    required: true
  }
})

const router = useRouter()

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'empty':
      return 'bg-slate-100 text-slate-600 border-slate-200'
    case 'draft':
      return 'bg-blue-50 text-blue-700 border-blue-200'
    case 'completed':
      return 'bg-green-50 text-green-700 border-green-200'
    default:
      return 'bg-slate-100 text-slate-600 border-slate-200'
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

const handleOpenSubject = () => {
  router.push({ name: 'content-council-subject-overview', params: { subjectId: props.subject.id } })
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 p-5 flex flex-col h-full hover:border-blue-300 hover:shadow-sm transition-all duration-200 group">
    <!-- Header: Code and Status -->
    <div class="flex items-start justify-between mb-3">
      <span class="text-xs font-semibold tracking-wider text-slate-500 uppercase">
        {{ subject.code }}
      </span>
      <span 
        class="text-xs font-medium px-2 py-0.5 rounded-full border"
        :class="getStatusBadgeClass(subject.status)"
      >
        {{ getStatusText(subject.status) }}
      </span>
    </div>

    <!-- Title and Description -->
    <h3 class="text-lg font-bold text-slate-900 leading-tight mb-2 line-clamp-2 group-hover:text-blue-700 transition-colors">
      {{ subject.name }}
    </h3>
    <p class="text-sm text-slate-600 line-clamp-2 mb-4 flex-grow">
      {{ subject.shortDescription }}
    </p>

    <!-- Stats Grid -->
    <div class="grid grid-cols-2 gap-x-2 gap-y-3 mb-5 mt-auto">
      <div class="flex flex-col">
        <span class="text-lg font-bold text-slate-800">{{ subject.chapterCount }}</span>
        <span class="text-xs text-slate-500 font-medium">Chương</span>
      </div>
      <div class="flex flex-col">
        <span class="text-lg font-bold text-slate-800">{{ subject.lessonCount }}</span>
        <span class="text-xs text-slate-500 font-medium">Bài học</span>
      </div>
      <div class="flex flex-col">
        <span class="text-lg font-bold text-slate-800">{{ subject.contentCount }}</span>
        <span class="text-xs text-slate-500 font-medium">Nội dung</span>
      </div>
      <div class="flex flex-col">
        <span class="text-lg font-bold text-slate-800">{{ subject.quizCount }}</span>
        <span class="text-xs text-slate-500 font-medium">Quiz</span>
      </div>
    </div>

    <!-- Footer: Date and Action -->
    <div class="flex items-center justify-between mt-auto pt-4 border-t border-slate-100">
      <span class="text-xs text-slate-500">
        {{ formatRelativeTime(subject.updatedAt) }}
      </span>
      
      <button 
        @click="handleOpenSubject"
        class="text-sm font-medium text-blue-600 hover:text-blue-800 flex items-center gap-1 group-hover:translate-x-1 transition-transform"
      >
        Mở môn học <span aria-hidden="true">&rarr;</span>
      </button>
    </div>
  </div>
</template>
