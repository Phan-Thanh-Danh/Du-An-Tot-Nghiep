<template>
  <div class="h-full w-full max-w-4xl mx-auto py-12 px-4">
    <div class="lg-card-glass lg-enter flex flex-col items-center justify-center py-20 text-center relative overflow-hidden">
      <!-- Mảng màu trang trí bên trong card -->
      <div class="pointer-events-none absolute inset-0 z-0 opacity-45">
        <div class="lg-blob bg-orange-500/10" style="top: -50%; left: -20%; width: 300px; height: 300px; filter: blur(50px); position: absolute;"></div>
        <div class="lg-blob bg-amber-500/10" style="bottom: -50%; right: -20%; width: 300px; height: 300px; filter: blur(50px); position: absolute;"></div>
      </div>
      
      <div class="relative z-10 flex flex-col items-center">
        <div class="flex h-20 w-20 items-center justify-center rounded-3xl surface-card shadow-(--lg-shadow-md) mb-4 border border-card">
          <svg xmlns="http://www.w3.org/2000/svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="#ea580c" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <line x1="10" y1="9" x2="8" y2="9"></line>
          </svg>
        </div>
        <h2 class="text-xl font-bold text-heading mb-2">Trang đang phát triển</h2>
        <p class="text-body max-w-md mb-8">
          Module này đang được xây dựng theo yêu cầu và sẽ sớm ra mắt trong các phiên bản cập nhật tiếp theo.
        </p>
        <router-link to="/parent/dashboard" class="lg-button-primary bg-orange-600 hover:bg-orange-700 text-white inline-flex items-center gap-2 px-4 py-2 rounded-xl transition font-semibold">
          <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="m15 18-6-6 6-6"/>
          </svg>
          Quay lại Tổng quan
        </router-link>
      </div>
    </div>
  </div>
</template>

<script setup>
</script>
