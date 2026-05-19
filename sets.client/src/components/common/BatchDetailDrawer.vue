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
                   class="rounded-xl p-4 transition-all"
                   :style="getSpecimenCardStyle(sp)">

                <!-- Top row: specimen no + status badge + action buttons -->
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
                    <!-- Flag badge — always visible when flagged -->
                    <span v-if="sp.isFlagged"
                          class="flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                          style="background-color: rgba(255,69,0,0.12); color: #FF4500;">
                      <span class="material-symbols-outlined" style="font-size: 11px;">flag</span>
                      Flagged
                    </span>

                    <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                          :style="sp.status === 'R'
                            ? 'background-color: var(--color-success-soft); color: var(--color-success);'
                            : sp.status === 'X'
                            ? 'background-color: var(--color-surface); color: var(--color-text-muted); border: 1px solid var(--color-border);'
                            : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                      {{ sp.status === 'R' ? 'Received' : sp.status === 'X' ? 'Cancelled' : 'Pending' }}
                    </span>

                    <!-- Flag / Unflag button — endorser side only, pending specimens only -->
                    <button v-if="allowFlag && sp.status === 'P'"
                            class="group w-5 h-5 rounded-full flex items-center justify-center transition-all cursor-pointer"
                            :style="sp.isFlagged
                              ? 'color: #BA7517; opacity: 0.8;'
                              : 'color: var(--color-text-muted); opacity: 0.3;'"
                            :title="sp.isFlagged ? 'Remove flag' : 'Flag this specimen'"
                            @click.stop="sp.isFlagged ? openUnflagConfirm(sp) : openFlagModal(sp)">
                      <span class="material-symbols-outlined transition-all group-hover:text-red-500"
                            :style="sp.isFlagged ? 'font-size: 15px; font-variation-settings: \'FILL\' 1;' : 'font-size: 13px;'">
                        flag
                      </span>
                    </button>

                    <!-- Cancel button — receiver side, TL or admin only -->
                    <button v-if="allowCancel && (sp.status === 'R' || sp.status === 'P') && (authStore.roleID === 2 || authStore.isAdmin)"
                            class="group w-5 h-5 rounded-full flex items-center justify-center transition-all opacity-20 hover:opacity-60"
                            style="color: var(--color-text-muted);"
                            title="Cancel receiving"
                            @click.stop="openCancelModal(sp)">
                      <span class="material-symbols-outlined transition-colors group-hover:text-red-500"
                            style="font-size: 13px;">
                        cancel
                      </span>
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

                <!-- Flag info strip -->
                <div v-if="sp.isFlagged"
                     class="mt-2 pt-2"
                     style="border-top: 1px solid rgba(186,117,23,0.2);">
                  <p class="text-[10px] font-bold uppercase tracking-widest mb-0.5"
                     style="color: #BA7517;">Flag Reason</p>
                  <p class="text-[10px] italic"
                     style="color: var(--color-text-muted);">
                    "{{ sp.flagReason }}"
                  </p>
                  <p class="text-[10px] mt-0.5"
                     style="color: var(--color-text-muted);">
                    {{ sp.flaggedBy }} · {{ formatDateTime(sp.flaggedAt) }}
                  </p>
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

  <!-- ── Flag Specimen Modal ─────────────────────────────────────────────── -->

  <div v-if="flagModal.visible"
        class="fixed inset-0 z-[90] flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
          @click="flagModal.visible = false"></div>
    <!-- Modal -->
    <div class="relative w-full max-w-md rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
          style="background-color: var(--color-surface);">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl"
              style="color: #BA7517; font-variation-settings: 'FILL' 1;">flag</span>
        <div>
          <p class="font-bold text-sm" style="color: var(--color-text);">Flag Specimen</p>
          <p class="text-xs font-mono mt-0.5" style="color: var(--color-text-muted);">
            {{ flagModal.specimenNo }}
          </p>
        </div>
      </div>

      <!-- Warning text -->
      <p class="text-xs" style="color: var(--color-text-muted);">
        This specimen will be flagged. The receiver will be warned before they can scan it and must confirm whether to proceed.
      </p>

      <!-- Reason field -->
      <div class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
                style="color: var(--color-text-muted);">
          Reason <span style="color: #BA7517;">*</span>
        </label>
        <textarea v-model="flagModal.reason"
                  rows="3"
                  placeholder="Enter reason for flagging this specimen..."
                  class="w-full px-3 py-2 rounded-xl text-sm outline-none resize-none transition-all"
                  style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        <p v-if="flagModal.error"
            class="text-[10px]"
            style="color: #BA7517;">
          {{ flagModal.error }}
        </p>
      </div>

      <!-- Actions -->
      <div class="flex gap-3 justify-end">
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                :disabled="flagModal.saving"
                @click="flagModal.visible = false">
          Dismiss
        </button>
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 flex items-center gap-1.5"
                style="background-color: #BA7517; color: #ffffff;"
                :disabled="!flagModal.reason.trim() || flagModal.saving"
                @click="confirmFlag">
          <span v-if="flagModal.saving"
                class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
          <span v-else>Confirm Flag</span>
        </button>
      </div>

    </div>
  </div>


  <!-- ── Unflag Confirm Modal ────────────────────────────────────────────── -->
  <div v-if="unflagModal.visible"
        class="fixed inset-0 z-[90] flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
          @click="unflagModal.visible = false"></div>
    <!-- Modal -->
    <div class="relative w-full max-w-md rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
          style="background-color: var(--color-surface);">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl"
              style="color: var(--color-text-muted);">flag</span>
        <div>
          <p class="font-bold text-sm" style="color: var(--color-text);">Remove Flag</p>
          <p class="text-xs font-mono mt-0.5" style="color: var(--color-text-muted);">
            {{ unflagModal.specimenNo }}
          </p>
        </div>
      </div>

      <!-- Warning text -->
      <p class="text-xs" style="color: var(--color-text-muted);">
        Are you sure you want to remove the flag on this specimen? The receiver will no longer be warned when they scan it.
      </p>

      <p v-if="unflagModal.error"
          class="text-[10px]"
          style="color: var(--color-warning);">
        {{ unflagModal.error }}
      </p>

      <!-- Actions -->
      <div class="flex gap-3 justify-end">
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                :disabled="unflagModal.saving"
                @click="unflagModal.visible = false">
          Dismiss
        </button>
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 flex items-center gap-1.5"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted); border: 1px solid var(--color-border);"
                :disabled="unflagModal.saving"
                @click="confirmUnflag">
          <span v-if="unflagModal.saving"
                class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
          <span v-else>Remove Flag</span>
        </button>
      </div>

    </div>
  </div>

  <!-- ── Cancel Specimen Modal ──────────────────────────────────────────── -->
  <div v-if="cancelModal.visible"
        class="fixed inset-0 z-[90] flex items-center justify-center p-4">
    <!-- Backdrop -->
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"
          @click="cancelModal.visible = false"></div>
    <!-- Modal -->
    <div class="relative w-full max-w-md rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
          style="background-color: var(--color-surface);">

      <!-- Header -->
      <div class="flex items-center gap-3">
        <span class="material-symbols-outlined text-2xl"
              style="color: var(--color-error, #dc2626);">cancel</span>
        <div>
          <p class="font-bold text-sm" style="color: var(--color-text);">Cancel Receiving</p>
          <p class="text-xs font-mono mt-0.5" style="color: var(--color-text-muted);">
            {{ cancelModal.specimenNo }}
          </p>
        </div>
      </div>

      <!-- Warning text -->
      <p class="text-xs" style="color: var(--color-text-muted);">
        This will cancel the specimen from this batch. This action cannot be undone.
      </p>

      <!-- Reason field -->
      <div class="flex flex-col gap-1.5">
        <label class="text-[10px] font-bold uppercase tracking-widest"
                style="color: var(--color-text-muted);">
          Reason <span style="color: var(--color-error, #dc2626);">*</span>
        </label>
        <textarea v-model="cancelModal.reason"
                  rows="3"
                  placeholder="Enter cancellation reason..."
                  class="w-full px-3 py-2 rounded-xl text-sm outline-none resize-none transition-all"
                  style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        <p v-if="cancelModal.error"
            class="text-[10px]"
            style="color: var(--color-error, #dc2626);">
          {{ cancelModal.error }}
        </p>
      </div>

      <!-- Actions -->
      <div class="flex gap-3 justify-end">
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                :disabled="cancelModal.saving"
                @click="cancelModal.visible = false">
          Dismiss
        </button>
        <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 flex items-center gap-1.5"
                style="background-color: var(--color-error, #dc2626); color: #ffffff;"
                :disabled="!cancelModal.reason.trim() || cancelModal.saving"
                @click="confirmCancel">
          <span v-if="cancelModal.saving"
                class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
          <span v-else>Confirm Cancel</span>
        </button>
      </div>

    </div>
  </div>

