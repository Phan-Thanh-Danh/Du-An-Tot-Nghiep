<template>
  <div class="h-full w-full max-w-4xl mx-auto py-12 px-4">
    <div class="lg-card-glass lg-enter flex flex-col items-center justify-center py-20 text-center relative overflow-hidden">
      <!-- Mảng màu trang trí bên trong card -->
      <div class="pointer-events-none absolute inset-0 z-0 opacity-40">
        <div class="lg-blob lg-blob-blue" style="top: -50%; left: -20%;"></div>
        <div class="lg-blob lg-blob-violet" style="bottom: -50%; right: -20%;"></div>
      </div>
      
      <div class="relative z-10 flex flex-col items-center">
        <div class="flex h-20 w-20 items-center justify-center rounded-3xl surface-card shadow-[var(--lg-shadow-md)] mb-4 border border-card">
          <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="color:var(--sidebar-accent)">
            <path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
          </svg>
        </div>
        <h2 class="text-xl font-bold text-heading mb-2">Trang đang phát triển</h2>
        <p class="text-body max-w-md mb-8">
          Module dành cho Ban giám hiệu đang được hoàn thiện. Vui lòng quay lại trong các phiên bản cập nhật tới.
        </p>
        <router-link to="/bgh/dashboard" class="lg-button-primary" style="background: linear-gradient(135deg, #4f46e5, #6366f1 52%, #8b5cf6);">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
          Quay lại Dashboard
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
</script>
