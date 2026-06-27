<script setup>
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import {
  GraduationCap,
  Presentation,
  HeartHandshake,
  ClipboardList,
  Landmark,
  BookOpenCheck,
  ShieldCheck,
  ArrowRight,
} from 'lucide-vue-next'
const props = defineProps({
  portal: { type: Object, required: true },
  variant: { type: String, default: 'standard' }, // featured, standard, compact
})

const iconMap = {
  GraduationCap,
  Presentation,
  HeartHandshake,
  ClipboardList,
  Landmark,
  BookOpenCheck,
  ShieldCheck,
}

const resolvedIcon = computed(() => iconMap[props.portal.icon] || GraduationCap)

const accentColors = {
  blue: { border: 'rgba(37, 99, 235, 0.2)', hoverBorder: 'rgba(37, 99, 235, 0.4)', iconBg: 'rgba(37, 99, 235, 0.08)', iconColor: '#2563eb', spotlight: 'rgba(239, 246, 255, 0.55)' },
  indigo: { border: 'rgba(79, 70, 229, 0.2)', hoverBorder: 'rgba(79, 70, 229, 0.4)', iconBg: 'rgba(79, 70, 229, 0.08)', iconColor: '#4f46e5', spotlight: 'rgba(238, 242, 255, 0.55)' },
  cyan: { border: 'rgba(8, 145, 178, 0.2)', hoverBorder: 'rgba(8, 145, 178, 0.4)', iconBg: 'rgba(8, 145, 178, 0.08)', iconColor: '#0891b2', spotlight: 'rgba(236, 254, 255, 0.55)' },
  teal: { border: 'rgba(15, 118, 110, 0.2)', hoverBorder: 'rgba(15, 118, 110, 0.4)', iconBg: 'rgba(15, 118, 110, 0.08)', iconColor: '#0f766e', spotlight: 'rgba(240, 253, 250, 0.55)' },
  navy: { border: 'rgba(30, 58, 138, 0.2)', hoverBorder: 'rgba(30, 58, 138, 0.4)', iconBg: 'rgba(30, 58, 138, 0.08)', iconColor: '#1e3a8a', spotlight: 'rgba(239, 246, 255, 0.55)' },
  violet: { border: 'rgba(109, 40, 217, 0.2)', hoverBorder: 'rgba(109, 40, 217, 0.4)', iconBg: 'rgba(109, 40, 217, 0.08)', iconColor: '#6d28d9', spotlight: 'rgba(245, 243, 255, 0.55)' },
  slate: { border: 'rgba(71, 85, 105, 0.2)', hoverBorder: 'rgba(71, 85, 105, 0.4)', iconBg: 'rgba(71, 85, 105, 0.08)', iconColor: '#475569', spotlight: 'rgba(248, 250, 252, 0.55)' },
}

const colors = computed(() => accentColors[props.portal.accent] || accentColors.blue)
</script>

