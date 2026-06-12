<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Assign RMT / Section Receiving</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
    </div>

    <!-- Mode toggle + Barcode scan field -->
    <div class="mb-5 rounded-2xl p-5 space-y-4"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <!-- On-Site toggle — only shown if enabled globally -->
      <div v-if="onSiteEnabled" class="flex items-center gap-3">
        <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Scan Mode</span>
        <div class="flex rounded-xl overflow-hidden"
             style="border: 1.5px solid var(--color-border); background-color: var(--color-surface-low);">
          <button class="px-4 py-1.5 text-[11px] font-bold uppercase tracking-widest transition-all"
                  :style="scanMode === 'standard'
                ? 'background: var(--color-primary-gradient); color: #fff;'
                : 'color: var(--color-text-muted);'"
                  @click="scanMode = 'standard'; specimenGroups = []; expandedSpecimen = null">
            Standard
          </button>
          <button class="px-4 py-1.5 text-[11px] font-bold uppercase tracking-widest transition-all"
                  :style="scanMode === 'onsite'
                ? 'background: var(--color-primary-gradient); color: #fff;'
                : 'color: var(--color-text-muted);'"
                  @click="scanMode = 'onsite'; specimenGroups = []; expandedSpecimen = null">
            <span class="flex items-center gap-1">
              <span class="material-symbols-outlined text-xs">location_on</span>
              On-Site
            </span>
          </button>
        </div>
        <span v-if="scanMode === 'onsite'"
              class="text-[10px] font-bold px-2.5 py-1 rounded-full"
              style="background-color: rgba(217,119,6,0.12); color: var(--color-warning);">
          On-Site / Mission Mode
        </span>
      </div>

      <!-- Cut-off banner — shown when cut-off is active and time has passed -->
      <div v-if="sectionCutOff && isCutOffPassedComputed"
           class="flex items-center gap-3 px-4 py-2.5 rounded-xl"
           style="background-color: rgba(217,119,6,0.08); border: 1px solid rgba(217,119,6,0.25);">
        <span class="material-symbols-outlined text-base flex-shrink-0" style="color: var(--color-warning)">schedule</span>
        <div>
          <p class="text-xs font-bold" style="color: var(--color-warning)">
            Cut-off time reached ({{ sectionCutOff }})
          </p>
          <p class="text-[10px]" style="color: var(--color-text-muted)">
            All NOW tests will be automatically set to END (Endorsed Next Day) on scan.
          </p>
        </div>
      </div>

      <!-- Barcode scan input -->
      <div class="flex items-center gap-4">
        <div class="p-3 rounded-xl flex-shrink-0"
             :style="scanMode === 'onsite'
           ? 'background-color: rgba(217,119,6,0.12);'
           : 'background-color: var(--color-primary-soft);'">
          <span class="material-symbols-outlined text-lg"
                :style="scanMode === 'onsite'
              ? 'color: var(--color-warning);'
              : 'color: var(--color-primary);'">
            barcode_scanner
          </span>
        </div>
        <div class="flex-1">
          <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Scan Barcode</p>
          <div class="flex items-center gap-2">
            <input ref="scanInput"
                   v-model="scanValue"
                   type="text"
                   placeholder="Scan or enter specimen no. here..."
                   class="flex-1 px-3 py-2 rounded-xl text-sm outline-none transition-all"
                   style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);"
                   @keydown.enter="scanSpecimen" />
            <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.97] whitespace-nowrap"
                    style="background: var(--color-primary-gradient); color: #fff;"
                    :disabled="scanning || !scanValue.trim()"
                    @click="scanSpecimen">
              {{ scanning ? 'Scanning...' : 'Add' }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Scanned list -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
        <div class="flex items-center gap-2">
          <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">assignment_ind</span>
          <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Scanned Specimens</h2>
        </div>
        <div class="flex items-center gap-3">
          <button v-if="specimenGroups.length"
                  :disabled="!hasFlippableTests"
                  class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl text-[11px] font-bold uppercase tracking-widest transition-all active:scale-95"
                  :style="hasFlippableTests
              ? 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a); cursor: pointer;'
              : 'background-color: var(--color-surface-low); color: var(--color-text-muted); cursor: not-allowed; opacity: 0.5;'"
                  @click="runAllNow">
            <span class="material-symbols-outlined text-sm">play_circle</span>
            Run All Now
          </button>
          <span class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ specimenGroups.length }} specimen{{ specimenGroups.length !== 1 ? 's' : '' }}
          </span>
        </div>
      </div>

      <!-- Empty state -->
      <div v-if="!specimenGroups.length" class="p-12 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">assignment_ind</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No specimens scanned</p>
        <p class="text-xs" style="color: var(--color-text-muted);">Scan a specimen barcode above to assign an RMT.</p>
      </div>

      <!-- Grouped table -->
      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr style="border-bottom: 1.5px solid var(--color-border); background-color: var(--color-surface-low);">
              <th class="w-8 px-4 py-3"></th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Group</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tests</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
              <th class="w-8 px-4 py-3"></th>
            </tr>
          </thead>
          <tbody>
            <template v-for="group in specimenGroups" :key="group.specimenNo">

              <!-- Specimen header row -->
              <tr class="cursor-pointer transition-colors"
                  :style="expandedSpecimen === group.specimenNo
                    ? 'background-color: var(--color-primary-soft);'
                    : ''"
                  @mouseenter="e => { if (expandedSpecimen !== group.specimenNo) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                  @mouseleave="e => { if (expandedSpecimen !== group.specimenNo) e.currentTarget.style.backgroundColor = '' }"
                  @click="toggleSpecimen(group.specimenNo)"
                  style="border-top: 1px solid var(--color-border);">
                <td class="px-4 py-3">
                  <span class="material-symbols-outlined text-sm transition-transform duration-200"
                        :style="{
                          color: 'var(--color-primary)',
                          display: 'block',
                          transform: expandedSpecimen === group.specimenNo ? 'rotate(90deg)' : 'rotate(0deg)'
                        }">
                    chevron_right
                  </span>
                </td>

                <td class="px-4 py-3">
                  <div class="flex items-center gap-2">
                    <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ group.specimenNo }}</p>
                    <!-- Specimen Alert badge -->
                    <span v-if="group.specimenAlert"
                          class="flex items-center gap-1 px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                          style="background-color: rgba(37,99,235,0.1); color: #2563eb;">
                      <span class="material-symbols-outlined" style="font-size: 11px;">info</span>
                      Specimen Alert
                    </span>
                  </div>
                  <p v-if="group.firstScan"
                     class="text-[10px] font-bold mt-0.5"
                     style="color: var(--color-success, #16a34a);">
                    ✓ Received now
                  </p>
                </td>

                <td class="px-4 py-3">
                  <p class="text-xs font-semibold" style="color: var(--color-text);">{{ group.patientName ?? '—' }}</p>
                  <p v-if="group.pid" class="text-[10px] font-mono mt-0.5" style="color: var(--color-text-muted);">{{ group.pid }}</p>
                </td>

                <td class="px-4 py-3">
                  <span class="font-bold text-xs font-mono px-2 py-0.5 rounded-lg"
                        style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                    {{ group.testGroupCode }}
                  </span>
                </td>

                <td class="px-4 py-3">
                  <span class="text-xs font-semibold" style="color: var(--color-text);">{{ group.sampleTypeName ?? group.sampleTypeCode }}</span>
                  <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ group.sampleTypeCode }})</span>
                </td>

                <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted);">
                  {{ group.firstScan ? 'Just now' : formatDt(group.received) }}
                </td>

                <td class="px-4 py-3">
                  <span class="text-xs font-bold" style="color: var(--color-text-muted);">
                    {{ group.tests.length }} test{{ group.tests.length !== 1 ? 's' : '' }}
                  </span>
                </td>

                <!-- Remarks button -->
                <td class="px-4 py-3" @click.stop>
                  <button class="p-1.5 rounded-lg transition-all"
                          :style="group.remarks
            ? 'color: var(--color-warning);'
            : 'color: var(--color-text-muted);'"
                          @mouseenter="e => e.currentTarget.style.color = 'var(--color-primary)'"
                          @mouseleave="e => e.currentTarget.style.color = group.remarks ? 'var(--color-warning)' : 'var(--color-text-muted)'"
                          @click="openRemarks(group)">
                    <span class="material-symbols-outlined text-sm">
                      {{ group.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                    </span>
                  </button>
                </td>

                <td class="px-4 py-3" @click.stop>
                  <button class="p-1.5 rounded-lg transition-all"
                          style="color: var(--color-text-muted);"
                          @mouseenter="e => e.currentTarget.style.color = '#ba1a1a'"
                          @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
                          @click="removeSpecimen(group.specimenNo)">
                    <span class="material-symbols-outlined text-sm">close</span>
                  </button>
                </td>
              </tr>

              <!-- Expanded child test rows -->
              <Transition name="expand">
                <tr v-if="expandedSpecimen === group.specimenNo" :key="`exp-${group.specimenNo}`">
                  <td colspan="9" class="px-0 py-0">
                    <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                         style="border: 1.5px solid var(--color-border);">
                      <table class="w-full text-xs">
                        <thead>
                          <tr style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border);">
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
                              <div class="flex items-center gap-1.5">
                                <span>{{ showCodeFirst ? 'Test Code' : 'Test Name' }}</span>
                                <button class="flex items-center p-0.5 rounded-md transition-all"
                                        style="color: var(--color-text-muted);"
                                        @mouseenter="e => e.currentTarget.style.color = 'var(--color-primary)'"
                                        @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
                                        @click="showCodeFirst = !showCodeFirst"
                                        title="Swap columns">
                                  <span class="material-symbols-outlined" style="font-size: 13px;">swap_horiz</span>
                                </button>
                              </div>
                            </th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
                              {{ showCodeFirst ? 'Test Name' : 'Test Code' }}
                            </th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Running Date</th>
                            <th class="px-4 py-2.5 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Run Date &amp; Time</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr v-for="test in group.tests" :key="test.id"
                              :style="(test.status === 'R' || test.status === 'X')
                              ? 'border-top: 1px solid var(--color-border); opacity: 0.45; pointer-events: none;'
                              : 'border-top: 1px solid var(--color-border);'">

                            <td class="px-4 py-3">
                              <span v-if="showCodeFirst" class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                              <span v-else style="color: var(--color-text);">{{ test.testName }}</span>
                            </td>
                            <td class="px-4 py-3" style="color: var(--color-text);">
                              <span v-if="showCodeFirst">{{ test.testName }}</span>
                              <span v-else class="font-mono font-bold">{{ test.testCode }}</span>
                            </td>

                            <td class="px-4 py-3">
                              <div class="flex items-center gap-1.5">
                                <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                      :style="testStatusStyle(test.status)">
                                  {{ testStatusLabel(test.status) }}
                                </span>
                                <span v-if="test.status === 'R' || test.status === 'X'"
                                      class="material-symbols-outlined text-xs"
                                      style="color: var(--color-text-muted);"
                                      title="Cannot be changed">
                                  lock
                                </span>
                              </div>
                            </td>

                            <!-- Assigned RMT — auto-filled to current user, editable -->
                            <td class="px-4 py-3">
                              <template v-if="test.status === 'R' || test.status === 'X'">
                                <span class="text-xs font-semibold" style="color: var(--color-text);">
                                  {{ test.assignedRMT ?? '—' }}
                                </span>
                              </template>
                              <template v-else>
                                <select v-model="test.selectedRMT"
                                        class="px-2 py-1 rounded-lg text-xs outline-none transition-all"
                                        style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text); min-width: 130px;">
                                  <option v-for="rmt in availableRMTs" :key="rmt.userID" :value="rmt.userID">
                                    {{ rmt.userID }}
                                  </option>
                                </select>
                              </template>
                            </td>

                            <!-- Schedule tag pills -->
                            <td class="px-4 py-3">
                              <div class="flex items-center gap-1.5 flex-wrap">
                                <label v-for="tag in scheduleTags" :key="tag.value"
                                       class="flex items-center gap-1 text-[10px] font-bold uppercase tracking-widest px-2.5 py-1 rounded-lg transition-all select-none"
                                       :class="tag.value === 'SRD' && !test.hasRunningDay ? 'opacity-40 cursor-not-allowed' : 'cursor-pointer'"
                                       :style="(tag.value === 'SRD' && !test.hasRunningDay)
                                       ? 'color: var(--color-text-muted); background-color: var(--color-surface-low); pointer-events: none;'
                                        : test.scheduleTag === tag.value
                                          ? scheduleTagActiveStyle(tag.value)
                                          : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                                       @click.prevent="tag.value === 'SRD' && !test.hasRunningDay ? null : test.scheduleTag = tag.value">
                                  <input type="radio"
                                         :name="`tag-${test.id}`"
                                         :value="tag.value"
                                         v-model="test.scheduleTag"
                                         class="hidden" />
                                  <!-- SRD pill: show next running date hint if available -->
                                  <template v-if="tag.value === 'SRD' && test.hasRunningDay && test.nextRunningDate">
                                    {{ tag.label }} · {{ formatNextRunningDate(test.nextRunningDate) }}
                                  </template>
                                  <template v-else>
                                    {{ tag.label }}
                                  </template>
                                </label>
                              </div>
                            </td>

                            <!-- Running date — only visible for CRD -->
                            <td class="px-4 py-3">
                              <input v-if="test.scheduleTag === 'CRD'"
                                     type="date"
                                     v-model="test.runningDate"
                                     class="px-2 py-1 rounded-lg text-xs outline-none"
                                     style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);" />
                              <span v-else class="text-[10px]" style="color: var(--color-text-muted);">—</span>
                            </td>

                            <td class="px-4 py-3">
                              <span v-if="test.runAt" class="text-xs" style="color: var(--color-text-muted);">
                                {{ formatDt(test.runAt) }}
                              </span>
                              <span v-else class="text-[10px]" style="color: var(--color-text-muted);">—</span>
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </td>
                </tr>
              </Transition>
            </template>
          </tbody>
        </table>

        <!-- Save / Clear bar -->
        <div class="px-6 py-4 flex items-center justify-end gap-3"
             style="border-top: 1.5px solid var(--color-border);">
          <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                  @click="clearAll">
            Clear All
          </button>
          <button class="px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.97]"
                  style="background: var(--color-primary-gradient); color: #fff;"
                  :disabled="saving || !specimenGroups.length"
                  @click="saveAssignments">
            {{ saving ? 'Saving...' : 'Save Assignments' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Remarks Modal -->
    <RemarkModal :isVisible="remarksModal.visible"
                 title="Specimen Remarks"
                 :initialText="remarksModal.text"
                 @save="saveRemarks"
                 @cancel="remarksModal.visible = false"
                 @close="remarksModal.visible = false" />

    <!-- Alert Modal -->
    <AlertModal :isVisible="alert.isVisible"
                :type="alert.type"
                :title="alert.title"
                :message="alert.message"
                @close="alert.isVisible = false"
                @confirm="alert.isVisible = false" />

    <!-- ── Specimen Alert Notification Modal ──────────────────────────────── -->
    <div v-if="specimenAlertModal.visible"
         class="fixed inset-0 z-[90] flex items-center justify-center p-4">
      <!-- Backdrop -->
      <div class="absolute inset-0 bg-black/40 backdrop-blur-sm"></div>
      <!-- Modal -->
      <div class="relative w-full max-w-md rounded-2xl shadow-2xl p-6 flex flex-col gap-4 animate-modal"
           style="background-color: var(--color-surface);">

        <!-- Header -->
        <div class="flex items-center gap-3">
          <span class="material-symbols-outlined text-2xl"
                style="color: #2563eb; font-variation-settings: 'FILL' 1;">info</span>
          <div>
            <p class="font-bold text-sm" style="color: var(--color-text);">Specimen Alert</p>
            <p class="text-xs font-mono mt-0.5" style="color: var(--color-text-muted);">
              {{ specimenAlertModal.specimenNo }}
            </p>
          </div>
        </div>

        <!-- Alert details -->
        <div class="rounded-xl p-4 flex flex-col gap-2"
             style="background-color: rgba(37,99,235,0.06); border: 1px solid rgba(37,99,235,0.2);">
          <p class="text-[10px] font-bold uppercase tracking-widest"
             style="color: #2563eb;">Alert from Processing</p>
          <p class="text-sm italic"
             style="color: var(--color-text);">"{{ specimenAlertModal.specimenAlert }}"</p>
          <p class="text-[10px]"
             style="color: var(--color-text-muted);">
            {{ specimenAlertModal.specimenAlertSetBy }} · {{ formatDt(specimenAlertModal.specimenAlertSetAt) }}
          </p>
        </div>

        <p class="text-xs" style="color: var(--color-text-muted);">
          Please take note of this alert before running the specimen.
        </p>

        <!-- Actions -->
        <div class="flex gap-3 justify-end">
          <button class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                  style="background-color: #2563eb; color: #ffffff;"
                  @click="acknowledgeSpecimenAlert">
            Acknowledged
          </button>
        </div>

      </div>
    </div>

  </AppLayout>
</template>

<script setup>
  import { ref, watch, onMounted, nextTick, computed } from 'vue'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import RemarkModal from '@/components/common/RemarkModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'

  const authStore = useAuthStore()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  const scanInput = ref(null)
  const scanValue = ref('')
  const scanning = ref(false)
  const saving = ref(false)
  const expandedSpecimen = ref(null)
  const specimenGroups = ref([])
  const availableRMTs = ref([])
  const onSiteEnabled = ref(false)
  const sectionCutOff = ref(null)
  const scanMode = ref('standard') // 'standard' | 'onsite'
  const showCodeFirst = ref(localStorage.getItem('assignRmt_showCodeFirst') !== 'false')

  // Schedule tag options
  const scheduleTags = [
    { value: 'NOW', label: 'Now' },
    { value: 'END', label: 'END' },
    { value: 'CRD', label: 'CRD' },
    { value: 'SRD', label: 'SRD' },
  ]

  // ── Expand / collapse ──────────────────────────────────────────────────────

  function toggleSpecimen(specimenNo) {
    expandedSpecimen.value = expandedSpecimen.value === specimenNo ? null : specimenNo
  }

  watch(showCodeFirst, val => {
    localStorage.setItem('assignRmt_showCodeFirst', val)
  })
  // ── Specimen Alert Modal ───────────────────────────────────────────────────

  const specimenAlertModal = ref({
    visible: false,
    specimenNo: null,
    specimenAlert: null,
    specimenAlertSetBy: null,
    specimenAlertSetAt: null,
    pendingEntry: null,   // the group entry waiting to be added
  })

  function acknowledgeSpecimenAlert() {
    // Add the pending entry to the list now that the user has acknowledged
    if (specimenAlertModal.value.pendingEntry) {
      specimenGroups.value.unshift(specimenAlertModal.value.pendingEntry)
      expandedSpecimen.value = specimenAlertModal.value.pendingEntry.specimenNo
    }
    specimenAlertModal.value.visible = false
  }

  // ── Scan ───────────────────────────────────────────────────────────────────

  async function scanSpecimen() {
    const val = scanValue.value.trim()
    if (!val) return

    if (specimenGroups.value.some(g => g.specimenNo === val)) {
      showAlert('warning', 'Already Scanned', `Specimen ${val} is already in the list.`)
      scanValue.value = ''
      await nextTick()
      scanInput.value?.focus()
      return
    }

    scanning.value = true
    try {
      const payload = {
        specimenNo: val,
        sectionCode: authStore.sectionCode,
        userID: authStore.userID,
      }

      const result = scanMode.value === 'onsite'
        ? await runnerApi.scanOnSiteSpecimen(payload)
        : await runnerApi.scanSpecimen(payload)

      const {
        firstScan, tests, specimenNo,
        testGroupCode, sampleTypeCode, sampleTypeName,
        received, receivedBy, patientName, pid, cutOffTime,
        specimenAlert, specimenAlertSetBy, specimenAlertSetAt
      } = result.data

      if (cutOffTime) sectionCutOff.value = cutOffTime

      if (!tests || !tests.length) {
        showAlert('warning', 'No Tests', `No pending tests found for specimen ${val}.`)
        return
      }

      const isCutOffPassed = (() => {
        if (!cutOffTime) return false
        const [coH, coM] = cutOffTime.split(':').map(Number)
        const now = new Date()
        const cutOffMinutes = coH * 60 + coM
        const nowMinutes = now.getHours() * 60 + now.getMinutes()
        return nowMinutes >= cutOffMinutes
      })()

      const entry = {
        specimenNo,
        headerId: result.data.headerId,
        testGroupCode,
        sampleTypeCode,
        sampleTypeName,
        firstScan,
        received,
        receivedBy,
        remarks: result.data.remarks ?? '',
        isOnSite: scanMode.value === 'onsite',
        patientName: patientName ?? null,
        pid: pid ?? null,
        specimenAlert: specimenAlert ?? null,
        specimenAlertSetBy: specimenAlertSetBy ?? null,
        specimenAlertSetAt: specimenAlertSetAt ?? null,
        tests: tests.map(t => {
          /*          const today = new Date().toISOString().split('T')[0]*/
          const now = new Date()
          const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
          const runningDateStr = t.runningDate ?? null

          let initialTag = 'NOW'
          if (t.scheduleTag && runningDateStr && runningDateStr > today) {
            initialTag = t.scheduleTag
          } else if (t.scheduleTag === 'SRD' && !runningDateStr) {
            initialTag = t.isTodayRunningDay ? 'NOW' : 'SRD'
          } else if (!t.scheduleTag && t.hasRunningDay) {
            initialTag = t.isTodayRunningDay ? 'NOW' : 'SRD'
          }

          // ── Cut-off enforcement ────────────────────────────────────────
          // Only flip tests that resolved to NOW — leave SRD/CRD/END untouched
          if (isCutOffPassed && initialTag === 'NOW' && scanMode.value !== 'onsite') {
            initialTag = t.hasRunningDay ? 'SRD' : 'END'
          }

          return {
            ...t,
            selectedRMT: authStore.userID,
            scheduleTag: initialTag,
            runningDate: initialTag === t.scheduleTag ? runningDateStr : null,
            nextRunningDate: t.nextRunningDate ?? null,
          }
        })
      }

      // ── Specimen Alert intercept ───────────────────────────────────────────
      // If alert exists, show modal first — entry added after acknowledgement
      if (specimenAlert) {
        specimenAlertModal.value = {
          visible: true,
          specimenNo,
          specimenAlert,
          specimenAlertSetBy,
          specimenAlertSetAt,
          pendingEntry: entry,
        }
      } else {
        // No alert — add to list immediately as normal
        specimenGroups.value.unshift(entry)
        expandedSpecimen.value = specimenNo
      }

    } catch (e) {
      showAlert('error', 'Scan Failed', e?.response?.data?.message ?? 'Could not scan specimen.')
    } finally {
      scanning.value = false
      scanValue.value = ''
      await nextTick()
      scanInput.value?.focus()
    }
  }

  const isCutOffPassedComputed = computed(() => {
    if (!sectionCutOff.value) return false
    const [coH, coM] = sectionCutOff.value.split(':').map(Number)
    const now = new Date()
    return (now.getHours() * 60 + now.getMinutes()) >= (coH * 60 + coM)
  })

  // ── Remove ─────────────────────────────────────────────────────────────────

  function removeSpecimen(specimenNo) {
    specimenGroups.value = specimenGroups.value.filter(g => g.specimenNo !== specimenNo)
    if (expandedSpecimen.value === specimenNo) expandedSpecimen.value = null
  }

  function clearAll() {
    specimenGroups.value = []
    expandedSpecimen.value = null
  }

  // ── Run All Now ────────────────────────────────────────────────────────────

  function runAllNow() {
    for (const group of specimenGroups.value) {
      for (const test of group.tests) {
        if (test.status === 'R' || test.status === 'X') continue
        test.scheduleTag = 'NOW'
        test.runningDate = null
      }
    }
  }

  const hasFlippableTests = computed(() =>
    specimenGroups.value.some(g =>
      g.tests.some(t =>
        t.status !== 'R' && t.status !== 'X' &&
        (t.scheduleTag === 'END' || t.scheduleTag === 'CRD' || t.scheduleTag === 'SRD')
      )
    )
  )

  // ── Save ───────────────────────────────────────────────────────────────────

  async function saveAssignments() {
    saving.value = true
    try {
      const assignments = specimenGroups.value.flatMap(g =>
        g.tests.map(t => ({
          testId: t.id,
          assignedRMT: t.selectedRMT || null,
          scheduleTag: t.scheduleTag === 'NOW' ? null : (t.scheduleTag || null),
          runningDate: t.scheduleTag === 'CRD' ? (t.runningDate || null) : null,
          runAt: t.scheduleTag === 'NOW' ? (t.runAt || null) : null,
        }))
      )

      for (const group of specimenGroups.value) {
        for (const test of group.tests) {
          if (test.scheduleTag === 'CRD' && !test.runningDate) {
            showAlert('warning', 'Running Date Required',
              `Test ${test.testCode} is tagged CRD but has no running date specified.`)
            saving.value = false
            return
          }
        }
      }

      // Check if mixed — all must be same mode
      const hasStandard = specimenGroups.value.some(g => !g.isOnSite)
      const hasOnSite = specimenGroups.value.some(g => g.isOnSite)

      if (hasStandard && hasOnSite) {
        showAlert('warning', 'Mixed Specimens',
          'Cannot save Standard and On-Site specimens together. Please save them separately.')
        saving.value = false
        return
      }

      const payload = {
        userID: authStore.userID,
        assignments,
        specimenRemarks: specimenGroups.value.map(g => ({
          headerId: g.headerId,
          remarks: g.remarks || null,
        }))
      }

      if (hasOnSite) {
        await runnerApi.saveOnSiteAssignments(payload)
      } else {
        await runnerApi.saveAssignments(payload)
      }

      showAlert('success', 'Saved', 'Assignments saved successfully.')
      specimenGroups.value = []
      expandedSpecimen.value = null

    } catch (e) {
      showAlert('error', 'Save Failed', e?.response?.data?.message ?? 'Could not save assignments.')
    } finally {
      saving.value = false
    }
  }

  // ── Remarks ────────────────────────────────────────────────────────────────

  const remarksModal = ref({ visible: false, specimenNo: '', text: '' })

  function openRemarks(group) {
    remarksModal.value = {
      visible: true,
      specimenNo: group.specimenNo,
      text: group.remarks ?? '',
    }
  }

  function saveRemarks(text) {
    const group = specimenGroups.value.find(g => g.specimenNo === remarksModal.value.specimenNo)
    if (group) group.remarks = text
    remarksModal.value.visible = false
  }

  // ── Formatters ─────────────────────────────────────────────────────────────

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', {
      month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit'
    })
  }

  function testStatusLabel(s) {
    return { P: 'Pending', S: 'Saved', R: 'Running', X: 'Released' }[s] ?? s
  }

  function testStatusStyle(s) {
    const map = {
      P: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
      S: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
      R: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
      X: 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);',
    }
    return map[s] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function scheduleTagActiveStyle(tag) {
    const map = {
      NOW: 'background-color: rgba(22,163,74,0.12); color: var(--color-success, #16a34a);',
      END: 'background-color: rgba(70,21,153,0.12); color: var(--color-primary);',
      CRD: 'background-color: rgba(217,119,6,0.12); color: var(--color-warning);',
      SRD: 'background-color: rgba(74,98,109,0.12); color: var(--color-info, #4a626d);',
    }
    return map[tag] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function getCurrentDateTimeLocal() {
    const now = new Date()
    const pad = n => String(n).padStart(2, '0')
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}T${pad(now.getHours())}:${pad(now.getMinutes())}`
  }

  function formatNextRunningDate(dateStr) {
    if (!dateStr) return null
    const d = new Date(dateStr + 'T00:00:00')
    return d.toLocaleDateString('en-PH', { weekday: 'short', month: 'short', day: 'numeric' })
  }

  // ── Mount ──────────────────────────────────────────────────────────────────

  onMounted(async () => {
    availableRMTs.value = [{
      userID: authStore.userID,
      fullName: authStore.userName ?? authStore.userID,
    }]

    // Check if On-Site scanning is enabled globally
    try {
      const data = await runnerApi.getOnSiteSettings()
      onSiteEnabled.value = data.isEnabled ?? false
    } catch {
      onSiteEnabled.value = false
    }

    await nextTick()
    scanInput.value?.focus()
  })
</script>

<style scoped>
  .expand-enter-active,
  .expand-leave-active {
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    overflow: hidden;
  }

  .expand-enter-from,
  .expand-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  .expand-enter-to,
  .expand-leave-from {
    opacity: 1;
    transform: translateY(0);
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
