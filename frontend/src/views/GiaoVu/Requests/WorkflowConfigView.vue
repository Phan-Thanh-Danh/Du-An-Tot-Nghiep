<script setup>
import { ref, computed } from 'vue'
import {
  Settings2, Plus, ArrowRight, CheckCircle2, Clock, Shield, Zap, FileJson,
  Layers, Save, Search,
  GitBranch, Users, FileSignature, PlayCircle, Lock
} from 'lucide-vue-next'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'

const roleOptions = ['Giáo vụ khoa', 'Trưởng phòng Giáo vụ', 'Ban Giám hiệu', 'Phòng Đào tạo', 'Hệ thống']

let nextStepId = 10, nextBranchId = 20

function makeDefaultSteps(n) {
  const names = ['Tiếp nhận hồ sơ', 'Phê duyệt trưởng phòng', 'Thực thi tự động']
  const roles = ['Giáo vụ khoa', 'Trưởng phòng Giáo vụ', 'Hệ thống']
  const slas = ['24h', '48h', 'Tự động']
  
  const steps = []
  for (let i = 0; i < n; i++) {
    steps.push({
      id: `s${nextStepId++}`,
      name: names[i] || `Bước ${i + 1}`,
      role: roles[i] || roleOptions[0],
      sla: slas[i] || '24h',
      branches: i < n - 1
        ? [
            { id: `b${nextBranchId++}`, label: 'Hợp lệ / Đồng ý', nextStepId: null, isReject: false },
            { id: `b${nextBranchId++}`, label: 'Không hợp lệ / Từ chối', nextStepId: null, isReject: true },
          ]
        : []
    })
  }
  
  // Link next steps
  for (let i = 0; i < n - 1; i++) {
    steps[i].branches[0].nextStepId = steps[i+1].id
  }
  
  return steps
}

const workflows = ref([
  { id: 'WF-01', name: 'Nghỉ học / Bảo lưu', sla: '72h', status: 'active', version: 'v2.1', steps: makeDefaultSteps(3) },
  { id: 'WF-02', name: 'Chuyển lớp học phần', sla: '48h', status: 'active', version: 'v1.3', steps: makeDefaultSteps(2) },
  { id: 'WF-03', name: 'Cấp giấy xác nhận', sla: '24h', status: 'active', version: 'v1.0', steps: makeDefaultSteps(1) },
  { id: 'WF-04', name: 'Thi lại / Hoãn thi', sla: '48h', status: 'draft', version: 'v0.5', steps: makeDefaultSteps(2) },
])

const activeWorkflow = ref(workflows.value[1])
const searchQuery = ref('')

const filteredWorkflows = computed(() => {
  let result = workflows.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    result = result.filter(w => w.name.toLowerCase().includes(q))
  }
  return result
})

function getStepIcon(role) {
  if (role === 'Hệ thống') return Zap
  if (role === 'Ban Giám hiệu' || role === 'Trưởng phòng Giáo vụ') return Shield
  if (role === 'Giáo vụ khoa') return Users
  return FileSignature
}

function getRoleColor(role) {
  if (role === 'Hệ thống') return 'text-purple-500'
  if (role === 'Ban Giám hiệu') return 'text-amber-500'
  if (role === 'Trưởng phòng Giáo vụ') return 'text-blue-500'
  return 'text-emerald-500'
}

function getRoleBg(role) {
  if (role === 'Hệ thống') return 'bg-purple-50 dark:bg-purple-950/30 border-purple-200 dark:border-purple-800'
  if (role === 'Ban Giám hiệu') return 'bg-amber-50 dark:bg-amber-950/30 border-amber-200 dark:border-amber-800'
  if (role === 'Trưởng phòng Giáo vụ') return 'bg-blue-50 dark:bg-blue-950/30 border-blue-200 dark:border-blue-800'
  return 'bg-emerald-50 dark:bg-emerald-950/30 border-emerald-200 dark:border-emerald-800'
}
</script>

