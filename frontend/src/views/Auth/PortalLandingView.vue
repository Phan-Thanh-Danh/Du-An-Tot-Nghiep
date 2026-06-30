<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { RouterLink } from 'vue-router'
import {
  Shield,
  ChevronDown,
  Briefcase,
  Award,
  Settings,
  ShieldCheck,
  LockKeyhole,
  RefreshCw,
  ArrowRight,
  MessageCircleQuestion
} from 'lucide-vue-next'
import { AUTH_PORTALS } from '@/data/authPortals'
import LearningOrbit from '@/components/auth/LearningOrbit.vue'
import RolePortalCard from '@/components/auth/RolePortalCard.vue'
import EduShaderBackground from '@/components/auth/EduShaderBackground.vue'
import { usePortalSpotlight } from '@/composables/usePortalSpotlight'

const year = new Date().getFullYear()
const staffOpen = ref(false)
const gatewayRef = ref(null)

const { handlePointerMove } = usePortalSpotlight(gatewayRef)

/* ── Entrance motion state (progressive enhancement) ── */
const motionEnabled = ref(false)
const motionReady = ref(false)
const motionPaused = ref(false)
const reducedMotion = ref(false)

let rafId1 = 0
let rafId2 = 0

function handleVisibilityChange() {
  motionPaused.value = document.hidden
}

onMounted(() => {
  const reduceMotionMq = window.matchMedia(
    '(prefers-reduced-motion: reduce)',
  )
  reducedMotion.value = reduceMotionMq.matches

  if (reducedMotion.value) {
    motionReady.value = true
    return
  }

  motionEnabled.value = true

  rafId1 = requestAnimationFrame(() => {
    rafId2 = requestAnimationFrame(() => {
      motionReady.value = true
    })
  })

  document.addEventListener('visibilitychange', handleVisibilityChange)
})

onBeforeUnmount(() => {
  cancelAnimationFrame(rafId1)
  cancelAnimationFrame(rafId2)
  document.removeEventListener('visibilitychange', handleVisibilityChange)
})

const studentPortal = computed(() => AUTH_PORTALS['student'])
const teacherPortal = computed(() => AUTH_PORTALS['teacher'])
const parentPortal = computed(() => AUTH_PORTALS['parent'])

const staffPortals = computed(() =>
  Object.values(AUTH_PORTALS).filter((p) => p.group === 'staff' && p.enabled),
)

function getStaffIcon(slug) {
  if (slug === 'staff') return Briefcase
  if (slug === 'bgh') return Award
  return Settings
}
</script>

