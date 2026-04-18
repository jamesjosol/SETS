<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-4">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Endorsements</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span>· {{ authStore.branchCode }} · All records

        </p>
      </div>

      <!-- Filters -->
      <div class="flex items-center gap-3 flex-wrap">

        <!-- Search (client-side) -->
        <div class="relative">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
                style="color: var(--color-text-muted);">search</span>
          <input v-model="searchQuery"
                 type="text"
                 placeholder="Batch No. or User..."
                 class="pl-9 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none border transition-all w-56"
                 style="background-color: var(--color-surface-low); color: var(--color-text); border-color: var(--color-border);"
                 @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                 @blur="e => e.target.style.borderColor = 'var(--color-border)'" />
        </div>

        <!-- Status Filter (client-side) -->
        <DropdownSelect v-model="statusFilter"
                        icon="filter_list"
                        :options="[
                  { value: '',   label: 'All Status' },
                  { value: 'P',  label: 'Pending'    },
                  { value: 'PA', label: 'Partial'    },
                  { value: 'C',  label: 'Completed'  },
                ]" />

        <!-- Date From (server-side — triggers re-fetch on change) -->
        <DatePicker v-model="dateFrom"
                    placeholder="Date From"
                    :max-date="dateTo"
                    @change="load" />
        <span class="text-xs font-bold" style="color: var(--color-text-muted);">to</span>
        <DatePicker v-model="dateTo"
                    placeholder="Date To"
                    :min-date="dateFrom"
                    @change="load" />

        <!-- Clear Filters -->
        <button v-if="hasActiveFilters"
                class="flex items-center gap-1.5 px-3 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                @click="clearFilters">
          <span class="material-symbols-outlined text-sm">filter_list_off</span>
          Clear
        </button>
      </div>
    </div>

    <!-- Table Card -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- Card header -->
      <div class="px-8 py-5 flex items-center justify-between" style="border-bottom: 1px solid var(--color-surface-low);">
        <h2 class="text-base font-bold" style="color: var(--color-text);">All Endorsements</h2>
        <div class="flex items-center gap-4">
          <span class="text-xs font-bold flex items-center gap-1" style="color: var(--color-text-muted);">
            <span class="material-symbols-outlined text-sm">calendar_month</span>
            {{ formatDateLabel(dateFrom) }} — {{ formatDateLabel(dateTo) }}
          </span>
          <span v-if="!loading && !error" class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ filteredBatches.length }} record{{ filteredBatches.length !== 1 ? 's' : '' }}
            <template v-if="totalPages > 1">
              · Page {{ currentPage }} of {{ totalPages }}
            </template>
          </span>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="px-8 py-16 flex items-center justify-center gap-3">
        <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
        <span class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</span>
      </div>

      <!-- Error -->
      <div v-else-if="error" class="px-8 py-16 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-error);">error_outline</span>
        <p class="text-sm font-bold" style="color: var(--color-text-muted);">{{ error }}</p>
        <button class="mt-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                @click="load">
          Retry
        </button>
      </div>

      <!-- Empty -->
      <div v-else-if="filteredBatches.length === 0" class="px-8 py-16 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
        <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">No endorsements found</p>
        <p class="text-xs" style="color: var(--color-text-muted);">
          {{ hasActiveClientFilters ? 'Try adjusting your filters.' : 'No endorsements in this date range.' }}
        </p>
      </div>

      <!-- Table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-left">
          <thead>
            <tr style="background-color: var(--color-bg);">
              <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Batch No.</th>
              <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Date &amp; Time Endorsed</th>
              <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Endorsed By</th>
              <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Destination</th>
              <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest"
                  style="color: var(--color-text-muted);">Status</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="batch in paginatedBatches"
                :key="batch.batchNo"
                class="cursor-pointer transition-colors"
                style="border-top: 1px solid var(--color-surface-low);"
                @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                @click="openDrawer(batch.batchNo)">

              <td class="px-8 py-4 font-mono text-xs font-bold"
                  style="color: var(--color-primary);">
                {{ batch.batchNo }}
              </td>

              <td class="px-4 py-4 text-sm"
                  style="color: var(--color-text-muted);">
                {{ formatDateTime(batch.endorsed) }}
              </td>

              <td class="px-4 py-4 text-sm font-bold"
                  style="color: var(--color-text);">
                {{ batch.endorsedBy }}
              </td>

              <td class="px-4 py-4 text-sm"
                  style="color: var(--color-text-muted);">
                {{ batch.destination }}
              </td>

              <td class="px-8 py-4">
                <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                      :style="getBatchStatusStyle(batch.status)">
                  <span class="w-1.5 h-1.5 rounded-full"
                        :style="`background-color: ${getBatchStatusDot(batch.status)}`"></span>
                  {{ getBatchStatusLabel(batch.status) }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div v-if="!loading && !error && totalPages > 1"
           class="px-8 py-4 flex items-center justify-between"
           style="border-top: 1px solid var(--color-surface-low);">
        <button :disabled="currentPage === 1"
                class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                @click="currentPage--">
          <span class="material-symbols-outlined text-sm">chevron_left</span>
          Previous
        </button>

        <div class="flex items-center gap-1.5">
          <button v-for="p in pageNumbers"
                  :key="p"
                  class="w-8 h-8 rounded-lg text-xs font-bold transition-all"
                  :style="p === currentPage
                    ? 'background-color: var(--color-primary); color: #fff;'
                    : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                  @click="currentPage = p">
            {{ p }}
          </button>
        </div>

        <button :disabled="currentPage === totalPages"
                class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                @click="currentPage++">
          Next
          <span class="material-symbols-outlined text-sm">chevron_right</span>
        </button>
      </div>

    </div>

    <!-- Batch Detail Drawer -->
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
  import { ref, computed, onMounted, watch } from 'vue'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import BatchDetailDrawer from '@/components/common/BatchDetailDrawer.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import DatePicker from '@/components/common/DatePicker.vue'
  import DropdownSelect from '@/components/common/DropdownSelect.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { batchApi } from '@/api/batchApi'

  const authStore = useAuthStore()

  // ── Alert ──────────────────────────────────────────────────────────────────

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Date range helpers ─────────────────────────────────────────────────────

  function toInputDate(date) {
    return date.toISOString().slice(0, 10)   // yyyy-MM-dd
  }

  function defaultDateFrom() {
    const d = new Date()
    d.setDate(d.getDate() - 30)
    return toInputDate(d)
  }

  function defaultDateTo() {
    return toInputDate(new Date())
  }

  // ── Server-side date range ─────────────────────────────────────────────────
  // Changing either input triggers @change → load()

  const dateFrom = ref(defaultDateFrom())
  const dateTo = ref(defaultDateTo())

  // ── Client-side filters ────────────────────────────────────────────────────
  // Applied on the already-fetched slice — no re-fetch needed

  const searchQuery = ref('')
  const statusFilter = ref('')

  const hasActiveClientFilters = computed(() =>
    !!searchQuery.value || !!statusFilter.value
  )

  const hasActiveFilters = computed(() =>
    hasActiveClientFilters.value ||
    dateFrom.value !== defaultDateFrom() ||
    dateTo.value !== defaultDateTo()
  )

  function clearFilters() {
    searchQuery.value = ''
    statusFilter.value = ''
    const prevFrom = dateFrom.value
    const prevTo = dateTo.value
    dateFrom.value = defaultDateFrom()
    dateTo.value = defaultDateTo()
    // Only re-fetch if dates actually changed
    if (prevFrom !== dateFrom.value || prevTo !== dateTo.value) load()
  }

  // ── Data ───────────────────────────────────────────────────────────────────

  const batches = ref([])
  const loading = ref(true)
  const error = ref(null)

  async function load() {
    if (dateFrom.value && dateTo.value && dateFrom.value > dateTo.value) {
      showAlert('warning', 'Invalid Date Range', '"Date From" cannot be later than "Date To".')
      return
    }

    loading.value = true
    error.value = null
    currentPage.value = 1

    try {
      if (authStore.isAdmin) {
        batches.value = await batchApi.getAllEndorsements(dateFrom.value, dateTo.value)
      } else {
        batches.value = await batchApi.getEndorsements(authStore.sectionCode, dateFrom.value, dateTo.value)
      }
    } catch (err) {
      error.value = err.response?.data?.message ?? 'Failed to load endorsements.'
      showAlert('error', 'Load Error', error.value)
    } finally {
      loading.value = false
    }
  }

  onMounted(load)

  // Reset page when client-side filters change
  watch([searchQuery, statusFilter], () => {
    currentPage.value = 1
  })

  // ── Client-side filtering ──────────────────────────────────────────────────

  const filteredBatches = computed(() => {
    let list = batches.value

    if (searchQuery.value.trim()) {
      const q = searchQuery.value.trim().toLowerCase()
      list = list.filter(b =>
        b.batchNo.toLowerCase().includes(q) ||
        b.endorsedBy?.toLowerCase().includes(q)
      )
    }

    if (statusFilter.value) {
      list = list.filter(b => b.status === statusFilter.value)
    }

    return list
  })

  // ── Pagination ─────────────────────────────────────────────────────────────

  const PAGE_SIZE = 20
  const currentPage = ref(1)

  const totalPages = computed(() =>
    Math.max(1, Math.ceil(filteredBatches.value.length / PAGE_SIZE))
  )

  const paginatedBatches = computed(() => {
    const start = (currentPage.value - 1) * PAGE_SIZE
    return filteredBatches.value.slice(start, start + PAGE_SIZE)
  })

  const pageNumbers = computed(() => {
    const total = totalPages.value
    const cur = currentPage.value
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
    let start = Math.max(1, cur - 2)
    let end = Math.min(total, start + 4)
    start = Math.max(1, end - 4)
    return Array.from({ length: end - start + 1 }, (_, i) => start + i)
  })

  // ── Drawer ─────────────────────────────────────────────────────────────────

  const drawerOpen = ref(false)
  const drawerLoading = ref(false)
  const drawerData = ref(null)

  async function openDrawer(batchNo) {
    drawerOpen.value = true
    drawerLoading.value = true
    drawerData.value = null
    try {
      drawerData.value = await batchApi.getBatchDetail(batchNo)
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        showAlert('error', 'Error', 'Unable to load batch details.')
      }
      drawerOpen.value = false
    } finally {
      drawerLoading.value = false
    }
  }

  function closeDrawer() {
    drawerOpen.value = false
    drawerData.value = null
  }

  // ── Helpers ────────────────────────────────────────────────────────────────

  function formatDateTime(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-US', {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: true
    })
  }

  function formatDateLabel(dateStr) {
    if (!dateStr) return '—'
    return new Date(dateStr + 'T00:00:00').toLocaleDateString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric'
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
</script>
