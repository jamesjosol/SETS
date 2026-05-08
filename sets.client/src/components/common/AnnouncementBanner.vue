<template>
  <Transition name="banner">
    <div v-if="store.isVisible && isTargeted"
         class="flex items-start gap-3 px-6 py-3"
         :style="`background-color: ${config.bg}; border-bottom: 1px solid ${config.border};`">

      <!-- Icon -->
      <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
            :style="`font-size: 17px; color: ${config.color};`">
        {{ config.icon }}
      </span>

      <!-- Text -->
      <div class="flex-1 min-w-0">
        <span v-if="announcement.title"
              class="text-xs font-extrabold mr-2"
              :style="`color: ${config.color}`">
          {{ announcement.title }}
        </span>
        <span class="text-xs font-medium"
              :style="`color: ${config.textColor}`">
          {{ announcement.message }}
        </span>
      </div>

      <!-- Expiry countdown -->
      <!--<span class="text-[10px] font-bold flex-shrink-0 mt-0.5"
            :style="`color: ${config.color}; opacity: 0.7;`">
        {{ countdown }}
      </span>-->

      <!-- Dismiss -->
      <button class="flex-shrink-0 p-0.5 rounded-lg transition-all opacity-60 hover:opacity-100"
              :style="`color: ${config.color}`"
              @click="store.dismiss()">
        <span class="material-symbols-outlined" style="font-size: 16px">close</span>
      </button>

    </div>
  </Transition>
</template>

<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useAnnouncementStore } from '@/stores/announcementStore'
import { useAuthStore } from '@/stores/authStore'

const store      = useAnnouncementStore()
const authStore  = useAuthStore()

const announcement = computed(() => store.announcement)

// ── Type config ───────────────────────────────────────────────────────────

const typeConfigs = {
  info: {
    bg:        'rgba(59,130,246,0.07)',
    border:    'rgba(59,130,246,0.2)',
    color:     '#3b82f6',
    textColor: 'var(--color-text)',
    icon:      'info',
  },
  warning: {
    bg:        'rgba(234,179,8,0.07)',
    border:    'rgba(234,179,8,0.25)',
    color:     '#ca8a04',
    textColor: 'var(--color-text)',
    icon:      'warning',
  },
  critical: {
    bg:        'rgba(186,26,26,0.07)',
    border:    'rgba(186,26,26,0.2)',
    color:     '#ba1a1a',
    textColor: 'var(--color-text)',
    icon:      'error',
  },
}

const config = computed(() => typeConfigs[announcement.value?.type ?? 'info'])

// ── Role targeting ────────────────────────────────────────────────────────

const isTargeted = computed(() => {
  if (!announcement.value) return false
  const t = announcement.value.targetRoles
  if (!t || t === 'all') return true
  // Admin sees all announcements
  if (authStore.isAdmin) return true
  const targets = t.split(',')
  return targets.includes(authStore.sectionCategory)
})

// ── Countdown ticker ──────────────────────────────────────────────────────

const countdown = ref('')
let ticker = null

function updateCountdown() {
  if (!announcement.value?.expiresAt) { countdown.value = ''; return }
  const diff = new Date(announcement.value.expiresAt) - new Date()
  if (diff <= 0) { countdown.value = 'Expired'; store.dismiss(); return }

  const h = Math.floor(diff / 3600000)
  const m = Math.floor((diff % 3600000) / 60000)
  const s = Math.floor((diff % 60000) / 1000)

  if (h > 0)      countdown.value = `${h}h ${m}m remaining`
  else if (m > 0) countdown.value = `${m}m ${s}s remaining`
  else            countdown.value = `${s}s remaining`
}

onMounted(() => {
  updateCountdown()
  ticker = setInterval(updateCountdown, 1000)
})

onUnmounted(() => clearInterval(ticker))
</script>

<style scoped>
  .banner-enter-active {
    transition: opacity 0.25s ease, transform 0.25s ease;
  }

  .banner-leave-active {
    transition: opacity 0.2s ease, transform 0.2s ease;
  }

  .banner-enter-from {
    opacity: 0;
    transform: translateY(-6px);
  }

  .banner-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }
</style>
