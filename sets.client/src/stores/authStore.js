import { defineStore } from 'pinia'
import { useAnnouncementStore } from '@/stores/announcementStore'

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null,
    branch: null,
    section: null,
    isAuthenticated: false,
    theme: 0,       // 0 = light, 1 = dark, 2 = dim
    accentColor: 0,  // 0 = purple, 1 = blue, 2 = teal, 3 = rose
    isContingencyMode: false,
    profilePicture: null,
    appVersion: '...',
    isDeveloper: false,
  }),
  actions: {
    setUser(user, branch, section, isContingency = false) {
      this.user = user
      this.branch = branch
      this.section = section
      this.isAuthenticated = true
      this.theme = user?.theme ?? 0
      this.accentColor = user?.accentColor ?? 0
      this.isContingencyMode = isContingency
      this.profilePicture = null
      this.isDeveloper = user?.isDeveloper ?? false

      const announcementStore = useAnnouncementStore()
      announcementStore.reset()
    },
    setTheme(theme) {
      this.theme = theme
    },
    setAccentColor(accentColor) {
      this.accentColor = accentColor
    },
    setProfilePicture(base64) {
      this.profilePicture = base64
    },
    setAppVersion(version) {
      this.appVersion = version
    },
    logout() {
      this.user = null
      this.branch = null
      this.section = null
      this.isAuthenticated = false
      this.theme = 0
      this.accentColor = 0
      this.isContingencyMode = false
      this.appVersion = '...'
      const announcementStore = useAnnouncementStore()
      announcementStore.reset()
    }
  },
  getters: {
    userID: (state) => state.user?.userID ?? '',
    userName: (state) => state.user?.userName ?? '',
    isAdmin: (state) => state.user?.isAdmin ?? false,
    isDeveloper: (state) => state.user?.isDeveloper ?? false,
    isTL: (state) => state.section?.roleID == 2 ?? false,
    sectionCode: (state) => state.section?.code ?? '',
    sectionName: (state) => state.section?.name ?? '',
    roleID: (state) => state.section?.roleID ?? null,
    branchCode: (state) => state.branch ?? '',
    sectionCategory: (state) => state.section?.category ?? null,
    isEndorser: (state) => !state.user?.isAdmin && state.section?.category === '1',
    isReceiver: (state) => !state.user?.isAdmin && state.section?.category === '2',
    isRunner: (state) => !state.user?.isAdmin && state.section?.category === '3',
    isContingency: (state) => state.isContingencyMode,
    version: (state) => state.appVersion,
  },
  persist: true
})
