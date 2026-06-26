import { ref, onUnmounted } from 'vue'

export function usePointerSpotlight() {
  const isHovering = ref(false)

  let rafId = null
  let el = null
  let px = 0
  let py = 0

  const supportsHover = typeof window !== 'undefined' && window.matchMedia('(hover: hover)').matches

  function process() {
    rafId = null
    if (!el) return
    el.style.setProperty('--spotlight-x', px + 'px')
    el.style.setProperty('--spotlight-y', py + 'px')
  }

  function onPointerMove(e) {
    if (!supportsHover) return
    el = e.currentTarget
    const rect = el.getBoundingClientRect()
    px = e.clientX - rect.left
    py = e.clientY - rect.top
    if (!rafId) rafId = requestAnimationFrame(process)
  }

  function onPointerEnter() {
    if (supportsHover) isHovering.value = true
  }

  function onPointerLeave() {
    if (supportsHover) isHovering.value = false
    el = null
  }

  onUnmounted(() => {
    if (rafId) cancelAnimationFrame(rafId)
  })

  return { isHovering, onPointerMove, onPointerEnter, onPointerLeave }
}
