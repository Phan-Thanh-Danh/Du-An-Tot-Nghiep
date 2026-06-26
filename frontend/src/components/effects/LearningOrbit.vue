<script setup>
import { Layers, GraduationCap, BookOpen, Calendar, Users } from 'lucide-vue-next'

defineProps({
  size: { type: Number, default: 270 },
})
</script>

<template>
  <div
    class="orbit-root"
    :style="{ '--sz': size + 'px' }"
    aria-hidden="true"
  >
    <!-- Center icon -->
    <div class="orbit-center">
      <Layers :size="Math.round(size * 0.15)" />
    </div>

    <!-- Orbit ring with dashed border -->
    <div class="orbit-ring">
      <div
        v-for="(item, i) in [
          { Icon: GraduationCap, pos: 'top' },
          { Icon: BookOpen, pos: 'bottom' },
          { Icon: Calendar, pos: 'left' },
          { Icon: Users, pos: 'right' },
        ]"
        :key="i"
        :class="'orbit-icon icon-' + item.pos"
      >
        <component :is="item.Icon" :size="Math.round(size * 0.08)" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.orbit-root {
  position: relative;
  width: var(--sz);
  height: var(--sz);
  flex-shrink: 0;
  display: flex;
  align-items: center;
  justify-content: center;
}

.orbit-center {
  position: absolute;
  width: calc(var(--sz) * 0.36);
  height: calc(var(--sz) * 0.36);
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.85);
  backdrop-filter: blur(12px);
  box-shadow: 0 12px 36px rgba(15, 23, 42, 0.06);
  display: flex;
  align-items: center;
  justify-content: center;
  color: #2563eb;
  z-index: 2;
  border: 1px solid rgba(255, 255, 255, 0.9);
}

.orbit-ring {
  position: absolute;
  inset: 0;
  border-radius: 50%;
  border: 1px dashed rgba(148, 163, 184, 0.3);
  animation: spin 30s linear infinite;
}

.orbit-icon {
  position: absolute;
  width: calc(var(--sz) * 0.18);
  height: calc(var(--sz) * 0.18);
  border-radius: 50%;
  background: white;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 14px rgba(15, 23, 42, 0.08);
  border: 1px solid rgba(226, 232, 240, 0.8);
  animation: counter-spin 30s linear infinite;
}

.icon-top {
  top: calc(var(--sz) * -0.09);
  left: calc(50% - var(--sz) * 0.09);
  color: #2563eb;
}

.icon-bottom {
  bottom: calc(var(--sz) * -0.09);
  left: calc(50% - var(--sz) * 0.09);
  color: #0891b2;
}

.icon-left {
  top: calc(50% - var(--sz) * 0.09);
  left: calc(var(--sz) * -0.09);
  color: #7c3aed;
}

.icon-right {
  top: calc(50% - var(--sz) * 0.09);
  right: calc(var(--sz) * -0.09);
  color: #16a34a;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

@keyframes counter-spin {
  to { transform: rotate(-360deg); }
}

@media (prefers-reduced-motion: reduce) {
  .orbit-ring,
  .orbit-icon {
    animation: none !important;
  }
}
</style>