</template>

<script setup>
  import { ref, watch, computed } from 'vue'
  import { useAuthStore } from '@/stores/authStore'
  import { receivingApi } from '@/api/receivingApi'
  import { flagApi } from '@/api/flagApi'

  const props = defineProps({
    isOpen: { type: Boolean, default: false },
    loading: { type: Boolean, default: false },
    data: { type: Object, default: null },
    pendingOnly: { type: Boolean, default: false },
    allowCancel: { type: Boolean, default: false },
    allowFlag: { type: Boolean, default: false },
  })

  const emit = defineEmits(['close', 'specimen-cancelled', 'specimen-flagged', 'specimen-unflagged'])

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

  // ── Specimen card style ────────────────────────────────────────────────────

  function getSpecimenCardStyle(sp) {
    if (sp.status === 'X') return 'background-color: var(--color-surface-low); opacity: 0.6;'
    if (sp.isFlagged) return 'background-color: var(--color-surface-low); border-left: 3px solid rgba(255,69,0,0.6);'
    return 'background-color: var(--color-surface-low);'
  }

  // ── Flag ───────────────────────────────────────────────────────────────────

  const flagModal = ref({
    visible: false,
    specimenNo: null,
    batchNo: null,
    reason: '',
    saving: false,
    error: ''
  })

  function openFlagModal(sp) {
    flagModal.value = {
      visible: true,
      specimenNo: sp.specimenNo,
      batchNo: sp.batchNo,
      reason: '',
      saving: false,
      error: ''
    }
  }

  async function confirmFlag() {
    if (!flagModal.value.reason.trim()) {
      flagModal.value.error = 'Please provide a reason.'
      return
    }

    flagModal.value.saving = true
    flagModal.value.error = ''

    try {
      await flagApi.flagSpecimen({
        specimenNo: flagModal.value.specimenNo,
        batchNo: flagModal.value.batchNo,
        flagReason: flagModal.value.reason.trim(),
        userID: authStore.userID
      })

      emit('specimen-flagged', {
        specimenNo: flagModal.value.specimenNo,
        batchNo: flagModal.value.batchNo,
        flagReason: flagModal.value.reason.trim(),
        flaggedBy: authStore.userID,
        flaggedAt: new Date().toISOString()
      })

      flagModal.value.visible = false
    } catch (err) {
      flagModal.value.error = err.response?.data?.message || 'An error occurred.'
    } finally {
      flagModal.value.saving = false
    }
  }

  // ── Unflag ─────────────────────────────────────────────────────────────────

  const unflagModal = ref({
    visible: false,
    specimenNo: null,
    batchNo: null,
    saving: false,
    error: ''
  })

  function openUnflagConfirm(sp) {
    unflagModal.value = {
      visible: true,
      specimenNo: sp.specimenNo,
      batchNo: sp.batchNo,
      saving: false,
      error: ''
    }
  }

  async function confirmUnflag() {
    unflagModal.value.saving = true
    unflagModal.value.error = ''

    try {
      await flagApi.unflagSpecimen({
        specimenNo: unflagModal.value.specimenNo,
        batchNo: unflagModal.value.batchNo,
        userID: authStore.userID
      })

      emit('specimen-unflagged', {
        specimenNo: unflagModal.value.specimenNo,
        batchNo: unflagModal.value.batchNo
      })

      unflagModal.value.visible = false
    } catch (err) {
      unflagModal.value.error = err.response?.data?.message || 'An error occurred.'
    } finally {
      unflagModal.value.saving = false
    }
  }

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

  .animate-modal {
    animation: modalIn 0.2s ease;
  }

  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.9) translateY(10px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }
</style>
