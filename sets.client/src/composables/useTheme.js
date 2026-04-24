import { watch } from 'vue'
import { useAuthStore } from '@/stores/authStore'

const themeMap = {
  0: 'light',
  1: 'dark',
  2: 'dim'
}

const accentMap = {
  0: 'purple',
  1: 'blue',
  2: 'teal',
  3: 'rose'
}

export function useTheme() {
  const authStore = useAuthStore()

  function applyTheme(themeValue) {
    const themeName = themeMap[themeValue] ?? 'light'
    document.documentElement.setAttribute('data-theme', themeName)
  }

  function applyAccent(accentValue) {
    const accentName = accentMap[accentValue] ?? 'purple'
    document.documentElement.setAttribute('data-accent', accentName)
  }

  function initTheme() {
    applyTheme(authStore.theme)
    applyAccent(authStore.accentColor)
  }

  watch(() => authStore.theme, (newTheme) => applyTheme(newTheme))
  watch(() => authStore.accentColor, (newAccent) => applyAccent(newAccent))

  return { initTheme, applyTheme, applyAccent, themeMap, accentMap }
}
