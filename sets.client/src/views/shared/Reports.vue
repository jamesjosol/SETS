<template>
  <AppLayout>
    <div class="flex h-full" style="min-height: calc(100vh - 64px);">

      <!-- ── Sidebar ──────────────────────────────────────────────────────── -->
      <aside class="w-56 flex-shrink-0 flex flex-col overflow-y-auto"
             style="border-right: 1px solid var(--color-border); background-color: var(--color-surface);">

        <div class="px-4 pt-5 pb-3 text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted); border-bottom: 0.5px solid var(--color-border);">Reports</div>

        <nav class="flex flex-col gap-0.5 px-2 py-3">
          <template v-for="item in reportItems" :key="item.key">

            <!-- Locked item -->
            <div v-if="!item.hasAccess"
                 class="flex items-start gap-2.5 px-3 py-2.5 rounded-xl cursor-not-allowed select-none"
                 style="opacity: 0.35;">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    style="color: var(--color-text-muted); font-size: 17px;">lock</span>
              <div>
                <div class="text-xs font-medium" style="color: var(--color-text-muted);">{{ item.label }}</div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </div>

            <!-- Accessible item -->
            <button v-else
                    class="w-full flex items-start gap-2.5 px-3 py-2.5 rounded-xl text-left transition-all"
                    :style="activeReport === item.key
                      ? 'background-color: var(--color-surface-low); border-left: 2.5px solid var(--color-primary);'
                      : 'border-left: 2.5px solid transparent;'"
                    @click="selectReport(item.key)">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    :style="`font-size: 17px; color: ${activeReport === item.key ? 'var(--color-primary)' : 'var(--color-text-muted)'}`">
                {{ item.icon }}
              </span>
              <div>
                <div class="text-xs font-medium"
                     :style="`color: ${activeReport === item.key ? 'var(--color-text)' : 'var(--color-text-muted)'}`">
                  {{ item.label }}
                </div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </button>

          </template>
        </nav>
      </aside>

      <!-- ── Main panel ───────────────────────────────────────────────────── -->
      <main class="flex-1 flex flex-col overflow-hidden" style="background-color: var(--color-bg);">

        <!-- ══ BATCH SUMMARY ═══════════════════════════════════════════════ -->
        <template v-if="activeReport === 'batch-summary'">

          <!-- Header -->
          <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div>
              <h1 class="text-base font-bold" style="color: var(--color-text);">Batch Summary Report</h1>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                Per-batch TAT for endorsement and completion · Excel export
              </p>
            </div>
            <button :disabled="!result || exporting"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="exportExcel">
              <span class="material-symbols-outlined text-sm">download</span>
              {{ exporting ? 'Exporting…' : 'Export Excel' }}
            </button>
          </div>

          <!-- Filters -->
          <div class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Location</label>
              <select v-model="filters.locationCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 90px;">
                <option value="">ALL</option>
                <option v-for="b in branches" :key="b.code" :value="b.code">{{ b.code }}</option>
              </select>
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date from</label>
              <DatePicker v-model="filters.dateFrom"
                          placeholder="Date from"
                          :max-date="filters.dateTo" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date to</label>
              <DatePicker v-model="filters.dateTo"
                          placeholder="Date to"
                          :min-date="filters.dateFrom" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Sample type</label>
              <select v-model="filters.sampleTypeCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 90px;">
                <option value="">ALL</option>
                <option v-for="st in sampleTypes" :key="st.code" :value="st.code">{{ st.name }}</option>
              </select>
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">User</label>
              <input type="text" v-model="filters.userID" placeholder="ALL"
                     class="text-xs px-3 py-2 rounded-xl outline-none transition-all"
                     style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); width: 100px;"
                     @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                     @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
            </div>

            <button :disabled="loading"
                    class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="generate">
              <span class="material-symbols-outlined text-sm"
                    :class="loading ? 'animate-spin' : ''">
                {{ loading ? 'progress_activity' : 'search' }}
              </span>
              {{ loading ? 'Loading…' : 'Generate' }}
            </button>

          </div>

          <!-- Content area -->
          <div class="flex-1 overflow-y-auto px-7 py-6">

            <!-- Initial state -->
            <div v-if="!result && !loading"
                 class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">table_chart</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
              <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
            </div>

            <!-- Loading skeleton -->
            <div v-else-if="loading" class="flex flex-col gap-3">
              <div class="grid grid-cols-5 gap-3 mb-2">
                <div v-for="i in 5" :key="i" class="h-16 rounded-xl animate-pulse"
                     style="background-color: var(--color-surface);"></div>
              </div>
              <div v-for="i in 6" :key="i" class="h-10 rounded-xl animate-pulse"
                   style="background-color: var(--color-surface);"></div>
            </div>

            <!-- Results -->
            <template v-else-if="result">

              <!-- Summary strip -->
              <div class="grid grid-cols-5 gap-3 mb-5">
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-primary);">{{ result.totalBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5"
                       style="color: var(--color-text-muted);">Total batches</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-success);">{{ result.completedBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5"
                       style="color: var(--color-text-muted);">Completed</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-warning);">{{ result.pendingBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5"
                       style="color: var(--color-text-muted);">Pending / Partial</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-xl font-bold" style="color: var(--color-text);">{{ result.avgTatEndorsement ?? '—' }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5"
                       style="color: var(--color-text-muted);">Avg TAT endorsement</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-xl font-bold" style="color: var(--color-text);">{{ result.avgTatCompletion ?? '—' }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5"
                       style="color: var(--color-text-muted);">Avg TAT completion</div>
                </div>
              </div>

              <!-- Empty rows -->
              <div v-if="!result.rows.length"
                   class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
                   style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
                <p class="text-sm font-bold" style="color: var(--color-text);">No batches found</p>
                <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
              </div>

              <!-- Table -->
              <div v-else class="rounded-2xl overflow-hidden"
                   style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
                <div class="overflow-x-auto">
                  <table class="w-full text-xs">
                    <thead>
                      <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Batch no.</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                            style="color: var(--color-text-muted);">Location</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Endorsed</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Endorsed by</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Received</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Received by</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">Completed</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                            style="color: var(--color-text-muted);">Temp</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest"
                            style="color: var(--color-text-muted);">Status</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">TAT (endorse)</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap"
                            style="color: var(--color-text-muted);">TAT (complete)</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="row in result.rows" :key="row.batchNo"
                          style="border-bottom: 0.5px solid var(--color-border);"
                          @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                          @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                        <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">
                          {{ row.batchNo }}
                        </td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ formatDt(row.endorsed) }}</td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.endorsedBy }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">
                          {{ row.procReceived ? formatDt(row.procReceived) : '—' }}
                        </td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.receivedBy ?? '—' }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">
                          {{ row.completed ? formatDt(row.completed) : '—' }}
                        </td>
                        <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.temp ?? '—' }}</td>
                        <td class="px-4 py-2.5">
                          <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                :style="statusStyle(row.status)">
                            {{ row.status }}
                          </span>
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
                        <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">
                          {{ result.avgTatEndorsement ?? '—' }}
                        </td>
                        <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">
                          {{ result.avgTatCompletion ?? '—' }}
                        </td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>

            </template>
          </div>
        </template>

        <!-- ══ COMING SOON ════════════════════════════════════════════════════ -->
        <template v-else>
          <div class="px-7 py-5 flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <h1 class="text-base font-bold" style="color: var(--color-text);">{{ activeItem?.label }}</h1>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ activeItem?.sublabel }}</p>
          </div>
          <div class="flex-1 flex flex-col items-center justify-center gap-4">
            <span class="material-symbols-outlined text-6xl" style="color: var(--color-text-muted);">engineering</span>
            <p class="text-sm font-bold" style="color: var(--color-text);">This report is in the build queue</p>
            <p class="text-xs text-center max-w-xs" style="color: var(--color-text-muted);">
              Pending clarification or implementation. Check back once all requirements are confirmed.
            </p>
          </div>
        </template>

      </main>
    </div>

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
  import DatePicker from '@/components/common/DatePicker.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { reportApi } from '@/api/reportApi'
  import { settingsApi } from '@/api/settingsApi'

  const authStore = useAuthStore()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  function formatDt(val) {
    if (!val) return '—'
    return new Date(val).toLocaleString('en-PH', {
      month: '2-digit', day: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: false
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

  function roleChipClass(role) {
    switch (role) {
      case 'All roles': return 'bg-teal-50 text-teal-800'
      case 'Processing': return 'bg-blue-50  text-blue-800'
      case 'TL': return 'bg-amber-50 text-amber-800'
      case 'Admin': return 'bg-purple-50 text-purple-800'
      default: return 'bg-gray-100  text-gray-600'
    }
  }

  // ── Access helpers ────────────────────────────────────────────────────────────

  const isTLorAdmin = computed(() => authStore.isAdmin || authStore.roleID === 2)
  const isProcessing = computed(() => authStore.isAdmin || authStore.roleID === 2 || authStore.sectionCategory === '2')

  // ── Report nav items ──────────────────────────────────────────────────────────

  const reportItems = computed(() => [
    {
      key: 'unprocessed-specimen',
      label: 'Unprocessed Specimen',
      sublabel: 'ERD / CRD / SRD per section',
      icon: 'science',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-endorsed',
      label: 'Specimen Not Endorsed',
      sublabel: 'Flagged during endorsement',
      icon: 'warning',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-received',
      label: 'Specimen Not Received',
      sublabel: 'Pending / unprocessed',
      icon: 'inventory_2',
      roles: ['Processing', 'TL', 'Admin'],
      hasAccess: isProcessing.value,
    },
    {
      key: 'test-management',
      label: 'Test Management',
      sublabel: 'Running days, TAT, status',
      icon: 'biotech',
      roles: ['TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
    {
      key: 'batch-summary',
      label: 'Batch Summary',
      sublabel: 'TAT endorsement & completion',
      icon: 'table_chart',
      roles: ['Processing', 'TL', 'Admin'],
      hasAccess: isProcessing.value,
    },
    {
      key: 'specimen-receipt-section',
      label: 'Specimen Receipt (Section)',
      sublabel: 'Section receipt TAT',
      icon: 'schedule_send',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'duplicate-endorsement',
      label: 'Duplicate Endorsement',
      sublabel: 'Re-endorsed specimens',
      icon: 'content_copy',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'beyond-14-days',
      label: 'Beyond 14 Days',
      sublabel: 'Old transaction flags',
      icon: 'calendar_clock',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'monthly-summary',
      label: 'Monthly Summary',
      sublabel: 'Aggregate per location',
      icon: 'bar_chart',
      roles: ['TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
  ])

  const firstAccessible = computed(() =>
    reportItems.value.find(i => i.hasAccess)?.key ?? 'batch-summary'
  )

  const activeReport = ref(null)
  const activeItem = computed(() => reportItems.value.find(i => i.key === activeReport.value))

  function selectReport(key) {
    if (activeReport.value === key) return
    activeReport.value = key
    result.value = null
  }

  // ── Reference data ────────────────────────────────────────────────────────────

  const branches = ref([])
  const sampleTypes = ref([])

  async function loadReferenceData() {
    try {
      const [brRes, stRes] = await Promise.all([
        settingsApi.getBranches(),
        settingsApi.getSampleTypes?.() ?? Promise.resolve({ data: [] }),
      ])
      branches.value = (brRes.data ?? []).filter(b => b.active)
      sampleTypes.value = stRes.data ?? []
    } catch {
      // non-critical
    }
  }

  // ── Batch Summary state ───────────────────────────────────────────────────────

  const today = new Date().toISOString().slice(0, 10)
  const filters = ref({
    locationCode: '',
    dateFrom: today,
    dateTo: today,
    sampleTypeCode: '',
    userID: '',
  })

  const loading = ref(false)
  const exporting = ref(false)
  const result = ref(null)

  async function generate() {
    loading.value = true
    result.value = null
    try {
      result.value = await reportApi.getBatchSummary({
        locationCode: filters.value.locationCode || null,
        dateFrom: filters.value.dateFrom,
        dateTo: filters.value.dateTo,
        sampleTypeCode: filters.value.sampleTypeCode || null,
        userID: filters.value.userID.trim() || null,
      })
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      loading.value = false
    }
  }

  // ── Excel export ──────────────────────────────────────────────────────────────

  async function exportExcel() {
    if (!result.value) return
    exporting.value = true
    try {
      await reportApi.exportBatchSummaryExcel({
        locationCode: filters.value.locationCode || null,
        dateFrom: filters.value.dateFrom,
        dateTo: filters.value.dateTo,
        sampleTypeCode: filters.value.sampleTypeCode || null,
        userID: filters.value.userID.trim() || null,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      exporting.value = false
    }
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────────

  onMounted(() => {
    activeReport.value = firstAccessible.value
    loadReferenceData()
  })
</script>