<template>
  <div
    class="portal-shell"
    :class="{
      'portal-motion-enabled': motionEnabled,
      'portal-motion-ready': motionReady,
      'portal-motion-paused': motionPaused,
    }"
  >
    <!-- Shader background layer -->
    <EduShaderBackground
      v-if="!reducedMotion"
    />

    <div class="portal-container shell-wrapper">
      <!-- ═══ HEADER ═══ -->
      <header
        class="portal-header portal-reveal"
        :style="{
          '--reveal-delay': '0ms',
          '--reveal-duration': '320ms',
          '--reveal-y': '-8px',
        }"
      >
        <div class="header-inner">
          <div class="header-brand">
            <div class="logo-orb">E</div>
            <div>
              <h1 class="header-title">EduLMS</h1>
              <p class="header-sub">Cơ sở Đồng Nai</p>
            </div>
          </div>
          <div class="header-help">
            <MessageCircleQuestion class="help-icon" />
            <span>Hỗ trợ</span>
          </div>
        </div>
      </header>

      <main class="portal-main">
        <div class="portal-two-col">
          <!-- ═══ Left: Hero ═══ -->
          <section class="hero-left">
            <!-- Eyebrow -->
            <div
              class="portal-reveal"
              :style="{
                '--reveal-delay': '80ms',
                '--reveal-duration': '420ms',
                '--reveal-y': '10px',
              }"
            >
              <div class="eyebrow">
                <span class="eyebrow-dot" />
                <span class="eyebrow-text">Cổng học tập nội bộ</span>
              </div>
            </div>

            <!-- Headline: block reveal per line -->
            <h2 class="headline">
              <span class="portal-title-line">
                <span
                  class="portal-title-line-inner portal-reveal"
                  :style="{
                    '--reveal-delay': '140ms',
                    '--reveal-duration': '560ms',
                    '--reveal-y': '14px',
                  }"
                >Một nền tảng,</span>
              </span>
              <span class="portal-title-line">
                <span
                  class="portal-title-line-inner text-gradient portal-reveal"
                  :style="{
                    '--reveal-delay': '210ms',
                    '--reveal-duration': '560ms',
                    '--reveal-y': '14px',
                  }"
                >Nhiều hành trình.</span>
              </span>
            </h2>

            <!-- Subtitle -->
            <p
              class="subtitle portal-reveal"
              :style="{
                '--reveal-delay': '220ms',
                '--reveal-duration': '480ms',
                '--reveal-y': '10px',
              }"
            >
              Đăng nhập vào không gian được thiết kế riêng cho vai trò của bạn, từ học tập và giảng dạy đến đồng hành và vận hành đào tạo.
            </p>

            <!-- Trust chips (stagger via index) -->
            <div class="trust-chips">
              <div
                class="chip portal-reveal"
                :style="{
                  '--reveal-delay': '300ms',
                  '--reveal-duration': '420ms',
                  '--reveal-y': '8px',
                }"
              >
                <ShieldCheck class="chip-icon icon-role" />
                <span>Đúng vai trò</span>
              </div>
              <div
                class="chip portal-reveal"
                :style="{
                  '--reveal-delay': '350ms',
                  '--reveal-duration': '420ms',
                  '--reveal-y': '8px',
                }"
              >
                <LockKeyhole class="chip-icon icon-secure" />
                <span>Bảo mật tài khoản</span>
              </div>
              <div
                class="chip portal-reveal"
                :style="{
                  '--reveal-delay': '400ms',
                  '--reveal-duration': '420ms',
                  '--reveal-y': '8px',
                }"
              >
                <RefreshCw class="chip-icon icon-sync" />
                <span>Đồng bộ dữ liệu</span>
              </div>
            </div>

            <!-- Constellation -->
            <div
              class="orbit-wrapper portal-reveal"
              :style="{
                '--reveal-delay': '360ms',
                '--reveal-duration': '520ms',
                '--reveal-y': '0px',
                '--reveal-scale': '0.97',
              }"
            >
              <LearningOrbit :size="230" />
            </div>
          </section>

          <!-- ═══ Right: Portal Gateway ═══ -->
          <section
            class="hero-right"
            ref="gatewayRef"
            @pointermove="handlePointerMove"
          >
            <!-- Student card -->
            <div
              class="portal-card-reveal portal-reveal"
              :style="{
                '--reveal-delay': '220ms',
                '--reveal-duration': '520ms',
                '--reveal-x': '14px',
              }"
            >
              <RolePortalCard
                :portal="studentPortal"
                variant="featured"
              />
            </div>

            <!-- Teacher + Parent -->
            <div class="card-pair">
              <div
                class="portal-card-reveal portal-reveal"
                :style="{
                  '--reveal-delay': '300ms',
                  '--reveal-duration': '480ms',
                  '--reveal-y': '12px',
                }"
              >
                <RolePortalCard
                  :portal="teacherPortal"
                  variant="standard"
                />
              </div>
              <div
                class="portal-card-reveal portal-reveal"
                :style="{
                  '--reveal-delay': '360ms',
                  '--reveal-duration': '480ms',
                  '--reveal-y': '12px',
                }"
              >
                <RolePortalCard
                  :portal="parentPortal"
                  variant="standard"
                />
              </div>
            </div>

            <!-- Staff accordion -->
            <div
              class="card-staff-container portal-reveal"
              :style="{
                '--reveal-delay': '430ms',
                '--reveal-duration': '460ms',
                '--reveal-y': '10px',
              }"
            >
              <div class="card-staff" :class="{ 'is-active': staffOpen }">
                <button
                  type="button"
                  :aria-expanded="staffOpen"
                  aria-controls="staff-portal-panel"
                  class="staff-button"
                  @click="staffOpen = !staffOpen"
                >
                  <div class="staff-button-inner">
                    <div class="card-icon--staff">
                      <Shield />
                    </div>
                    <div class="staff-button-text">
                      <div class="staff-head">
                        <h3 class="card-title">Cán bộ nhà trường</h3>
                        <ChevronDown
                          class="staff-chevron"
                          :class="{ 'is-open': staffOpen }"
                        />
                      </div>
                      <p class="staff-sub">Giáo vụ, Ban giám hiệu và quản trị hệ thống</p>
                    </div>
                  </div>
                </button>

                <div
                  id="staff-portal-panel"
                  class="staff-panel"
                  :class="{ 'is-open': staffOpen }"
                  :aria-hidden="!staffOpen"
                  :inert="!staffOpen || undefined"
                >
                  <div class="staff-panel-inner">
                    <div class="staff-panel-content">
                      <RouterLink
                        v-for="(portal, idx) in staffPortals"
                        :key="portal.slug"
                        :to="{ name: 'role-login', params: { portal: portal.slug } }"
                        class="staff-link"
                        :class="`staff-link-${portal.slug}`"
                        :style="{
                          '--staff-link-delay': `${40 + idx * 40}ms`,
                        }"
                      >
                        <div class="staff-link-left">
                          <div class="staff-link-icon-wrap">
                            <component
                              :is="getStaffIcon(portal.slug)"
                              class="staff-link-icon"
                            />
                          </div>
                          <span class="staff-link-label">{{ portal.label }}</span>
                        </div>
                        <ArrowRight class="staff-link-arrow" />
                      </RouterLink>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </section>
        </div>
      </main>

      <!-- ═══ FOOTER ═══ -->
      <footer
        class="portal-footer portal-reveal"
        :style="{
          '--reveal-delay': '520ms',
          '--reveal-duration': '360ms',
          '--reveal-y': '0px',
        }"
      >
        <div class="footer-inner">
          <div class="footer-left">
            <Shield class="footer-icon" />
            <span class="footer-text">Học tập an toàn và liền mạch</span>
          </div>
          <p class="footer-text footer-right">&copy; {{ year }} EduLMS &middot; Cơ sở Đồng Nai</p>
        </div>
      </footer>
    </div>
  </div>