<template>
  <RouterLink
    :to="{ name: 'role-login', params: { portal: portal.slug } }"
    class="portal-card"
    :class="[
      `variant-${variant}`,
      `accent-${portal.accent || 'blue'}`
    ]"
    data-portal-spotlight
    :style="{
      '--card-border': colors.border,
      '--card-hover-border': colors.hoverBorder,
      '--card-icon-bg': colors.iconBg,
      '--card-icon-color': colors.iconColor,
      '--card-spotlight': colors.spotlight,
    }"
  >
    <!-- Featured variant specific decorations -->
    <div v-if="variant === 'featured'" class="featured-decoration" aria-hidden="true">
      <svg class="deco-cap" viewBox="0 0 80 60" fill="none">
        <path d="M40 8L8 24L40 40L72 24L40 8Z" stroke="currentColor" stroke-width="1.2" opacity="0.08" />
        <path d="M8 24V36L40 52L72 36V24" stroke="currentColor" stroke-width="1" opacity="0.05" />
      </svg>
    </div>

    <div class="card-inner">
      <div class="card-icon-wrapper">
        <component :is="resolvedIcon" :size="variant === 'compact' ? 18 : (variant === 'featured' ? 24 : 20)" aria-hidden="true" class="card-icon-svg" />
      </div>

      <div v-if="variant === 'featured'" class="card-content-featured">
        <div class="card-header">
          <h3 class="card-title">{{ portal.label }}</h3>
          <div class="card-arrow-wrapper">
            <ArrowRight class="card-arrow" aria-hidden="true" />
          </div>
        </div>
        <p class="card-desc">{{ portal.description || portal.audience }}</p>
        <div class="card-chips" v-if="portal.features && portal.features.length">
          <span v-for="f in portal.features" :key="f" class="feature-chip">{{ f }}</span>
        </div>
      </div>

      <div v-else-if="variant === 'standard'" class="card-content-standard">
        <div class="card-header-inline">
          <h3 class="card-title">{{ portal.label }}</h3>
        </div>
        <p class="card-desc">{{ portal.description || portal.audience }}</p>
      </div>

      <div v-else class="card-content-compact">
        <h3 class="card-title-compact">{{ portal.label }}</h3>
      </div>
      
      <ArrowRight
        v-if="variant === 'standard'"
        class="card-arrow-sm"
        aria-hidden="true"
      />
    </div>
  </RouterLink>
</template>

<style scoped>
.portal-card {
  display: flex;
  flex-direction: column;
  position: relative;
  text-decoration: none;
  border-radius: 20px;
  /* D3 Motion Tokens applied here */
  transition:
    transform var(--card-motion-dur, 200ms) cubic-bezier(0.2, 0.8, 0.2, 1),
    border-color 200ms ease,
    background-color 200ms ease,
    box-shadow 200ms ease;
  overflow: hidden;
  outline: none;
  height: 100%;
}

/* Glass edge highlight for all cards */
.portal-card::after {
  content: "";
  position: absolute;
  inset: 0;
  pointer-events: none;
  border-radius: inherit;
  border: 1px solid rgba(255, 255, 255, 0.58);
  mask-image: linear-gradient(135deg, black 0%, rgba(0, 0, 0, 0.4) 34%, transparent 70%);
  z-index: 2; /* Keep above spotlight */
}

/* Spotlight pseudoelement */
.portal-card::before {
  content: "";
  position: absolute;
  inset: 0;
  z-index: 0;
  pointer-events: none;
  border-radius: inherit;
  opacity: 0;
  background: radial-gradient(
    var(--spotlight-size, 360px) circle at var(--spotlight-x, 50%) var(--spotlight-y, 50%),
    var(--card-spotlight, rgba(255,255,255,0.4)),
    transparent 46%
  );
  transition: opacity 180ms ease-out;
}

/* Focus-visible overrides */
.portal-card:focus-visible {
  outline: 2px solid var(--card-icon-color);
  outline-offset: 3px;
  transform: none !important;
}
.portal-card:focus-visible::before {
  display: none !important;
}

/* Active state (press down) */
.portal-card:active {
  transform: translateY(-1px) scale(0.998) !important;
  transition-duration: 120ms !important;
}

/* Base Styles */
.card-inner {
  display: flex;
  position: relative;
  z-index: 1; /* Above spotlight */
  flex: 1;
}

