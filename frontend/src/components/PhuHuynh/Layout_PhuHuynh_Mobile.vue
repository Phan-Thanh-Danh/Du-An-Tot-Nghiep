<script setup>
/**
 * Layout_PhuHuynh_Mobile.vue
 * ─────────────────────────────────────────────
 * Mobile-first app shell dành riêng cho Phụ huynh.
 * Không có sidebar – dùng bottom navigation như native app.
 * ─────────────────────────────────────────────
 */
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import {
  LayoutDashboard,
  BookOpen,
  CreditCard,
  Bell,
  User as UserIcon,
  LogOut,
  ChevronLeft,
  Search,
} from 'lucide-vue-next'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

// ── Bottom Nav tabs ────────────────────────────
const tabs = [
  { path: '/parent/dashboard',            label: 'Trang chủ',  icon: LayoutDashboard },
  { path: '/parent/learning/grades',      label: 'Học tập',    icon: BookOpen },
  { path: '/parent/finance/tuition',      label: 'Tài chính',  icon: CreditCard },
  { path: '/parent/notifications/history',label: 'Thông báo',  icon: Bell },
  { path: '/parent/profile/info',         label: 'Cá nhân',    icon: UserIcon },
]

function isTabActive(tabPath) {
  if (tabPath === '/parent/dashboard') return route.path === tabPath
  const prefix = tabPath.split('/').slice(0, 3).join('/')
  return route.path.startsWith(prefix)
}

// ── Page title map ────────────────────────────
const pageTitleMap = {
  '/parent/dashboard':              'Dashboard',
  '/parent/children/list':          'Danh sách con',
  '/parent/children/overview':      'Tổng quan học tập',
  '/parent/learning/grades':        'Bảng điểm',
  '/parent/learning/attendance':    'Điểm danh',
  '/parent/learning/schedule':      'Thời khóa biểu',
  '/parent/learning/alerts':        'Cảnh báo học tập',
  '/parent/finance/tuition':        'Học phí',
  '/parent/finance/payment':        'Thanh toán',
  '/parent/finance/transactions':   'Lịch sử giao dịch',
  '/parent/finance/invoices':       'Hóa đơn',
  '/parent/notifications/system':   'Cảnh báo hệ thống',
  '/parent/notifications/history':  'Thông báo',
  '/parent/profile/info':           'Hồ sơ cá nhân',
  '/parent/profile/access-rights':  'Quyền truy cập',
}

const pageTitle = computed(() => {
  if (pageTitleMap[route.path]) return pageTitleMap[route.path]
  for (const [key, val] of Object.entries(pageTitleMap)) {
    if (route.path.startsWith(key + '/')) return val
  }
  return 'Phụ huynh'
})

const isDashboard = computed(() => route.path === '/parent/dashboard')

function goBack() {
  router.back()
}

function logout() {
  authStore.logout()
  router.replace('/login')
}

// Notification count mock
const unreadCount = ref(2)
</script>

<template>
  <div
    class="mobile-shell font-sans"
    :style="{
      '--mob-accent': '#ea580c',
      '--mob-accent-light': '#fff7ed',
      '--mob-accent-dark': '#c2410c',
    }"
  >
    <!-- ═══ TOPBAR MOBILE ═══ -->
    <header class="mob-topbar">
      <!-- Left: Back hoặc Avatar -->
      <div class="mob-topbar-left">
        <button v-if="!isDashboard" class="mob-back-btn" @click="goBack">
          <ChevronLeft :size="22" />
        </button>
        <div v-else class="mob-avatar">
          <span>{{ authStore.initials || 'PH' }}</span>
        </div>
      </div>

      <!-- Center: Title -->
      <h1 class="mob-topbar-title">{{ pageTitle }}</h1>

      <!-- Right: Notification + Search -->
      <div class="mob-topbar-right">
        <button class="mob-icon-btn" @click="$router.push('/parent/notifications/history')">
          <Bell :size="20" />
          <span v-if="unreadCount > 0" class="mob-notif-badge">{{ unreadCount }}</span>
        </button>
      </div>
    </header>

    <!-- ═══ CONTENT ═══ -->
    <main class="mob-content">
      <router-view v-slot="{ Component }">
        <Transition
          enter-active-class="mob-enter-active"
          enter-from-class="mob-enter-from"
          enter-to-class="mob-enter-to"
          leave-active-class="mob-leave-active"
          leave-from-class="mob-leave-from"
          leave-to-class="mob-leave-to"
          mode="out-in"
        >
          <Suspense timeout="0">
            <template #default>
              <component :is="Component" v-if="Component" />
              <div v-else class="mob-empty-state">
                <div class="mob-empty-icon">📋</div>
                <p class="mob-empty-title">Trang đang phát triển</p>
                <button class="mob-btn-primary" @click="$router.push('/parent/dashboard')">
                  ← Về trang chủ
                </button>
              </div>
            </template>
            <template #fallback>
              <div class="mob-loading">
                <div class="mob-spinner"></div>
                <p>Đang tải...</p>
              </div>
            </template>
          </Suspense>
        </Transition>
      </router-view>
    </main>

    <!-- ═══ BOTTOM NAVIGATION ═══ -->
    <nav class="mob-bottom-nav">
      <router-link
        v-for="tab in tabs"
        :key="tab.path"
        :to="tab.path"
        class="mob-tab"
        :class="{ 'mob-tab--active': isTabActive(tab.path) }"
      >
        <div class="mob-tab-icon-wrap" :class="{ 'mob-tab-icon-wrap--active': isTabActive(tab.path) }">
          <component :is="tab.icon" :size="20" />
        </div>
        <span class="mob-tab-label">{{ tab.label }}</span>
      </router-link>
    </nav>
  </div>
