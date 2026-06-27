<script setup>
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const tabs = [
  {
    name: 'Tổng quan',
    routeName: 'content-council-subject-overview',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Trình soạn nội dung',
    routeName: 'content-council-subject-editor',
    disabled: false,
    tooltip: ''
  },
  {
    name: 'Cài đặt',
    routeName: 'content-council-subject-settings', // Not implemented yet
    disabled: true,
    tooltip: 'Sẽ triển khai ở bước tiếp theo'
  }
]
</script>

<template>
  <div class="surface-card lg-glass-soft border-x border-b border-card rounded-b-xl mb-3 shadow-sm overflow-hidden">
    <div class="px-2 sm:px-4">
      <nav class="-mb-px flex space-x-6 sm:space-x-6 overflow-x-auto hide-scrollbar" aria-label="Tabs">
        <button
          v-for="tab in tabs"
          :key="tab.name"
          @click="!tab.disabled && router.push({ name: tab.routeName })"
          class="whitespace-nowrap py-2.5 px-1 border-b-2 font-medium text-sm transition-colors relative focus:outline-none"
          :class="[
            route.name === tab.routeName
              ? 'border-blue-600 text-blue-500'
              : 'border-transparent text-label hover:text-heading hover:border-slate-300',
            tab.disabled ? 'opacity-50 cursor-not-allowed hover:text-label hover:border-transparent' : 'cursor-pointer'
          ]"
          :disabled="tab.disabled"
          :title="tab.tooltip"
          :aria-current="route.name === tab.routeName ? 'page' : undefined"
          :aria-disabled="tab.disabled"
        >
          {{ tab.name }}
        </button>
      </nav>
    </div>
  </div>
</template>

<style scoped>
.hide-scrollbar::-webkit-scrollbar {
  display: none;
}
.hide-scrollbar {
  -ms-overflow-style: none;
  scrollbar-width: none;
}
</style>
