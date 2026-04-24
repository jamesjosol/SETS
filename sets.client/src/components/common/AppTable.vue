<template>
  <div>
    <!-- Loading -->
    <div v-if="loading"
         class="flex items-center justify-center py-12 gap-3"
         style="color: var(--color-text-muted);">
      <span class="material-symbols-outlined text-xl animate-spin">progress_activity</span>
      <span class="text-sm font-medium">{{ loadingText }}</span>
    </div>

    <!-- Empty -->
    <div v-else-if="!rows.length"
         class="py-12 flex flex-col items-center gap-2"
         style="color: var(--color-text-muted);">
      <span class="material-symbols-outlined text-3xl">{{ emptyIcon }}</span>
      <p class="text-sm font-medium">{{ emptyText }}</p>
    </div>

    <!-- Table -->
    <template v-else>
      <table class="w-full text-sm">
        <thead>
          <tr style="border-bottom: 1px solid var(--color-border);">
            <th v-for="col in columns" :key="col.key"
                class="text-left pb-3 text-[10px] font-bold uppercase tracking-widest"
                :class="col.class ?? 'pr-6'"
                style="color: var(--color-text-muted);">
              {{ col.label }}
            </th>
            <!-- Actions column header (empty) -->
            <th v-if="$slots.actions" class="pb-3"></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(row, idx) in paginated" :key="rowKey ? row[rowKey] : idx"
              style="border-bottom: 1px solid var(--color-surface-low);"
              :class="onRowClick ? 'cursor-pointer' : ''"
              @click="onRowClick?.(row)"
              @mouseenter="(e) => { if (onRowClick) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
              @mouseleave="(e) => { if (onRowClick) e.currentTarget.style.backgroundColor = 'transparent' }">

            <!-- Each column cell via slot or default -->
            <td v-for="col in columns" :key="col.key"
                class="py-3"
                :class="col.class ?? 'pr-6'">
              <!-- Named slot per column: #cell-[key] -->
              <slot :name="`cell-${col.key}`" :row="row" :value="row[col.key]">
                <span style="color: var(--color-text);">{{ row[col.key] ?? '—' }}</span>
              </slot>
            </td>

            <!-- Actions slot -->
            <td v-if="$slots.actions" class="py-3">
              <slot name="actions" :row="row" />
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <AppPagination v-model="page"
                     :total="rows.length"
                     :page-size="pageSize" />
    </template>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import AppPagination from './AppPagination.vue'

const props = defineProps({
  // Data
  rows: {
    type: Array,
    default: () => [],
  },
  columns: {
    // [{ key: 'userName', label: 'Name', class: 'pr-6' }]
    type: Array,
    required: true,
  },
  rowKey: {
    // field name to use as :key, e.g. 'id', 'userID', 'code'
    type: String,
    default: null,
  },

  // Pagination
  pageSize: {
    type: Number,
    default: 10,
  },

  // States
  loading: {
    type: Boolean,
    default: false,
  },
  loadingText: {
    type: String,
    default: 'Loading...',
  },
  emptyText: {
    type: String,
    default: 'No data found.',
  },
  emptyIcon: {
    type: String,
    default: 'inbox',
  },

  // Optional row click handler
  onRowClick: {
    type: Function,
    default: null,
  },
})

// ── Pagination ───────────────────────────────────────────────────────────────
const page = ref(1)

// Reset to page 1 when rows change (e.g. after search/filter)
watch(() => props.rows, () => { page.value = 1 })

const paginated = computed(() => {
  const start = (page.value - 1) * props.pageSize
  return props.rows.slice(start, start + props.pageSize)
})
</script>
