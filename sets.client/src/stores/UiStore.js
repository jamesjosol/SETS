import { defineStore } from 'pinia'

export const useUiStore = defineStore('ui', {
  state: () => ({
    sidebarCollapsed: {
      '1': false,
      '2': false,
      '3': false,
    }
  }),
  actions: {
    toggleCategory(categoryCode) {
      const key = String(categoryCode)
      if (key in this.sidebarCollapsed) {
        this.sidebarCollapsed[key] = !this.sidebarCollapsed[key]
      }
    },
    setCollapsed(categoryCode, value) {
      const key = String(categoryCode)
      if (key in this.sidebarCollapsed) {
        this.sidebarCollapsed[key] = value
      }
    }
  },
  persist: true
})
