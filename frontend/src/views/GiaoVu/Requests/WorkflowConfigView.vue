<script setup>
import { ref, computed, onMounted } from 'vue'
import {
  Network, Search, Settings2, Plus, Download,
  ArrowRight, Clock,
  Bot, ShieldCheck, AlignLeft, CheckCircle2,
  Loader2
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import ListSkeleton from '@/components/common/skeleton/ListSkeleton.vue'
import { workflowApi } from '@/services/workflowApi'
import { usePopupStore } from '@/stores/popup'

const popup = usePopupStore()

const workflows = ref([])
const selectedWf = ref(null)
const loading = ref(true)
const saving = ref(false)

const wfSteps = computed(() => {
  return selectedWf.value?.wfSteps || []
})

async function fetchWorkflows() {
  loading.value = true
  try {
    workflows.value = await workflowApi.getWorkflows()
    if (workflows.value.length > 0) {
      selectedWf.value = workflows.value[0]
    }
  } catch (err) {
    popup.error('Lỗi tải dữ liệu', err?.message || 'Không thể tải cấu hình quy trình')
  } finally {
    loading.value = false
  }
}

async function toggleActive() {
  if (!selectedWf.value) return
  saving.value = true
  try {
    const updated = await workflowApi.updateWorkflow(selectedWf.value.id, {
      active: !selectedWf.value.active
    })
    const index = workflows.value.findIndex(w => w.id === updated.id)
    if (index !== -1) workflows.value[index] = updated
    selectedWf.value = updated
    popup.success('Thành công', 'Đã cập nhật trạng thái quy trình')
  } catch (err) {
    popup.error('Lỗi lưu', err?.message || 'Không thể lưu thay đổi')
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  fetchWorkflows()
})
</script>

<template>
  <div class="h-full flex flex-col space-y-4 max-w-[1600px] mx-auto w-full">
    <!-- Header -->
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <Network class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Cấu hình quy trình</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Thiết kế luồng xử lý đơn từ theo từng loại đơn với Stable Process Builder Layout.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary"><Download :size="15" class="mr-1" /> Xuất JSON</GlassButton>
        <GlassButton variant="primary"><Plus :size="15" class="mr-1" /> Quy trình mới</GlassButton>
      </div>
    </div>

    <!-- Main Content Layout (Stable Builder) -->
    <div class="flex gap-4 flex-1 min-h-0 flex-col lg:flex-row">

      <!-- Left Panel: Workflow List -->
      <div class="w-full lg:w-72 shrink-0 surface-card border border-(--border-card) rounded-2xl flex flex-col min-h-0 shadow-sm overflow-hidden">
        <div class="p-3 border-b border-(--border-default) bg-(--surface-input)">
          <div class="relative">
            <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="14" />
            <input type="text" placeholder="Tìm quy trình..." class="pl-8 pr-3 h-9 w-full bg-(--surface-card) border border-(--border-input) rounded-xl text-sm outline-none focus:ring-2 focus:ring-(--lg-primary)" />
          </div>
        </div>

        <div class="flex-1 overflow-y-auto p-2 space-y-1">
           <div v-if="loading" class="p-4">
             <ListSkeleton :rows="6" />
           </div>
           <div v-else-if="workflows.length === 0" class="p-4 text-center text-sm text-(--text-muted)">
             Không có quy trình nào.
           </div>
           <div v-else v-for="wf in workflows" :key="wf.id"
                @click="selectedWf = wf"
                class="p-3 rounded-xl cursor-pointer transition-all border flex flex-col gap-2"
                :class="selectedWf?.id === wf.id ? 'bg-(--lg-primary)/10 border-(--lg-primary) shadow-sm' : 'border-transparent hover:bg-(--surface-hover)'">
              <div class="flex items-start justify-between">
                 <h3 class="font-bold text-sm text-(--text-heading) leading-tight">{{ wf.name }}</h3>
                 <div class="w-2 h-2 rounded-full mt-1 shrink-0" :class="wf.active ? 'bg-emerald-500' : 'bg-(--text-muted)'"></div>
              </div>
              <div class="flex items-center gap-3 text-xs font-medium text-(--text-muted)">
                 <span class="flex items-center gap-1"><AlignLeft :size="12"/> {{ wf.steps }} bước</span>
                 <span class="flex items-center gap-1"><Clock :size="12"/> SLA: {{ wf.sla }}</span>
              </div>
           </div>
        </div>
      </div>

      <!-- Right Panel: Process Builder -->
      <div class="flex-1 surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col min-h-0 overflow-hidden">

         <!-- Builder Header -->
        <div class="p-5 border-b border-(--border-default) flex items-start justify-between bg-(--surface-input)">
          <div v-if="selectedWf">
             <div class="flex items-center gap-3 mb-2">
                <h2 class="text-xl font-bold text-(--text-heading)">{{ selectedWf.name }}</h2>
                <GlassBadge 
                  :variant="selectedWf.active ? 'success' : 'neutral'" 
                  class="cursor-pointer hover:opacity-80 transition-opacity"
                  @click="toggleActive"
                >
                  <Loader2 v-if="saving" class="w-3 h-3 mr-1 animate-spin inline-block" />
                  {{ selectedWf.active ? 'Đang kích hoạt' : 'Bản nháp' }}
                </GlassBadge>
             </div>
             <p class="text-sm text-(--text-muted)">ID: {{ selectedWf.loaiDon }}</p>
          </div>
          <GlassButton variant="primary"><Settings2 :size="15" class="mr-1"/> Lưu thay đổi</GlassButton>
        </div>

        <!-- Stable Timeline Process -->
        <div class="flex-1 overflow-y-auto p-4 lg:p-8 flex justify-center">
           <div class="max-w-2xl w-full flex flex-col gap-0 py-4 relative">

              <!-- Vertical Line connector -->
              <div class="absolute left-[23px] top-6 bottom-6 w-0.5 bg-(--border-default)"></div>

              <!-- Steps -->
              <div v-for="(step, idx) in wfSteps" :key="step.id" class="flex gap-4 relative">

                 <!-- Step Number Node -->
                 <div class="shrink-0 flex flex-col items-center z-10 pt-1">
                    <div class="w-12 h-12 rounded-full border-2 bg-(--surface-card) flex items-center justify-center font-bold shadow-sm"
                         :class="step.type === 'auto' ? 'border-blue-500 text-blue-600' : 'border-emerald-500 text-emerald-600'">
                       {{ idx + 1 }}
                    </div>
                 </div>

                 <!-- Step Card -->
                 <div class="flex-1 pb-8">
                    <div class="surface-card border border-(--border-card) rounded-2xl p-5 shadow-sm hover:shadow-md transition-shadow relative">
                       <div class="flex justify-between items-start mb-3">
                          <h3 class="font-bold text-lg text-(--text-heading)">{{ step.name }}</h3>
                          <GlassBadge :variant="step.type === 'auto' ? 'info' : 'success'">
                             <span class="flex items-center gap-1">
                                <Bot v-if="step.type === 'auto'" :size="12"/>
                                <ShieldCheck v-else :size="12"/>
                                {{ step.role }}
                             </span>
                          </GlassBadge>
                       </div>

                       <p class="text-sm text-(--text-muted) mb-4">SLA yêu cầu: <strong class="text-(--text-heading)">{{ step.sla }}</strong></p>

                       <!-- Branch conditions (if any) -->
                       <div v-if="idx === 1" class="mt-4 pt-4 border-t border-(--border-default) space-y-2">
                          <p class="text-xs font-bold text-(--text-muted) uppercase">Quy tắc rẽ nhánh:</p>
                          <div class="flex flex-col gap-2">
                             <div class="flex flex-wrap items-center gap-2 p-3 rounded-xl bg-(--color-success-bg) border border-(--border-default) text-sm">
                                <CheckCircle2 class="text-(--color-success-text) shrink-0" :size="16"/>
                                <span class="font-medium text-(--color-success-text)">Đủ điều kiện (Còn slot)</span>
                                <ArrowRight class="text-(--color-success-text) shrink-0 mx-1" :size="14" />
                                <span class="text-(--text-heading) font-medium">Tiếp tục Bước 3</span>
                             </div>
                             <div class="flex flex-wrap items-center gap-2 p-3 rounded-xl bg-(--color-danger-bg) border border-(--border-default) text-sm">
                                <X class="text-(--color-danger-text) shrink-0" :size="16"/>
                                <span class="font-medium text-(--color-danger-text)">Không đủ điều kiện (Lớp đầy)</span>
                                <ArrowRight class="text-(--color-danger-text) shrink-0 mx-1" :size="14" />
                                <span class="text-(--text-heading) font-medium">Từ chối tự động & Kết thúc</span>
                             </div>
                          </div>
                       </div>

                    </div>
                 </div>
              </div>

              <!-- End node -->
              <div class="flex gap-4 relative mt-2">
                 <div class="shrink-0 flex flex-col items-center z-10">
                    <div class="w-12 h-12 rounded-full border-2 border-(--border-default) bg-(--surface-input) flex items-center justify-center shadow-sm">
                       <CheckCircle2 :size="24" class="text-(--text-muted)"/>
                    </div>
                 </div>
                 <div class="flex-1 flex items-center h-12">
                    <span class="font-bold text-(--text-muted) uppercase tracking-wide">Hoàn tất quy trình</span>
                 </div>
              </div>

           </div>
        </div>

      </div>
    </div>
  </div>
</template>
