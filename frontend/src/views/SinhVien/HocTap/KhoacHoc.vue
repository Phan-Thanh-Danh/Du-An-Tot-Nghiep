<script setup>
import { ref } from 'vue'
import {
  Search, Filter, ChevronDown, CheckCircle2,
  Clock, Users, BookOpen, MoreVertical,
  PlayCircle, LayoutGrid, List
} from 'lucide-vue-next'

// Mock Data
const courses = ref([
  {
    id: 'CTDL101',
    name: 'Cấu trúc dữ liệu và Giải thuật',
    instructor: 'TS. Nguyễn Minh Khoa',
    credits: 3,
    progress: 72,
    totalSessions: 15,
    completedSessions: 11,
    status: 'learning',
    lastAccessed: '2 giờ trước',
    image: 'https://images.unsplash.com/photo-1555066931-4365d14bab8c?auto=format&fit=crop&q=80&w=300&h=150',
    color: 'blue'
  },
  {
    id: 'TRR201',
    name: 'Toán rời rạc',
    instructor: 'ThS. Trần Thu Hà',
    credits: 3,
    progress: 45,
    totalSessions: 15,
    completedSessions: 7,
    status: 'learning',
    lastAccessed: 'Hôm qua',
    image: 'https://images.unsplash.com/photo-1635070041078-e363dbe005cb?auto=format&fit=crop&q=80&w=300&h=150',
    color: 'indigo'
  },
  {
    id: 'LTW301',
    name: 'Lập trình Web (Vue & Nodejs)',
    instructor: 'KS. Lê Văn Tâm',
    credits: 4,
    progress: 90,
    totalSessions: 20,
    completedSessions: 18,
    status: 'learning',
    lastAccessed: 'Hôm nay, 08:30',
    image: 'https://images.unsplash.com/photo-1627398240309-089a14405537?auto=format&fit=crop&q=80&w=300&h=150',
    color: 'green'
  },
  {
    id: 'HQTCSDL401',
    name: 'Hệ quản trị Cơ sở dữ liệu',
    instructor: 'PGS. TS Lê Thị Bình',
    credits: 3,
    progress: 100,
    totalSessions: 15,
    completedSessions: 15,
    status: 'completed',
    lastAccessed: 'Tuần trước',
    image: 'https://images.unsplash.com/photo-1544383835-bda2bc66a55d?auto=format&fit=crop&q=80&w=300&h=150',
    color: 'slate'
  },
  {
    id: 'MMT501',
    name: 'Mạng máy tính cơ bản',
    instructor: 'ThS. Phạm Hữu Vinh',
    credits: 3,
    progress: 0,
    totalSessions: 15,
    completedSessions: 0,
    status: 'upcoming',
    lastAccessed: 'Chưa bắt đầu',
    image: 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?auto=format&fit=crop&q=80&w=300&h=150',
    color: 'purple'
  }
])

const viewMode = ref('grid') // 'grid' | 'list'
const searchQuery = ref('')
const selectedFilter = ref('all') // 'all' | 'learning' | 'completed' | 'upcoming'

const colorMap = {
  blue: 'bg-blue-500',
  indigo: 'bg-indigo-500',
  green: 'bg-green-500',
  orange: 'bg-orange-500',
  purple: 'bg-purple-500',
  slate: 'bg-slate-400'
}
</script>

