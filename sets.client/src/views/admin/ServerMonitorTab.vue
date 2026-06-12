<script setup>
import { ref, computed, onMounted, onBeforeUnmount } from 'vue'
import { monitorApi } from '@/api/monitorApi'

const emit = defineEmits(['toast'])

// ── State ──────────────────────────────────────────────────────────────────

const snapshot = ref(null)
const loading  = ref(true)   // first load only — polls after that are silent
const offline  = ref(false)

const POLL_MS = 3000
let timer = null

// ── Polling ────────────────────────────────────────────────────────────────

async function load() {
  try {
    const data = await monitorApi.getSets()

    if (!data || data.available === false) {
      offline.value = true
    } else {
      offline.value = false
      snapshot.value = data
    }
  } catch {
    offline.value = true
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  load()
  // Silent refresh — skip ticks while the browser tab is hidden
  timer = setInterval(() => {
    if (!document.hidden) load()
  }, POLL_MS)
})

onBeforeUnmount(() => clearInterval(timer))

// ── Derived ────────────────────────────────────────────────────────────────

const history  = computed(() => snapshot.value?.history ?? [])
const current  = computed(() => snapshot.value?.current ?? null)
const requests = computed(() => snapshot.value?.requests ?? [])

const isRunning = computed(() => snapshot.value?.status === 'running')

const siteStateConfig = computed(() => {
  const state = snapshot.value?.siteState ?? 'unknown'
  if (state === 'started') return { label: 'STARTED', color: '#16a34a', bg: 'rgba(22,163,74,0.10)' }
  if (state === 'stopped') return { label: 'STOPPED', color: '#dc2626', bg: 'rgba(220,38,38,0.10)' }
  return { label: state.toUpperCase(), color: 'var(--color-text-muted)', bg: 'var(--color-surface-low)' }
})

// ── Charts (inline SVG, Task Manager style) ────────────────────────────────

const CHART_W = 300
const CHART_H = 70

function buildPaths(values, max) {
  if (!values || values.length < 2) return { line: '', area: '' }

  const m = Math.max(max, 1)
  const stepX = CHART_W / (values.length - 1)

  const pts = values.map((v, i) => {
    const x = (i * stepX).toFixed(1)
    const y = (CHART_H - Math.min((v ?? 0) / m, 1) * CHART_H).toFixed(1)
    return `${x},${y}`
  })

  return {
    line: 'M' + pts.join(' L'),
    area: `M0,${CHART_H} L` + pts.join(' L') + ` L${CHART_W},${CHART_H} Z`,
  }
}

// CPU — fixed 0–100 scale
const cpuChart = computed(() => buildPaths(history.value.map(s => s.cpu), 100))

// Memory — dynamic scale, floor 512 MB so a flat line doesn't fill the chart
const memMax = computed(() =>
  Math.max(...history.value.map(s => s.workingSet), 512 * 1024 * 1024) * 1.15)
const memChart = computed(() => buildPaths(history.value.map(s => s.workingSet), memMax.value))

// Network — shared scale for sent/recv, floor 10 KB/s
const netMax = computed(() =>
  Math.max(...history.value.map(s => Math.max(s.netSent, s.netRecv)), 10 * 1024) * 1.15)
const netSentChart = computed(() => buildPaths(history.value.map(s => s.netSent), netMax.value))
const netRecvChart = computed(() => buildPaths(history.value.map(s => s.netRecv), netMax.value))

// Disk I/O — shared scale for read/write, floor 10 KB/s
const ioMax = computed(() =>
  Math.max(...history.value.map(s => Math.max(s.ioRead, s.ioWrite)), 10 * 1024) * 1.15)
const ioReadChart  = computed(() => buildPaths(history.value.map(s => s.ioRead), ioMax.value))
const ioWriteChart = computed(() => buildPaths(history.value.map(s => s.ioWrite), ioMax.value))

const timeStart = computed(() => history.value[0]?.t ?? '')
const timeEnd   = computed(() => history.value[history.value.length - 1]?.t ?? '')

// ── Formatting helpers ─────────────────────────────────────────────────────

function fmtBytes(b) {
  if (b == null) return '—'
  if (b < 1024) return `${Math.round(b)} B`
  if (b < 1024 ** 2) return `${(b / 1024).toFixed(1)} KB`
  if (b < 1024 ** 3) return `${(b / 1024 ** 2).toFixed(1)} MB`
  return `${(b / 1024 ** 3).toFixed(2)} GB`
}

const fmtRate = b => `${fmtBytes(b)}/s`

function fmtUptime(s) {
  if (s == null) return '—'
  const d = Math.floor(s / 86400)
  const h = Math.floor((s % 86400) / 3600)
  const m = Math.floor((s % 3600) / 60)
  if (d > 0) return `${d}d ${h}h ${m}m`
  if (h > 0) return `${h}h ${m}m`
  return `${m}m`
}

