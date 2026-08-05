<script setup>
/**
 * OrgChartNode.vue
 * Component cây đệ quy hiển thị sơ đồ tổ chức dạng gia phả (Top-Down Org Chart)
 * Thiết kế chuẩn Liquid Glass UI hỗ trợ 2 chế độ Sáng/Tối.
 */
import { computed } from 'vue'
import { Shield, Building2, MapPin, Users, BookOpen, Lock, CheckCircle2 } from 'lucide-vue-next'

const props = defineProps({
  node: {
    type: Object,
    required: true
  },
  selectedId: {
    type: [Number, String],
    default: null
  },
  level: {
    type: Number,
    default: 0
  }
})

const emit = defineEmits(['select-org'])

const isSelected = computed(() => props.node.id === props.selectedId)

const getTypeColorClass = (type) => {
  if (type === 'Root') return 'bg-purple-600 dark:bg-purple-500 text-white shadow-purple-500/20'
  if (type === 'Campus') return 'bg-blue-600 dark:bg-blue-500 text-white shadow-blue-500/20'
  return 'bg-teal-600 dark:bg-teal-500 text-white shadow-teal-500/20'
}

const getTypeBadgeText = (type) => {
  if (type === 'Root') return 'Tổ hợp Gốc (Root)'
  if (type === 'Campus') return 'Cơ sở chính (Campus)'
  return 'Chi nhánh (Sub-campus)'
}

const getTypeIcon = (type) => {
  if (type === 'Root') return Shield
  if (type === 'Campus') return Building2
  return MapPin
}

const selectCard = () => {
  emit('select-org', props.node)
}

const handleChildSelect = (childNode) => {
  emit('select-org', childNode)
}
</script>

<template>
  <div class="org-chart-node-wrapper flex flex-col items-center">
    <!-- Node Card Container -->
    <div
      class="org-card group relative cursor-pointer select-none rounded-2xl transition-all duration-300 transform hover:-translate-y-1 hover:shadow-xl"
      :class="[
        isSelected
          ? 'ring-2 ring-blue-500 dark:ring-blue-400 shadow-lg shadow-blue-500/10 lg-glass-strong border-blue-400 dark:border-blue-500'
          : 'lg-glass-soft surface-card border-default hover:border-slate-400 dark:hover:border-slate-500 shadow-sm'
      ]"
      style="min-width: 220px; max-width: 260px;"
      @click="selectCard"
    >
      <!-- Top level accent bar -->
      <div
        class="h-1.5 w-full rounded-t-2xl transition-colors"
        :class="[
          node.type === 'Root' ? 'bg-purple-600' : (node.type === 'Campus' ? 'bg-blue-600' : 'bg-teal-600')
        ]"
      />

      <div class="p-3.5 space-y-2.5">
        <!-- Node Header -->
        <div class="flex items-center justify-between gap-2">
          <span
            class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-[10px] font-extrabold uppercase tracking-wider"
            :class="getTypeColorClass(node.type)"
          >
            <component :is="getTypeIcon(node.type)" class="h-3 w-3" />
            {{ node.type }}
          </span>

          <span
            class="inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-bold"
            :class="[
              node.status === 'Locked'
                ? 'bg-rose-500/10 text-rose-600 dark:text-rose-400 border border-rose-300 dark:border-rose-500/30'
                : 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-300 dark:border-emerald-500/30'
            ]"
          >
            <component :is="node.status === 'Locked' ? Lock : CheckCircle2" class="h-3 w-3" />
            {{ node.status === 'Locked' ? 'Khóa' : 'Active' }}
          </span>
        </div>

        <!-- Node Name & Code -->
        <div>
          <h4 class="text-sm font-extrabold text-heading line-clamp-1 group-hover:text-link transition-colors">
            {{ node.name }}
          </h4>
          <p class="text-[11px] font-mono text-muted mt-0.5">
            Mã: <span class="font-bold text-label">{{ node.code }}</span>
          </p>
        </div>

        <!-- Mini Metrics -->
        <div class="grid grid-cols-2 gap-1.5 pt-2 border-t border-default/60 text-[11px]">
          <div class="flex items-center gap-1.5 text-muted">
            <Users class="h-3.5 w-3.5 text-blue-500" />
            <span><strong class="text-heading font-bold">{{ node.metrics?.users || 0 }}</strong> ng dùng</span>
          </div>
          <div class="flex items-center gap-1.5 text-muted">
            <BookOpen class="h-3.5 w-3.5 text-teal-500" />
            <span><strong class="text-heading font-bold">{{ node.metrics?.classes || 0 }}</strong> lớp</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Connector line down from parent to children branch -->
    <div
      v-if="node.children && node.children.length > 0"
      class="w-0.5 h-6 bg-slate-300 dark:bg-slate-600 transition-colors"
    />

    <!-- Children Branches Container -->
    <div
      v-if="node.children && node.children.length > 0"
      class="children-container relative flex flex-row items-start justify-center pt-2"
    >
      <!-- Horizontal crossbar connecting all child branches -->
      <div
        v-if="node.children.length > 1"
        class="absolute top-0 h-0.5 bg-slate-300 dark:bg-slate-600 transition-colors"
        :style="{
          left: `calc(100% / ${node.children.length * 2})`,
          right: `calc(100% / ${node.children.length * 2})`
        }"
      />

      <!-- Sub-tree per child -->
      <div
        v-for="(child, idx) in node.children"
        :key="child.id"
        class="child-branch relative px-3 flex flex-col items-center"
      >
        <!-- Vertical connector line from crossbar into child card -->
        <div class="w-0.5 h-4 bg-slate-300 dark:bg-slate-600 mb-2 transition-colors" />

        <!-- Recursive rendering of child node -->
        <OrgChartNode
          :node="child"
          :selected-id="selectedId"
          :level="level + 1"
          @select-org="handleChildSelect"
        />
      </div>
    </div>
  </div>
</template>

<style scoped>
.org-chart-node-wrapper {
  position: relative;
}

.children-container {
  display: flex;
  width: 100%;
}
</style>
