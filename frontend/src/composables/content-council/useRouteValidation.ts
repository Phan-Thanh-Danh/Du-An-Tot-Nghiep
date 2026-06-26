import { useRoute, useRouter } from 'vue-router'

export function useRouteValidation() {
  const route = useRoute()
  const router = useRouter()

  function validateNumericParam(paramName: string, fallbackRouteName: string) {
    const val = route.params[paramName]
    const num = Number(val)
    if (isNaN(num) || num <= 0) {
      router.replace({ name: fallbackRouteName })
      return null
    }
    return num
  }

  return {
    validateNumericParam
  }
}