/* ════════ LEVEL 1 — FEATURED (Student) ════════ */
.variant-featured {
  --card-motion-dur: 220ms;
  --spotlight-size: 400px;
  background:
    radial-gradient(circle at 88% 42%, rgba(37, 99, 235, 0.10), transparent 30%),
    linear-gradient(135deg, rgba(255, 255, 255, 0.94), rgba(248, 251, 255, 0.88));
  backdrop-filter: blur(16px);
  border: 1px solid rgba(148, 163, 184, 0.25);
  box-shadow: 0 20px 50px rgba(15, 23, 42, 0.09), 0 4px 14px rgba(37, 99, 235, 0.07);
  border-radius: 26px;
}
.variant-featured .card-inner {
  padding: 24px 26px;
  flex-direction: row;
  align-items: flex-start;
  gap: 20px;
}
@media (hover: hover) and (pointer: fine) {
  .variant-featured:hover {
    transform: translateY(-3px) scale(1.006);
    border-color: rgba(37, 99, 235, 0.35);
    background:
      radial-gradient(circle at 88% 42%, rgba(37, 99, 235, 0.12), transparent 30%),
      linear-gradient(135deg, rgba(255, 255, 255, 0.96), rgba(246, 250, 255, 0.90));
    box-shadow: 0 24px 54px rgba(15, 23, 42, 0.12), 0 6px 18px rgba(37, 99, 235, 0.1);
  }
  .variant-featured:hover::before {
    opacity: 0.38;
  }
}
.variant-featured .card-icon-wrapper {
  width: 50px;
  height: 50px;
  border-radius: 14px;
  background: linear-gradient(135deg, rgba(255,255,255,0.8), rgba(239,246,255,0.8));
  color: var(--card-icon-color);
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.08), inset 0 1px 0 rgba(255, 255, 255, 0.8);
  border: 1px solid rgba(148, 163, 184, 0.15);
}

/* ════════ LEVEL 2 — STANDARD (Teacher/Parent) ════════ */
.variant-standard {
  --card-motion-dur: 200ms;
  --spotlight-size: 340px;
  background: rgba(255, 255, 255, 0.84);
  backdrop-filter: blur(14px);
  border: 1px solid rgba(148, 163, 184, 0.22);
  box-shadow: 0 12px 32px rgba(15, 23, 42, 0.07);
  border-radius: 22px;
}
.variant-standard.accent-indigo {
  background:
    radial-gradient(circle at 15% 0%, rgba(79, 70, 229, 0.07), transparent 32%),
    rgba(255, 255, 255, 0.84);
}
.variant-standard.accent-cyan, .variant-standard.accent-teal {
  background:
    radial-gradient(circle at 15% 0%, rgba(8, 145, 178, 0.07), transparent 32%),
    rgba(255, 255, 255, 0.84);
}
.variant-standard .card-inner {
  padding: 18px 20px;
  flex-direction: column;
  align-items: flex-start;
}
@media (hover: hover) and (pointer: fine) {
  .variant-standard:hover {
    transform: translateY(-2px) scale(1.004);
    border-color: var(--card-hover-border);
  }
  .variant-standard:hover::before {
    opacity: 0.35;
  }
}
.variant-standard .card-icon-wrapper {
  width: 42px;
  height: 42px;
  border-radius: 12px;
  margin-bottom: 12px;
  background: var(--card-icon-bg);
  color: var(--card-icon-color);
}

/* ════════ COMPACT ════════ */
.variant-compact {
  background: rgba(255, 255, 255, 0.74);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(148, 163, 184, 0.15);
  border-radius: 12px;
}
.variant-compact .card-inner {
  padding: 10px 14px;
  align-items: center;
  gap: 12px;
}
.variant-compact:hover {
  background: rgba(255, 255, 255, 0.9);
}
.variant-compact .card-icon-wrapper {
  width: 32px;
  height: 32px;
  border-radius: 8px;
}

/* ════════ ICONS & WRAPPERS ════════ */
.card-icon-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.variant-featured .card-icon-svg {
  transition: transform 200ms cubic-bezier(0.2, 0.8, 0.2, 1);
}
.variant-standard .card-icon-svg {
  transition: transform 180ms cubic-bezier(0.2, 0.8, 0.2, 1);
}
@media (hover: hover) and (pointer: fine) {
  .variant-featured:hover .card-icon-svg {
    transform: scale(1.035);
  }
  .variant-standard:hover .card-icon-svg {
    transform: scale(1.03);
  }
}

