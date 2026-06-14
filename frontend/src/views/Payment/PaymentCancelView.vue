<script setup>
import { ArrowLeft, XCircle } from 'lucide-vue-next'
</script>

<template>
  <main class="payment-page">
    <section class="payment-panel">
      <div class="payment-icon">
        <XCircle :size="28" />
      </div>
      <h1>Bạn đã hủy thanh toán</h1>
      <p>Giao dịch chưa được ghi nhận. Bạn có thể quay lại trang học phí để thực hiện lại.</p>
      <router-link to="/student/tuition" class="payment-action">
        <ArrowLeft :size="16" />
        Về trang học phí
      </router-link>
    </section>
  </main>
</template>

<style scoped>
.payment-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 1rem;
  background: var(--surface-page);
  color: var(--text-heading);
}

.payment-panel {
  width: min(100%, 28rem);
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: .75rem;
  text-align: center;
  padding: 1.25rem;
  border: 1px solid var(--border-card);
  border-radius: 18px;
  background: var(--surface-card);
  box-shadow: var(--lg-shadow-sm);
}

.payment-icon {
  width: 3.5rem;
  height: 3.5rem;
  display: grid;
  place-items: center;
  border-radius: 999px;
  color: var(--color-danger-text);
  background: var(--color-danger-bg);
}

h1 {
  margin: .25rem 0 0;
  font-size: 1.15rem;
  font-weight: 900;
}

p {
  margin: 0;
  color: var(--text-body);
  font-size: .9rem;
  line-height: 1.5;
}

.payment-action {
  margin-top: .5rem;
  display: inline-flex;
  align-items: center;
  gap: .45rem;
  min-height: 2.5rem;
  padding: 0 1rem;
  border-radius: 10px;
  color: var(--text-inverse);
  background: var(--accent-primary);
  font-size: .85rem;
  font-weight: 800;
  text-decoration: none;
  box-shadow: var(--lg-shadow-sm);
}
</style>
