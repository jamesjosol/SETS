<template>
  <div class="kiosk-root">

    <!-- ── Header Strip ──────────────────────────────────────────────────── -->
    <header class="kiosk-header">

      <!-- Left: SETS logo + full wordmark (from AppSidebar) -->
      <div class="kiosk-brand">
        <svg width="36" height="36" viewBox="0 0 60 52" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M4 8 L4 36 Q4 46 10 46 Q16 46 16 36 L16 8" stroke="var(--color-primary)" stroke-width="2" fill="none" />
          <line x1="2" y1="8" x2="18" y2="8" stroke="var(--color-primary)" stroke-width="2.5" stroke-linecap="round" />
          <path d="M4 28 L4 36 Q4 46 10 46 Q16 46 16 36 L16 28 Z" fill="var(--color-primary)" opacity="0.4" />
          <path d="M22 4 L22 36 Q22 46 28 46 Q34 46 34 36 L34 4" stroke="var(--color-primary)" stroke-width="2" fill="none" />
          <line x1="20" y1="4" x2="36" y2="4" stroke="var(--color-primary)" stroke-width="2.5" stroke-linecap="round" />
          <path d="M22 20 L22 36 Q22 46 28 46 Q34 46 34 36 L34 20 Z" fill="var(--color-primary)" opacity="0.4" />
          <circle cx="26" cy="30" r="1.5" fill="var(--color-primary)" opacity="0.7" />
          <circle cx="30" cy="35" r="1" fill="var(--color-primary)" opacity="0.5" />
          <path d="M40 10 L40 36 Q40 46 46 46 Q52 46 52 36 L52 10" stroke="var(--color-primary)" stroke-width="2" fill="none" />
          <line x1="38" y1="10" x2="54" y2="10" stroke="var(--color-primary)" stroke-width="2.5" stroke-linecap="round" />
          <path d="M40 22 L40 36 Q40 46 46 46 Q52 46 52 36 L52 22 Z" fill="var(--color-primary)" opacity="0.4" />
        </svg>
        <div class="kiosk-brand-text">
          <p class="kiosk-brand-name">SETS</p>
          <p class="kiosk-brand-sub">Specimen Endorsement &amp; Tracking System</p>
        </div>
      </div>

      <!-- Center: Branch name + date -->
      <div class="kiosk-title">
        <span class="kiosk-branch">{{ branchName }}</span>
        <span class="kiosk-date">{{ todayLabel }}</span>
      </div>

      <!-- Right: Show Completed toggle + live clock -->
      <div class="kiosk-header-right">

        <button class="kiosk-toggle-btn"
                :class="{ 'kiosk-toggle-active': showCompleted }"
                @click="showCompleted = !showCompleted">
          <span class="material-symbols-outlined kiosk-toggle-icon">
            {{ showCompleted ? 'visibility' : 'visibility_off' }}
          </span>
          <span class="kiosk-toggle-label">
            {{ showCompleted ? 'Hide Completed' : 'Show Completed' }}
          </span>
        </button>

        <div class="kiosk-clock">
          <span class="material-symbols-outlined kiosk-clock-icon">schedule</span>
          <span class="kiosk-clock-time">{{ clockTime }}</span>
        </div>

      </div>
    </header>

    <!-- ── KPI Row ────────────────────────────────────────────────────────── -->
    <div ref="kpiRowRef" class="kiosk-kpi-row">

      <div class="kiosk-kpi-card">
        <div class="kiosk-kpi-icon" style="background-color: var(--color-primary-soft);">
          <span class="material-symbols-outlined" style="color: var(--color-primary);">clinical_notes</span>
        </div>
        <div class="kiosk-kpi-body">
          <span class="kiosk-kpi-value" style="color: var(--color-text);">
            <span v-if="summaryLoading">-</span><span v-else>{{ displayTotal }}</span>
          </span>
          <span class="kiosk-kpi-label">Total Batches</span>
        </div>
        <span class="kiosk-kpi-badge" style="color: var(--color-text-muted);">Today</span>
      </div>

      <div class="kiosk-kpi-card">
        <div class="kiosk-kpi-icon" style="background-color: var(--color-warning-soft);">
          <span class="material-symbols-outlined" style="color: var(--color-warning);">pending_actions</span>
        </div>
        <div class="kiosk-kpi-body">
          <span class="kiosk-kpi-value" style="color: var(--color-warning);">
            <span v-if="summaryLoading">-</span><span v-else>{{ displayPending }}</span>
          </span>
          <span class="kiosk-kpi-label">Pending</span>
        </div>
        <span class="kiosk-kpi-badge" style="color: var(--color-warning);">Awaiting</span>
      </div>

      <div class="kiosk-kpi-card">
        <div class="kiosk-kpi-icon" style="background-color: rgba(77,159,255,0.12);">
          <span class="material-symbols-outlined" style="color: #4d9fff;">move_to_inbox</span>
        </div>
        <div class="kiosk-kpi-body">
          <span class="kiosk-kpi-value" style="color: #4d9fff;">
            <span v-if="summaryLoading">-</span><span v-else>{{ displayPartial }}</span>
          </span>
          <span class="kiosk-kpi-label">Partial</span>
        </div>
        <span class="kiosk-kpi-badge" style="color: #4d9fff;">In Progress</span>
      </div>

      <div class="kiosk-kpi-card">
        <div class="kiosk-kpi-icon" style="background-color: var(--color-success-soft);">
          <span class="material-symbols-outlined" style="color: var(--color-success);">inventory</span>
        </div>
        <div class="kiosk-kpi-body">
          <span class="kiosk-kpi-value" style="color: var(--color-success);">
            <span v-if="summaryLoading">-</span><span v-else>{{ displayCompleted }}</span>
          </span>
          <span class="kiosk-kpi-label">Completed</span>
        </div>
        <span class="kiosk-kpi-badge" style="color: var(--color-success);">Done</span>
      </div>

    </div>

    <!-- ── Monitoring Grid ────────────────────────────────────────────────── -->
    <div class="kiosk-grid-wrapper">

      <div v-if="monitoringLoading" class="kiosk-loading">
        <span class="material-symbols-outlined animate-spin"
              style="color: var(--color-text-muted); font-size: 2rem;">progress_activity</span>
        <p class="kiosk-loading-text">Loading monitoring data...</p>
      </div>

      <div v-else
           ref="monitoringGridRef"
           class="kiosk-grid"
           :style="`grid-template-columns: repeat(${monitoringData.length || 1}, 1fr);`">

        <div v-for="section in monitoringData" :key="section.sectionCode" class="kiosk-section-col">

          <div class="kiosk-section-header">
            <p class="kiosk-section-name">{{ section.sectionName }}</p>
          </div>

          <div class="kiosk-batch-list">

            <!-- No batches at all today -->
            <div v-if="section.batches.length === 0" class="kiosk-empty">
              <span class="material-symbols-outlined"
                    style="color: var(--color-text-muted); font-size: 1.5rem;">inbox</span>
              <p class="kiosk-empty-text">No batches today</p>
            </div>

            <template v-else>

              <!-- Active (P / PA) batches — always visible -->
              <div v-for="batch in getActiveBatches(section)"
                   :key="batch.batchNo"
                   class="kiosk-batch-card"
                   :style="getBatchCardStyle(batch)">
                <div class="kiosk-batch-top">
                  <span class="kiosk-batch-no">{{ batch.batchNo }}</span>
                  <span class="kiosk-status-dot"
                        :style="`background-color: ${getStatusDotColor(batch)};`"></span>
                </div>
                <p class="kiosk-batch-progress">
                  {{ batch.receivedSpecimens }}/{{ batch.totalSpecimens }} specimens
                </p>
                <div class="kiosk-progress-track">
                  <div class="kiosk-progress-fill" :style="getProgressFillStyle(batch)"></div>
                </div>
              </div>

              <!-- All-active-done indicator when completed are hidden -->
              <div v-if="getActiveBatches(section).length === 0 && !showCompleted"
                   class="kiosk-all-done">
                <span class="material-symbols-outlined"
                      style="color: var(--color-success); font-size: 1.1rem;">check_circle</span>
                <p class="kiosk-all-done-text">All done</p>
              </div>

              <!-- Completed batches — shown only when toggle is on -->
              <template v-if="showCompleted">

                <div v-if="getActiveBatches(section).length > 0 && getCompletedBatches(section).length > 0"
                     class="kiosk-completed-divider">
                  <span class="kiosk-completed-divider-label">Completed</span>
                </div>

                <div v-for="batch in getCompletedBatches(section)"
                     :key="batch.batchNo"
                     class="kiosk-batch-card"
                     :style="getBatchCardStyle(batch)">
                  <div class="kiosk-batch-top">
                    <span class="kiosk-batch-no">{{ batch.batchNo }}</span>
                    <span class="kiosk-status-dot"
                          :style="`background-color: ${getStatusDotColor(batch)};`"></span>
                  </div>
                  <p class="kiosk-batch-progress">
                    {{ batch.receivedSpecimens }}/{{ batch.totalSpecimens }} specimens
                  </p>
                  <div class="kiosk-progress-track">
                    <div class="kiosk-progress-fill" :style="getProgressFillStyle(batch)"></div>
                  </div>
                </div>

              </template>

            </template>
          </div>
        </div>

      </div>
    </div>

    <!-- ── Footer ─────────────────────────────────────────────────────────── -->
    <footer class="kiosk-footer">
      <span class="kiosk-footer-text">
        HI-Precision Diagnostics &middot; SETS Monitoring Display &middot; Auto-refreshes every 5 seconds
      </span>
      <span class="kiosk-footer-dot"></span>
      <span class="kiosk-footer-text" :class="{ 'kiosk-refreshing': isRefreshing }">
        {{ lastRefreshLabel }}
      </span>
    </footer>

  </div>
