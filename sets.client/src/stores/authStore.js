import { defineStore } from 'pinia'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    branch: null,
    section: null,
    isAuthenticated: false,
    theme: 0  // 0 = light, 1 = dark, 2 = dim
  }),

  actions: {
    setUser(user, branch, section) {
      this.user = user
      this.branch = branch
      this.section = section
      this.isAuthenticated = true
      this.theme = user?.theme ?? 0
    },
    setTheme(theme) {
      this.theme = theme
    },
    logout() {
      this.user = null
      this.branch = null
      this.section = null
      this.isAuthenticated = false
      this.theme = 0
    }
  },

  getters: {
    userID: (state) => state.user?.userID ?? '',
    userName: (state) => state.user?.userName ?? '',
    isAdmin: (state) => state.user?.isAdmin ?? false,
    sectionCode: (state) => state.section?.code ?? '',
    sectionName: (state) => state.section?.name ?? '',
    roleID: (state) => state.section?.roleID ?? null,
    branchCode: (state) => state.branch ?? '',
    sectionCategory: (state) => state.section?.category ?? null,

    // helper getters based on category code
    isEndorser: (state) => !state.user?.isAdmin && state.section?.category === '1',
    isReceiver: (state) => !state.user?.isAdmin && state.section?.category === '2',
    isRunner: (state) => !state.user?.isAdmin && state.section?.category === '3',
  },

  persist: true
})
