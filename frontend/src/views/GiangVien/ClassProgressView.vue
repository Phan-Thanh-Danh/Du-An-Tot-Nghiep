<script setup>
import { ref } from 'vue'
import { 
  ArrowLeft, Search, User, Mail, Activity, Target, 
  BookOpen, Clock, ChevronRight, MoreVertical 
} from 'lucide-vue-next'

const students = ref([
  { id: 'SV16001', name: 'Nguyễn Văn A', email: 'anv@student.edu.vn', progress: 85, gpa: 8.2, absent: 1 },
  { id: 'SV16002', name: 'Trần Thị B', email: 'btt@student.edu.vn', progress: 70, gpa: 7.5, absent: 3 },
  { id: 'SV16003', name: 'Lê Hoàng C', email: 'clh@student.edu.vn', progress: 95, gpa: 9.0, absent: 0 },
  { id: 'SV16004', name: 'Phạm Minh D', email: 'dpm@student.edu.vn', progress: 45, gpa: 5.4, absent: 5 },
])

const overallProgress = 74
const completedLessons = 18
const totalLessons = 24
</script>

<template>
  <div class="space-y-6 pb-10">
    <!-- Header with Back -->
    <div class="flex items-center gap-4">
      <router-link to="/teacher/classes" class="p-2 rounded-xl bg-white border border-slate-100 text-slate-400 hover:text-indigo-600 transition-all shadow-sm">
        <ArrowLeft :size="20" />
      </router-link>
      <div>
        <h1 class="text-2xl font-bold text-slate-800 tracking-tight">Chi tiết lớp SE1601</h1>
        <p class="text-sm text-slate-500">Tiến độ học tập và danh sách sinh viên.</p>
      </div>
    </div>

    <!-- Stats & Progress Section -->
    <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
      <!-- Progress Bar Card -->
      <div class="lg-card-glass p-6 bg-indigo-900 text-white border-none shadow-indigo-200">
        <div class="flex justify-between items-center mb-6">
           <h3 class="font-bold text-indigo-100 uppercase tracking-widest text-xs">Tiến độ chung của lớp</h3>
           <Activity :size="20" class="text-indigo-300" />
        </div>
        <div class="flex items-baseline gap-2 mb-2">
           <span class="text-5xl font-black">{{ overallProgress }}%</span>
           <span class="text-indigo-300 text-sm">Hoàn thành</span>
        </div>
        <!-- Progress Bar -->
        <div class="w-full h-3 bg-white/10 rounded-full overflow-hidden mb-6">
           <div class="h-full bg-gradient-to-r from-indigo-400 to-purple-400" :style="{ width: overallProgress + '%' }"></div>
        </div>
        <div class="grid grid-cols-2 gap-4">
           <div class="bg-white/10 rounded-2xl p-3 border border-white/10">
              <p class="text-[10px] text-indigo-200 font-bold uppercase tracking-wider">Đã học</p>
              <p class="text-xl font-black mt-1">{{ completedLessons }} bài</p>
           </div>
           <div class="bg-white/10 rounded-2xl p-3 border border-white/10">
              <p class="text-[10px] text-indigo-200 font-bold uppercase tracking-wider">Chưa học</p>
              <p class="text-xl font-black mt-1">{{ totalLessons - completedLessons }} bài</p>
           </div>
        </div>
      </div>

      <!-- Chart Placeholder -->
      <div class="lg-card-glass lg:col-span-2 p-6 flex flex-col">
        <div class="flex justify-between items-center mb-4">
           <h3 class="font-bold text-slate-800">Biểu đồ phân bổ tiến độ</h3>
           <Target :size="20" class="text-slate-300" />
        </div>
        <div class="flex-1 flex items-end justify-between px-4 pb-4 gap-4">
           <!-- Mock Bars for Progress Distribution -->
           <div v-for="(h, i) in [20, 35, 60, 45, 90, 70, 85, 30]" :key="i" 
                class="flex-1 bg-indigo-500 rounded-t-lg transition-all hover:bg-indigo-600 cursor-help" 
                :style="{ height: h + '%' }" 
                :title="'Nhóm ' + (i+1) + ': ' + h + ' SV'" />
        </div>
        <div class="pt-4 border-t border-slate-50 flex justify-around text-[10px] font-bold text-slate-400 uppercase tracking-widest">
           <span>< 20%</span> <span>40%</span> <span>60%</span> <span>80%</span> <span>100%</span>
        </div>
      </div>
    </div>

    <!-- Students Table Section -->
    <div class="rounded-[28px] border border-slate-100 bg-white shadow-sm overflow-hidden">
      <div class="p-6 border-b border-slate-50 flex flex-col md:flex-row md:items-center justify-between gap-4">
        <h2 class="text-xl font-bold text-slate-800">Danh sách sinh viên trong lớp</h2>
        <div class="relative w-full md:w-72">
          <Search :size="16" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400" />
          <input type="text" placeholder="Tìm sinh viên..." class="w-full rounded-xl border border-slate-100 bg-slate-50 pl-9 pr-4 py-2 text-xs outline-none focus:border-indigo-300" />
        </div>
      </div>
      
      <div class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-slate-50/50 text-[11px] font-bold uppercase tracking-wider text-slate-400">
              <th class="px-8 py-4">Sinh viên</th>
              <th class="px-6 py-4">Email</th>
              <th class="px-6 py-4">Tiến độ</th>
              <th class="px-6 py-4">Điểm TB</th>
              <th class="px-6 py-4">Vắng</th>
              <th class="px-8 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-slate-50">
            <tr v-for="sv in students" :key="sv.id" class="group hover:bg-slate-50/50 transition-colors">
              <td class="px-8 py-4">
                <div class="flex items-center gap-3">
                  <div class="h-9 w-9 rounded-full bg-gradient-to-br from-indigo-100 to-purple-100 flex items-center justify-center text-indigo-700 font-bold text-xs">
                    {{ sv.name.split(' ').pop()[0] }}
                  </div>
                  <div>
                    <p class="text-sm font-bold text-slate-800">{{ sv.name }}</p>
                    <p class="text-[10px] text-slate-400">{{ sv.id }}</p>
                  </div>
                </div>
              </td>
              <td class="px-6 py-4 text-sm text-slate-500">{{ sv.email }}</td>
              <td class="px-6 py-4">
                <div class="flex items-center gap-3">
                  <div class="flex-1 h-1.5 w-24 bg-slate-100 rounded-full overflow-hidden">
                    <div :class="['h-full rounded-full', sv.progress < 50 ? 'bg-rose-500' : 'bg-emerald-500']" :style="{ width: sv.progress + '%' }"></div>
                  </div>
                  <span class="text-xs font-bold text-slate-700">{{ sv.progress }}%</span>
                </div>
              </td>
              <td class="px-6 py-4 text-sm font-black" :class="sv.gpa < 5 ? 'text-rose-500' : 'text-slate-800'">{{ sv.gpa }}</td>
              <td class="px-6 py-4">
                <span :class="['rounded-full px-2 py-0.5 text-[10px] font-bold', sv.absent > 4 ? 'bg-red-50 text-red-600' : 'bg-slate-100 text-slate-500']">
                  {{ sv.absent }} buổi
                </span>
              </td>
              <td class="px-8 py-4 text-right">
                <div class="flex items-center justify-end gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button class="p-2 text-slate-400 hover:text-indigo-600 hover:bg-indigo-50 rounded-lg transition-all" title="View Profile"><User :size="16" /></button>
                  <button class="p-2 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-all" title="View Submissions"><BookOpen :size="16" /></button>
                  <button class="p-2 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-all" title="View Grades"><Activity :size="16" /></button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
.shadow-indigo-200 {
  shadow-color: rgba(79, 70, 229, 0.2);
}
</style>
