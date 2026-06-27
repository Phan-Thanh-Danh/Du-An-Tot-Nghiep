<script setup>
import { Layers3, CalendarDays, BookOpen, ClipboardCheck, Award } from 'lucide-vue-next'

const props = defineProps({
  size: { type: Number, default: 270 },
})
</script>

<template>
  <div
    class="constellation-root"
    :style="{ '--sz': props.size + 'px' }"
    aria-hidden="true"
  >
    <!-- Static Halo background -->
    <div class="constellation-halo"></div>

    <!-- Connector Lines with pulse animation -->
    <svg class="connector-lines" viewBox="0 0 100 100">
      <line class="connector connector-0" x1="50" y1="50" x2="50" y2="15" stroke="currentColor" stroke-width="1" stroke-dasharray="2 4" />
      <line class="connector connector-1" x1="50" y1="50" x2="85" y2="50" stroke="currentColor" stroke-width="1" stroke-dasharray="2 4" />
      <line class="connector connector-2" x1="50" y1="50" x2="50" y2="85" stroke="currentColor" stroke-width="1" stroke-dasharray="2 4" />
      <line class="connector connector-3" x1="50" y1="50" x2="15" y2="50" stroke="currentColor" stroke-width="1" stroke-dasharray="2 4" />

      <!-- Pulse overlay lines (data flow from core to node) -->
      <line class="connector-pulse pulse-0" x1="50" y1="50" x2="50" y2="15" stroke="currentColor" stroke-width="1.5" stroke-dasharray="6 40" />
      <line class="connector-pulse pulse-1" x1="50" y1="50" x2="85" y2="50" stroke="currentColor" stroke-width="1.5" stroke-dasharray="6 40" />
      <line class="connector-pulse pulse-2" x1="50" y1="50" x2="50" y2="85" stroke="currentColor" stroke-width="1.5" stroke-dasharray="6 40" />
      <line class="connector-pulse pulse-3" x1="50" y1="50" x2="15" y2="50" stroke="currentColor" stroke-width="1.5" stroke-dasharray="6 40" />
    </svg>

    <!-- Nodes -->
    <div
      v-for="(item, i) in [
        { Icon: CalendarDays, label: 'Lịch học', color: '#2563eb', x: 50, y: 15 },
        { Icon: BookOpen, label: 'Khóa học', color: '#0891b2', x: 85, y: 50 },
        { Icon: ClipboardCheck, label: 'Bài tập', color: '#7c3aed', x: 50, y: 85 },
        { Icon: Award, label: 'Kết quả', color: '#16a34a', x: 15, y: 50 },
      ]"
      :key="i"
      class="constellation-node"
      :style="{ left: item.x + '%', top: item.y + '%' }"
    >
      <div class="node-icon" :style="{ color: item.color }">
        <component :is="item.Icon" :size="Math.round(props.size * 0.075)" />
      </div>
      <div class="node-label">{{ item.label }}</div>
    </div>
    
    <!-- Core Center with breathing -->
    <div class="constellation-core constellation-ambient">
      <div class="core-glow constellation-ambient"></div>
      <Layers3 :size="Math.round(props.size * 0.14)" />
    </div>
  </div>
</template>

<style scoped>
.constellation-root {
  position: relative;
  width: var(--sz);
  height: var(--sz);
  flex-shrink: 0;
}

.constellation-halo {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: calc(var(--sz) * 0.7);
  height: calc(var(--sz) * 0.7);
  border-radius: 50%;
  background: radial-gradient(circle, rgba(37, 99, 235, 0.05), transparent 70%);
  z-index: 0;
  pointer-events: none;
}

/* ═══ Core with breathing ambient ═══ */
.constellation-core {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  width: calc(var(--sz) * 0.32);
  height: calc(var(--sz) * 0.32);
  border-radius: 50%;
  background: radial-gradient(circle at 30% 22%, rgba(255, 255, 255, 0.96), rgba(239, 246, 255, 0.86));
  backdrop-filter: blur(12px);
  box-shadow: 0 16px 36px rgba(37, 99, 235, 0.12), inset 0 1px 0 rgba(255, 255, 255, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #2563eb;
  z-index: 3;
  border: 1px solid rgba(255, 255, 255, 0.6);
  animation: core-breathe 5.8s ease-in-out infinite alternate;
}

.core-glow {
  position: absolute;
  inset: -6px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(37, 99, 235, 0.15), transparent 70%);
  z-index: -1;
  pointer-events: none;
  animation: core-glow-pulse 5.8s ease-in-out infinite alternate;
}

@keyframes core-breathe {
  0% { transform: translate(-50%, -50%) scale(1); }
  100% { transform: translate(-50%, -50%) scale(1.012); }
}

@keyframes core-glow-pulse {
  0% { opacity: 0.62; }
  100% { opacity: 0.82; }
}

/* ═══ Connector lines ═══ */
.connector-lines {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  color: rgba(148, 163, 184, 0.24);
  z-index: 1;
}

/* ═══ Connector pulse (data flow from core to node) ═══ */
.connector-pulse {
  color: rgba(37, 99, 235, 0.28);
  animation: connector-flow 5.4s linear infinite;
}

.pulse-0 { animation-delay: 0s; }
.pulse-1 { animation-delay: -1.2s; }
.pulse-2 { animation-delay: -2.4s; }
.pulse-3 { animation-delay: -3.6s; }

@keyframes connector-flow {
  0% { stroke-dashoffset: 0; }
  100% { stroke-dashoffset: -46; }
}

/* ═══ Nodes (static position, no drift) ═══ */
.constellation-node {
  position: absolute;
  transform: translate(-50%, -50%);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 8px;
  z-index: 2;
}

.node-icon {
  width: calc(var(--sz) * 0.16);
  height: calc(var(--sz) * 0.16);
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.88);
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 6px 16px rgba(15, 23, 42, 0.06), inset 0 1px 0 rgba(255, 255, 255, 0.7);
  border: 1px solid rgba(148, 163, 184, 0.18);
  backdrop-filter: blur(8px);
}

.node-label {
  font-size: 11.5px;
  font-weight: 600;
  color: #475569;
  background: rgba(255, 255, 255, 0.78);
  backdrop-filter: blur(6px);
  padding: 3px 10px;
  border-radius: 12px;
  border: 1px solid rgba(148, 163, 184, 0.2);
  box-shadow: 0 2px 8px rgba(15, 23, 42, 0.03);
  white-space: nowrap;
}

/* ═══ Pause when tab hidden ═══ */
.portal-motion-paused .constellation-ambient {
  animation-play-state: paused;
}
.portal-motion-paused .connector-pulse {
  animation-play-state: paused;
}

/* ═══ Reduced motion ═══ */
@media (prefers-reduced-motion: reduce) {
  .constellation-core {
    animation: none !important;
  }
  .core-glow {
    animation: none !important;
    opacity: 0.72;
  }
  .connector-pulse {
    animation: none !important;
    opacity: 0;
  }
}
</style>
