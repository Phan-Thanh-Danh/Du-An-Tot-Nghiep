<template>
  <div class="bg-white rounded-xl border border-slate-200 overflow-hidden shadow-sm">
    <div class="overflow-x-auto">
      <table class="w-full text-left">
        <thead class="bg-slate-50 border-b border-slate-200">
          <tr>
            <th v-for="i in 8" :key="i" class="p-4">
              <div class="h-4 bg-slate-200 rounded w-20 animate-pulse"></div>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-slate-100">
          <tr v-for="i in 8" :key="i">
            <td v-for="j in 8" :key="j" class="p-4">
              <div class="h-4 bg-slate-100 rounded w-full max-w-[120px] animate-pulse" :style="{ width: Math.random() * 60 + 40 + '%' }"></div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
