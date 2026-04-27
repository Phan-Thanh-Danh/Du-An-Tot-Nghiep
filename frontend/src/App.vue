<script setup>
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <div class="min-h-screen">
    <header class="border-b border-slate-200/80 bg-white/70 backdrop-blur">
      <div class="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
        <div class="flex items-center gap-3">
          <img alt="Vue logo" class="h-10 w-10 rounded-xl shadow-sm" src="@/assets/logo.svg" />
          <div>
            <p class="text-sm font-medium text-sky-600">Vue + Tailwind</p>
            <h1 class="text-lg font-semibold text-slate-900">Frontend Starter</h1>
          </div>
        </div>

        <nav class="flex items-center gap-2 text-sm font-medium">
          <RouterLink
            to="/"
            class="rounded-full px-4 py-2 text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
          >
            Home
          </RouterLink>
          <RouterLink
            to="/about"
            class="rounded-full px-4 py-2 text-slate-600 transition hover:bg-slate-100 hover:text-slate-900"
          >
            About
          </RouterLink>
        </nav>
      </div>
    </header>

    <main class="mx-auto max-w-6xl px-6 py-10">
      <section class="mb-8 rounded-3xl border border-slate-200 bg-white/80 p-8 shadow-sm">
        <span
          class="inline-flex rounded-full border border-sky-200 bg-sky-50 px-3 py-1 text-xs font-semibold uppercase tracking-[0.2em] text-sky-700"
        >
          Tailwind ready
        </span>
        <h2 class="mt-4 text-4xl font-bold tracking-tight text-slate-900">
          Bạn có thể bắt đầu làm giao diện bằng utility classes ngay bây giờ.
        </h2>
        <p class="mt-3 max-w-2xl text-base leading-7 text-slate-600">
          Ví dụ: <code class="rounded bg-slate-100 px-2 py-1 text-sm">class="grid gap-6 md:grid-cols-2"</code>
        </p>
      </section>

      <RouterView />
    </main>
  </div>
</template>
