<template>
  <AppLayout>
    <div class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text)">
        Section Settings
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted)">
        <span style="color: var(--color-primary); font-weight: 700">TEAM LEAD</span>
        · {{ authStore.sectionName }} · {{ authStore.branchCode }}
      </p>
    </div>

    <div class="flex gap-6">
      <aside class="w-56 flex-shrink-0">
        <div class="rounded-2xl overflow-hidden sticky top-6"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow)">
          <div class="p-2 space-y-0.5">
            <button v-for="tab in settingsTabs" :key="tab.key"
                    class="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-xs font-bold uppercase tracking-widest transition-all text-left"
                    :style="activeTab === tab.key
                      ? 'background-color: var(--color-primary-soft); color: var(--color-primary); border-left: 3px solid var(--color-primary); padding-left: calc(1rem - 3px);'
                      : 'color: var(--color-text-muted); border-left: 3px solid transparent; padding-left: calc(1rem - 3px);'"
                    @click="activeTab = tab.key">
              <span class="material-symbols-outlined text-base">{{ tab.icon }}</span>
              {{ tab.label }}
            </button>
          </div>
        </div>
      </aside>

      <div class="flex-1 min-w-0">
        <TLPcRegistrationTab v-if="activeTab === 'pc'" @toast="showToast" />
        <TLUserManagementTab v-if="activeTab === 'users'" @toast="showToast" />
        <RunningDaysTab v-if="activeTab === 'runningDays'" @toast="showToast" />
      </div>
    </div>

    <Transition name="toast">
      <div v-if="toast.visible"
           class="fixed bottom-6 left-1/2 -translate-x-1/2 z-50 px-5 py-3 rounded-2xl shadow-xl flex items-center gap-3 text-sm font-bold"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border); color: var(--color-text);">
        <span class="material-symbols-outlined text-base" style="color: var(--color-primary)">check_circle</span>
        {{ toast.message }}
      </div>
    </Transition>
  </AppLayout>
</template>

<script setup>
  import { ref } from 'vue'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import { useAuthStore } from '@/stores/authStore'
  import TLPcRegistrationTab from '@/components/settings/TLPcRegistrationTab.vue'
  import TLUserManagementTab from '@/components/settings/TLUserManagementTab.vue'
  import RunningDaysTab from '@/views/admin/RunningDaysTab.vue'

  const authStore = useAuthStore()

  const settingsTabs = [
    { key: 'pc', label: 'PC Registration', icon: 'computer' },
    { key: 'users', label: 'User Management', icon: 'manage_accounts' },
    { key: 'runningDays', label: 'Running Days', icon: 'calendar_month' },
  ]

  const activeTab = ref('pc')

  const toast = ref({ visible: false, message: '' })
  let toastTimer = null

  function showToast(msg) {
    clearTimeout(toastTimer)
    toast.value = { visible: true, message: msg }
    toastTimer = setTimeout(() => { toast.value.visible = false }, 3000)
  }
</script>

<style scoped>
  .toast-enter-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
  }

  .toast-leave-active {
    transition: opacity 0.15s ease, transform 0.15s ease;
  }

  .toast-enter-from {
    opacity: 0;
    transform: translateX(-50%) translateY(8px);
  }

  .toast-leave-to {
    opacity: 0;
    transform: translateX(-50%) translateY(8px);
  }
</style>
