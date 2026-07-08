<script setup>
import { defineProps } from 'vue'
import { BookOpen, FileText, Edit } from 'lucide-vue-next'

const props = defineProps({
  subjects: {
    type: Array,
    required: true
  }
})

const getStatusBadgeClass = (status) => {
  switch (status) {
    case 'empty':
      return 'bg-slate-100 text-slate-600 border-slate-200'
    case 'draft':
      return 'bg-amber-50 text-amber-600 border-amber-200'
    case 'completed':
      return 'bg-emerald-50 text-emerald-600 border-emerald-200'
    default:
      return 'bg-slate-100 text-slate-600 border-slate-200'
  }
}

const getStatusLabel = (status) => {
  switch (status) {
    case 'empty':
      return 'Chưa có nội dung'
    case 'draft':
      return 'Đang biên soạn'
    case 'completed':
      return 'Đã hoàn thiện'
    default:
      return 'Chưa rõ'
  }
}
</script>

<template>
  <div class="bg-white rounded-xl border border-slate-200 overflow-hidden">
    <div class="overflow-x-auto">
      <table class="w-full text-left border-collapse">
        <thead>
          <tr class="bg-slate-50 border-b border-slate-200">
            <th class="py-3 px-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Môn học</th>
            <th class="py-3 px-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Trạng thái</th>
            <th class="py-3 px-4 text-xs font-semibold text-slate-500 uppercase tracking-wider">Thống kê</th>
            <th class="py-3 px-4 text-xs font-semibold text-slate-500 uppercase tracking-wider text-right">Thao tác</th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-200">
          <tr v-for="subject in subjects" :key="subject.id" class="hover:bg-slate-50 transition-colors">
            <td class="py-3 px-4">
              <div class="flex items-center gap-3">
                <div class="w-10 h-10 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center font-bold text-sm">
                  {{ subject.code.substring(0, 3) }}
                </div>
                <div>
                  <h4 class="font-medium text-slate-900 line-clamp-1">{{ subject.name }}</h4>
                  <p class="text-sm text-slate-500 mt-0.5">{{ subject.code }}</p>
                </div>
              </div>
            </td>
            <td class="py-3 px-4">
              <span 
                class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium border"
                :class="getStatusBadgeClass(subject.status)"
              >
                {{ getStatusLabel(subject.status) }}
              </span>
            </td>
            <td class="py-3 px-4">
              <div class="flex items-center gap-4 text-sm text-slate-500">
                <div class="flex items-center gap-1" title="Số chương">
                  <BookOpen class="w-4 h-4 text-slate-400" />
                  {{ subject.chapterCount }}
                </div>
                <div class="flex items-center gap-1" title="Số bài học">
                  <FileText class="w-4 h-4 text-slate-400" />
                  {{ subject.lessonCount }}
                </div>
                <div class="flex items-center gap-1" title="Nội dung">
                  <Edit class="w-4 h-4 text-slate-400" />
                  {{ subject.contentCount }}
                </div>
              </div>
            </td>
            <td class="py-3 px-4 text-right">
              <router-link
                :to="`/content-council/subjects/${subject.id}`"
                class="inline-flex items-center justify-center px-3 py-1.5 bg-white border border-slate-200 text-slate-700 rounded-lg text-sm font-medium hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-1 transition-colors mr-2"
              >
                Chi tiết
              </router-link>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
