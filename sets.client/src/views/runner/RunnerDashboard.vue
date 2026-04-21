<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Dashboard</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
      <span class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ today }}</span>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
      <div v-for="card in kpiCards" :key="card.label"
           class="rounded-2xl p-5 flex flex-col gap-2"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex items-center justify-between">
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ card.label }}</span>
          <div class="p-1.5 rounded-lg" :style="{ backgroundColor: card.iconBg }">
            <span class="material-symbols-outlined text-sm" :style="{ color: card.iconColor }">{{ card.icon }}</span>
          </div>
        </div>
        <div v-if="loading" class="h-8 rounded-lg animate-pulse" style="background-color: var(--color-surface-low);"></div>
        <span v-else class="text-3xl font-black tracking-tight" :style="{ color: card.valueColor || 'var(--color-text)' }">
          {{ card.value }}
        </span>
      </div>
    </div>

    <!-- Recent Activity -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
        <div class="flex items-center gap-2">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">history</span>
          <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Recent Specimens</h2>
        </div>
        <router-link to="/runner/pending"
                     class="text-xs font-bold uppercase tracking-widest transition-all"
                     style="color: var(--color-primary);">
          View All →
        </router-link>
      </div>

      <div v-if="loading" class="p-6 flex flex-col gap-3">
        <div v-for="i in 4" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
      </div>

      <div v-else-if="!recentSpecimens.length" class="p-12 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
        <p class="text-sm font-medium" style="color: var(--color-text-muted);">No specimens today</p>
      </div>

      <div v-else class="divide-y" style="--tw-divide-color: var(--color-border);">
        <div v-for="item in recentSpecimens" :key="item.id"
             class="px-6 py-4 flex items-center justify-between gap-4">
          <div class="flex items-center gap-3 min-w-0">
            <div class="p-2 rounded-xl flex-shrink-0" style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">biotech</span>
            </div>
            <div class="min-w-0">
              <p class="text-sm font-bold truncate" style="color: var(--color-text);">{{ item.specimenNo }}</p>
              <p class="text-xs truncate" style="color: var(--color-text-muted);">{{ item.patientName }}</p>
            </div>
          </div>
          <div class="flex items-center gap-3 flex-shrink-0">
            <span class="text-xs font-bold" style="color: var(--color-text-muted);">{{ item.testGroupCode }}</span>
            <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                  :style="statusStyle(item.status)">
              {{ statusLabel(item.status) }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Alert Modal -->
    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />
  </AppLayout>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import { useAuthStore } from '@/stores/authStore'
import { runnerApi } from '@/api/runnerApi'

const authStore = useAuthStore()

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

const today = computed(() =>
  new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)

const loading = ref(true)
const recentSpecimens = ref([])
const summary = ref({ pending: 0, saved: 0, running: 0, completed: 0 })

const kpiCards = computed(() => [
  {
    label: 'Pending',
    value: summary.value.pending,
    icon: 'pending_actions',
    iconBg: 'rgba(70,21,153,0.1)',
    iconColor: 'var(--color-primary)',
  },
  {
    label: 'Saved',
    value: summary.value.saved,
    icon: 'bookmark',
    iconBg: 'rgba(74,98,109,0.1)',
    iconColor: 'var(--color-info, #4a626d)',
  },
  {
    label: 'Running',
    value: summary.value.running,
    icon: 'science',
    iconBg: 'rgba(217,119,6,0.1)',
    iconColor: 'var(--color-warning)',
  },
  {
    label: 'Completed',
    value: summary.value.completed,
    icon: 'check_circle',
    iconBg: 'rgba(22,163,74,0.1)',
    iconColor: 'var(--color-success, #16a34a)',
  },
])

function statusLabel(s) {
  return { P: 'Pending', S: 'Saved', R: 'Running', C: 'Completed' }[s] ?? s
}

function statusStyle(s) {
  const map = {
    P: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
    S: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
    R: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
    C: 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);',
  }
  return map[s] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
}

async function load() {
  loading.value = true
  try {
    const data = await runnerApi.getPendingSpecimens(authStore.sectionCode)
    recentSpecimens.value = Array.isArray(data) ? data.slice(0, 8) : []

    // Derive summary counts from pending list + you can extend with a dedicated summary endpoint later
    summary.value.pending = Array.isArray(data) ? data.filter(d => d.status === 'P').length : 0
    summary.value.saved   = Array.isArray(data) ? data.filter(d => d.status === 'S').length : 0
  } catch (e) {
    showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load dashboard.')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
