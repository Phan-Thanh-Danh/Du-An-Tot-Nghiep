import { onBeforeUnmount } from 'vue'

export function usePortalSpotlight(gatewayRef) {
  let pointerFrame = 0
  let pendingPointerEvent = null

  function handlePointerMove(event) {
    pendingPointerEvent = event

    if (pointerFrame) return

    pointerFrame = requestAnimationFrame(() => {
      pointerFrame = 0

      const pointerEvent = pendingPointerEvent
      pendingPointerEvent = null

      if (!pointerEvent) return

      // Find the closest card that allows spotlight
      const card = pointerEvent.target?.closest('[data-portal-spotlight]')

      // Ensure the card is within our gateway
      if (!card || !gatewayRef.value?.contains(card)) {
        return
      }

      const rect = card.getBoundingClientRect()

      // Calculate relative position of the pointer within the card
      const x = pointerEvent.clientX - rect.left
      const y = pointerEvent.clientY - rect.top

      card.style.setProperty('--spotlight-x', `${x}px`)
      card.style.setProperty('--spotlight-y', `${y}px`)
    })
  }

  onBeforeUnmount(() => {
    if (pointerFrame) {
      cancelAnimationFrame(pointerFrame)
    }
    pendingPointerEvent = null
  })

  return {
    handlePointerMove
  }
}
