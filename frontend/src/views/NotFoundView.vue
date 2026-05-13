<template>
  <main class="lg-page-bg flex min-h-screen items-center justify-center px-4 py-16">
    <section class="lg-glass w-full max-w-md rounded-[2rem] p-8 text-center">
      <div class="mx-auto flex h-16 w-16 items-center justify-center rounded-2xl bg-blue-50">
        <svg class="h-8 w-8 text-blue-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
        <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
      </svg>
    </div>
      <h1 class="mt-4 text-xl font-bold text-slate-950">Trang không tồn tại</h1>
      <p class="mt-2 text-sm leading-6 text-slate-600">Đường dẫn bạn truy cập không hợp lệ.</p>
      <router-link
        to="/"
        class="mt-6 inline-flex items-center justify-center rounded-xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-900/20 transition hover:bg-blue-800 focus:outline-none focus:ring-4 focus:ring-blue-500/25"
      >
        Về Dashboard
      </router-link>
    </section>
  </main>
</template>
