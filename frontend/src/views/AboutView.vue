<template>
  <main class="lg-page-bg flex min-h-screen items-center justify-center px-4 py-16">
    <section class="lg-glass w-full max-w-2xl rounded-[2rem] p-8">
      <span class="inline-flex rounded-full border border-blue-200 bg-blue-50/80 px-3 py-1 text-xs font-semibold text-blue-700">
        LMS Academic Management System
      </span>
      <h1 class="mt-5 text-3xl font-extrabold leading-tight text-slate-950">
        Cổng học tập và quản lý đào tạo
      </h1>
      <p class="mt-4 text-sm leading-7 text-slate-600">
        Giao diện frontend dùng Vue 3, Pinia, Vue Router và Tailwind CSS theo phong cách
        LMS Liquid Glass Academic. Các module nghiệp vụ chưa có backend controller sẽ được
        đánh dấu dự kiến/cần bổ sung.
      </p>
      <router-link
        to="/login"
        class="mt-6 inline-flex rounded-xl bg-blue-900 px-5 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-900/20 transition hover:bg-blue-800 focus:outline-none focus:ring-4 focus:ring-blue-500/25"
      >
        Đăng nhập
      </router-link>
    </section>
  </main>
</template>
