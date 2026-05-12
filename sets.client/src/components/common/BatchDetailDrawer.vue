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

            <!-- Temp / TempRemarks / BagNo — subtle hover icons, only shown if value exists -->
            <div v-if="data.temp || data.tempRemarks || data.bagNo"
                 class="col-span-2">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-2"
                 style="color: var(--color-text-muted);">Bag Info</p>
              <div class="flex items-center gap-3">

                <!-- Temperature -->
                <div v-if="data.temp" class="relative group/temp">
                  <div class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg cursor-default"
                       style="background-color: var(--color-surface-low);">
                    <span class="material-symbols-outlined"
                          style="font-size: 14px; color: var(--color-text-muted);">thermometer</span>
                    <span class="text-xs font-bold"
                          style="color: var(--color-text);">{{ data.temp }} °C</span>
                  </div>
                  <div v-if="data.tempRemarks"
                       class="absolute bottom-full left-0 mb-2 w-48 rounded-xl p-3 text-xs shadow-xl z-10 pointer-events-none opacity-0 group-hover/temp:opacity-100 transition-opacity"
                       style="background-color: var(--color-surface); border: 1px solid var(--color-border);">
                    <p class="text-[10px] font-bold uppercase tracking-widest mb-1"
                       style="color: var(--color-text-muted);">Temp Remarks</p>
                    <p style="color: var(--color-text);">{{ data.tempRemarks }}</p>
                  </div>
                </div>

                <!-- Bag No -->
                <div v-if="data.bagNo" class="relative group/bagno">
                  <div class="flex items-center gap-1.5 px-3 py-1.5 rounded-lg cursor-default"
                       style="background-color: var(--color-surface-low);">
                    <span class="material-symbols-outlined"
                          style="font-size: 14px; color: var(--color-text-muted);">shopping_bag</span>
                    <span class="text-xs font-bold"
                          style="color: var(--color-text);">{{ data.bagNo }}</span>
                  </div>
                </div>

              </div>
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
              {{ tab === 'Specimens' ? visibleSpecimens.length : visibleNonBarcoded.length }}
            </span>
          </button>
        </div>

        <!-- Tab Content -->
        <div class="flex-1 overflow-y-auto px-6 py-4">

          <!-- Specimens Tab -->
          <template v-if="activeTab === 'Specimens'">
            <div v-if="visibleSpecimens.length === 0"
                 class="flex flex-col items-center justify-center py-12 gap-2">
              <span class="material-symbols-outlined text-3xl"
                    style="color: var(--color-text-muted);">biotech</span>
              <p class="text-xs font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">No specimens</p>
            </div>
            <div v-else class="space-y-2">
              <div v-for="sp in visibleSpecimens"
                   :key="sp.id"
                   class="rounded-xl p-4"
                   :style="sp.status === 'X'
                     ? 'background-color: var(--color-surface-low); opacity: 0.6;'
                     : 'background-color: var(--color-surface-low);'">

                <!-- Top row: specimen no + status badge + cancel button -->
                <div class="flex justify-between items-start mb-2">
                  <p class="text-xs font-bold font-mono flex items-center gap-1.5"
                     :style="sp.status === 'X'
                 ? 'color: var(--color-text-muted); text-decoration: line-through;'
                 : 'color: var(--color-primary);'">
                    {{ sp.specimenNo }}
                    <span v-if="data.isOutbound && !sp.isPostedToDest && sp.status !== 'X'"
                          class="material-symbols-outlined flex-shrink-0"
                          style="font-size: 13px; color: var(--color-warning);"
                          title="Transaction not yet posted to destination HCLAB">
                      cloud_off
                    </span>
                  </p>

                  <div class="flex items-center gap-2">
                    <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                          :style="sp.status === 'R'
                            ? 'background-color: var(--color-success-soft); color: var(--color-success);'
                            : sp.status === 'X'
                            ? 'background-color: var(--color-surface); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                            : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                      {{ sp.status === 'R' ? 'Received' : sp.status === 'X' ? 'Cancelled' : 'Pending' }}
                    </span>
                    <!-- Cancel button — TL or admin only, received specimens only -->
                    <button v-if="allowCancel && (sp.status === 'R' || sp.status === 'P') && (authStore.roleID === 2 || authStore.isAdmin)"
                            class="w-5 h-5 rounded-full flex items-center justify-center transition-all opacity-20 hover:opacity-60"
                            style="color: var(--color-text-muted);"
                            title="Cancel receiving"
                            @click.stop="openCancelModal(sp)">
                      <span class="material-symbols-outlined" style="font-size: 13px;">cancel</span>
                    </button>
                  </div>
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

                <!-- Cancelled info strip -->
                <div v-if="sp.status === 'X'"
                     class="mt-2 pt-2"
                     style="border-top: 1px solid var(--color-border);">
                  <p class="text-[10px] font-bold uppercase tracking-widest mb-0.5"
                     style="color: var(--color-text-muted);">Cancelled</p>
                  <p class="text-[10px]" style="color: var(--color-text-muted);">
                    {{ sp.cancelledBy }} · {{ formatDateTime(sp.cancelledAt) }}
                  </p>
                  <p v-if="sp.cancelReason"
                     class="text-[10px] italic mt-0.5"
                     style="color: var(--color-text-muted);">
                    "{{ sp.cancelReason }}"
                  </p>
                </div>

              </div>
            </div>
          </template>

          <!-- Non-Barcoded Tab -->
          <template v-else>
            <div v-if="visibleNonBarcoded.length === 0"
                 class="flex flex-col items-center justify-center py-12 gap-2">
              <span class="material-symbols-outlined text-3xl"
                    style="color: var(--color-text-muted);">description</span>
              <p class="text-xs font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">No non-barcoded items</p>
            </div>
            <div v-else class="space-y-2">
              <div v-for="nb in visibleNonBarcoded"
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

  <!-- ── Cancel Specimen Modal ──────────────────────────────────────────── -->
  <transition name="fade">
    <div v-if="cancelModal.visible"
         class="fixed inset-0 z-[90] flex items-center justify-center"
         style="background-color: rgba(0,0,0,0.5);">
      <div class="rounded-2xl p-6 w-80 flex flex-col gap-4"
           style="background-color: var(--color-surface); border: 1px solid var(--color-border); box-shadow: 0 8px 32px rgba(0,0,0,0.2);">

        <!-- Header -->
        <div class="flex items-center gap-3">
          <span class="material-symbols-outlined text-xl"
                style="color: var(--color-text-muted);">cancel</span>
          <div>
            <p class="text-sm font-extrabold" style="color: var(--color-text);">Cancel Receiving</p>
            <p class="text-[10px] font-mono" style="color: var(--color-text-muted);">
              {{ cancelModal.specimenNo }}
            </p>
          </div>
        </div>

        <!-- Reason input -->
        <div>
          <label class="block text-[10px] font-bold uppercase tracking-widest mb-1.5"
                 style="color: var(--color-text-muted);">
            Reason <span style="color: var(--color-warning);">*</span>
          </label>
          <textarea v-model="cancelModal.reason"
                    rows="3"
                    placeholder="State the reason for cancelling..."
                    class="w-full rounded-xl px-3 py-2 text-xs outline-none resize-none"
                    style="background-color: var(--color-surface-low); color: var(--color-text); border: 1px solid var(--color-border);" />
          <p v-if="cancelModal.error"
             class="text-[10px] mt-1"
             style="color: var(--color-warning);">
            {{ cancelModal.error }}
          </p>
        </div>

        <!-- Actions -->
        <div class="flex gap-2">
          <button class="flex-1 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  :disabled="cancelModal.saving"
                  @click="cancelModal.visible = false">
            Dismiss
          </button>
          <button class="flex-1 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all flex items-center justify-center gap-1.5"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);"
                  :disabled="cancelModal.saving"
                  @click="confirmCancel">
            <span v-if="cancelModal.saving"
                  class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
            {{ cancelModal.saving ? 'Cancelling...' : 'Confirm' }}
          </button>
        </div>

      </div>
    </div>
  </transition>

