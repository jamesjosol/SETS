<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">
        Dashboard
      </h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        {{ today }} · <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span>
      </p>
    </div>

    <!-- KPI Cards -->
    <div class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">

      <!-- Total Batches -->
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-primary);">clinical_notes</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest"
                style="color: var(--color-text-muted);">Today</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ summary.totalEndorsed }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter"
           style="color: var(--color-text-muted);">Total Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500"
             style="background-color: var(--color-primary);"></div>
      </div>

      <!-- Pending -->
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-warning-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-warning);">pending_actions</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest"
                style="color: var(--color-warning);">Awaiting</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-warning);">
          <span v-if="loading">—</span><span v-else>{{ summary.pending }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter"
           style="color: var(--color-text-muted);">Pending Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500"
             style="background-color: var(--color-warning);"></div>
      </div>

      <!-- Partial -->
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: rgba(37,99,235,0.08);">
            <span class="material-symbols-outlined" style="color: #2563eb;">move_to_inbox</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest"
                style="color: #2563eb;">In Progress</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: #2563eb;">
          <span v-if="loading">—</span><span v-else>{{ summary.outsideTAT }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter"
           style="color: var(--color-text-muted);">Partial Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500"
             style="background-color: #2563eb;"></div>
      </div>

      <!-- Completed -->
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-success-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-success);">inventory</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest"
                style="color: var(--color-success);">Done</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-success);">
          <span v-if="loading">—</span><span v-else>{{ summary.received }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter"
           style="color: var(--color-text-muted);">Completed Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500"
             style="background-color: var(--color-success);"></div>
      </div>

    </div>

    <!-- Monitoring Dashboard -->
    <div class="rounded-2xl overflow-hidden"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Header + Toggle -->
      <div class="px-8 py-5 flex items-center justify-between"
           style="border-bottom: 1px solid var(--color-surface-low);">
        <div>
          <h2 class="text-base font-bold" style="color: var(--color-text);">Monitoring Dashboard</h2>
          <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
            Batches endorsed today per section
          </p>
        </div>

        <!-- View Toggle -->
        <div class="flex items-center rounded-xl overflow-hidden"
             style="background-color: var(--color-surface-low);">
          <button class="px-4 py-2 text-xs font-bold uppercase tracking-widest transition-all"
                  :style="viewMode === 'pending'
                    ? 'background-color: var(--color-primary); color: #ffffff;'
                    : 'color: var(--color-text-muted);'"
                  @click="viewMode = 'pending'">
            View Pending
          </button>
          <button class="px-4 py-2 text-xs font-bold uppercase tracking-widest transition-all"
                  :style="viewMode === 'all'
                    ? 'background-color: var(--color-primary); color: #ffffff;'
                    : 'color: var(--color-text-muted);'"
                  @click="viewMode = 'all'">
            View All
          </button>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="monitoringLoading"
           class="px-8 py-16 flex items-center justify-center gap-3">
        <span class="material-symbols-outlined animate-spin"
              style="color: var(--color-text-muted);">progress_activity</span>
        <p class="text-xs font-bold uppercase tracking-widest"
           style="color: var(--color-text-muted);">Loading...</p>
      </div>

      <!-- Grid -->
      <div v-else class="p-6 overflow-x-auto">
        <div class="grid gap-4"
             :style="`grid-template-columns: repeat(${Math.min(visibleSections.length, 5)}, minmax(160px, 1fr));`">

          <div v-for="section in visibleSections"
               :key="section.sectionCode">

            <!-- Section Header -->
            <div class="rounded-xl px-4 py-2 mb-3 text-center"
                 style="background-color: var(--color-primary); ">
              <p class="text-[10px] font-bold uppercase tracking-widest text-white truncate">
                {{ section.sectionName }}
              </p>
            </div>

            <!-- Batch List -->
            <div class="space-y-1 min-h-16">
              <div v-if="getVisibleBatches(section).length === 0"
                   class="text-center py-4">
                <p class="text-[10px]" style="color: var(--color-text-muted);">—</p>
              </div>

              <div v-for="batch in getVisibleBatches(section)"
                   :key="batch.batchNo"
                   class="px-3 py-2 rounded-lg cursor-pointer transition-all hover:scale-[1.02]"
                   :style="getBatchCellStyle(batch)"
                   @click="openBatchDetail(batch)">
                <p class="text-xs font-bold font-mono">{{ batch.batchNo }}</p>
                <p class="text-[10px] mt-0.5 opacity-70">
                  {{ batch.receivedSpecimens }}/{{ batch.totalSpecimens }} specimens
                </p>
              </div>

            </div>
          </div>

        </div>
      </div>
    </div>

    <!-- Main Bottom Grid -->
    <div class="grid grid-cols-12 gap-6 mt-6">

      <div class="col-span-12 lg:col-span-8 grid grid-cols-2 gap-4">

        <!-- Daily Specimen Received — weekly bars -->
        <div class="rounded-2xl p-6"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-6"
              style="color: var(--color-text);">
            Daily Specimen Received
          </h2>

          <div v-if="flowLoading"
               class="h-36 flex items-center justify-center gap-3">
            <span class="material-symbols-outlined animate-spin"
                  style="color: var(--color-text-muted);">progress_activity</span>
          </div>

          <template v-else>
            <div class="h-36 flex items-end justify-between gap-2 px-2">
              <div v-for="(bar, i) in flowBars"
                   :key="i"
                   class="relative w-full h-full flex flex-col items-center justify-end group/bar">
                <span class="absolute -top-5 text-[10px] font-bold opacity-0 group-hover/bar:opacity-100 transition-opacity"
                      style="color: var(--color-primary);">{{ bar.count }}</span>
                <div class="w-full rounded-t-lg transition-all duration-500"
                     :style="`height: ${bar.height}%; background-color: ${bar.active ? 'var(--color-primary)' : 'var(--color-primary-soft)'};`"
                     @mouseenter="e => { if (!bar.active) e.currentTarget.style.opacity = '0.7' }"
                     @mouseleave="e => { if (!bar.active) e.currentTarget.style.opacity = '1' }">
                </div>
              </div>
            </div>
            <div class="flex justify-between mt-3">
              <span v-for="bar in flowBars"
                    :key="bar.day"
                    class="text-[10px] font-bold uppercase tracking-widest"
                    :style="bar.active ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
                {{ bar.day }}
              </span>
            </div>
          </template>
        </div>

        <!-- Hourly Breakdown — line chart -->
        <div class="rounded-2xl p-6"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-6"
              style="color: var(--color-text);">
            Today's Hourly Breakdown
          </h2>

          <div v-if="hourlyLoading"
               class="h-36 flex items-center justify-center gap-3">
            <span class="material-symbols-outlined animate-spin"
                  style="color: var(--color-text-muted);">progress_activity</span>
          </div>

          <template v-else>
            <!-- SVG Line Chart -->
            <div class="relative h-36">
              <svg class="w-full h-full overflow-visible" viewBox="0 0 300 120" preserveAspectRatio="none">

                <!-- Grid lines -->
                <line v-for="i in 4" :key="i"
                      x1="0" :y1="(i / 4) * 100"
                      x2="300" :y2="(i / 4) * 100"
                      stroke="var(--color-surface-low)"
                      stroke-width="1" />

                <!-- Area fill -->
                <path :d="areaPath"
                      fill="var(--color-primary)"
                      opacity="0.08" />

                <!-- Line -->
                <path :d="linePath"
                      fill="none"
                      stroke="var(--color-primary)"
                      stroke-width="1.5"
                      stroke-linecap="round"
                      stroke-linejoin="round" />

                <!-- Pass 1: Dots only -->
                <circle v-for="(point, i) in chartPoints"
                        :key="`dot-${i}`"
                        :cx="point.x"
                        :cy="point.y"
                        r="2"
                        :fill="hourlyFlow[i]?.isCurrent ? 'var(--color-primary)' : 'var(--color-surface)'"
                        stroke="var(--color-primary)"
                        stroke-width="1.5" />

                <!-- Pass 2: Tooltip triggers on top of everything -->
                <g v-for="(point, i) in chartPoints"
                   :key="`tip-${i}`"
                   class="group/tip"
                   style="cursor: pointer;">

                  <!-- Invisible hit area -->
                  <circle :cx="point.x"
                          :cy="point.y"
                          r="8"
                          fill="transparent" />

                  <!-- Tooltip — rendered on top, hidden by default -->
                  <g class="opacity-0 group-hover/tip:opacity-100 transition-opacity">
                    <rect :x="point.x - 18"
                          :y="point.y - 28"
                          width="36"
                          height="18"
                          rx="4"
                          fill="var(--color-primary)" />
                    <text :x="point.x"
                          :y="point.y - 15"
                          text-anchor="middle"
                          font-size="8"
                          font-weight="bold"
                          fill="white">
                      {{ hourlyFlow[i]?.count }}
                    </text>
                  </g>

                </g>

              </svg>
            </div>

            <!-- Hour labels — show every 3 hours to avoid crowding -->
            <div class="flex justify-between mt-2 px-1">
              <span v-for="(bar, i) in hourlyFlow"
                    :key="bar.hour"
                    class="text-[9px] font-bold"
                    :style="bar.isCurrent
                ? 'color: var(--color-primary); font-weight: 800;'
                : 'color: var(--color-text-muted);'">
                {{ i % 3 === 0 ? bar.hour : '' }}
              </span>
            </div>
          </template>

        </div>

      </div>

      <!-- System Status -->
      <div class="col-span-12 lg:col-span-4">
        <div class="rounded-2xl p-6"
             style="background-color: var(--color-surface-low); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-5"
              style="color: var(--color-text);">
            System Status
          </h2>
          <div class="space-y-3">
            <div v-for="status in systemStatus"
                 :key="status.label"
                 class="flex items-center justify-between p-3 rounded-xl"
                 style="background-color: var(--color-surface);">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-lg"
                      :style="`color: ${status.iconColor}`">{{ status.icon }}</span>
                <span class="text-xs font-bold"
                      style="color: var(--color-text-muted);">{{ status.label }}</span>
              </div>
              <div class="flex flex-col items-end">
                <span class="text-[10px] font-bold px-2 py-0.5 rounded uppercase"
                      :style="getStatusBadgeStyle(status.state)">{{ status.state }}</span>
                <span v-if="status.note"
                      class="text-[8px] mt-0.5"
                      style="color: var(--color-text-muted);">{{ status.note }}</span>
              </div>
            </div>
          </div>
          <div class="mt-5 pt-4 flex justify-between text-[10px] font-bold uppercase tracking-widest"
               style="border-top: 1px solid var(--color-border); color: var(--color-text-muted);">
            <span>Last Global Sync</span>
            <span>2 mins ago</span>
          </div>
        </div>
      </div>

    </div>

    <!-- ── Batch Detail Drawer ─────────────────────────────────────────── -->
    <BatchDetailDrawer :isOpen="drawerOpen"
                       :loading="drawerLoading"
                       :data="drawerData"
                       @close="closeDrawer" />
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
import BatchDetailDrawer from '@/components/common/BatchDetailDrawer.vue'
import { useAuthStore } from '@/stores/authStore'
import { receivingApi } from '@/api/receivingApi'
import { batchApi } from '@/api/batchApi'

const authStore = useAuthStore()

// ── Alert ──────────────────────────────────────────────────────────────────

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

// ── Date ───────────────────────────────────────────────────────────────────

const today = computed(() =>
  new Date().toLocaleDateString('en-US', {
    weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
  })
)

// ── KPI Summary ────────────────────────────────────────────────────────────

const loading = ref(true)
const summary = ref({ totalEndorsed: 0, pending: 0, received: 0, outsideTAT: 0 })

// ── Monitoring ─────────────────────────────────────────────────────────────

const monitoringLoading = ref(true)
const monitoringData = ref([])
const viewMode = ref('pending')  // 'pending' | 'all'

const visibleSections = computed(() => {
  if (viewMode.value === 'all') return monitoringData.value

  // In pending mode, only show sections that have at least one non-completed batch
  return monitoringData.value.filter(section =>
    section.batches.some(b => b.status !== 'C')
  )
})

function getVisibleBatches(section) {
  if (viewMode.value === 'all') return section.batches
  return section.batches.filter(b => b.status !== 'C')
}

function getBatchCellStyle(batch) {
  if (batch.status === 'C') {
    return 'background-color: var(--color-surface-low); color: var(--color-text-muted); opacity: 0.5;'
  }
  if (batch.status === 'PA') {
    return 'background-color: rgba(37,99,235,0.08); color: #2563eb;'
  }
  // P — pending, no specimen received
  return 'background-color: var(--color-surface-low); color: var(--color-text);'
}

// ── Drawer ─────────────────────────────────────────────────────────────────

const drawerOpen = ref(false)
const drawerLoading = ref(false)
const drawerData = ref(null)
const drawerTab = ref('Specimens')

async function openBatchDetail(batch) {
  drawerOpen.value = true
  drawerLoading.value = true
  drawerTab.value = 'Specimens'
  drawerData.value = null
  try {
    drawerData.value = await batchApi.getBatchDetail(batch.batchNo)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load batch details.')
    }
  } finally {
    drawerLoading.value = false
  }
}