</template>

<style>
/* ══════════════════════════════════════════
   MOBILE SHELL – Layout_PhuHuynh_Mobile
   ══════════════════════════════════════════ */

.mobile-shell {
  display: flex;
  flex-direction: column;
  height: 100dvh;               /* dynamic viewport height – handles mobile chrome bar */
  width: 100%;
  background: var(--surface-page, #f8fafc);
  overflow: hidden;
  position: relative;
}

/* ── Topbar ── */
.mob-topbar {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 50;
  height: 56px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 14px;
  background: var(--surface-topbar, rgba(255,255,255,0.9));
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-bottom: 1px solid var(--border-card, #e2e8f0);
  box-shadow: 0 1px 8px rgba(0,0,0,0.05);
}

.mob-topbar-left {
  display: flex;
  align-items: center;
  width: 40px;
}

.mob-topbar-title {
  font-size: 16px;
  font-weight: 700;
  color: var(--text-heading, #0f172a);
  letter-spacing: -0.02em;
  flex: 1;
  text-align: center;
  margin: 0;
}

.mob-topbar-right {
  display: flex;
  align-items: center;
  gap: 6px;
  width: 40px;
  justify-content: flex-end;
}

.mob-back-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--surface-input, #f1f5f9);
  border: none;
  color: var(--text-heading, #0f172a);
  cursor: pointer;
  transition: background 0.15s;
  -webkit-tap-highlight-color: transparent;
}
.mob-back-btn:active { background: #e2e8f0; }

.mob-avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: linear-gradient(135deg, #c2410c, #ea580c 55%, #fb923c);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  color: white;
  box-shadow: 0 2px 8px rgba(234, 88, 12, 0.3);
}

.mob-icon-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: transparent;
  border: none;
  color: var(--text-label, #64748b);
  cursor: pointer;
  position: relative;
  -webkit-tap-highlight-color: transparent;
  transition: background 0.15s;
}
.mob-icon-btn:active { background: var(--surface-input, #f1f5f9); }

.mob-notif-badge {
  position: absolute;
  top: 4px;
  right: 4px;
  min-width: 16px;
  height: 16px;
  background: #ea580c;
  color: white;
  font-size: 9px;
  font-weight: 700;
  border-radius: 99px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 4px;
  border: 2px solid var(--surface-topbar, white);
}

/* ── Content ── */
.mob-content {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding-top: 56px;   /* topbar height */
  padding-bottom: 72px; /* bottom nav height */
  -webkit-overflow-scrolling: touch;
  overscroll-behavior-y: contain;
}

/* ── Page transitions ── */
.mob-enter-active {
  transition: all 0.22s cubic-bezier(0.4, 0, 0.2, 1);
}
.mob-enter-from {
  opacity: 0;
  transform: translateY(8px);
}
.mob-enter-to {
  opacity: 1;
  transform: translateY(0);
}
.mob-leave-active {
  transition: opacity 0.1s ease;
}
.mob-leave-from { opacity: 1; }
.mob-leave-to { opacity: 0; }

/* ── Bottom Nav ── */
.mob-bottom-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: 50;
  height: 64px;
  padding-bottom: env(safe-area-inset-bottom, 0px);
  display: flex;
  align-items: center;
  background: var(--surface-topbar, rgba(255,255,255,0.95));
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border-top: 1px solid var(--border-card, #e2e8f0);
  box-shadow: 0 -4px 16px rgba(0,0,0,0.06);
}

.mob-tab {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 3px;
  padding: 6px 0;
  text-decoration: none;
  color: var(--text-placeholder, #94a3b8);
  transition: color 0.2s;
  -webkit-tap-highlight-color: transparent;
  cursor: pointer;
}

.mob-tab--active {
  color: var(--mob-accent, #ea580c);
}

.mob-tab-icon-wrap {
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
  transition: background 0.2s, transform 0.15s;
}
.mob-tab-icon-wrap--active {
  background: #fff7ed;
  transform: scale(1.05);
}

.mob-tab-label {
  font-size: 9.5px;
  font-weight: 600;
  letter-spacing: 0.01em;
  line-height: 1;
}

/* ── Empty / Loading ── */
.mob-empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: calc(100dvh - 140px);
  gap: 12px;
  padding: 24px;
}
.mob-empty-icon { font-size: 48px; }
.mob-empty-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--text-heading, #0f172a);
}

.mob-loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: calc(100dvh - 140px);
  gap: 12px;
  color: var(--text-placeholder, #94a3b8);
  font-size: 13px;
}
.mob-spinner {
  width: 28px;
  height: 28px;
  border: 3px solid #fed7aa;
  border-top-color: #ea580c;
  border-radius: 50%;
  animation: mob-spin 0.8s linear infinite;
}
@keyframes mob-spin { to { transform: rotate(360deg); } }

.mob-btn-primary {
  background: #ea580c;
  color: white;
  border: none;
  border-radius: 12px;
  padding: 10px 20px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

/* Font */
.font-sans {
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  -webkit-font-smoothing: antialiased;
}
</style>