</template>

<style scoped>
/* ═══════════════ LOCAL TOKENS ═══════════════ */
.portal-shell {
  --portal-text-heading: #0f172a;
  --portal-text-body: #475569;
  --portal-text-muted: #64748b;

  --portal-blue: #2563eb;
  --portal-blue-dark: #1d4ed8;
  --portal-cyan: #0891b2;
  --portal-indigo: #4f46e5;
  --portal-teal: #0f9f98;

  --portal-glass-strong: rgba(255, 255, 255, 0.90);
  --portal-glass-medium: rgba(255, 255, 255, 0.82);
  --portal-glass-soft: rgba(255, 255, 255, 0.68);

  --portal-border: rgba(148, 163, 184, 0.24);
  --portal-border-light: rgba(255, 255, 255, 0.72);

  --portal-shadow-featured:
    0 20px 50px rgba(15, 23, 42, 0.09),
    0 4px 14px rgba(37, 99, 235, 0.07);

  --portal-shadow-card:
    0 12px 32px rgba(15, 23, 42, 0.07);

  --portal-shadow-soft:
    0 6px 18px rgba(15, 23, 42, 0.055);

  /* ── Motion tokens ── */
  --motion-instant: 120ms;
  --motion-fast: 180ms;
  --motion-base: 240ms;
  --motion-medium: 320ms;
  --motion-slow: 480ms;
  --motion-entrance: 560ms;
  --motion-entrance-easing: cubic-bezier(0.16, 1, 0.3, 1);
  --motion-interaction-easing: cubic-bezier(0.2, 0.8, 0.2, 1);

  /* Page base styles */
  position: relative;
  min-height: 100svh;
  overflow-x: hidden;
  background:
    radial-gradient(circle at 13% 23%, rgba(37, 99, 235, 0.12), transparent 34%),
    radial-gradient(circle at 84% 62%, rgba(8, 145, 178, 0.10), transparent 38%),
    linear-gradient(135deg, #f8fbff 0%, #f4f7fb 52%, #eef6ff 100%);
  color: var(--portal-text-body);
}

/* Base static grid noise */
.portal-shell::before {
  content: "";
  position: fixed;
  inset: 0;
  pointer-events: none;
  opacity: 0.06;
  background-image:
    linear-gradient(rgba(148, 163, 184, 0.22) 1px, transparent 1px),
    linear-gradient(90deg, rgba(148, 163, 184, 0.18) 1px, transparent 1px);
  background-size: 56px 56px;
  mask-image: radial-gradient(circle at 50% 35%, black, transparent 78%);
  z-index: 1;
}

.portal-container {
  width: 100%;
  padding-inline: 48px;
  margin-inline: auto;
}

.shell-wrapper {
  position: relative;
  z-index: 10;
  min-height: 100svh;
  display: flex;
  flex-direction: column;
}

/* ═══════════════ ENTRANCE ANIMATION SYSTEM ═══════════════ */
/* Default: fully visible (no JS needed) */
.portal-reveal {
  opacity: 1;
  transform: none;
}

/* Phase 1: JS enabled motion, waiting for ready — hide elements */
.portal-motion-enabled:not(.portal-motion-ready) .portal-reveal {
  opacity: 0;
  transform:
    translate3d(
      var(--reveal-x, 0),
      var(--reveal-y, 12px),
      0
    )
    scale(var(--reveal-scale, 1));
  will-change: transform, opacity;
}

/* Phase 2: Ready — animate into view */
.portal-motion-enabled.portal-motion-ready .portal-reveal {
  opacity: 1;
  transform: translate3d(0, 0, 0) scale(1);
  transition:
    opacity var(--reveal-duration, 480ms) var(--motion-entrance-easing),
    transform var(--reveal-duration, 480ms) var(--motion-entrance-easing);
  transition-delay: var(--reveal-delay, 0ms);
}

/* Reduced motion override: instant visibility, no transform */
@media (prefers-reduced-motion: reduce) {
  .portal-reveal,
  .portal-motion-enabled:not(.portal-motion-ready) .portal-reveal,
  .portal-motion-enabled.portal-motion-ready .portal-reveal {
    opacity: 1 !important;
    transform: none !important;
    transition: none !important;
    will-change: auto !important;
  }
}

/* ═══════════════ HEADLINE BLOCK REVEAL ═══════════════ */
.portal-title-line {
  display: block;
  overflow: hidden;
  padding-bottom: 0.04em;
}

.portal-title-line-inner {
  display: block;
}

/* ═══════════════ PORTAL CARD REVEAL WRAPPER ═══════════════ */
.portal-card-reveal {
  min-width: 0;
  height: 100%;
  overflow: visible;
}

/* ═══════════════ HEADER ═══════════════ */
.portal-header {
  height: 80px;
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

.header-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
}

.header-brand {
  display: flex;
  align-items: center;
  gap: 12px;
}

.logo-orb {
  width: 40px;
  height: 40px;
  border-radius: 12px;
  background: linear-gradient(135deg, var(--portal-blue), var(--portal-blue-dark));
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: 700;
  font-size: 16px;
  box-shadow: 0 4px 12px rgba(37, 99, 235, 0.2);
  user-select: none;
}

.header-title {
  font-weight: 700;
  font-size: 17px;
  color: var(--portal-text-heading);
  line-height: 1.2;
}

.header-sub {
  font-size: 13px;
  color: var(--portal-text-muted);
  font-weight: 500;
}

.header-help {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  color: var(--portal-text-muted);
  font-weight: 500;
  cursor: default;
  user-select: none;
}

.help-icon {
  width: 16px;
  height: 16px;
}

/* ═══════════════ MAIN ═══════════════ */
.portal-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  justify-content: center;
  padding: 32px 0 40px;
}

