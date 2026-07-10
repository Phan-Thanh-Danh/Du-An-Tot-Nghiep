<script setup>
import { computed, onMounted, ref } from 'vue'
import { AlertCircle, Database, RefreshCw } from 'lucide-vue-next'

const props = defineProps({
  title: { type: String, required: true },
  subtitle: { type: String, default: '' },
  sourceLabel: { type: String, required: true },
  actionNote: { type: String, default: 'Các thao tác ghi sẽ chỉ bật khi endpoint tương ứng được xác nhận trong P16.' },
  emptyText: { type: String, default: 'API trả về danh sách rỗng.' },
  columns: { type: Array, required: true },
  loader: { type: Function, required: true },
})

const rows = ref([])
const loading = ref(false)
const error = ref('')

const hasRows = computed(() => rows.value.length > 0)

async function loadRows() {
  loading.value = true
  error.value = ''

  try {
    const result = await props.loader()
    rows.value = Array.isArray(result) ? result : []
  } catch (err) {
    rows.value = []
    error.value = err?.message || 'Không tải được dữ liệu từ API.'
  } finally {
    loading.value = false
  }
}

onMounted(loadRows)
</script>

<template>
  <section class="space-y-4">
    <header class="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
      <div>
        <p class="text-sm font-semibold text-label">SuperAdmin</p>
        <h1 class="text-2xl font-bold text-heading">{{ title }}</h1>
        <p v-if="subtitle" class="mt-1 text-sm text-body">{{ subtitle }}</p>
      </div>

      <button
        type="button"
        class="inline-flex items-center gap-2 rounded-lg border border-card px-3 py-2 text-sm font-semibold text-heading transition hover:bg-[var(--surface-card-hover)] disabled:cursor-wait disabled:opacity-60"
        :disabled="loading"
        @click="loadRows"
      >
        <RefreshCw class="h-4 w-4" :class="{ 'animate-spin': loading }" />
        Tải lại
      </button>
    </header>

    <div class="rounded-lg border border-card surface-card p-4">
      <div class="flex items-start gap-3">
        <Database class="mt-0.5 h-5 w-5 text-link" />
        <div>
          <p class="text-sm font-semibold text-heading">Nguồn dữ liệu thật</p>
          <p class="text-sm text-body">{{ sourceLabel }}</p>
          <p class="mt-1 text-xs text-label">{{ actionNote }}</p>
        </div>
      </div>
    </div>

    <div v-if="error" class="rounded-lg border border-[var(--color-danger-text)] bg-[var(--color-danger-bg)] p-4 text-sm text-[var(--color-danger-text)]">
      <div class="flex items-start gap-2">
        <AlertCircle class="mt-0.5 h-4 w-4" />
        <span>{{ error }}</span>
      </div>
    </div>

    <div class="overflow-hidden rounded-lg border border-card surface-card">
      <div v-if="loading" class="p-6 text-sm text-body">Đang tải dữ liệu từ API...</div>
      <div v-else-if="!hasRows" class="p-6 text-sm text-body">{{ emptyText }}</div>
      <div v-else class="overflow-x-auto">
        <table class="min-w-full divide-y divide-[var(--border-card)] text-sm">
          <thead class="bg-[var(--surface-muted)] text-left text-xs font-semibold uppercase text-label">
            <tr>
              <th v-for="column in columns" :key="column.key" class="px-4 py-3">
                {{ column.label }}
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-[var(--border-card)]">
            <tr v-for="(row, index) in rows" :key="row.id || row.code || index">
              <td v-for="column in columns" :key="column.key" class="px-4 py-3 text-body">
                {{ row[column.key] || 'Chưa có dữ liệu' }}
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </section>
</template>
