import { watch } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const themeMap = {
  0: 'light',
  1: 'dark',
  2: 'dim'
}

export function useTheme() {
  const authStore = useAuthStore()

  function applyTheme(themeValue) {
    const themeName = themeMap[themeValue] ?? 'light'
    document.documentElement.setAttribute('data-theme', themeName)
  }

  function initTheme() {
    applyTheme(authStore.theme)
  }

  // watch for theme changes and apply automatically
  watch(
    () => authStore.theme,
    (newTheme) => applyTheme(newTheme)
  )

  return { initTheme, applyTheme, themeMap }
}