.portal-two-col {
  display: flex;
  justify-content: space-between;
  gap: 80px;
  align-items: center;
  width: 100%;
}

.hero-left {
  display: flex;
  flex-direction: column;
  justify-content: center;
  flex: 1;
  max-width: 500px;
}

.eyebrow {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  height: 30px;
  padding: 0 12px;
  border-radius: 999px;
  background: rgba(37, 99, 235, 0.06);
  border: 1px solid rgba(37, 99, 235, 0.12);
  width: fit-content;
}

.eyebrow-dot {
  width: 6px;
  height: 6px;
  border-radius: 50%;
  background: var(--portal-blue);
}

.eyebrow-text {
  font-size: 11px;
  font-weight: 600;
  color: var(--portal-blue-dark);
  letter-spacing: 0.02em;
  text-transform: uppercase;
}

.headline {
  font-size: 58px;
  font-weight: 780;
  line-height: 1.04;
  letter-spacing: -0.04em;
  color: var(--portal-text-heading);
  margin-top: 24px;
  margin-bottom: 20px;
}

.text-gradient {
  background: linear-gradient(100deg, var(--portal-blue-dark) 0%, var(--portal-blue) 48%, var(--portal-cyan) 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
  background-clip: text;
}

.subtitle {
  font-size: 16px;
  color: var(--portal-text-body);
  line-height: 1.6;
  max-width: 500px;
  margin-bottom: 24px;
}

.trust-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  margin-bottom: 20px;
}

