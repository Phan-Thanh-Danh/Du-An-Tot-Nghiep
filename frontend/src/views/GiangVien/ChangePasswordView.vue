<script setup>
import { computed, ref } from 'vue'
import { usePopupStore } from '@/stores/popup'
import { AlertCircle, ArrowLeft, CheckCircle2, KeyRound, Lock, ShieldCheck } from 'lucide-vue-next'
import { useRouter } from 'vue-router'
import GlassBadge from '@/components/ui/GlassBadge.vue'
import GlassButton from '@/components/ui/GlassButton.vue'
import GlassPanel from '@/components/ui/GlassPanel.vue'

const router = useRouter()
const popupStore = usePopupStore()

const passwords = ref({
  current: '',
  new: '',
  confirm: '',
})

const passwordRules = computed(() => [
  { label: 'Ít nhất 8 ký tự', done: passwords.value.new.length >= 8 },
  { label: 'Chứa ít nhất một chữ hoa', done: /[A-Z]/.test(passwords.value.new) },
  { label: 'Chứa ít nhất một số', done: /\d/.test(passwords.value.new) },
  { label: 'Không trùng mật khẩu cũ', done: passwords.value.new && passwords.value.new !== passwords.value.current },
])

function handlePasswordChange() {
  popupStore.success('Đã đổi mật khẩu', 'Mật khẩu của bạn đã được cập nhật.')
  router.push('/teacher/profile')
}
</script>

<template>
  <div class="change-password-page">
    <GlassPanel variant="soft" density="compact" class="page-header" :clip="false">
      <div class="header-main">
        <router-link to="/teacher/profile" class="back-link" aria-label="Quay lại hồ sơ">
          <ArrowLeft :size="18" />
        </router-link>
        <div class="min-w-0">
          <div class="eyebrow">Account security</div>
          <h1 class="page-title">Đổi mật khẩu</h1>
          <p class="page-subtitle">Cập nhật mật khẩu để bảo vệ tài khoản giảng viên của bạn.</p>
        </div>
      </div>
    </GlassPanel>

    <div class="password-layout">
      <GlassPanel variant="surface" density="compact" class="security-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Yêu cầu mật khẩu</h2>
              <p>Các điều kiện sẽ chuyển trạng thái khi bạn nhập mật khẩu mới.</p>
            </div>
            <ShieldCheck :size="18" />
          </div>
        </template>

        <div class="rule-list">
          <div v-for="rule in passwordRules" :key="rule.label" class="rule-row">
            <span :class="['rule-dot', rule.done && 'is-done']">
              <CheckCircle2 v-if="rule.done" :size="12" />
            </span>
            <span>{{ rule.label }}</span>
            <GlassBadge :variant="rule.done ? 'success' : 'neutral'" size="sm">
              {{ rule.done ? 'Đạt' : 'Chưa đạt' }}
            </GlassBadge>
          </div>
        </div>

        <div class="security-note">
          <AlertCircle :size="15" />
          <span>Bạn sẽ được chuyển về hồ sơ sau khi đổi mật khẩu thành công.</span>
        </div>
      </GlassPanel>

      <GlassPanel variant="readable" density="compact" class="form-panel">
        <template #header>
          <div class="panel-heading">
            <div>
              <h2>Thông tin mật khẩu</h2>
              <p>Nhập mật khẩu hiện tại và xác nhận mật khẩu mới.</p>
            </div>
            <GlassBadge variant="warning" size="sm">Bảo mật</GlassBadge>
          </div>
        </template>

        <div class="password-form">
          <label class="field">
            <span>Mật khẩu hiện tại</span>
            <div class="input-wrap">
              <KeyRound :size="16" />
              <input v-model="passwords.current" type="password" placeholder="Nhập mật khẩu hiện tại" />
            </div>
          </label>

          <label class="field">
            <span>Mật khẩu mới</span>
            <div class="input-wrap">
              <Lock :size="16" />
              <input v-model="passwords.new" type="password" placeholder="Nhập mật khẩu mới" />
            </div>
          </label>

          <label class="field">
            <span>Xác nhận mật khẩu mới</span>
            <div class="input-wrap">
              <Lock :size="16" />
              <input v-model="passwords.confirm" type="password" placeholder="Nhập lại mật khẩu mới" />
            </div>
          </label>

          <div class="match-note">
            <AlertCircle :size="15" />
            <span v-if="passwords.confirm && passwords.new !== passwords.confirm">
              Mật khẩu xác nhận chưa khớp với mật khẩu mới.
            </span>
            <span v-else>
              Kiểm tra kỹ mật khẩu mới trước khi cập nhật.
            </span>
          </div>

          <div class="form-actions">
            <router-link to="/teacher/profile" class="cancel-link">Hủy</router-link>
            <GlassButton variant="primary" size="sm" @click="handlePasswordChange">
              Cập nhật mật khẩu
            </GlassButton>
          </div>
        </div>
      </GlassPanel>
    </div>
  </div>