<template>
  <div class="workflow-view h-full flex flex-col space-y-4">
    <!-- Header -->
    <div class="flex items-start justify-between flex-wrap gap-4">
      <div>
        <div class="flex items-center gap-2">
          <GitBranch class="text-(--lg-primary)" :size="24" />
          <h1 class="text-xl font-bold text-(--text-heading)">Cấu hình quy trình (Worktree)</h1>
        </div>
        <p class="text-sm text-(--text-muted) mt-0.5 ml-8">Thiết kế luồng xử lý đơn từ dưới dạng cây trực quan.</p>
      </div>
      <div class="flex gap-2">
        <GlassButton variant="secondary">
          <FileJson :size="15" class="mr-1" /> Xuất JSON
        </GlassButton>
        <GlassButton variant="primary">
          <Plus :size="15" class="mr-1" /> Tạo quy trình mới
        </GlassButton>
      </div>
    </div>

    <!-- Main Layout -->
    <div class="flex gap-4 flex-1 min-h-0">
      
      <!-- Left: Workflow List -->
      <div class="w-72 flex flex-col gap-3 shrink-0">
        <div class="surface-card border border-(--border-card) rounded-2xl p-3 shadow-sm">
          <div class="relative">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 text-(--text-muted)" :size="15" />
            <input v-model="searchQuery" type="text" placeholder="Tìm quy trình..." class="w-full pl-9 pr-3 h-9 bg-(--surface-input) border border-(--border-input) rounded-lg text-sm text-(--text-body) outline-none focus:ring-2 focus:ring-(--border-focus)" />
          </div>
        </div>

        <div class="flex-1 overflow-y-auto space-y-2 pb-4 scrollbar-thin">
          <div
            v-for="wf in filteredWorkflows" :key="wf.id"
            class="surface-card border rounded-2xl p-3 cursor-pointer transition-all hover:shadow-md group relative overflow-hidden"
            :class="activeWorkflow?.id === wf.id ? 'border-(--lg-primary) ring-1 ring-(--lg-primary) bg-(--lg-primary)/5' : 'border-(--border-card)'"
            @click="activeWorkflow = wf"
          >
            <div class="flex items-center justify-between mb-1">
              <span class="text-xs font-mono font-bold text-(--text-muted)">{{ wf.id }}</span>
              <GlassBadge :variant="wf.status === 'active' ? 'success' : 'neutral'" size="xs">
                {{ wf.status === 'active' ? 'Đang áp dụng' : 'Bản nháp' }}
              </GlassBadge>
            </div>
            <p class="text-sm font-bold text-(--text-heading) line-clamp-2 leading-tight mb-2">{{ wf.name }}</p>
            <div class="flex items-center justify-between text-[11px] text-(--text-muted)">
              <div class="flex items-center gap-1">
                <Clock :size="12" /> SLA: {{ wf.sla }}
              </div>
              <div class="flex items-center gap-1 font-mono font-bold">
                <Layers :size="12" /> {{ wf.version }}
              </div>
            </div>
            
            <div v-if="activeWorkflow?.id === wf.id" class="absolute left-0 top-0 bottom-0 w-1 bg-(--lg-primary)"></div>
          </div>
        </div>
      </div>

      <!-- Right: Worktree Canvas -->
      <div class="flex-1 surface-card border border-(--border-card) rounded-2xl shadow-sm flex flex-col min-w-0 overflow-hidden relative">
        <!-- Canvas Toolbar -->
        <div class="p-3 border-b border-(--border-default) flex items-center justify-between bg-(--surface-solid) z-10">
          <div class="flex items-center gap-3">
            <h2 class="font-bold text-(--text-heading)">{{ activeWorkflow?.name }}</h2>
            <GlassBadge variant="primary" size="sm">Phiên bản {{ activeWorkflow?.version }}</GlassBadge>
          </div>
          <div class="flex gap-2">
            <div class="flex items-center bg-(--surface-input) rounded-lg p-0.5 border border-(--border-input)">
              <button class="px-3 py-1 text-xs font-medium rounded-md bg-(--surface-card) shadow-sm text-(--text-heading)">Tree View</button>
              <button class="px-3 py-1 text-xs font-medium rounded-md text-(--text-muted) hover:text-(--text-heading)">List View</button>
            </div>
            <GlassButton variant="primary" size="sm">
              <Save :size="14" class="mr-1" /> Lưu thay đổi
            </GlassButton>
          </div>
        </div>

        <!-- Tree Canvas -->
        <div class="flex-1 overflow-auto p-8 relative bg-slate-50/50 dark:bg-slate-900/20" id="worktree-canvas">
          <!-- Background grid -->
          <div class="absolute inset-0" style="background-image: radial-gradient(var(--border-default) 1px, transparent 1px); background-size: 24px 24px; opacity: 0.5;"></div>
          
          <div class="relative min-w-max flex items-start gap-12 pt-8 pl-8">
            <!-- Start Node -->
            <div class="flex flex-col items-center relative z-10">
              <div class="w-12 h-12 rounded-full bg-(--surface-card) border-2 border-(--lg-primary) text-(--lg-primary) flex items-center justify-center shadow-lg z-10">
                <PlayCircle :size="24" />
              </div>
              <p class="text-xs font-bold text-(--text-heading) mt-2 text-center">Bắt đầu<br/><span class="font-normal text-(--text-muted)">Sinh viên nộp đơn</span></p>
              
              <!-- Connection Line to Step 1 -->
              <div class="absolute top-6 left-12 w-12 border-t-2 border-dashed border-(--border-focus) -z-10"></div>
              <div class="absolute top-4 left-24 text-(--lg-primary)">
                <ArrowRight :size="16" />
              </div>
            </div>

            <!-- Workflow Steps (Horizontal Flow) -->
            <div class="flex gap-16 relative z-10">
              <template v-for="step in activeWorkflow.steps" :key="step.id">
                <div class="relative flex flex-col group">
                  <!-- The Step Node Card -->
                  <div class="w-64 rounded-xl border-2 shadow-sm transition-all hover:shadow-md hover:border-(--lg-primary) z-10 bg-(--surface-card)"
                       :class="getRoleBg(step.role)">
                    
                    <div class="p-3 border-b border-inherit flex items-center gap-2 bg-white/50 dark:bg-black/20 rounded-t-xl">
                      <component :is="getStepIcon(step.role)" :size="16" :class="getRoleColor(step.role)" />
                      <span class="text-xs font-bold uppercase tracking-wide flex-1 truncate text-(--text-heading)">{{ step.role }}</span>
                      <button class="text-(--text-muted) hover:text-(--text-heading) opacity-0 group-hover:opacity-100 transition-opacity"><Settings2 :size="14" /></button>
                    </div>
                    
                    <div class="p-4 space-y-3 bg-(--surface-card) rounded-b-xl">
                      <div>
                        <p class="font-bold text-(--text-heading) text-base leading-tight">{{ step.name }}</p>
                        <p class="text-xs text-(--text-muted) mt-1 font-mono">ID: {{ step.id }}</p>
                      </div>
                      
                      <div class="flex items-center gap-2 text-xs">
                        <Clock :size="14" class="text-(--text-muted)" />
                        <span class="text-(--text-body)">SLA xử lý: <strong>{{ step.sla }}</strong></span>
                      </div>
                    </div>
                  </div>

                  <!-- Branches / Next Steps -->
                  <div v-if="step.branches && step.branches.length > 0" class="absolute left-64 top-1/2 w-16 -z-10 border-t-2 border-(--border-default)"></div>
                  
                  <div v-if="step.branches && step.branches.length > 0" class="absolute left-[calc(16rem+16px)] top-1/2 -translate-y-1/2 flex flex-col gap-12 z-10 pl-6">
                    <!-- Branch connecting vertical line -->
                    <div class="absolute left-0 top-1/2 -translate-y-1/2 h-[calc(100%-48px)] w-px bg-(--border-default) -z-10"></div>

                    <div v-for="branch in step.branches" :key="branch.id" class="relative flex items-center gap-2">
                      <!-- Branch horizontal line -->
                      <div class="absolute right-full top-1/2 w-6 border-t-2 border-(--border-default) -z-10"></div>
                      
                      <GlassBadge :variant="branch.isReject ? 'danger' : 'success'" class="cursor-pointer hover:scale-105 transition-transform z-10 shadow-sm border border-inherit bg-(--surface-card)">
                        <span class="text-xs font-bold">{{ branch.label }}</span>
                      </GlassBadge>

                      <!-- If it connects to next step (handled by outer flex gap), just show arrow -->
                      <div v-if="!branch.isReject" class="absolute left-full top-1/2 w-10 border-t-2 border-(--border-focus) -z-10 border-dashed"></div>
                      <ArrowRight v-if="!branch.isReject" :size="16" class="absolute -right-10 top-1/2 -translate-y-1/2 text-(--lg-primary)" />

                      <!-- If it rejects, show end node -->
                      <div v-if="branch.isReject" class="absolute left-full top-1/2 w-8 border-t-2 border-red-200 dark:border-red-800 -z-10"></div>
                      <div v-if="branch.isReject" class="absolute -right-20 top-1/2 -translate-y-1/2 flex items-center gap-1 text-red-500">
                        <ArrowRight :size="14" />
                        <div class="w-8 h-8 rounded-full bg-red-50 dark:bg-red-950/40 border border-red-200 dark:border-red-800 flex items-center justify-center">
                          <Lock :size="14" />
                        </div>
                      </div>
                    </div>
                  </div>
                  
                  <!-- If this is the last step, show Finish node -->
                  <div v-if="!step.branches || step.branches.length === 0" class="absolute left-full top-1/2 -translate-y-1/2 flex items-center gap-2 pl-4 z-10">
                     <div class="w-8 border-t-2 border-emerald-500 -z-10"></div>
                     <ArrowRight :size="16" class="text-emerald-500" />
                     <div class="w-12 h-12 rounded-full bg-emerald-50 dark:bg-emerald-950/40 border-2 border-emerald-500 text-emerald-600 dark:text-emerald-400 flex items-center justify-center shadow-md ml-2">
                       <CheckCircle2 :size="24" />
                     </div>
                  </div>
                </div>
              </template>
            </div>
            
            <!-- Padding for right scrolling -->
            <div class="w-32 h-1 shrink-0"></div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.workflow-view {
  min-height: calc(100vh - 100px);
}
</style>