.chip {
  display: flex;
  align-items: center;
  gap: 8px;
  height: 36px;
  padding: 0 14px;
  background: var(--portal-glass-soft);
  border: 1px solid var(--portal-border);
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  color: var(--portal-text-body);
  cursor: default;
  transition: border-color 0.2s ease, background 0.2s ease;
}

.chip:hover {
  background: var(--portal-glass-medium);
  border-color: rgba(148, 163, 184, 0.35);
}

.chip-icon {
  width: 15px;
  height: 15px;
}
.chip-icon.icon-role { color: var(--portal-teal); }
.chip-icon.icon-secure { color: var(--portal-blue); }
.chip-icon.icon-sync { color: var(--portal-indigo); }

.orbit-wrapper {
  margin-top: 20px;
  width: 100%;
  display: flex;
  justify-content: flex-start;
  padding-left: 20px;
}

/* ── Right: Portals ── */
.hero-right {
  display: flex;
  flex-direction: column;
  gap: 16px;
  flex: 1;
  max-width: 600px;
  width: 100%;
}

.card-pair {
  display: flex;
  flex-direction: row;
  gap: 16px;
}

.card-pair > * {
  flex: 1;
  min-width: 0;
}

/* ═══════════════ D4.3 STAFF ACCORDION ═══════════════ */
.card-staff {
  background: rgba(255, 255, 255, 0.82);
  backdrop-filter: blur(14px);
  border: 1px solid var(--portal-border);
  box-shadow: var(--portal-shadow-soft);
  border-radius: 22px;
  overflow: hidden;
  transition: border-color 0.2s ease, background-color 0.2s ease;
}

.card-staff:hover {
  border-color: rgba(148, 163, 184, 0.4);
}

.card-staff.is-active {
  background: rgba(255, 255, 255, 0.94);
  border-color: rgba(148, 163, 184, 0.35);
  box-shadow: var(--portal-shadow-card);
}

.staff-button {
  width: 100%;
  display: flex;
  padding: 16px 24px;
  cursor: pointer;
  background: transparent;
  border: none;
  text-align: left;
  outline: none;
}

.staff-button:focus-visible {
  outline: 2px solid var(--portal-blue);
  outline-offset: -2px;
  border-radius: 22px;
}

.staff-button-inner {
  display: flex;
  align-items: center;
  gap: 16px;
  width: 100%;
}

.card-icon--staff {
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(71, 85, 105, 0.06);
  color: var(--portal-text-muted);
  width: 44px;
  height: 44px;
  border-radius: 12px;
  flex-shrink: 0;
}

.card-icon--staff svg {
  width: 22px;
  height: 22px;
}

.staff-button-text {
  flex: 1;
  min-width: 0;
}

.staff-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.card-title {
  font-size: 17px;
  font-weight: 700;
  color: var(--portal-text-heading);
}

.staff-chevron {
  width: 20px;
  height: 20px;
  color: var(--portal-text-muted);
  flex-shrink: 0;
  transition: transform 240ms var(--motion-interaction-easing), color 0.2s ease;
}

.staff-button:hover .staff-chevron {
  color: var(--portal-text-heading);
}

.staff-chevron.is-open {
  transform: rotate(180deg);
}

.staff-sub {
  font-size: 13px;
  color: var(--portal-text-body);
  margin-top: 4px;
}

/* D4.3: Enhanced staff panel with grid + opacity + visibility */
.staff-panel {
  display: grid;
  grid-template-rows: 0fr;
  opacity: 0;
  visibility: hidden;
  transition:
    grid-template-rows 280ms var(--motion-interaction-easing),
    opacity 220ms ease-out,
    visibility 0s linear 280ms;
}

.staff-panel.is-open {
  grid-template-rows: 1fr;
  opacity: 1;
  visibility: visible;
  transition:
    grid-template-rows 280ms var(--motion-interaction-easing),
    opacity 220ms ease-out,
    visibility 0s linear 0s;
}

.staff-panel-inner {
  min-height: 0;
  overflow: hidden;
}

