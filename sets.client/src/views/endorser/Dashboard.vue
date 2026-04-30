<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Dashboard</h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        {{ today }} · <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span>
      </p>
    </div>

    <!-- KPI Cards — Regular / Team Lead -->
    <div v-if="!authStore.isAdmin" class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-primary);">clinical_notes</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Today</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ summary.totalEndorsed }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Total Batches Endorsed</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-primary);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-warning-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-warning);">pending_actions</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-warning);">Awaiting</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-warning);">
          <span v-if="loading">—</span><span v-else>{{ summary.pending }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Pending Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-warning);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-success-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-success);">inventory</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-success);">Completed</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ summary.received }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Received Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-success);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-error-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-error);">timer_off</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-error);">TAT</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-error);">
          <span v-if="loading">—</span><span v-else>{{ summary.outsideTAT }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Outside TAT</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-error);"></div>
      </div>
    </div>

    <!-- KPI Cards — Admin -->
    <div v-else class="mb-8">
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div v-for="n in 4" :key="n" class="rounded-2xl p-6 animate-pulse"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow); height: 130px;"></div>
      </div>
      <div v-else>
        <div v-for="group in adminGroups" :key="group.category" class="mb-6">
          <div class="flex items-center gap-2 mb-3">
            <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">{{ group.icon }}</span>
            <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ group.label }}</p>
          </div>
          <div class="grid gap-4" :style="`grid-template-columns: repeat(${Math.min(group.sections.length, 4)}, minmax(0, 1fr));`">
            <div v-for="sec in group.sections" :key="sec.sectionCode"
                 class="rounded-2xl p-5 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
                 style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-3 truncate" style="color: var(--color-primary);">{{ sec.sectionName }}</p>
              <div class="grid grid-cols-4 gap-2">
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-text);">{{ sec.totalEndorsed }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Total</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-warning);">{{ sec.pending }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Pending</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-success);">{{ sec.received }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Received</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-error);">{{ sec.outsideTAT }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Outside TAT</p>
                </div>
              </div>
              <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-primary);"></div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <!-- TAT Countdown Bar — Endorser only -->
    <div v-if="!authStore.isAdmin && tatCycle.hasOpenCycle"
         class="mb-6 flex items-center gap-4 px-5 py-3 rounded-xl"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Icon + Label -->
      <div class="flex items-center gap-2 flex-shrink-0">
        <span class="material-symbols-outlined text-base"
              :class="tatExceeded ? 'animate-pulse' : ''"
              :style="tatColorStyle">
          {{ tatExceeded ? 'timer_off' : 'timer' }}
        </span>
        <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
          Next Endorsement Due
        </p>
      </div>

      <!-- Progress bar -->
      <div class="flex-1 h-1 rounded-full overflow-hidden" style="background-color: var(--color-surface-low);">
        <div class="h-full rounded-full transition-all duration-1000"
             :style="`width: ${tatProgressPct}%; ${tatBarStyle}`"></div>
      </div>

      <!-- Countdown -->
      <p class="text-xs font-extrabold font-mono flex-shrink-0 tabular-nums" :style="tatColorStyle">
        {{ tatExceeded ? 'EXCEEDED' : tatCountdown }}
      </p>

      <!-- Divider -->
      <div class="h-4 w-px flex-shrink-0" style="background-color: var(--color-border);"></div>

      <!-- Appeal Button -->
      <button v-if="tatCycle.canAppeal"
              class="flex-shrink-0 flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest transition-all active:scale-95 cursor-pointer"
              :class="tatLoading ? 'opacity-60 pointer-events-none' : ''"
              style="color: var(--color-text-muted);"
              @mouseenter="e => e.currentTarget.style.color = 'var(--color-text)'"
              @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
              @click="confirmAppeal">
        <span class="material-symbols-outlined text-sm">do_not_disturb_on</span>
        Nothing to Endorse
      </button>

      <!-- Appeal not available hint -->
      <p v-else
         class="flex-shrink-0 text-[10px] font-bold uppercase tracking-widest"
         style="color: var(--color-text-muted);">
        Appeal after TAT breach
      </p>

    </div>

    <!-- Appeal Confirm Modal -->
    <ConfirmModal :isVisible="appealConfirm.visible"
                  type="warning"
                  icon="do_not_disturb_on"
                  title="Nothing to Endorse?"
                  message="This will log an appeal and reset the TAT timer. Only proceed if there are genuinely no specimens to endorse at this time."
                  confirmText="Yes, Submit Appeal"
                  cancelText="Cancel"
                  @confirm="handleAppeal"
                  @close="appealConfirm.visible = false" />
    <!-- Main Grid -->
    <div class="grid grid-cols-12 gap-6">

      <!-- Recent Batches Table -->
      <div class="col-span-12 lg:col-span-8">
        <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Table Header -->
          <div class="px-8 py-5 flex justify-between items-center" style="border-bottom: 1px solid var(--color-surface-low);">
            <div class="flex items-center gap-3">
              <h2 class="text-base font-bold" style="color: var(--color-text);">Today's Batches</h2>
              <span v-if="recentBatches.length > 0 && recentBatchPages > 1"
                    class="text-xs font-bold" style="color: var(--color-text-muted);">
                {{ recentBatchPage }}/{{ recentBatchPages }}
              </span>
            </div>
            <button class="text-xs font-bold uppercase tracking-widest transition-all"
                    style="color: var(--color-primary);"
                    @click="router.push('/endorsements')">
              View All
            </button>
          </div>

          <!-- Loading -->
          <div v-if="tableLoading" class="px-8 py-12 flex items-center justify-center gap-3">
            <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
            <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</p>
          </div>

          <!-- Empty -->
          <div v-else-if="recentBatches.length === 0" class="px-8 py-12 flex flex-col items-center justify-center gap-2">
            <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
            <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">No batches endorsed today</p>
          </div>

          <!-- Table -->
          <div v-else class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-bg);">
                  <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Batch No.</th>
                  <th v-if="authStore.isAdmin" class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed By</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Destination</th>
                  <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="batch in paginatedRecentBatches" :key="batch.batchNo"
                    class="cursor-pointer transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="openDrawer(batch.batchNo)">
                  <td class="px-8 py-4 font-mono text-sm font-bold" style="color: var(--color-primary);">{{ batch.batchNo }}</td>
                  <td v-if="authStore.isAdmin" class="px-4 py-4 text-sm" style="color: var(--color-text);">{{ batch.location }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ formatDateTime(batch.endorsed) }}</td>
                  <td class="px-4 py-4 text-xs font-bold" style="color: var(--color-text);">{{ batch.endorsedBy }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ batch.destination }}</td>
                  <td class="px-8 py-4">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                          :style="getBatchStatusStyle(batch.status)">
                      <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getBatchStatusDot(batch.status)}`"></span>
                      {{ getBatchStatusLabel(batch.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
            <!-- Pagination -->
            <div v-if="recentBatchPages > 1"
                 class="px-8 py-3 flex items-center justify-between"
                 style="border-top: 1px solid var(--color-surface-low);">
              <button :disabled="recentBatchPage === 1"
                      class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                      style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                      @click="recentBatchPage--">
                <span class="material-symbols-outlined text-sm">chevron_left</span>
                Prev
              </button>
              <div class="flex items-center gap-1">
                <button v-for="p in recentBatchPages"
                        :key="p"
                        class="w-7 h-7 rounded-lg text-xs font-bold transition-all"
                        :style="p === recentBatchPage
              ? 'background-color: var(--color-primary); color: #fff;'
              : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                        @click="recentBatchPage = p">
                  {{ p }}
                </button>
              </div>
              <button :disabled="recentBatchPage === recentBatchPages"
                      class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                      style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                      @click="recentBatchPage++">
                Next
                <span class="material-symbols-outlined text-sm">chevron_right</span>
              </button>
            </div>
          </div>

        </div>
      </div>

      <!-- Right Panel -->
      <div class="col-span-12 lg:col-span-4 space-y-6">

        <!-- Daily Batch Flow -->
        <div class="rounded-2xl p-6" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-6" style="color: var(--color-text);">Daily Batch Flow</h2>
          <div class="h-48 flex items-end justify-between gap-2 px-2">
            <div v-for="(bar, i) in flowBars" :key="i"
                 class="relative w-full h-full flex flex-col items-center justify-end group/bar">
              <!-- Count tooltip -->
              <span class="absolute -top-5 text-[10px] font-bold opacity-0 group-hover/bar:opacity-100 transition-opacity"
                    style="color: var(--color-primary);">{{ bar.count }}</span>
              <!-- Bar -->
              <div class="w-full rounded-t-lg transition-all duration-500"
                   :style="`height: ${bar.height}%; background-color: ${bar.active ? 'var(--color-primary)' : 'var(--color-primary-soft)'};`"
                   @mouseenter="(e) => { if (!bar.active) e.currentTarget.style.opacity = '0.7'; }"
                   @mouseleave="(e) => { if (!bar.active) e.currentTarget.style.opacity = '1'; }">
              </div>
            </div>
          </div>
          <div class="flex justify-between mt-4">
            <span v-for="bar in flowBars" :key="bar.day"
                  class="text-[10px] font-bold uppercase tracking-widest"
                  :style="bar.active ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
              {{ bar.day }}
            </span>
          </div>
        </div>

        <!-- System Status -->
        <div class="col-span-12 lg:col-span-4">
          <SystemStatus />
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
  import { ref, computed, onMounted, onUnmounted } from 'vue'
  import { useRouter } from 'vue-router'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { batchApi } from '@/api/batchApi'
  import { healthApi } from '@/api/healthApi'
  import { tatApi } from '@/api/tatApi'
  import AlertModal from '@/components/common/AlertModal.vue'
  import SystemStatus from '@/components/common/SystemStatus.vue'
  import BatchDetailDrawer from '@/components/common/BatchDetailDrawer.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'

  const authStore = useAuthStore()
  const router = useRouter()

  // ── Alert ──────────────────────────────────────────────────────────────────

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  const today = computed(() =>
    new Date().toLocaleDateString('en-US', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    })
  )

  // ── KPI State ──────────────────────────────────────────────────────────────

  const loading = ref(true)

  const summary = ref({ totalEndorsed: 0, pending: 0, received: 0, outsideTAT: 0 })

  const allSections = ref([])

  const adminGroups = computed(() => {
    const categoryMap = {
      '1': { label: 'Phlebo / Send-In', icon: 'vaccines', category: '1' },
      '2': { label: 'Processing', icon: 'labs', category: '2' },
      '3': { label: 'Laboratory', icon: 'science', category: '3' },
    }
    const groups = {}
    for (const sec of allSections.value) {
      const cat = sec.sectionCategory
      if (!groups[cat]) {
        groups[cat] = { ...categoryMap[cat] ?? { label: `Category ${cat}`, icon: 'category', category: cat }, sections: [] }
      }
      groups[cat].sections.push(sec)
    }
    return Object.values(groups).sort((a, b) => a.category.localeCompare(b.category))
  })

  // ── Recent Batches State ───────────────────────────────────────────────────

  const tableLoading = ref(true)
  const recentBatches = ref([])

  // ── Recent Batches Pagination ──────────────────────────────────────────────

  const recentBatchPage = ref(1)
  const RECENT_PAGE_SIZE = 8

  const recentBatchPages = computed(() =>
    Math.max(1, Math.ceil(recentBatches.value.length / RECENT_PAGE_SIZE))
  )

  const paginatedRecentBatches = computed(() => {
    const start = (recentBatchPage.value - 1) * RECENT_PAGE_SIZE
    return recentBatches.value.slice(start, start + RECENT_PAGE_SIZE)
  })

  // ── Drawer State ───────────────────────────────────────────────────────────

  const drawerOpen = ref(false)
  const drawerLoading = ref(false)
  const drawerData = ref(null)
  const drawerTab = ref('Specimens')

  async function openDrawer(batchNo) {
    drawerOpen.value = true
    drawerLoading.value = true
    drawerTab.value = 'Specimens'
    drawerData.value = null
    try {
      drawerData.value = await batchApi.getBatchDetail(batchNo)
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Drawer fetch error:', err)
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
  let refreshInterval = null
  onMounted(async () => {
    // First load — show spinners
    await fetchKPIs()
    loading.value = false

    await fetchTableData()
    tableLoading.value = false

    // Start silent refresh every 5 seconds
    refreshInterval = setInterval(silentRefresh, 5000)

    // Tick every second for the countdown
    tickInterval = setInterval(() => { nowTick.value = Date.now() }, 1000)

    // Silent refresh every 5 seconds
    refreshInterval = setInterval(silentRefresh, 5000)
  })

  onUnmounted(() => {
    clearInterval(refreshInterval)
    clearInterval(tickInterval)
  })


  // ── Fetch ──────────────────────────────────────────────────────────────────

  async function fetchKPIs() {
    try {
      if (authStore.isAdmin) {
        const data = await batchApi.getAllSectionsSummary()
        allSections.value = data
      } else {
        const data = await batchApi.getDashboardSummary(authStore.sectionCode)
        summary.value = data
      }
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Dashboard KPI fetch error:', err)
      }
    }
  }

  async function fetchTableData() {
    try {
      if (authStore.isAdmin) {
        const [recentData, flowData] = await Promise.all([
          batchApi.getAllSectionsRecentBatches(),
          batchApi.getAllSectionsWeeklyFlow(),
        ])
        recentBatches.value = recentData
        weeklyFlow.value = flowData
      } else {
        const [recentData, flowData] = await Promise.all([
          batchApi.getRecentBatches(authStore.sectionCode),
          batchApi.getWeeklyFlow(authStore.sectionCode),
        ])
        recentBatches.value = recentData
        weeklyFlow.value = flowData
      }
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Dashboard table fetch error:', err)
      }
    }
  }

  // Silent refresh — no spinners touched
  async function silentRefresh() {
    await Promise.all([fetchKPIs(), fetchTableData(), loadTatCycle()])
  }


  // ── Helpers ────────────────────────────────────────────────────────────────

  function formatDateTime(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-US', {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: true,
    })
  }

  function getBatchStatusLabel(status) {
    const map = { P: 'Pending', PA: 'Partial', C: 'Completed' }
    return map[status] ?? status
  }

  function getBatchStatusStyle(status) {
    const map = {
      P: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      PA: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
      C: 'background-color: var(--color-success-soft); color: var(--color-success);',
    }
    return map[status] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function getBatchStatusDot(status) {
    const map = { P: 'var(--color-warning)', PA: '#2563eb', C: 'var(--color-success)' }
    return map[status] ?? 'var(--color-text-muted)'
  }
  // ── Static placeholders ────────────────────────────────────────────────────

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

  // ── TAT Countdown ─────────────────────────────────────────────────────────

  const tatCycle = ref({ hasOpenCycle: false, cycleStart: null, thresholdMins: null, canAppeal: false })
  const nowTick = ref(Date.now())
  const tatLoading = ref(false)
  let tickInterval = null

  async function loadTatCycle() {
    if (authStore.isAdmin) return
    try {
      tatCycle.value = await tatApi.getOpenCycle(authStore.sectionCode)
    } catch {
      tatCycle.value = { hasOpenCycle: false, cycleStart: null, thresholdMins: null, canAppeal: false }
    }
  }

  const appealConfirm = ref({ visible: false })

  function confirmAppeal() {
    appealConfirm.value.visible = true
  }

  async function handleAppeal() {
    if (tatLoading.value) return
    tatLoading.value = true
    try {
      await tatApi.appeal(authStore.sectionCode)
      await loadTatCycle()
    } catch (err) {
      console.error('Appeal failed:', err)
    } finally {
      tatLoading.value = false
    }
  }

  const tatSecondsRemaining = computed(() => {
    if (!tatCycle.value.hasOpenCycle || !tatCycle.value.cycleStart || !tatCycle.value.thresholdMins) return null
    const start = new Date(tatCycle.value.cycleStart).getTime()
    const threshold = tatCycle.value.thresholdMins * 60 * 1000
    const elapsed = nowTick.value - start
    return Math.floor((threshold - elapsed) / 1000)
  })

  const tatCountdown = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return null
    if (secs <= 0) return 'EXCEEDED'
    const h = Math.floor(secs / 3600)
    const m = Math.floor((secs % 3600) / 60)
    const s = secs % 60
    if (h > 0) return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  })

  const tatProgressPct = computed(() => {
    if (!tatCycle.value.thresholdMins || tatSecondsRemaining.value === null) return 0
    const total = tatCycle.value.thresholdMins * 60
    const remaining = Math.max(tatSecondsRemaining.value, 0)
    return Math.round((remaining / total) * 100)
  })

  // green → orange (≤25%) → red (exceeded)
  const tatColorStyle = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return ''
    if (secs <= 0) return 'color: var(--color-error);'
    const pct = tatProgressPct.value
    if (pct <= 25) return 'color: var(--color-warning);'
    return 'color: var(--color-success);'
  })

  const tatBarStyle = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return ''
    if (secs <= 0) return 'background-color: var(--color-error);'
    const pct = tatProgressPct.value
    if (pct <= 25) return 'background-color: var(--color-warning);'
    return 'background-color: var(--color-success);'
  })

  const tatExceeded = computed(() => tatSecondsRemaining.value !== null && tatSecondsRemaining.value <= 0)

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
