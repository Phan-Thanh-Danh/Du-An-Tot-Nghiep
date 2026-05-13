<template>
  <div class="h-full w-full max-w-4xl mx-auto py-12 px-6">
    <div class="lg-card-glass lg-enter flex flex-col items-center justify-center py-20 text-center relative overflow-hidden">
      <!-- Mảng màu trang trí bên trong card -->
      <div class="pointer-events-none absolute inset-0 z-0 opacity-40">
        <div class="lg-blob lg-blob-cyan" style="top: -50%; left: -20%;"></div>
        <div class="lg-blob lg-blob-violet" style="bottom: -50%; right: -20%;"></div>
      </div>
      
      <div class="relative z-10 flex flex-col items-center">
        <div class="flex h-20 w-20 items-center justify-center rounded-3xl bg-white/60 shadow-[0_8px_32px_rgba(15,23,42,0.12),inset_0_1px_0_rgba(255,255,255,0.8)] backdrop-blur-md mb-6 border border-white/50">
          <svg xmlns="http://www.w3.org/-svg" width="36" height="36" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="text-indigo-600">
            <path d="M14.5 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7.5L14.5 2z"></path>
            <polyline points="14 2 14 8 20 8"></polyline>
            <line x1="16" y1="13" x2="8" y2="13"></line>
            <line x1="16" y1="17" x2="8" y2="17"></line>
            <line x1="10" y1="9" x2="8" y2="9"></line>
          </svg>
        </div>
        <h2 class="text-2xl font-bold text-slate-800 mb-2">Trang đang phát triển</h2>
        <p class="text-slate-500 max-w-md mb-8">
          Module này đang được xây dựng theo kiến trúc Liquid Glass và sẽ sớm ra mắt trong các phiên bản cập nhật tiếp theo.
        </p>
        <router-link to="/staff/dashboard" class="lg-button-primary">
          <svg xmlns="http://www.w3.org/-svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
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
