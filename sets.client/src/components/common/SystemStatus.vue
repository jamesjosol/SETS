<template>
  <div class="rounded-2xl p-6"
       style="background-color: var(--color-surface-low); box-shadow: 0 1px 3px var(--color-shadow);">

    <h2 class="text-xs font-bold uppercase tracking-widest mb-5" style="color: var(--color-text);">
      System Status
    </h2>

    <div class="space-y-3 overflow-visible">
      <div v-for="status in systemStatus" :key="status.label"
           class="flex items-center justify-between p-3 rounded-xl relative group"
           style="background-color: var(--color-surface);">

        <!-- Left: icon + label -->
        <div class="flex items-center gap-3">
          <span class="material-symbols-outlined text-lg" :style="`color: ${status.iconColor}`">
            {{ status.icon }}
          </span>
          <span class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ status.label }}
          </span>
        </div>

        <!-- Right: badge + latency note -->
        <div class="flex flex-col items-end">
          <span class="text-[10px] font-bold px-2 py-0.5 rounded uppercase"
                :style="getBadgeStyle(status.state)">
            {{ status.state }}
          </span>
          <span v-if="status.note" class="text-[8px] mt-0.5" style="color: var(--color-text-muted);">
            {{ status.note }}
          </span>
        </div>

        <!-- Hover tooltip — middleware task detail only -->
        <div v-if="status.tasks && status.tasks.length > 0"
             class="absolute right-0 top-full mt-1 z-50 min-w-56 rounded-xl p-3 shadow-xl pointer-events-none
                    opacity-0 group-hover:opacity-100 transition-opacity duration-200"
             style="background-color: var(--color-surface-high); border: 1px solid var(--color-border);">
          <p class="text-[9px] font-bold uppercase tracking-widest mb-2"
             style="color: var(--color-text-muted);">Tasks</p>
          <div v-for="task in status.tasks" :key="task.name" class="flex items-start gap-2 py-1">
            <span class="w-1.5 h-1.5 rounded-full flex-shrink-0 mt-1"
                  :style="task.running
                    ? 'background-color: var(--color-success)'
                    : 'background-color: var(--color-error)'">
            </span>
            <div class="min-w-0">
              <p class="text-[10px] font-bold truncate" style="color: var(--color-text);">
                {{ task.name }}
              </p>
              <p class="text-[9px] truncate" style="color: var(--color-text-muted);">
                {{ task.lastRun }}
              </p>
            </div>
          </div>
        </div>

      </div>
    </div>

    <!-- Footer: last checked -->
    <div class="mt-5 pt-4 flex justify-between text-[10px] font-bold uppercase tracking-widest"
         style="border-top: 1px solid var(--color-border); color: var(--color-text-muted);">
      <span>Last Status Check</span>
      <span>{{ lastStatusCheck }}</span>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { healthApi } from '@/api/healthApi'

// ── State ──────────────────────────────────────────────────────────────────

const lastStatusCheck = ref('—')

const systemStatus = ref([
  { label: 'HCLAB Connectivity', icon: 'router',          iconColor: 'var(--color-text-muted)', state: 'Checking', note: null, tasks: null },
  { label: 'SETS Database',      icon: 'database',        iconColor: 'var(--color-text-muted)', state: 'Checking', note: null, tasks: null },
  { label: 'SETS Host',          icon: 'dns',             iconColor: 'var(--color-text-muted)', state: 'Checking', note: null, tasks: null },
  { label: 'SETS Middleware',    icon: 'settings_suggest', iconColor: 'var(--color-text-muted)', state: 'Checking', note: null, tasks: []   },
])

// ── Helpers ────────────────────────────────────────────────────────────────

function getBadgeStyle(state) {
  const map = {
    'Online':       'background-color: var(--color-success-soft); color: var(--color-success);',
    'Slight Delay': 'background-color: rgba(202,138,4,0.1); color: #ca8a04;',
    'Delay':        'background-color: var(--color-warning-soft); color: var(--color-warning);',
    'Severe Delay': 'background-color: rgba(234,88,12,0.1); color: #ea580c;',
    'Offline':      'background-color: var(--color-error-soft); color: var(--color-error);',
    'Checking':     'background-color: var(--color-surface-low); color: var(--color-text-muted);',
  }
  return map[state] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
}

function applyState(index, online, latencyMs = 0) {
  const item = systemStatus.value[index]
  if (online) {
    if      (latencyMs >= 200) { item.state = 'Severe Delay'; item.iconColor = '#ea580c' }
    else if (latencyMs >= 100) { item.state = 'Delay';        item.iconColor = '#d97706' }
    else if (latencyMs >= 50)  { item.state = 'Slight Delay'; item.iconColor = '#ca8a04' }
    else                       { item.state = 'Online';       item.iconColor = '#059669' }
    item.note = latencyMs > 0 ? `${latencyMs}ms` : null
  } else {
    item.state     = 'Offline'
    item.iconColor = 'var(--color-error)'
    item.note      = null
  }
}

function setOffline(index) {
  systemStatus.value[index].state     = 'Offline'
  systemStatus.value[index].iconColor = 'var(--color-error)'
  systemStatus.value[index].note      = null
}

// ── Fetch ──────────────────────────────────────────────────────────────────

async function fetchSystemStatus() {
  // [2] SETS Host + [1] SETS Database — single request
  try {
    const { hostLatencyMs, db } = await healthApi.ping()
    applyState(2, true, hostLatencyMs)
    applyState(1, db.online, db.latencyMs)
  } catch {
    setOffline(2)
    setOffline(1)
  }

  // [0] HCLAB Connectivity
  try {
    const hclab = await healthApi.hclab()
    applyState(0, hclab.online, hclab.latencyMs)
  } catch {
    setOffline(0)
  }

  // [3] SETS Middleware
  try {
    const mw = await healthApi.middleware()
    systemStatus.value[3].tasks = mw.tasks ?? []
    applyState(3, mw.online, 0)
  } catch {
    setOffline(3)
    systemStatus.value[3].tasks = []
  }

  lastStatusCheck.value = new Date().toLocaleTimeString('en-US', {
    hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true
  })
}

// ── Lifecycle ──────────────────────────────────────────────────────────────

let statusInterval = null

onMounted(async () => {
  await fetchSystemStatus()
  statusInterval = setInterval(fetchSystemStatus, 30000)
})

onUnmounted(() => {
  clearInterval(statusInterval)
})
</script>
