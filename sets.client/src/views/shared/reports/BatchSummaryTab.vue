<template>
  <div class="flex-1 flex flex-col overflow-hidden">

    <!-- Header -->
    <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
         style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
      <div>
        <h1 class="text-base font-bold" style="color: var(--color-text);">Batch Summary Report</h1>
        <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
          Per-batch TAT for endorsement and completion · Excel export
        </p>
      </div>
      <button ref="exportBtnRef"
              :disabled="!result || exporting"
              class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
              style="background: var(--color-primary-gradient); color: #ffffff;"
              @click="handleExport">
        <span ref="exportIconRef"
              class="material-symbols-outlined text-sm"
              :class="exporting ? 'animate-spin' : ''">
          {{ exporting ? 'progress_activity' : exportDone ? 'check_circle' : 'download' }}
        </span>
        {{ exporting ? 'Exporting…' : exportDone ? 'Exported!' : 'Export Excel' }}
      </button>
    </div>

    <!-- Filters -->
    <div ref="filterRef"
         class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
         style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

      <div v-if="authStore.isAdmin || authStore.sectionCategory === '2'" class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Location</label>
        <select v-model="filters.locationCode"
                class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 140px;">
          <option value="">ALL</option>
          <option v-for="s in endorsingSections" :key="s.code" :value="s.code">{{ s.name }}</option>
        </select>
      </div>

      <div v-else class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Section</label>
        <div class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold"
             style="background-color: var(--color-primary-soft); color: var(--color-primary);">
          <span class="material-symbols-outlined text-base">outbox</span>
          {{ authStore.sectionName }}
        </div>
      </div>

      <div class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Date from</label>
        <DatePicker v-model="filters.dateFrom" placeholder="Date from" :max-date="filters.dateTo" />
      </div>

      <div class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">Date to</label>
        <DatePicker v-model="filters.dateTo" placeholder="Date to" :min-date="filters.dateFrom" />
      </div>

      <button ref="generateBtnRef"
              :disabled="loading"
              class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
              style="background: var(--color-primary-gradient); color: #ffffff;"
              @click="handleGenerate">
        <span class="material-symbols-outlined text-sm" :class="loading ? 'animate-spin' : ''">
          {{ loading ? 'progress_activity' : 'search' }}
        </span>
        {{ loading ? 'Loading…' : 'Generate' }}
      </button>
    </div>

    <!-- Content -->
    <div class="flex-1 overflow-y-auto px-7 py-6">

      <!-- Idle -->
      <div v-if="!result && !loading"
           class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
           style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">table_chart</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
        <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
      </div>

      <!-- Skeleton -->
      <div v-else-if="loading" class="flex flex-col gap-3">
        <div class="grid grid-cols-5 gap-3 mb-2">
          <div v-for="i in 5" :key="i" class="h-16 rounded-xl animate-pulse"
               style="background-color: var(--color-surface);"></div>
        </div>
        <div v-for="i in 6" :key="i" class="h-10 rounded-xl animate-pulse"
             style="background-color: var(--color-surface);"></div>
      </div>

      <template v-else-if="result">

        <!-- KPI strip -->
        <div ref="summaryRef" class="grid grid-cols-5 gap-3 mb-5">
          <div class="rounded-xl p-4 text-center"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <div class="text-2xl font-bold" style="color: var(--color-primary);">{{ displayTotal }}</div>
            <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Total batches</div>
          </div>
          <div class="rounded-xl p-4 text-center"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <div class="text-2xl font-bold" style="color: var(--color-success);">{{ displayCompleted }}</div>
            <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Completed</div>
          </div>
          <div class="rounded-xl p-4 text-center"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <div class="text-2xl font-bold" style="color: var(--color-warning);">{{ displayPending }}</div>
            <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Pending / Partial</div>
          </div>
          <div class="rounded-xl p-4 text-center"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <div class="text-xl font-bold" style="color: var(--color-text);">{{ result.avgTatEndorsement ?? '—' }}</div>
            <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Avg TAT endorsement</div>
          </div>
          <div class="rounded-xl p-4 text-center"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <div class="text-xl font-bold" style="color: var(--color-text);">{{ result.avgTatCompletion ?? '—' }}</div>
            <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Avg TAT completion</div>
          </div>
        </div>

        <!-- Empty -->
        <div v-if="!result.rows.length" ref="emptyRef"
             class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
             style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
          <p class="text-sm font-bold" style="color: var(--color-text);">No batches found</p>
          <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
        </div>

        <!-- Table -->
        <div v-else ref="tableRef" class="rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="overflow-x-auto">
            <table class="w-full text-xs">
              <thead>
                <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Batch no.</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed by</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Received</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Received by</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Completed</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Temp</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">TAT (endorse)</th>
                  <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">TAT (complete)</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="row in result.rows" :key="row.batchNo"
                    style="border-bottom: 0.5px solid var(--color-border);"
                    @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                    @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                  <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">{{ row.batchNo }}</td>
                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location }}</td>
                  <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ formatDt(row.endorsed) }}</td>
                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.endorsedBy }}</td>
                  <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.procReceived ? formatDt(row.procReceived) : '—' }}</td>
                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.receivedBy ?? '—' }}</td>
                  <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.completed ? formatDt(row.completed) : '—' }}</td>
                  <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.temp ?? '—' }}</td>
                  <td class="px-4 py-2.5">
                    <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                          :style="statusStyle(row.status)">{{ row.status }}</span>
                  </td>
                  <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.tatEndorsement ?? '—' }}</td>
                  <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.tatCompletion ?? '—' }}</td>
                </tr>
              </tbody>
              <tfoot>
                <tr style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low);">
                  <td colspan="9" class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                      style="color: var(--color-text-muted);">
                    Average TAT
                  </td>
                  <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">{{ result.avgTatEndorsement ?? '—' }}</td>
                  <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">{{ result.avgTatCompletion ?? '—' }}</td>
                </tr>
              </tfoot>
            </table>
          </div>
        </div>

      </template>
    </div>

    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />
  </div>
