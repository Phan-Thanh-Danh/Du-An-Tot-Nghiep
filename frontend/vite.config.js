import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    tailwindcss(),
    vue(),
    vueJsx(),
    vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  build: {
    chunkSizeWarningLimit: 1500,
    rolldownOptions: {
      output: {
        codeSplitting: {
          groups: [
            {
              test: /[\\/]node_modules[\\/](?:vue|pinia|vue-router)[\\/]/,
              name: 'vendor-vue',
              priority: 20,
            },
            {
              test: /[\\/]node_modules[\\/]xlsx[\\/]/,
              name: 'vendor-xlsx',
              priority: 20,
            },
            {
              test: /[\\/]node_modules[\\/](?:html2pdf\.js|html2canvas|jspdf|dompurify|canvg|core-js|raf|rgbcolor|svg-pathdata|stackblur-canvas|performance-now|fflate|fast-png|iobuffer|pako|css-line-break|text-segmentation|utrie|base64-arraybuffer|@babel[\\/]runtime)[\\/]/,
              name: 'vendor-pdf',
              priority: 20,
            },
            {
              test: /[\\/]node_modules[\\/]/,
              name: 'vendor',
              priority: 10,
            },
          ],
        },
      },
    },
  },
})