/* ════════ CONTENT ════════ */
.card-content-featured {
  flex: 1;
}
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 8px;
}
.card-title {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
}
.card-arrow-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 38px;
  height: 38px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.6);
  border: 1px solid rgba(148, 163, 184, 0.15);
  transition: background 0.2s ease, border-color 0.2s ease;
}
.portal-card:focus-visible .card-arrow-wrapper {
  background: rgba(255, 255, 255, 0.9);
  border-color: rgba(37, 99, 235, 0.3);
}
@media (hover: hover) and (pointer: fine) {
  .variant-featured:hover .card-arrow-wrapper {
    background: rgba(255, 255, 255, 0.9);
    border-color: rgba(37, 99, 235, 0.3);
  }
  .variant-featured:hover .card-arrow {
    transform: translateX(4px);
  }
}
.card-arrow {
  width: 18px;
  height: 18px;
  color: var(--card-icon-color);
  transition: transform 180ms cubic-bezier(0.2, 0.8, 0.2, 1);
}
.card-desc {
  font-size: 13.5px;
  line-height: 1.5;
  color: #475569;
  margin-bottom: 16px;
}
.card-chips {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}
.feature-chip {
  font-size: 11px;
  padding: 4px 10px;
  background: rgba(239, 246, 255, 0.6);
  border: 1px solid rgba(148, 163, 184, 0.12);
  border-radius: 8px;
  color: #334155;
  font-weight: 500;
  height: 24px;
  display: inline-flex;
  align-items: center;
}

/* Standard Content */
.card-content-standard {
  flex: 1;
}
.card-header-inline {
  margin-bottom: 6px;
}
.card-arrow-sm {
  position: absolute;
  top: 18px;
  right: 20px;
  width: 20px;
  height: 20px;
  color: #94a3b8;
  transition: color 0.2s ease, transform 180ms cubic-bezier(0.2, 0.8, 0.2, 1);
}
.portal-card:focus-visible .card-arrow-sm {
  color: var(--card-icon-color);
}
@media (hover: hover) and (pointer: fine) {
  .variant-standard:hover .card-arrow-sm {
    color: var(--card-icon-color);
    transform: translateX(3px);
  }
}

/* Compact Content */
.card-content-compact {
  flex: 1;
}
.card-title-compact {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
}

/* Featured Decoration (Line-art) */
.featured-decoration {
  position: absolute;
  top: 0;
  right: 0;
  bottom: 0;
  left: 0;
  overflow: hidden;
  border-radius: inherit;
  color: var(--card-icon-color);
  pointer-events: none;
}
.deco-cap {
  position: absolute;
  top: -10px;
  right: -10px;
  width: 120px;
  height: 120px;
  opacity: 0.08;
  transition: opacity 220ms ease;
}
@media (hover: hover) and (pointer: fine) {
  .variant-featured:hover .deco-cap {
    opacity: 0.11;
  }
}

/* ════════ REDUCED MOTION ════════ */
@media (prefers-reduced-motion: reduce) {
  .portal-card,
  .portal-card:hover,
  .portal-card:active,
  .portal-card:focus-visible {
    transform: none !important;
  }
  .portal-card::before {
    display: none !important;
  }
  .card-icon-svg,
  .card-arrow,
  .card-arrow-sm {
    transform: none !important;
  }
  .deco-cap {
    opacity: 0.08 !important;
  }
}

/* ════════ MOBILE / TOUCH ════════ */
@media (hover: none), (pointer: coarse) {
  .portal-card:hover {
    transform: none !important;
  }
  .portal-card::before {
    display: none !important;
  }
  .card-icon-svg,
  .card-arrow,
  .card-arrow-sm {
    transform: none !important;
  }
  
  /* Tap feedback only */
  .portal-card:active {
    transform: none !important;
    border-color: var(--card-hover-border);
  }
}
</style>