<template>
  <div class="space-y-6 pb-10">

    <!-- Header Actions -->
    <div class="flex flex-col sm:flex-row gap-4 justify-between items-center bg-white p-4 rounded-2xl border border-slate-100 shadow-sm">
      <div class="flex flex-1 items-center gap-4 w-full">
        <!-- Search -->
        <div class="relative w-full sm:w-80">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" :size="18" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm kiếm môn học..."
            class="w-full rounded-xl border-none bg-slate-50 py-2.5 pl-10 pr-4 text-sm text-slate-800 placeholder-slate-400 focus:outline-none focus:ring-2 focus:ring-blue-100 transition-shadow"
          >
        </div>

        <!-- Filter Select -->
        <div class="relative hidden sm:block">
          <select
            v-model="selectedFilter"
            class="appearance-none rounded-xl border border-slate-200 bg-white py-2.5 pl-4 pr-10 text-sm font-medium text-slate-700 outline-none hover:bg-slate-50 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 cursor-pointer"
          >
            <option value="all">Tất cả khóa học</option>
            <option value="learning">Đang học</option>
            <option value="completed">Đã hoàn thành</option>
            <option value="upcoming">Sắp diễn ra</option>
          </select>
          <ChevronDown class="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none text-slate-400" :size="16" />
        </div>
      </div>

      <div class="flex items-center gap-2">
        <div class="flex items-center rounded-xl border border-slate-200 bg-slate-50 p-1">
          <button
            @click="viewMode = 'grid'"
            :class="['rounded-lg p-1.5 transition-colors', viewMode === 'grid' ? 'bg-white text-blue-600 shadow-sm' : 'text-slate-400 hover:text-slate-600']"
            title="Lưới"
          >
            <LayoutGrid :size="18" />
          </button>
          <button
            @click="viewMode = 'list'"
            :class="['rounded-lg p-1.5 transition-colors', viewMode === 'list' ? 'bg-white text-blue-600 shadow-sm' : 'text-slate-400 hover:text-slate-600']"
            title="Danh sách"
          >
            <List :size="18" />
          </button>
        </div>
      </div>
    </div>

    <!-- Stats -->
    <div class="flex gap-4 overflow-x-auto pb-2 scrollbar-hide">
      <div class="flex items-center gap-3 rounded-2xl bg-white px-5 py-3 border border-slate-100 shadow-sm min-w-max">
        <div class="flex h-10 w-10 items-center justify-center rounded-full bg-blue-50 text-blue-600">
          <BookOpen :size="18" />
        </div>
        <div>
          <p class="text-xs text-slate-500">Đang học</p>
          <p class="text-base font-bold text-slate-800">3 môn</p>
        </div>
      </div>
      <div class="flex items-center gap-3 rounded-2xl bg-white px-5 py-3 border border-slate-100 shadow-sm min-w-max">
        <div class="flex h-10 w-10 items-center justify-center rounded-full bg-green-50 text-green-600">
          <CheckCircle2 :size="18" />
        </div>
        <div>
          <p class="text-xs text-slate-500">Đã hoàn thành</p>
          <p class="text-base font-bold text-slate-800">1 môn</p>
        </div>
      </div>
      <div class="flex items-center gap-3 rounded-2xl bg-white px-5 py-3 border border-slate-100 shadow-sm min-w-max">
        <div class="flex h-10 w-10 items-center justify-center rounded-full bg-purple-50 text-purple-600">
          <Clock :size="18" />
        </div>
        <div>
          <p class="text-xs text-slate-500">Sắp tới</p>
          <p class="text-base font-bold text-slate-800">1 môn</p>
        </div>
      </div>
    </div>

    <!-- Course Grid -->
    <div
      v-if="viewMode === 'grid'"
      class="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-6"
    >
      <div
        v-for="course in courses"
        :key="course.id"
        class="group flex flex-col overflow-hidden rounded-2xl border border-slate-100 bg-white shadow-sm transition-all hover:shadow-md hover:-translate-y-1"
      >
        <!-- Thumbnail -->
        <div class="relative h-36 w-full overflow-hidden bg-slate-100">
          <img :src="course.image" :alt="course.name" class="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" />
          <div class="absolute inset-0 bg-gradient-to-t from-slate-900/60 to-transparent opacity-0 transition-opacity group-hover:opacity-100" />
          <button class="absolute left-1/2 top-1/2 -translate-x-1/2 -translate-y-1/2 text-white opacity-0 transition-all duration-300 group-hover:opacity-100 hover:scale-110">
            <PlayCircle :size="48" stroke-width="1.5" />
          </button>

          <!-- Status Badge -->
          <div class="absolute left-3 top-3">
            <span v-if="course.status === 'learning'" class="rounded-full bg-blue-500/90 backdrop-blur px-2.5 py-1 text-[10px] font-bold text-white uppercase tracking-wider">
              Đang học
            </span>
            <span v-else-if="course.status === 'completed'" class="rounded-full bg-green-500/90 backdrop-blur px-2.5 py-1 text-[10px] font-bold text-white uppercase tracking-wider">
              Hoàn thành
            </span>
            <span v-else class="rounded-full bg-purple-500/90 backdrop-blur px-2.5 py-1 text-[10px] font-bold text-white uppercase tracking-wider">
              Sắp tới
            </span>
          </div>
        </div>

        <!-- Content -->
        <div class="flex flex-1 flex-col p-5">
          <div class="mb-3 flex items-start justify-between gap-3">
            <div>
              <p class="text-xs font-semibold text-blue-600 mb-1">{{ course.id }}</p>
              <h3 class="text-base font-bold text-slate-800 leading-tight line-clamp-2 hover:text-blue-600 cursor-pointer transition-colors">{{ course.name }}</h3>
            </div>
            <button class="text-slate-300 hover:text-slate-600">
              <MoreVertical :size="18" />
            </button>
          </div>

          <div class="space-y-2 mt-auto">
            <div class="flex items-center gap-2 text-sm text-slate-500">
              <Users :size="14" />
              <span>{{ course.instructor }}</span>
            </div>
            <div class="flex items-center gap-2 text-sm text-slate-500">
              <BookOpen :size="14" />
              <span>{{ course.credits }} tín chỉ</span>
            </div>
          </div>

          <!-- Progress -->
          <div class="mt-6 pt-5 border-t border-slate-50">
            <div class="flex items-center justify-between mb-2">
              <span class="text-xs font-semibold text-slate-700">Tiến độ</span>
              <span class="text-xs font-bold text-slate-900">{{ course.progress }}%</span>
            </div>
            <div class="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
              <div :class="['h-full rounded-full transition-all duration-1000', colorMap[course.color]]" :style="{ width: course.progress + '%' }" />
            </div>
            <p class="mt-2 text-[11px] text-slate-400">Đã học {{ course.completedSessions }}/{{ course.totalSessions }} buổi · Truy cập: {{ course.lastAccessed }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Course List -->
    <div
      v-else
      class="flex flex-col gap-3"
    >
      <div
        v-for="course in courses"
        :key="course.id"
        class="group flex items-center gap-4 overflow-hidden rounded-2xl border border-slate-100 bg-white p-3 shadow-sm transition-all hover:border-blue-200 hover:shadow-md cursor-pointer"
      >
        <div class="h-20 w-32 flex-shrink-0 overflow-hidden rounded-xl bg-slate-100 relative">
          <img :src="course.image" :alt="course.name" class="h-full w-full object-cover transition-transform duration-500 group-hover:scale-105" />
          <div class="absolute inset-0 bg-slate-900/10 group-hover:bg-transparent transition-colors" />
        </div>

        <div class="flex min-w-0 flex-1 items-center justify-between gap-6">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2 mb-1">
              <span v-if="course.status === 'learning'" class="h-2 w-2 rounded-full bg-blue-500" />
              <span v-else-if="course.status === 'completed'" class="h-2 w-2 rounded-full bg-green-500" />
              <span v-else class="h-2 w-2 rounded-full bg-purple-500" />
              <p class="text-xs font-semibold text-slate-500">{{ course.id }}</p>
            </div>
            <h3 class="text-sm font-bold text-slate-800 leading-tight truncate group-hover:text-blue-600 transition-colors">{{ course.name }}</h3>
            <div class="flex items-center gap-4 mt-1">
              <span class="text-xs text-slate-500">{{ course.instructor }}</span>
              <span class="text-xs text-slate-400 hidden sm:inline-block">• {{ course.credits }} tín chỉ</span>
            </div>
          </div>

          <div class="hidden md:block w-48 flex-shrink-0">
            <div class="flex items-center justify-between mb-1">
              <span class="text-[11px] font-medium text-slate-500">{{ course.completedSessions }}/{{ course.totalSessions }} buổi</span>
              <span class="text-[11px] font-bold text-slate-700">{{ course.progress }}%</span>
            </div>
            <div class="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
              <div :class="['h-full rounded-full', colorMap[course.color]]" :style="{ width: course.progress + '%' }" />
            </div>
          </div>

          <div class="hidden lg:block w-32 flex-shrink-0 text-right">
            <p class="text-[10px] text-slate-400 uppercase tracking-wider mb-0.5">Truy cập lần cuối</p>
            <p class="text-xs font-medium text-slate-700">{{ course.lastAccessed }}</p>
          </div>

          <div class="flex-shrink-0 pl-2">
            <div class="flex h-8 w-8 items-center justify-center rounded-full bg-slate-50 text-slate-400 group-hover:bg-blue-50 group-hover:text-blue-600 transition-colors">
              <ChevronRight :size="16" />
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>
