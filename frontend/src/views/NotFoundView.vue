<template>
  <div class="flex flex-col items-center justify-center py-24 text-center">
    <div class="flex h-16 w-16 items-center justify-center rounded-2xl bg-slate-100">
      <svg class="h-8 w-8 text-slate-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
      </svg>
    </div>
    <h3 class="mt-4 text-base font-semibold text-slate-600">Trang không tồn tại</h3>
    <p class="mt-1 text-sm text-slate-400">Đường dẫn bạn truy cập không hợp lệ.</p>
    <router-link to="/student/dashboard" class="mt-5 inline-flex items-center gap-2 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 transition-colors">
      ← Về Dashboard
    </router-link>
  </div>
</template>