function closeDrawer() {
  drawerOpen.value = false
  drawerData.value = null
}

// ── Fetch ──────────────────────────────────────────────────────────────────

onMounted(async () => {
  try {
    summary.value = await receivingApi.getDashboardSummary(authStore.sectionCode)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load summary.')
    }
  } finally {
    loading.value = false
  }

  try {
    monitoringData.value = await receivingApi.getMonitoringDashboard(authStore.sectionCode)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load monitoring data.')
    }
  } finally {
    monitoringLoading.value = false
  }

  try {
    weeklyFlow.value = await receivingApi.getWeeklyFlow(authStore.sectionCode)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load weekly flow.')
    }
  } finally {
    flowLoading.value = false
  }

  try {
    hourlyFlow.value = await receivingApi.getHourlyFlow(authStore.sectionCode)
  } catch (err) {
    if (err.response?.status === 401) {
      showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
    } else {
      showAlert('error', 'Error', 'Unable to load hourly flow.')
    }
  } finally {
    hourlyLoading.value = false
  }

})

// ── Helpers ────────────────────────────────────────────────────────────────

function formatDateTime(dt) {
  if (!dt) return '—'
  return new Date(dt).toLocaleString('en-US', {
    month: 'short', day: 'numeric',
    hour: '2-digit', minute: '2-digit', hour12: true
  })
}

