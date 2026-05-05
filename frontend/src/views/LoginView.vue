<template>
  <div class="flex min-h-screen items-center justify-center bg-slate-100">
    <div class="w-full max-w-sm rounded-2xl bg-white p-8 shadow-xl">
      <div class="flex justify-center mb-6">
        <div class="flex h-12 w-12 items-center justify-center rounded-xl bg-blue-600 shadow-lg shadow-blue-200">
          <svg class="h-6 w-6 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4.26 10.147a60.436 60.436 0 00-.491 6.347A48.627 48.627 0 0112 20.904a48.627 48.627 0 018.232-4.41 60.46 60.46 0 00-.491-6.347m-15.482 0a50.57 50.57 0 00-2.658-.813A59.905 59.905 0 0112 3.493a59.902 59.902 0 0110.399 5.84c-.896.248-1.783.52-2.658.814m-15.482 0A50.697 50.697 0 0112 13.489a50.702 50.702 0 017.74-3.342M6.75 15a.75.75 0 100-1.5.75.75 0 000 1.5zm0 0v-3.675A55.378 55.378 0 0112 8.443m-7.007 11.55A5.981 5.981 0 006.75 15.75v-1.5" />
          </svg>
        </div>
      </div>
      <h1 class="text-center text-xl font-bold text-slate-800">EduLMS</h1>
      <p class="text-center text-sm text-slate-500 mt-1 mb-6">Đăng nhập để tiếp tục</p>
      <router-link to="/student/dashboard" class="block w-full rounded-lg bg-blue-600 py-2.5 text-center text-sm font-semibold text-white hover:bg-blue-700 transition-colors">
        Đăng nhập (Demo)
      </router-link>
    </div>
  </div>
</template>
