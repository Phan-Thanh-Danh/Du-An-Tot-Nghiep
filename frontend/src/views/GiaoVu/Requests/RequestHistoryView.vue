<script setup>
import { ref } from 'vue'
import {
  History, CheckCircle2, XCircle, Search, Eye, Download, ShieldCheck, Bot
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const summaryCards = [
  { label: 'Tổng đơn đã xử lý', value: 850, tone: 'primary', icon: History },
  { label: 'Phê duyệt', value: 720, tone: 'success', icon: CheckCircle2 },
  { label: 'Từ chối', value: 130, tone: 'danger', icon: XCircle },
  { label: 'Xử lý tự động (AI)', value: 450, tone: 'info', icon: Bot },
]

const history = ref([
  { id: 'REQ-801', student: 'Nguyễn Văn A', type: 'Giấy xác nhận SV', result: 'approved', handler: 'Hệ thống (Auto)', date: '10/05/2026', sla: '2 phút' },
  { id: 'REQ-802', student: 'Trần Thị B', type: 'Chuyển lớp học phần', result: 'rejected', handler: 'Nguyễn Giáo Vụ', date: '09/05/2026', sla: '1.5 ngày' },
  { id: 'REQ-803', student: 'Lê Văn C', type: 'Xin thi lại', result: 'approved', handler: 'Phạm Giáo Vụ', date: '08/05/2026', sla: '2 ngày' },
])

const getResultBadge = (r) => ({
  approved: { label: 'Đã phê duyệt', variant: 'success' },
  rejected: { label: 'Đã từ chối', variant: 'danger' }
}[r] || { label: r, variant: 'neutral' })
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-7xl mx-auto w-full">
    <!-- Header -->
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <History class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Đơn đã xử lý</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Tra cứu các đơn đã phê duyệt, từ chối hoặc hoàn tất xử lý.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary">
          <Download :size="15" class="mr-1" /> Xuất báo cáo
        </GlassButton>
      </div>
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="card in summaryCards" :key="card.label" class="surface-card border border-(--border-card) rounded-2xl p-5 flex items-center justify-between shadow-sm h-full">
        <div>
          <p class="text-xs font-bold text-(--text-muted) uppercase tracking-wide">{{ card.label }}</p>
          <p class="text-3xl font-bold text-(--text-heading) mt-2">{{ card.value }}</p>
        </div>
        <div class="w-12 h-12 rounded-full flex items-center justify-center"
             :class="{
               'bg-(--accent-primary-soft) text-(--lg-primary)': card.tone === 'primary',
               'bg-(--color-danger-bg) text-(--color-danger-text)': card.tone === 'danger',
               'bg-(--color-warning-bg) text-(--color-warning-text)': card.tone === 'warning',
               'bg-(--color-success-bg) text-(--color-success-text)': card.tone === 'success',
               'bg-(--color-info-bg) text-(--color-info-text)': card.tone === 'info'
             }">
          <component :is="card.icon" :size="24" />
        </div>
      </div>
    </div>

    <!-- Filter & Table Shell -->
    <div class="surface-card border border-(--border-card) rounded-2xl flex flex-col min-h-0 flex-1 shadow-sm overflow-hidden">
      <!-- Filters -->
      <div class="p-4 border-b border-(--border-default) flex flex-wrap gap-3 bg-(--surface-input)">
        <div class="relative flex-1 min-w-[200px] max-w-sm">
          <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="15" />
          <input type="text" placeholder="Tra cứu mã đơn, mã SV..." class="pl-9 pr-4 h-10 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
        </div>
        <input type="date" class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm text-(--text-muted) outline-none focus:ring-2 focus:ring-(--lg-primary)" />
        <select class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
          <option value="">Kết quả xử lý</option>
          <option value="approved">Phê duyệt</option>
          <option value="rejected">Từ chối</option>
        </select>
        <select class="h-10 px-3 bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--border-focus)">
          <option value="">Người xử lý</option>
          <option value="auto">Hệ thống tự động</option>
          <option value="manual">Giáo vụ</option>
        </select>
      </div>

      <!-- Table -->
      <div class="flex-1 overflow-x-auto">
        <table class="w-full text-left text-sm whitespace-nowrap">
          <thead class="bg-(--surface-input) border-b border-(--border-default) text-(--text-muted)">
            <tr>
              <th class="px-5 py-4 font-semibold w-24">Mã đơn</th>
              <th class="px-5 py-4 font-semibold">Sinh viên & Loại đơn</th>
              <th class="px-5 py-4 font-semibold">Kết quả</th>
              <th class="px-5 py-4 font-semibold">Người xử lý</th>
              <th class="px-5 py-4 font-semibold">Ngày xử lý</th>
              <th class="px-5 py-4 font-semibold">SLA</th>
              <th class="px-5 py-4 font-semibold text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-(--border-default)">
            <tr v-for="item in history" :key="item.id" class="hover:bg-(--surface-hover) transition-colors">
              <td class="px-5 py-4 font-mono text-xs font-bold text-(--text-muted)">{{ item.id }}</td>
              <td class="px-5 py-4">
                <p class="font-bold text-(--text-heading)">{{ item.student }}</p>
                <p class="text-xs text-(--text-muted) mt-0.5">{{ item.type }}</p>
              </td>
              <td class="px-5 py-4">
                <GlassBadge :variant="getResultBadge(item.result).variant" size="sm">{{ getResultBadge(item.result).label }}</GlassBadge>
              </td>
              <td class="px-5 py-4">
                <div class="flex items-center gap-1.5 font-medium text-(--text-body)">
                  <Bot v-if="item.handler.includes('Auto')" :size="14" class="text-blue-500" />
                  <ShieldCheck v-else :size="14" class="text-emerald-500" />
                  {{ item.handler }}
                </div>
              </td>
              <td class="px-5 py-4 text-(--text-muted)">{{ item.date }}</td>
              <td class="px-5 py-4">
                <span class="inline-flex px-2 py-0.5 rounded bg-(--surface-input) text-xs font-mono font-medium">{{ item.sla }}</span>
              </td>
              <td class="px-5 py-4 text-right">
                <GlassButton variant="secondary" size="sm">
                  <Eye :size="14" class="mr-1" /> Chi tiết
                </GlassButton>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="border-t border-(--border-default) p-4 flex justify-between items-center bg-(--surface-input)">
         <p class="text-sm text-(--text-muted)">Hiển thị {{ history.length }} kết quả</p>
         <div class="flex gap-2">
            <GlassButton variant="secondary" size="sm">Trang trước</GlassButton>
            <GlassButton variant="primary" size="sm">1</GlassButton>
            <GlassButton variant="secondary" size="sm">Trang sau</GlassButton>
         </div>
      </div>
    </div>
  </div>
</template>