</template>

<script setup>
  import { ref, watch, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import { useAuthStore } from '@/stores/authStore'
  import { reportApi } from '@/api/reportApi'
  import DatePicker from '@/components/common/DatePicker.vue'
  import AlertModal from '@/components/common/AlertModal.vue'

  const props = defineProps({
    endorsingSections: { type: Array, default: () => [] },
    labSections: { type: Array, default: () => [] },
  })

  const authStore = useAuthStore()

  // ── Alert ─────────────────────────────────────────────────────────────────────
  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────
  const today = new Date().toISOString().slice(0, 10)

  function formatDt(val) {
    if (!val) return '—'
    return new Date(val).toLocaleString('en-PH', {
      month: '2-digit', day: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: false,
    })
  }

  function statusStyle(status) {
    switch (status) {
      case 'Complete': return 'background-color: rgba(29,158,117,0.12); color: var(--color-success);'
      case 'Partial': return 'background-color: rgba(59,130,246,0.1);  color: var(--color-info);'
      case 'Pending': return 'background-color: rgba(217,119,6,0.1);   color: var(--color-warning);'
      case 'Cancelled': return 'background-color: rgba(239,68,68,0.1);   color: var(--color-error);'
      default: return 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
    }
  }

  // ── State ─────────────────────────────────────────────────────────────────────
  const filters = ref({
    locationCode: authStore.sectionCategory === '1' ? authStore.sectionCode : '',
    dateFrom: today,
    dateTo: today,
  })
  const loading = ref(false)
  const exporting = ref(false)
  const result = ref(null)

  // Count-up display refs
  const displayTotal = ref(0)
  const displayCompleted = ref(0)
  const displayPending = ref(0)

  // ── API ───────────────────────────────────────────────────────────────────────
  async function generate() {
    loading.value = true
    result.value = null
    try {
      result.value = await reportApi.getBatchSummary({
        locationCode: filters.value.locationCode || null,
        dateFrom: filters.value.dateFrom,
        dateTo: filters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      loading.value = false
    }
  }

  async function exportExcel() {
    if (!result.value) return
    exporting.value = true
    try {
      await reportApi.exportBatchSummaryExcel({
        locationCode: filters.value.locationCode || null,
        dateFrom: filters.value.dateFrom,
        dateTo: filters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      exporting.value = false
    }
  }

  // ── GSAP refs ─────────────────────────────────────────────────────────────────
  const generateBtnRef = ref(null)
  const exportBtnRef = ref(null)
  const exportIconRef = ref(null)
  const filterRef = ref(null)
  const tableRef = ref(null)
  const summaryRef = ref(null)
  const emptyRef = ref(null)

  // ── GSAP helpers ──────────────────────────────────────────────────────────────
  function punchButton(btnRef) {
    if (!btnRef?.value) return
    gsap.killTweensOf(btnRef.value)
    gsap.set(btnRef.value, { scale: 1 })
    gsap.to(btnRef.value, { scale: 0.93, duration: 0.08, ease: 'power2.in', yoyo: true, repeat: 1 })
  }

  function dimFilter(isLoading) {
    if (!filterRef.value) return
    gsap.killTweensOf(filterRef.value)
    gsap.to(filterRef.value, { opacity: isLoading ? 0.45 : 1, duration: 0.2, ease: 'power1.out' })
  }

  async function animateTableEntrance() {
    if (!tableRef.value) return
    await nextTick()
    const rows = tableRef.value.querySelectorAll('tbody tr')
    gsap.set(tableRef.value, { opacity: 0, y: 14 })
    gsap.to(tableRef.value, { opacity: 1, y: 0, duration: 0.22, ease: 'power2.out' })
    if (rows.length) {
      gsap.set(rows, { opacity: 0, x: -5 })
      gsap.to(rows, { opacity: 1, x: 0, duration: 0.15, stagger: 0.02, ease: 'power1.out', delay: 0.12 })
    }
  }

  async function animateEmptyState() {
    if (!emptyRef.value) return
    await nextTick()
    gsap.set(emptyRef.value, { scale: 0.92, opacity: 0 })
    gsap.to(emptyRef.value, { scale: 1, opacity: 1, duration: 0.32, ease: 'back.out(1.5)' })
  }

  function countUp(displayRef, target) {
    const obj = { val: 0 }
    gsap.killTweensOf(obj)
    gsap.to(obj, {
      val: target, duration: 0.65, ease: 'power2.out',
      onUpdate: () => { displayRef.value = Math.round(obj.val) },
    })
  }

  // ── Export animation — Option A (icon swap) ──────────────────────────────────
  const exportDone = ref(false)
  let exportResetTimer = null

  async function handleGenerate() {
    punchButton(generateBtnRef)
    await generate()
  }

  async function handleExport() {
    punchButton(exportBtnRef)
    exportDone.value = false
    let success = false
    try {
      await exportExcel()
      success = true
    } catch {
      // exportExcel already shows alert internally
    }
    if (success && exportIconRef.value) {
      exportDone.value = true
      // Icon pops in with back.out bounce
      await nextTick()
      gsap.set(exportIconRef.value, { scale: 0, opacity: 0 })
      gsap.to(exportIconRef.value, {
        scale: 1, opacity: 1,
        duration: 0.35,
        ease: 'back.out(2.5)',
      })
      // Reset back to download after 1.5s
      clearTimeout(exportResetTimer)
      exportResetTimer = setTimeout(() => {
        exportDone.value = false
      }, 1500)
    }
  }

  // ── Watchers ──────────────────────────────────────────────────────────────────
  watch(loading, (v) => dimFilter(v))

  watch(result, async (val) => {
    if (!val) return
    await nextTick()

    // KPI cards stagger
    if (summaryRef.value) {
      gsap.set(summaryRef.value.children, { opacity: 0, y: 10 })
      gsap.to(summaryRef.value.children, {
        opacity: 1, y: 0, duration: 0.22, stagger: 0.055, ease: 'power2.out',
      })
    }

    // Count-up integers
    countUp(displayTotal, val.totalBatches)
    countUp(displayCompleted, val.completedBatches)
    countUp(displayPending, val.pendingBatches)

    if (val.rows?.length) {
      await animateTableEntrance()
    } else {
      await animateEmptyState()
    }
  })
</script>
