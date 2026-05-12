<template>
  <!-- Table Card -->
  <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

    <!-- Card header -->
    <div class="px-8 py-5 flex items-center justify-between" style="border-bottom: 1px solid var(--color-surface-low);">
      <h2 class="text-base font-bold" style="color: var(--color-text);">{{ title }}</h2>
      <div class="flex items-center gap-4">
        <span class="text-xs font-bold flex items-center gap-1" style="color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm">calendar_month</span>
          {{ formatDateLabel(dateFrom) }} — {{ formatDateLabel(dateTo) }}
        </span>
        <span v-if="!loading && !error" class="text-xs font-bold" style="color: var(--color-text-muted);">
          {{ recordCount }} record{{ recordCount !== 1 ? 's' : '' }}
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
              @click="$emit('retry')">
        Retry
      </button>
    </div>

    <!-- Empty -->
    <div v-else-if="recordCount === 0" class="px-8 py-16 flex flex-col items-center gap-3">
      <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
      <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ emptyLabel }}</p>
      <p class="text-xs" style="color: var(--color-text-muted);">
        {{ hasActiveClientFilters ? 'Try adjusting your filters.' : `No ${emptyContext} in this date range.` }}
      </p>
    </div>

    <!-- Table -->
    <div v-else class="overflow-x-auto">
      <table class="w-full text-left">
        <thead>
          <tr style="background-color: var(--color-bg);">
            <slot name="head" />
          </tr>
        </thead>
        <tbody ref="tableBodyRef">
          <slot name="body" />
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
              @click="$emit('update:currentPage', currentPage - 1)">
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
                @click="$emit('update:currentPage', p)">
          {{ p }}
        </button>
      </div>

      <button :disabled="currentPage === totalPages"
              class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
              style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
              @click="$emit('update:currentPage', currentPage + 1)">
        Next
        <span class="material-symbols-outlined text-sm">chevron_right</span>
      </button>
    </div>

  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'
import { gsap } from 'gsap'

const props = defineProps({
  title:                { type: String,  required: true },
  emptyLabel:           { type: String,  default: 'No records found' },
  emptyContext:         { type: String,  default: 'records' },
  loading:              { type: Boolean, default: false },
  error:                { type: String,  default: null },
  recordCount:          { type: Number,  required: true },
  totalPages:           { type: Number,  required: true },
  currentPage:          { type: Number,  required: true },
  pageNumbers:          { type: Array,   required: true },
  dateFrom:             { type: String,  default: null },
  dateTo:               { type: String,  default: null },
  hasActiveClientFilters: { type: Boolean, default: false },
})

defineEmits(['retry', 'update:currentPage'])

// ── GSAP row animation ─────────────────────────────────────────────────────

const tableBodyRef = ref(null)

async function animateTableRows() {
  await nextTick()
  if (!tableBodyRef.value) return
  const rows = tableBodyRef.value.querySelectorAll('tr')
  if (!rows.length) return
  gsap.set(rows, { opacity: 0, x: -6 })
  gsap.to(rows, {
    opacity: 1,
    x: 0,
    duration: 0.18,
    stagger: 0.025,
    ease: 'power1.out',
  })
}

// Re-animate whenever loading finishes (loading flips false → rows appear)
watch(() => props.loading, (val) => {
  if (!val) animateTableRows()
})

// Re-animate on page change
watch(() => props.currentPage, animateTableRows)

// Expose so parent can trigger manually (e.g. after client-side filter change)
defineExpose({ animateTableRows })

// ── Helpers ────────────────────────────────────────────────────────────────

function formatDateLabel(dateStr) {
  if (!dateStr) return '—'
  return new Date(dateStr + 'T00:00:00').toLocaleDateString('en-US', {
    month: 'short', day: 'numeric', year: 'numeric'
  })
}
</script>