.staff-panel-content {
  border-top: 1px solid rgba(148, 163, 184, 0.15);
  padding: 12px 24px 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

/* D4.3: Mini portal links with stagger */
.staff-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 14px;
  border-radius: 14px;
  background: rgba(255, 255, 255, 0.68);
  backdrop-filter: blur(12px);
  border: 1px solid var(--portal-border);
  box-shadow: 0 4px 12px rgba(15, 23, 42, 0.03);
  transition: border-color 0.2s ease, background 0.2s ease, opacity 260ms ease-out, transform 260ms var(--motion-interaction-easing);
  text-decoration: none;
  outline: none;
  /* Stagger entrance within panel */
  opacity: 0;
  transform: translateY(4px);
  transition-delay: 0ms;
}

.staff-panel.is-open .staff-link {
  opacity: 1;
  transform: translateY(0);
  transition-delay: var(--staff-link-delay, 0ms);
}

.staff-link:hover {
  background: rgba(255, 255, 255, 0.90);
  border-color: rgba(148, 163, 184, 0.35);
}

.staff-link:focus-visible {
  outline: 2px solid var(--portal-blue);
  outline-offset: 2px;
}

.staff-link-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.staff-link-icon-wrap {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: rgba(71, 85, 105, 0.06);
  color: var(--portal-text-muted);
}

/* Accent for BGH */
.staff-link-bgh .staff-link-icon-wrap {
  background: rgba(30, 58, 138, 0.06);
  color: #1e3a8a;
}
.staff-link-bgh:hover .staff-link-icon-wrap {
  background: rgba(30, 58, 138, 0.1);
}

/* Accent for SuperAdmin */
.staff-link-super-admin .staff-link-icon-wrap {
  background: rgba(51, 65, 85, 0.08);
  color: #334155;
}
.staff-link-super-admin:hover .staff-link-icon-wrap {
  background: rgba(51, 65, 85, 0.12);
}

.staff-link-icon {
  width: 16px;
  height: 16px;
}

.staff-link-label {
  font-size: 13px;
  font-weight: 600;
  color: var(--portal-text-heading);
}

.staff-link-arrow {
  width: 16px;
  height: 16px;
  color: var(--portal-text-muted);
  transition: color 0.2s ease;
}

.staff-link:hover .staff-link-arrow {
  color: var(--portal-text-heading);
}

/* ═══════════════ FOOTER ═══════════════ */
.portal-footer {
  padding: 20px 0;
  flex-shrink: 0;
}

.footer-inner {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.footer-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.footer-icon {
  width: 14px;
  height: 14px;
  color: var(--portal-text-muted);
}

.footer-text {
  font-size: 13px;
  color: var(--portal-text-muted);
}

/* ═══════════════ RESPONSIVE ═══════════════ */
@media (min-width: 1024px) and (max-height: 820px) {
  .portal-header { height: 64px; }
  .portal-main { padding: 16px 0 24px; }
  .headline {
    font-size: 50px;
    margin-top: 16px;
    margin-bottom: 16px;
  }
  .subtitle { margin-bottom: 20px; }
  .trust-chips { margin-bottom: 16px; }
  .hero-right { gap: 14px; }
  .card-pair { gap: 14px; }
  .staff-button { padding: 14px 20px; }
  .staff-panel-content { padding: 10px 20px 14px; }
}

@media (max-width: 1023px) {
  .portal-two-col {
    flex-direction: column;
    gap: 40px;
  }
  .portal-main {
    padding-top: 20px;
    padding-bottom: 40px;
  }
  .hero-left { padding-right: 0; }
  .hero-right { max-width: 100%; }
  .footer-inner {
    flex-direction: column;
    gap: 8px;
    text-align: center;
  }
  .card-staff {
    backdrop-filter: blur(10px);
  }
}

@media (max-width: 640px) {
  .headline { font-size: 40px; }
  .card-pair { flex-direction: column; }
}

/* ═══════════════ REDUCED MOTION — ACCORDION ═══════════════ */
@media (prefers-reduced-motion: reduce) {
  .staff-chevron {
    transition: none !important;
  }
  .staff-panel,
  .staff-panel.is-open {
    transition: none !important;
  }
  .staff-link {
    opacity: 1 !important;
    transform: none !important;
    transition: border-color 0.2s ease, background 0.2s ease !important;
  }
}
</style>
