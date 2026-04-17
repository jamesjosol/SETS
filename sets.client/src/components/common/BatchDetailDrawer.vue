<template>
  <!-- Backdrop -->
  <transition name="fade">
    <div v-if="isOpen"
         class="fixed inset-0 z-[60]"
         style="background-color: rgba(0,0,0,0.4);"
         @click="$emit('close')"></div>
  </transition>

  <!-- Drawer Panel -->
  <transition name="slide">
    <div v-if="isOpen"
         class="fixed top-0 right-0 h-full z-[70] flex flex-col"
         style="width: 480px; background-color: var(--color-surface); border-left: 1px solid var(--color-border); box-shadow: -4px 0 24px rgba(0,0,0,0.12);">

      <!-- Header -->
      <div class="px-6 py-5 flex items-center justify-between"
           style="border-bottom: 1px solid var(--color-border);">
        <div>
          <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
             style="color: var(--color-text-muted);">Batch Detail</p>
          <p class="text-base font-extrabold font-mono"
             style="color: var(--color-primary);">
            {{ loading ? '...' : data?.batchNo }}
          </p>
        </div>
        <button class="p-2 rounded-xl transition-all"
                style="color: var(--color-text-muted);"
                @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                @click="$emit('close')">
          <span class="material-symbols-outlined">close</span>
        </button>
      </div>

      <!-- Loading -->
      <div v-if="loading"
           class="flex-1 flex items-center justify-center gap-3">
        <span class="material-symbols-outlined animate-spin"
              style="color: var(--color-text-muted);">progress_activity</span>
        <p class="text-xs font-bold uppercase tracking-widest"
           style="color: var(--color-text-muted);">Loading...</p>
      </div>

      <template v-else-if="data">

        <!-- Batch Info -->
        <div class="px-6 py-4 space-y-3"
             style="border-bottom: 1px solid var(--color-border);">
          <div class="grid grid-cols-2 gap-3">
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Location</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">{{ data.location }}</p>
            </div>
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Endorsed By</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">{{ data.endorsedBy }}</p>
            </div>
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Date & Time Endorsed</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">
                {{ formatDateTime(data.endorsed) }}
              </p>
            </div>
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Destination</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">{{ data.destination }}</p>
            </div>

            <!-- Receiver-specific fields — only shown when available -->
            <div v-if="data.procReceived !== undefined">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Bag Received</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">
                {{ data.procReceived ? formatDateTime(data.procReceived) : '—' }}
              </p>
            </div>
            <div v-if="data.completed !== undefined">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Completed</p>
              <p class="text-sm font-bold" style="color: var(--color-text);">
                {{ data.completed ? formatDateTime(data.completed) : '—' }}
              </p>
            </div>

            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                 style="color: var(--color-text-muted);">Status</p>
              <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                    :style="getBatchStatusStyle(data.status)">
                <span class="w-1.5 h-1.5 rounded-full"
                      :style="`background-color: ${getBatchStatusDot(data.status)}`"></span>
                {{ getBatchStatusLabel(data.status) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Tabs -->
        <div class="flex px-6 pt-4 gap-1"
             style="border-bottom: 1px solid var(--color-border);">
          <button v-for="tab in ['Specimens', 'Non-Barcoded']"
                  :key="tab"
                  class="px-4 py-2 text-xs font-bold uppercase tracking-widest rounded-t-lg transition-all"
                  :style="activeTab === tab
                    ? 'color: var(--color-primary); border-bottom: 2px solid var(--color-primary); margin-bottom: -1px;'
                    : 'color: var(--color-text-muted);'"
                  @click="activeTab = tab">
            {{ tab }}
            <span class="ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full font-bold"
                  :style="activeTab === tab
                    ? 'background-color: var(--color-primary-soft); color: var(--color-primary);'
                    : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
              {{ tab === 'Specimens' ? data.specimens.length : data.nonBarcoded.length }}
            </span>
          </button>
        </div>

        <!-- Tab Content -->
        <div class="flex-1 overflow-y-auto px-6 py-4">

          <!-- Specimens Tab -->
          <template v-if="activeTab === 'Specimens'">
            <div v-if="data.specimens.length === 0"
                 class="flex flex-col items-center justify-center py-12 gap-2">
              <span class="material-symbols-outlined text-3xl"
                    style="color: var(--color-text-muted);">biotech</span>
              <p class="text-xs font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">No specimens</p>
            </div>
            <div v-else class="space-y-2">
              <div v-for="sp in data.specimens"
                   :key="sp.id"
                   class="rounded-xl p-4"
                   style="background-color: var(--color-surface-low);">

                <!-- Top row: specimen no + status -->
                <div class="flex justify-between items-start mb-2">
                  <p class="text-xs font-bold font-mono"
                     style="color: var(--color-primary);">{{ sp.specimenNo }}</p>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                        :style="sp.status === 'R'
            ? 'background-color: var(--color-success-soft); color: var(--color-success);'
            : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                    {{ sp.status === 'R' ? 'Received' : 'Pending' }}
                  </span>
                </div>

                <!-- Patient name -->
                <p class="text-sm font-bold mb-0.5"
                   style="color: var(--color-text);">{{ sp.patientName }}</p>

                <!-- Sample type + lab no + remark icons all on same row -->
                <div class="flex items-center gap-3 mt-1">
                  <span class="text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted);">{{ sp.sampleTypeName }}</span>
                  <span class="text-[10px]" style="color: var(--color-text-muted);">·</span>
                  <span class="text-[10px] font-mono"
                        style="color: var(--color-text-muted);">{{ sp.labNo }}</span>

                  <!-- Push icons to the right -->
                  <div class="ml-auto flex items-center gap-1.5">

                    <!-- Endorsement Remarks -->
                    <div class="relative group/endrem">
                      <button class="w-5 h-5 rounded-full flex items-center justify-center transition-all"
                              :style="sp.remarks
                ? 'color: var(--color-warning);'
                : 'color: var(--color-text-muted); opacity: 0.3;'"
                              :disabled="!sp.remarks">
                        <span class="material-symbols-outlined" style="font-size: 14px;">
                          {{ sp.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                        </span>
                      </button>
                      <div v-if="sp.remarks"
                           class="absolute bottom-full right-0 mb-2 w-56 rounded-xl p-3 text-xs shadow-xl z-10 pointer-events-none opacity-0 group-hover/endrem:opacity-100 transition-opacity"
                           style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                        <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                           style="color: var(--color-warning);">Endorsement Remarks</p>
                        <p class="whitespace-pre-wrap" style="color: var(--color-text);">{{ sp.remarks }}</p>
                      </div>
                    </div>

                    <!-- Receiving Remarks -->
                    <div class="relative group/recrem">
                      <button class="w-5 h-5 rounded-full flex items-center justify-center transition-all"
                              :style="sp.receivingRemarks
                ? 'color: var(--color-primary);'
                : 'color: var(--color-text-muted); opacity: 0.3;'"
                              :disabled="!sp.receivingRemarks">
                        <span class="material-symbols-outlined" style="font-size: 14px;">
                          {{ sp.receivingRemarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                        </span>
                      </button>
                      <div v-if="sp.receivingRemarks"
                           class="absolute bottom-full right-0 mb-2 w-56 rounded-xl p-3 text-xs shadow-xl z-10 pointer-events-none opacity-0 group-hover/recrem:opacity-100 transition-opacity"
                           style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                        <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                           style="color: var(--color-primary);">Receiving Remarks</p>
                        <p class="whitespace-pre-wrap" style="color: var(--color-text);">{{ sp.receivingRemarks }}</p>
                      </div>
                    </div>

                  </div>
                </div>

              </div>
            </div>
          </template>

          <!-- Non-Barcoded Tab -->
          <template v-else>
            <div v-if="data.nonBarcoded.length === 0"
                 class="flex flex-col items-center justify-center py-12 gap-2">
              <span class="material-symbols-outlined text-3xl"
                    style="color: var(--color-text-muted);">description</span>
              <p class="text-xs font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">No non-barcoded items</p>
            </div>
            <div v-else class="space-y-2">
              <div v-for="nb in data.nonBarcoded"
                   :key="nb.itemID"
                   class="rounded-xl p-4"
                   style="background-color: var(--color-surface-low);">

                <!-- Top row: description + status -->
                <div class="flex justify-between items-start mb-1">
                  <p class="text-sm font-bold"
                     style="color: var(--color-text);">{{ nb.description }}</p>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                        :style="nb.status === 'R'
            ? 'background-color: var(--color-success-soft); color: var(--color-success);'
            : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                    {{ nb.status === 'R' ? 'Received' : 'Pending' }}
                  </span>
                </div>

                <!-- Qty + remark icons on same row -->
                <div class="flex items-center gap-3 mt-1">
                  <span class="text-[10px] font-bold uppercase tracking-widest"
                        style="color: var(--color-text-muted);">Qty: {{ nb.quantity }}</span>

                  <!-- Push icons to the right -->
                  <div class="ml-auto flex items-center gap-1.5">

                    <!-- Endorsement Remarks -->
                    <div class="relative group/endrem">
                      <button class="w-5 h-5 rounded-full flex items-center justify-center transition-all"
                              :style="nb.remarks
                  ? 'color: var(--color-warning);'
                  : 'color: var(--color-text-muted); opacity: 0.3;'"
                              :disabled="!nb.remarks">
                        <span class="material-symbols-outlined" style="font-size: 14px;">
                          {{ nb.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                        </span>
                      </button>
                      <div v-if="nb.remarks"
                           class="absolute bottom-full right-0 mb-2 w-56 rounded-xl p-3 text-xs shadow-xl z-10 pointer-events-none opacity-0 group-hover/endrem:opacity-100 transition-opacity"
                           style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                        <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                           style="color: var(--color-warning);">Endorsement Remarks</p>
                        <p class="whitespace-pre-wrap" style="color: var(--color-text);">{{ nb.remarks }}</p>
                      </div>
                    </div>

                    <!-- Receiving Remarks -->
                    <div class="relative group/recrem">
                      <button class="w-5 h-5 rounded-full flex items-center justify-center transition-all"
                              :style="nb.receivingRemarks
                  ? 'color: var(--color-primary);'
                  : 'color: var(--color-text-muted); opacity: 0.3;'"
                              :disabled="!nb.receivingRemarks">
                        <span class="material-symbols-outlined" style="font-size: 14px;">
                          {{ nb.receivingRemarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                        </span>
                      </button>
                      <div v-if="nb.receivingRemarks"
                           class="absolute bottom-full right-0 mb-2 w-56 rounded-xl p-3 text-xs shadow-xl z-10 pointer-events-none opacity-0 group-hover/recrem:opacity-100 transition-opacity"
                           style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                        <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                           style="color: var(--color-primary);">Receiving Remarks</p>
                        <p class="whitespace-pre-wrap" style="color: var(--color-text);">{{ nb.receivingRemarks }}</p>
                      </div>
                    </div>

                  </div>
                </div>

              </div>
            </div>
          </template>

        </div>
      </template>

    </div>
  </transition>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  isOpen:  { type: Boolean, default: false },
  loading: { type: Boolean, default: false },
  data:    { type: Object,  default: null  },
})

defineEmits(['close'])

const activeTab = ref('Specimens')

// Reset tab when drawer opens
watch(() => props.isOpen, (val) => {
  if (val) activeTab.value = 'Specimens'
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
