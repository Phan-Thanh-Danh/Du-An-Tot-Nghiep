import { watch } from 'vue'

/**
 * @param {import('vue').WatchSource<boolean|undefined>} source - reactive source to watch
 */
export function useBodyScrollLock(source) {
  watch(source, (value) => {
    if (value) {
      document.body.style.overflow = 'hidden'
    } else {
      document.body.style.overflow = ''
    }
  })
}