</template>

<script setup>
  import { ref, onMounted, onUnmounted, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import { kioskApi } from '@/api/kioskApi'

  // ── Theme ──────────────────────────────────────────────────────────────────
  // Force data-theme="dark" so all --color-* CSS vars resolve to the dark
  // palette defined in main.css. Saved and restored on unmount.

  const savedTheme = ref(null)
  const savedAccent = ref(null)

  onMounted(() => {
    const html = document.documentElement
    savedTheme.value = html.getAttribute('data-theme')
    savedAccent.value = html.getAttribute('data-accent')
    html.setAttribute('data-theme', 'dark')
  })

  onUnmounted(() => {
    const html = document.documentElement
    if (savedTheme.value) html.setAttribute('data-theme', savedTheme.value)
    else html.removeAttribute('data-theme')
    if (savedAccent.value) html.setAttribute('data-accent', savedAccent.value)
    else html.removeAttribute('data-accent')
  })

  // ── Completed toggle ───────────────────────────────────────────────────────

  const showCompleted = ref(false)

  // ── Clock ──────────────────────────────────────────────────────────────────

  const clockTime = ref('')
  const todayLabel = ref('')

  function updateClock() {
    const now = new Date()
    clockTime.value = now.toLocaleTimeString('en-US', {
      hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true,
    })
    todayLabel.value = now.toLocaleDateString('en-US', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    })
  }

  let clockInterval = null

  // ── Summary / KPIs ─────────────────────────────────────────────────────────

  const summaryLoading = ref(true)
  const branchName = ref('-')

  const displayTotal = ref(0)
  const displayPending = ref(0)
  const displayPartial = ref(0)
  const displayCompleted = ref(0)

  function countUp(target, toValue) {
    const obj = { val: 0 }
    gsap.killTweensOf(obj)
    gsap.to(obj, {
      val: toValue, duration: 0.75, ease: 'power2.out',
      onUpdate: () => { target.value = Math.round(obj.val) },
    })
  }

  async function fetchSummary() {
    try {
      const data = await kioskApi.getSummary()
      branchName.value = data.branchName ?? data.branchCode ?? '-'

      if (summaryLoading.value) {
        summaryLoading.value = false
        await nextTick()
        countUp(displayTotal, data.totalEndorsed)
        countUp(displayPending, data.pending)
        countUp(displayPartial, data.partial)
        countUp(displayCompleted, data.completed)
      } else {
        displayTotal.value = data.totalEndorsed
        displayPending.value = data.pending
        displayPartial.value = data.partial
        displayCompleted.value = data.completed
      }
    } catch {
      // Silent fail — keep showing stale data on network blip
    }
  }

  // ── Monitoring Grid ────────────────────────────────────────────────────────

  const monitoringLoading = ref(true)
  const monitoringData = ref([])
  const procTatThreshold = ref(null)

  const monitoringGridRef = ref(null)
  const kpiRowRef = ref(null)

  async function animateGrid() {
    await nextTick()
    if (!monitoringGridRef.value) return
    const cols = monitoringGridRef.value.querySelectorAll(':scope > div')
    if (!cols.length) return
    gsap.set(cols, { opacity: 0, y: 16 })
    gsap.to(cols, { opacity: 1, y: 0, duration: 0.3, stagger: 0.05, ease: 'power2.out' })
  }

  async function animateKpi() {
    await nextTick()
    if (!kpiRowRef.value) return
    const cards = kpiRowRef.value.querySelectorAll(':scope > div')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, { opacity: 1, y: 0, duration: 0.35, stagger: 0.07, ease: 'power2.out' })
  }

  async function fetchMonitoring() {
    try {
      const res = await kioskApi.getMonitoring()
      monitoringData.value = res.sections ?? []
      procTatThreshold.value = res.thresholdMins ?? null
      if (monitoringLoading.value) {
        monitoringLoading.value = false
        await animateGrid()
      }
    } catch {
      // Silent fail
    }
  }

  // ── Batch helpers ──────────────────────────────────────────────────────────

  function isOverdue(batch) {
    if (batch.status !== 'PA') return false
    if (!batch.procReceived || procTatThreshold.value == null) return false
    return (Date.now() - new Date(batch.procReceived).getTime()) / 60000 > procTatThreshold.value
  }

  function getActiveBatches(section) {
    return section.batches
      .filter(b => b.status !== 'C')
      .sort((a, b) => (isOverdue(a) ? 0 : 1) - (isOverdue(b) ? 0 : 1))
  }

  function getCompletedBatches(section) {
    return section.batches.filter(b => b.status === 'C')
  }

  function getBatchCardStyle(batch) {
    if (batch.status === 'C') {
      return batch.isOutsideProcTat
        ? 'background-color: rgba(239,68,68,0.08); border-color: rgba(239,68,68,0.2); opacity: 0.55;'
        : 'background-color: rgba(77,159,255,0.05); border-color: rgba(77,159,255,0.12); opacity: 0.55;'
    }
    if (batch.status === 'P') {
      return 'background-color: var(--color-surface-low); border-color: var(--color-border);'
    }
    // PA
    if (isOverdue(batch)) {
      return 'background-color: rgba(239,68,68,0.10); border-color: rgba(239,68,68,0.28);'
    }
    return 'background-color: rgba(77,159,255,0.08); border-color: rgba(77,159,255,0.18);'
  }

  function getStatusDotColor(batch) {
    if (batch.status === 'C') return batch.isOutsideProcTat ? '#ef4444' : '#4d9fff'
    if (batch.status === 'P') return 'var(--color-warning)'
    if (isOverdue(batch)) return '#ef4444'
    return '#4d9fff'
  }

  function getProgressFillStyle(batch) {
    const total = batch.totalSpecimens
    if (!total) return 'width: 0%; background-color: var(--color-text-muted);'
    const pct = Math.round((batch.receivedSpecimens / total) * 100)
    let color = 'var(--color-text-muted)'
    if (batch.status === 'C') color = batch.isOutsideProcTat ? '#ef4444' : '#4d9fff'
    else if (batch.status === 'P') color = 'var(--color-warning)'
    else if (isOverdue(batch)) color = '#ef4444'
    else color = '#4d9fff'
    return `width: ${pct}%; background-color: ${color};`
  }

  // ── Refresh tracking ───────────────────────────────────────────────────────

  const lastRefreshed = ref(null)
  const lastRefreshLabel = ref('Not yet refreshed')
  const isRefreshing = ref(false)

  function updateRefreshLabel() {
    if (!lastRefreshed.value) { lastRefreshLabel.value = 'Not yet refreshed'; return }
    lastRefreshLabel.value = `Last updated: ${lastRefreshed.value.toLocaleTimeString('en-US', {
      hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: true,
    })}`
  }

  async function silentRefresh() {
    isRefreshing.value = true
    await Promise.all([fetchSummary(), fetchMonitoring()])
    lastRefreshed.value = new Date()
    updateRefreshLabel()
    isRefreshing.value = false
  }

  // ── Lifecycle ──────────────────────────────────────────────────────────────

  let refreshInterval = null

  onMounted(async () => {
    updateClock()
    clockInterval = setInterval(updateClock, 1000)

    await fetchSummary()
    await animateKpi()
    await fetchMonitoring()

    lastRefreshed.value = new Date()
    updateRefreshLabel()

    refreshInterval = setInterval(silentRefresh, 5000)
  })

  onUnmounted(() => {
    clearInterval(clockInterval)
    clearInterval(refreshInterval)
  })