function getBatchStatusLabel(status) {
  const map = { P: 'Pending', PA: 'Partial', C: 'Completed' }
  return map[status] ?? status
}

function getBatchStatusStyle(status) {
  const map = {
    P:  'background-color: var(--color-warning-soft); color: var(--color-warning);',
    PA: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
    C:  'background-color: var(--color-success-soft); color: var(--color-success);',
  }
  return map[status] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
}

function getBatchStatusDot(status) {
  const map = { P: 'var(--color-warning)', PA: '#2563eb', C: 'var(--color-success)' }
  return map[status] ?? 'var(--color-text-muted)'
}

  // ── Weekly Flow ────────────────────────────────────────────────────────────

  const flowLoading = ref(true)
  const weeklyFlow = ref([])

  const flowBars = computed(() => {
    if (weeklyFlow.value.length === 0) return []
    const max = Math.max(...weeklyFlow.value.map(d => d.count), 1)
    return weeklyFlow.value.map(d => ({
      height: Math.max(Math.round((d.count / max) * 100), 4),
      active: d.isToday,
      count: d.count,
      day: d.day,
    }))
  })

  // ── Hourly Flow ────────────────────────────────────────────────────────────

  const hourlyLoading = ref(true)
  const hourlyFlow = ref([])

  const chartPoints = computed(() => {
    if (hourlyFlow.value.length === 0) return []

    const max = Math.max(...hourlyFlow.value.map(d => d.count), 1)
    const total = hourlyFlow.value.length
    const padX = 10  // small left/right padding inside SVG

    return hourlyFlow.value.map((d, i) => ({
      x: padX + (i / (total - 1)) * (300 - padX * 2),
      y: 100 - Math.round((d.count / max) * 90)  // 90 = usable height, leaves top margin
    }))
  })

  const linePath = computed(() => {
    if (chartPoints.value.length === 0) return ''
    return chartPoints.value
      .map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.x} ${p.y}`)
      .join(' ')
  })

  const areaPath = computed(() => {
    if (chartPoints.value.length === 0) return ''
    const points = chartPoints.value
    const first = points[0]
    const last = points[points.length - 1]
    return `${linePath.value} L ${last.x} 100 L ${first.x} 100 Z`
  })

  // ── System Status ──────────────────────────────────────────────────────────

  const systemStatus = [
    { label: 'HCLAB Connectivity', icon: 'router', iconColor: '#059669', state: 'Online', note: null },
    { label: 'SETS Database', icon: 'database', iconColor: '#059669', state: 'Online', note: null },
    { label: 'Endorsement API', icon: 'api', iconColor: '#d97706', state: 'Delayed', note: '140ms latency' },
  ]

  function getStatusBadgeStyle(state) {
    const map = {
      Online: 'background-color: var(--color-success-soft); color: var(--color-success);',
      Delayed: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      Offline: 'background-color: var(--color-error-soft); color: var(--color-error);',
    }
    return map[state] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }


</script>

<style scoped>
  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.25s ease;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }

  .slide-enter-active, .slide-leave-active {
    transition: transform 0.3s ease;
  }

  .slide-enter-from, .slide-leave-to {
    transform: translateX(100%);
  }
</style>
