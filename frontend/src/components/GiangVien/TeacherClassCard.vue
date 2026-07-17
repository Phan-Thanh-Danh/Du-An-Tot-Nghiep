<script setup>
import { computed } from 'vue'
import {
  BookOpen,
  Calendar,
  Layers,
  Users,
  CheckCircle2,
  Clock,
  AlertCircle
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'

const props = defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  semester: { type: String, default: 'Học kỳ hiện tại' },
  status: { type: String, default: '' },
  studentsCount: { type: [Number, String], default: null },
  lessonsCount: { type: [Number, String], default: null },
  progress: { type: Number, default: null },
  icon: { type: Object, default: () => BookOpen }
})

function getStatusText(status) {
  switch (status) {
    case 'Published': return 'Đang dạy'
    case 'Draft': return 'Bản nháp'
    case 'Archived': return 'Đã kết thúc'
    default: return status
  }
}

function getStatusVariant(status) {
  switch (status) {
    case 'Published': return 'success'
    case 'Draft': return 'warning'
    case 'Archived': return 'neutral'
    default: return 'neutral'
  }
}
</script>

<template>
  <div class="course-card group">
    <div class="course-card-header">
      <div class="course-icon-wrapper">
        <component :is="icon" :size="24" class="text-link" />
      </div>
      <GlassBadge v-if="status" :variant="getStatusVariant(status)" size="sm" class="status-badge">
        <CheckCircle2 v-if="status === 'Published'" :size="12" />
        <Clock v-else-if="status === 'Draft'" :size="12" />
        <AlertCircle v-else :size="12" />
        {{ getStatusText(status) }}
      </GlassBadge>
    </div>
    
    <div class="course-card-body">
      <h3 class="course-title" :title="title">{{ title }}</h3>
      
      <div class="course-meta">
        <span class="meta-item" v-if="semester">
          <Calendar :size="14" />
          {{ semester }}
        </span>
        <span class="meta-item" v-if="subtitle">
          <Layers :size="14" />
          {{ subtitle }}
        </span>
        <span class="meta-item" v-if="studentsCount !== null">
          <Users :size="14" />
          {{ studentsCount }} SV
        </span>
      </div>
      
      <!-- Optional Progress Section -->
      <div v-if="progress !== null || lessonsCount !== null" class="course-progress">
        <div class="progress-header">
          <span class="text-xs font-semibold text-muted" v-if="lessonsCount !== null">{{ lessonsCount }} bài học</span>
          <span class="text-xs font-bold text-link" v-if="progress !== null">{{ progress }}%</span>
        </div>
        <div class="progress-track" v-if="progress !== null">
          <div class="progress-fill" :style="{ width: `${progress}%` }"></div>
        </div>
      </div>
    </div>
    
    <div class="course-card-footer">
      <slot name="action"></slot>
    </div>
  </div>
</template>

<style scoped>
.course-card {
  display: flex;
  flex-direction: column;
  background: var(--surface-card);
  border: 1px solid var(--border-card);
  border-radius: var(--radius-xl);
  overflow: hidden;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -1px rgba(0, 0, 0, 0.03);
}

.course-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 12px 20px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -4px rgba(0, 0, 0, 0.04);
  border-color: rgba(37, 99, 235, 0.3);
}

.course-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  padding: 1.25rem 1.25rem 0;
}

.course-icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 3rem;
  height: 3rem;
  border-radius: var(--radius-lg);
  background: var(--surface-input);
  border: 1px solid var(--border-input);
  transition: all 0.3s ease;
}

.course-card:hover .course-icon-wrapper {
  background: rgba(37, 99, 235, 0.1);
  border-color: rgba(37, 99, 235, 0.2);
}

.status-badge {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.course-card-body {
  padding: 1.25rem;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.course-title {
  margin: 0 0 0.75rem;
  font-size: 1.125rem;
  font-weight: 800;
  color: var(--text-heading);
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  text-overflow: ellipsis;
}

.course-meta {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-muted);
  background: var(--surface-input);
  padding: 0.25rem 0.625rem;
  border-radius: var(--radius-md);
  border: 1px solid var(--border-input);
}

.course-progress {
  margin-top: auto;
}

.progress-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 0.5rem;
}

.progress-track {
  width: 100%;
  height: 0.375rem;
  background: var(--surface-input);
  border-radius: 999px;
  overflow: hidden;
  border: 1px solid var(--border-card);
}

.progress-fill {
  height: 100%;
  background: var(--text-link);
  border-radius: inherit;
  transition: width 0.8s cubic-bezier(0.4, 0, 0.2, 1);
}

.course-card-footer {
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--border-card);
  background: var(--surface-sidebar);
}
</style>