</script>

<style scoped>

  /* ── Root ─────────────────────────────────────────────────────────────── */
  .kiosk-root {
    min-height: 100dvh;
    display: flex;
    flex-direction: column;
    background-color: var(--color-bg);
    color: var(--color-text);
    font-family: "Manrope", sans-serif;
    overflow: hidden;
  }

  /* ── Header ───────────────────────────────────────────────────────────── */
  .kiosk-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 0.875rem 2rem;
    border-bottom: 1px solid var(--color-border);
    background-color: var(--color-surface);
    flex-shrink: 0;
  }

  .kiosk-brand {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }

  .kiosk-brand-text {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
  }

  .kiosk-brand-name {
    font-size: 1.25rem;
    font-weight: 900;
    letter-spacing: -0.03em;
    color: var(--color-primary);
    line-height: 1;
  }

  .kiosk-brand-sub {
    font-size: 0.58rem;
    font-weight: 700;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--color-text-muted);
    line-height: 1;
  }

  .kiosk-title {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.2rem;
  }

  .kiosk-branch {
    font-size: 1.1rem;
    font-weight: 800;
    letter-spacing: 0.06em;
    text-transform: uppercase;
    color: var(--color-text);
  }

  .kiosk-date {
    font-size: 0.68rem;
    font-weight: 600;
    color: var(--color-text-muted);
  }

  .kiosk-header-right {
    display: flex;
    align-items: center;
    gap: 1rem;
  }

  /* Show/Hide Completed toggle */
  .kiosk-toggle-btn {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    padding: 0.4rem 0.85rem;
    border-radius: 0.6rem;
    border: 1px solid var(--color-border);
    background-color: var(--color-surface-low);
    color: var(--color-text-muted);
    cursor: pointer;
    transition: all 0.2s ease;
  }

    .kiosk-toggle-btn:hover {
      border-color: var(--color-primary);
      color: var(--color-primary);
    }

  .kiosk-toggle-active {
    background-color: var(--color-primary-soft);
    border-color: var(--color-primary);
    color: var(--color-primary) !important;
  }

  .kiosk-toggle-icon {
    font-size: 1rem;
  }

  .kiosk-toggle-label {
    font-size: 0.62rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  }

  .kiosk-clock {
    display: flex;
    align-items: center;
    gap: 0.35rem;
  }

  .kiosk-clock-icon {
    font-size: 1rem;
    color: var(--color-text-muted);
  }

  .kiosk-clock-time {
    font-size: 1.05rem;
    font-weight: 800;
    font-variant-numeric: tabular-nums;
    color: var(--color-text);
    letter-spacing: 0.03em;
  }

  /* ── KPI Row ──────────────────────────────────────────────────────────── */
  .kiosk-kpi-row {
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 0.875rem;
    padding: 0.875rem 2rem;
    flex-shrink: 0;
  }

  .kiosk-kpi-card {
    display: flex;
    align-items: center;
    gap: 0.875rem;
    padding: 0.875rem 1.1rem;
    border-radius: 1rem;
    background-color: var(--color-surface);
    box-shadow: 0 1px 3px var(--color-shadow);
    position: relative;
    overflow: hidden;
  }

  .kiosk-kpi-icon {
    padding: 0.55rem;
    border-radius: 0.65rem;
    flex-shrink: 0;
  }

    .kiosk-kpi-icon .material-symbols-outlined {
      font-size: 1.35rem;
      display: block;
    }

  .kiosk-kpi-body {
    display: flex;
    flex-direction: column;
    gap: 0.1rem;
    flex: 1;
  }

  .kiosk-kpi-value {
    font-size: 1.9rem;
    font-weight: 900;
    line-height: 1;
    font-variant-numeric: tabular-nums;
  }

  .kiosk-kpi-label {
    font-size: 0.6rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
    color: var(--color-text-muted);
  }

  .kiosk-kpi-badge {
    font-size: 0.58rem;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    position: absolute;
    top: 0.55rem;
    right: 0.7rem;
  }

  /* ── Grid Wrapper ─────────────────────────────────────────────────────── */
  .kiosk-grid-wrapper {
    flex: 1;
    padding: 0 2rem 1rem;
    min-height: 0;
    display: flex;
    flex-direction: column;
  }

  .kiosk-loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    height: 100%;
    gap: 0.75rem;
    padding-top: 4rem;
  }

  .kiosk-loading-text {
    font-size: 0.72rem;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: var(--color-text-muted);
  }

  /* ── Monitoring Grid ──────────────────────────────────────────────────── */
  .kiosk-grid {
    display: grid;
    gap: 1rem;
    align-items: stretch;
    flex: 1;
    min-height: 0;
  }

  .kiosk-section-col {
    display: flex;
    flex-direction: column;
    gap: 0.45rem;
    min-height: 0;
  }

  .kiosk-section-header {
    border-radius: 0.65rem;
    padding: 0.5rem 1rem;
    text-align: center;
    background-color: var(--color-primary);
  }

  .kiosk-section-name {
    font-size: 0.62rem;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.1em;
    color: #ffffff;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .kiosk-batch-list {
    display: flex;
    flex-direction: column;
    gap: 0.35rem;
    flex: 1;
    min-height: 0;
    overflow-y: auto;
    /* Hide scrollbar — TV display doesn't need it visible */
    scrollbar-width: none;
  }

    .kiosk-batch-list::-webkit-scrollbar {
      display: none;
    }

  .kiosk-empty {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 1.25rem 0;
    gap: 0.35rem;
  }

  .kiosk-empty-text {
    font-size: 0.62rem;
    color: var(--color-text-muted);
    font-weight: 600;
  }

  .kiosk-all-done {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.4rem;
    padding: 0.75rem 0;
  }

  .kiosk-all-done-text {
    font-size: 0.65rem;
    font-weight: 700;
    color: var(--color-success);
  }

  /* Divider between active and completed sections */
  .kiosk-completed-divider {
    display: flex;
    align-items: center;
    gap: 0.4rem;
    margin: 0.15rem 0;
  }

    .kiosk-completed-divider::before,
    .kiosk-completed-divider::after {
      content: '';
      flex: 1;
      height: 1px;
      background-color: var(--color-border);
    }

  .kiosk-completed-divider-label {
    font-size: 0.52rem;
    font-weight: 800;
    text-transform: uppercase;
    letter-spacing: 0.12em;
    color: var(--color-text-muted);
    white-space: nowrap;
  }

  /* ── Batch Card ───────────────────────────────────────────────────────── */
  .kiosk-batch-card {
    padding: 0.6rem 0.8rem;
    border-radius: 0.6rem;
    border: 1px solid transparent;
    display: flex;
    flex-direction: column;
    gap: 0.28rem;
  }

  .kiosk-batch-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }

  .kiosk-batch-no {
    font-size: 0.9rem;
    font-weight: 800;
    font-family: ui-monospace, SFMono-Regular, Menlo, monospace;
    color: var(--color-text);
  }

  .kiosk-status-dot {
    width: 0.45rem;
    height: 0.45rem;
    border-radius: 50%;
    flex-shrink: 0;
  }

  .kiosk-batch-progress {
    font-size: 0.62rem;
    color: var(--color-text-muted);
    font-weight: 600;
  }

  /* ── Progress Bar ─────────────────────────────────────────────────────── */
  .kiosk-progress-track {
    height: 2px;
    border-radius: 999px;
    background-color: var(--color-surface-low);
    overflow: hidden;
  }

  .kiosk-progress-fill {
    height: 100%;
    border-radius: 999px;
    transition: width 0.4s ease;
  }

  /* ── Footer ───────────────────────────────────────────────────────────── */
  .kiosk-footer {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.6rem;
    padding: 0.45rem 2rem;
    border-top: 1px solid var(--color-border);
    background-color: var(--color-surface);
    flex-shrink: 0;
  }

  .kiosk-footer-text {
    font-size: 0.58rem;
    font-weight: 600;
    letter-spacing: 0.04em;
    color: var(--color-text-muted);
    transition: opacity 0.3s ease;
  }

  .kiosk-footer-dot {
    width: 3px;
    height: 3px;
    border-radius: 50%;
    background-color: var(--color-text-muted);
    opacity: 0.35;
  }

  .kiosk-refreshing {
    opacity: 0.35;
  }
</style>