</template>

<script setup>
  import { ref, watch, computed } from 'vue'
  import { useAuthStore } from '@/stores/authStore'
  import { receivingApi } from '@/api/receivingApi'

  const props = defineProps({
    isOpen: { type: Boolean, default: false },
    loading: { type: Boolean, default: false },
    data: { type: Object, default: null },
    pendingOnly: { type: Boolean, default: false },
    allowCancel: { type: Boolean, default: false },
  })

  const emit = defineEmits(['close', 'specimen-cancelled'])

  const authStore = useAuthStore()

  const activeTab = ref('Specimens')

  const visibleSpecimens = computed(() => {
    if (!props.data?.specimens) return []
    if (props.pendingOnly) return props.data.specimens.filter(s => s.status !== 'R' && s.status !== 'X')
    return props.data.specimens
  })

  const visibleNonBarcoded = computed(() => {
    if (!props.data?.nonBarcoded) return []
    if (props.pendingOnly) return props.data.nonBarcoded.filter(n => n.status !== 'R')
    return props.data.nonBarcoded
  })

  // Reset tab when drawer opens
  watch(() => props.isOpen, (val) => {
    if (val) activeTab.value = 'Specimens'
  })

  // ── Cancel ─────────────────────────────────────────────────────────────────

  const cancelModal = ref({
    visible: false,
    specimenNo: null,
    batchNo: null,
    reason: '',
    saving: false,
    error: ''
  })

  function openCancelModal(sp) {
    cancelModal.value = {
      visible: true,
      specimenNo: sp.specimenNo,
      batchNo: sp.batchNo,
      reason: '',
      saving: false,
      error: ''
    }
  }

  async function confirmCancel() {
    if (!cancelModal.value.reason.trim()) {
      cancelModal.value.error = 'Please provide a reason.'
      return
    }

    cancelModal.value.saving = true
    cancelModal.value.error = ''

    try {
      await receivingApi.cancelSpecimen({
        specimenNo: cancelModal.value.specimenNo,
        batchNo: cancelModal.value.batchNo,
        cancelReason: cancelModal.value.reason.trim(),
        userID: authStore.userID
      })

      emit('specimen-cancelled', {
        specimenNo: cancelModal.value.specimenNo,
        batchNo: cancelModal.value.batchNo
      })

      cancelModal.value.visible = false
    } catch (err) {
      cancelModal.value.error = err.response?.data?.message || 'An error occurred.'
    } finally {
      cancelModal.value.saving = false
    }
  }

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