</template>

<style scoped>
.change-password-page {
  display: grid;
  gap: 1rem;
  max-width: 64rem;
  margin: 0 auto;
  padding-bottom: 2rem;
  color: var(--text-body);
}

.page-header,
.header-main,
.back-link,
.panel-heading,
.rule-row,
.security-note,
.input-wrap,
.match-note,
.form-actions,
.cancel-link {
  display: flex;
  align-items: center;
}

.page-header,
.panel-heading,
.form-actions {
  justify-content: space-between;
  gap: 1rem;
}

.header-main {
  gap: 0.875rem;
}

.back-link {
  justify-content: center;
  flex: 0 0 auto;
  width: 2.5rem;
  height: 2.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  color: var(--text-link);
  text-decoration: none;
}

.eyebrow,
.page-subtitle,
.panel-heading p,
.rule-row,
.security-note,
.match-note {
  color: var(--text-muted);
}

.eyebrow {
  font-size: 0.6875rem;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.page-title {
  margin: 0;
  color: var(--text-heading);
  font-size: clamp(1.125rem, 2vw, 1.5rem);
  font-weight: 900;
}

.page-subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  line-height: 1.5;
}

.password-layout {
  display: grid;
  grid-template-columns: minmax(16rem, 0.34fr) minmax(0, 1fr);
  gap: 1rem;
  align-items: start;
}

.panel-heading h2 {
  margin: 0;
  color: var(--text-heading);
  font-size: 0.9375rem;
  font-weight: 900;
}

.panel-heading p {
  margin: 0.125rem 0 0;
  font-size: 0.75rem;
  font-weight: 600;
}

.rule-list,
.password-form {
  display: grid;
  gap: 0.75rem;
}

.rule-row {
  gap: 0.625rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--surface-input);
  padding: 0.625rem 0.75rem;
  font-size: 0.8125rem;
  font-weight: 700;
}

.rule-row > span:nth-child(2) {
  flex: 1;
}

.rule-dot {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1rem;
  height: 1rem;
  border-radius: 999px;
  border: 1px solid var(--border-card);
  background: var(--surface-card);
}

.rule-dot.is-done {
  color: var(--color-success-text);
}

.security-note,
.match-note {
  gap: 0.5rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-card);
  background: var(--color-warning-bg);
  color: var(--color-warning-text);
  padding: 0.75rem;
  font-size: 0.75rem;
  font-weight: 800;
  line-height: 1.45;
}

.field {
  display: grid;
  gap: 0.5rem;
}

.field span {
  color: var(--text-label);
  font-size: 0.75rem;
  font-weight: 800;
}

.input-wrap {
  gap: 0.625rem;
  min-height: 2.75rem;
  border-radius: var(--radius-lg);
  border: 1px solid var(--border-input);
  background: var(--surface-input);
  color: var(--text-muted);
  padding: 0 0.875rem;
}

.input-wrap input {
  width: 100%;
  min-width: 0;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--text-body);
  font-size: 0.875rem;
  font-weight: 650;
}

.input-wrap input::placeholder {
  color: var(--text-placeholder);
}

.input-wrap:focus-within {
  border-color: var(--border-input-focus);
  box-shadow: 0 0 0 3px var(--border-focus-ring);
}

.form-actions {
  flex-wrap: wrap;
  margin-top: 0.5rem;
  padding-top: 0.875rem;
  border-top: 1px solid var(--border-card);
}

.cancel-link {
  justify-content: center;
  min-height: var(--control-height-sm);
  border-radius: var(--radius-md);
  border: 1px solid var(--border-card);
  background: var(--surface-card);
  color: var(--text-body);
  padding: 0 0.875rem;
  font-size: 0.78125rem;
  font-weight: 800;
  text-decoration: none;
}

.cancel-link:hover {
  color: var(--text-link);
}

@media (max-width: 900px) {
  .page-header {
    align-items: flex-start;
  }

  .password-layout {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .form-actions,
  .cancel-link,
  .form-actions :deep(.glass-button) {
    width: 100%;
  }
}
</style>
