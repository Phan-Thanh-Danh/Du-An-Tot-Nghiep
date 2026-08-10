import { fileURLToPath, URL } from 'node:url'
import fs from 'node:fs'

import { defineConfig, loadEnv } from 'vite'
import tailwindcss from '@tailwindcss/vite'
import vue from '@vitejs/plugin-vue'
import vueJsx from '@vitejs/plugin-vue-jsx'
import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, fileURLToPath(new URL('.', import.meta.url)), '')
  const backendOrigin = env.VITE_BACKEND_ORIGIN || 'https://localhost:7150'

  const plugins = [
    tailwindcss(),
    vue(),
    vueJsx(),
  ]

  // Plugin devtools chỉ bật ở development — khi bundle vào production build
  // nó hook vào mọi component setup và gây lỗi "subTree, t.$ is undefined"
  // https://github.com/vitejs/vite-plugin-vue-devtools/issues/173
  if (mode === 'development') {
    plugins.push(vueDevTools())
  }

  return {
    plugins,
    server: {
      host: '0.0.0.0',
      https: fs.existsSync('../certs/lms.pem') && fs.existsSync('../certs/lms-key.pem') ? {
        cert: fs.readFileSync('../certs/lms.pem'),
        key: fs.readFileSync('../certs/lms-key.pem'),
      } : false,
      proxy: {
        '/api': {
          target: backendOrigin,
          changeOrigin: true,
          secure: false,
        },
      },
    },
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
                test: /[\\/]node_modules[\\/]/,
                name: 'vendor',
                priority: 10,
              },
            ],
          },
        },
      },
    },
  }
})