function fmtElapsed(ms) {
  const n = Number(ms)
  if (Number.isNaN(n)) return ms || '—'
  if (n < 1000) return `${n} ms`
  return `${(n / 1000).toFixed(1)} s`
}
</script>

<template>
  <div class="space-y-4">

    <!-- ── Initial loading ──────────────────────────────────────────────── -->
    <div v-if="loading"
         class="rounded-2xl px-8 py-12 text-center"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <span class="material-symbols-outlined animate-spin text-2xl"
            style="color: var(--color-text-muted);">progress_activity</span>
    </div>

    <!-- ── Middleware offline ───────────────────────────────────────────── -->
    <div v-else-if="offline"
         class="rounded-2xl px-8 py-12 text-center"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <span class="material-symbols-outlined text-3xl mb-2 block"
            style="color: #d97706;">cloud_off</span>
      <p class="text-sm font-bold" style="color: var(--color-text);">Middleware Offline</p>
      <p class="text-xs mt-1" style="color: var(--color-text-muted);">
        SETSMiddleware is not reachable on this server. Monitoring resumes automatically once it's running.
      </p>
    </div>

    <!-- ── Dashboard ────────────────────────────────────────────────────── -->
    <template v-else-if="snapshot">

      <!-- Status bar -->
      <div class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center justify-between gap-4 flex-wrap">
          <div class="flex items-center gap-4">
            <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
                 style="background-color: var(--color-primary-soft);">
              <span class="material-symbols-outlined text-base"
                    style="color: var(--color-primary); font-variation-settings: 'FILL' 1;">
                monitor_heart
              </span>
            </div>
            <div>
              <h2 class="text-base font-extrabold tracking-tight flex items-center gap-2"
                  style="color: var(--color-text);">
                SETS Server Monitor
                <span class="relative flex h-2 w-2">
                  <span class="animate-ping absolute inline-flex h-full w-full rounded-full opacity-60"
                        :style="`background-color: ${isRunning ? '#16a34a' : '#d97706'};`"></span>
                  <span class="relative inline-flex rounded-full h-2 w-2"
                        :style="`background-color: ${isRunning ? '#16a34a' : '#d97706'};`"></span>
                </span>
              </h2>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                App pool <span class="font-mono font-bold">{{ snapshot.appPool }}</span>
                · sampled every {{ snapshot.sampleSeconds }}s · last sample {{ snapshot.sampledAt }}
              </p>
            </div>
          </div>

          <!-- Chips -->
          <div class="flex items-center gap-2 flex-wrap">
            <span class="px-2.5 py-1 rounded-full text-[10px] font-extrabold tracking-widest"
                  :style="`background-color: ${siteStateConfig.bg}; color: ${siteStateConfig.color};`">
              SITE {{ siteStateConfig.label }}
            </span>
            <span class="px-2.5 py-1 rounded-full text-[10px] font-bold tracking-widest"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
              {{ isRunning ? `PID ${snapshot.pid}` : 'NO WORKER PROCESS' }}
            </span>
            <span v-if="isRunning"
                  class="px-2.5 py-1 rounded-full text-[10px] font-bold tracking-widest"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
              UP {{ fmtUptime(snapshot.uptimeSeconds) }}
            </span>
            <span v-if="current"
                  class="px-2.5 py-1 rounded-full text-[10px] font-bold tracking-widest"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);">
              {{ current.threads }} THREADS · {{ current.handles }} HANDLES
            </span>
          </div>
        </div>
      </div>

      <!-- Chart grid -->
      <div v-if="current" class="grid grid-cols-2 gap-4">

        <!-- CPU -->
        <div class="rounded-2xl px-6 py-5"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="flex items-baseline justify-between mb-3">
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">CPU</p>
            <p class="text-xl font-extrabold tabular-nums" style="color: var(--color-text);">
              {{ current.cpu }}<span class="text-xs font-bold" style="color: var(--color-text-muted);">%</span>
            </p>
          </div>
          <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" preserveAspectRatio="none"
               class="w-full" style="height: 70px;">
            <path :d="cpuChart.area" fill="var(--color-primary)" fill-opacity="0.12" />
            <path :d="cpuChart.line" fill="none" stroke="var(--color-primary)" stroke-width="1.5" />
          </svg>
          <div class="flex justify-between mt-1">
            <span class="text-[9px] font-mono" style="color: var(--color-text-muted);">{{ timeStart }}</span>
            <span class="text-[9px] font-mono" style="color: var(--color-text-muted);">{{ timeEnd }}</span>
          </div>
        </div>

        <!-- Memory -->
        <div class="rounded-2xl px-6 py-5"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="flex items-baseline justify-between mb-3">
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Memory · Working Set</p>
            <p class="text-xl font-extrabold tabular-nums" style="color: var(--color-text);">
              {{ fmtBytes(current.workingSet) }}
            </p>
          </div>
          <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" preserveAspectRatio="none"
               class="w-full" style="height: 70px;">
            <path :d="memChart.area" fill="#2563eb" fill-opacity="0.12" />
            <path :d="memChart.line" fill="none" stroke="#2563eb" stroke-width="1.5" />
          </svg>
          <div class="flex justify-between mt-1">
            <span class="text-[9px] font-mono" style="color: var(--color-text-muted);">{{ timeStart }}</span>
            <span class="text-[9px]" style="color: var(--color-text-muted);">
              private: {{ fmtBytes(current.privateBytes) }}
            </span>
            <span class="text-[9px] font-mono" style="color: var(--color-text-muted);">{{ timeEnd }}</span>
          </div>
        </div>

        <!-- Network -->
        <div class="rounded-2xl px-6 py-5"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="flex items-baseline justify-between mb-3">
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Network · SETS site</p>
            <p class="text-xs font-bold tabular-nums" style="color: var(--color-text);">
              {{ current.connections }} conn · {{ current.requestsPerSec }} req/s
            </p>
          </div>
          <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" preserveAspectRatio="none"
               class="w-full" style="height: 70px;">
            <path :d="netSentChart.area" fill="#16a34a" fill-opacity="0.10" />
            <path :d="netSentChart.line" fill="none" stroke="#16a34a" stroke-width="1.5" />
            <path :d="netRecvChart.line" fill="none" stroke="#d97706" stroke-width="1.5" />
          </svg>
          <div class="flex justify-between mt-1 text-[9px]" style="color: var(--color-text-muted);">
            <span><span style="color:#16a34a;">●</span> sent {{ fmtRate(current.netSent) }}</span>
            <span><span style="color:#d97706;">●</span> recv {{ fmtRate(current.netRecv) }}</span>
          </div>
        </div>

        <!-- Disk I/O -->
        <div class="rounded-2xl px-6 py-5"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="flex items-baseline justify-between mb-3">
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Disk I/O · w3wp</p>
            <p class="text-xs font-bold tabular-nums" style="color: var(--color-text);">
              {{ fmtRate(current.ioRead) }} R · {{ fmtRate(current.ioWrite) }} W
            </p>
          </div>
          <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" preserveAspectRatio="none"
               class="w-full" style="height: 70px;">
            <path :d="ioReadChart.area" fill="#2563eb" fill-opacity="0.10" />
            <path :d="ioReadChart.line" fill="none" stroke="#2563eb" stroke-width="1.5" />
            <path :d="ioWriteChart.line" fill="none" stroke="#dc2626" stroke-width="1.5" />
          </svg>
          <div class="flex justify-between mt-1 text-[9px]" style="color: var(--color-text-muted);">
            <span><span style="color:#2563eb;">●</span> read</span>
            <span><span style="color:#dc2626;">●</span> write</span>
          </div>
        </div>
      </div>

      <!-- Active requests -->
      <div class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center gap-4"
             style="border-bottom: 1px solid var(--color-border);">
          <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined text-base"
                  style="color: var(--color-primary);">swap_vert</span>
          </div>
          <div>
            <h2 class="text-base font-extrabold tracking-tight" style="color: var(--color-text);">
              In-Flight Requests
            </h2>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
              {{ requests.length }} request{{ requests.length !== 1 ? 's' : '' }} executing right now inside the worker process
            </p>
          </div>
        </div>

        <!-- Empty -->
        <div v-if="requests.length === 0" class="px-8 py-8 text-center">
          <p class="text-xs" style="color: var(--color-text-muted);">
            No requests in flight — responses are completing faster than the snapshot interval.
          </p>
        </div>

        <!-- Table -->
        <table v-else class="w-full text-xs">
          <thead>
            <tr style="border-bottom: 1px solid var(--color-border);">
              <th class="px-8 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Verb</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">URL</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Elapsed</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Stage</th>
              <th class="px-8 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Module</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(r, i) in requests" :key="i"
                style="border-bottom: 1px solid var(--color-border);">
              <td class="px-8 py-3 font-mono font-bold" style="color: var(--color-primary);">{{ r.verb }}</td>
              <td class="px-4 py-3 font-mono max-w-md truncate" style="color: var(--color-text);"
                  :title="r.url">
                {{ r.url }}
              </td>
              <td class="px-4 py-3 tabular-nums" style="color: var(--color-text);">{{ fmtElapsed(r.timeElapsedMs) }}</td>
              <td class="px-4 py-3" style="color: var(--color-text-muted);">{{ r.stage || '—' }}</td>
              <td class="px-8 py-3" style="color: var(--color-text-muted);">{{ r.module || '—' }}</td>
            </tr>
          </tbody>
        </table>
      </div>

    </template>
  </div>
</template>
