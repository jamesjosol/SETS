<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Saved Specimens</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          {{ authStore.sectionName }} · {{ authStore.branchCode }}
        </p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
              style="background: var(--color-primary-gradient); color: #fff;"
              @click="load">
        <span class="material-symbols-outlined text-sm">refresh</span>
        Refresh
      </button>
    </div>

    <!-- Tag filter pills -->
    <div class="mb-4 flex gap-2 flex-wrap">
      <button v-for="tag in tagFilters" :key="tag.value"
              class="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
              :style="activeTag === tag.value
                ? `background-color: ${tag.activeBg}; color: ${tag.activeColor};`
                : 'background-color: var(--color-surface); color: var(--color-text-muted); box-shadow: 0 1px 3px var(--color-shadow);'"
              @click="activeTag = tag.value">
        {{ tag.label }}
        <span class="ml-1.5 px-1.5 py-0.5 rounded-full text-[9px]"
              :style="activeTag === tag.value
                ? `background-color: ${tag.countBg}; color: ${tag.activeColor};`
                : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
          {{ tagCount(tag.value) }}
        </span>
      </button>
    </div>

    <!-- Table -->
    <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

      <div v-if="loading" class="p-6 flex flex-col gap-3">
        <div v-for="i in 4" :key="i" class="h-14 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
      </div>

      <div v-else-if="!filteredTests.length" class="p-16 flex flex-col items-center gap-3">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">bookmark_border</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No saved specimens</p>
        <p class="text-xs" style="color: var(--color-text-muted);">No specimens are currently tagged with {{ activeTag === 'ALL' ? 'any schedule' : activeTag }}.</p>
      </div>

      <div v-else class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead>
            <tr style="border-bottom: 1.5px solid var(--color-border);">
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tag</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Running Date</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
              <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Action</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="test in filteredTests" :key="test.id"
                class="transition-colors"
                style="border-top: 1px solid var(--color-border);"
                @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
              <td class="px-4 py-3">
                <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ test.specimenNo }}</p>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ test.patientName }}</p>
              </td>
              <td class="px-4 py-3">
                <p class="font-bold text-xs font-mono" style="color: var(--color-text);">{{ test.testCode }}</p>
                <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ test.testName }}</p>
              </td>
              <td class="px-4 py-3">
                <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                      :style="scheduleTagStyle(test.scheduleTag)">
                  {{ test.scheduleTag }}
                </span>
              </td>
              <td class="px-4 py-3">
                <!-- CRD — allow changing date -->
                <div v-if="test.scheduleTag === 'CRD'" class="flex items-center gap-2">
                  <input type="date"
                         :value="test.runningDate"
                         class="px-2 py-1 rounded-lg text-xs outline-none"
                         style="background-color: var(--color-surface-low); border: 1.5px solid var(--color-border); color: var(--color-text);"
                         @change="reschedule(test, $event.target.value)" />
                </div>
                <span v-else class="text-xs" style="color: var(--color-text-muted);">{{ test.runningDate ?? '—' }}</span>
              </td>
              <td class="px-4 py-3 text-xs" style="color: var(--color-text-muted);">
                {{ test.assignedRMT ?? '—' }}
              </td>
              <td class="px-4 py-3">
                <!-- No action needed — specimens return to Pending automatically on running date -->
                <span class="text-[10px] italic" style="color: var(--color-text-muted);">Auto-returns on running date</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

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
import { ref, computed, onMounted } from 'vue'
import AppLayout from '@/components/layout/AppLayout.vue'
import AlertModal from '@/components/common/AlertModal.vue'
import { useAuthStore } from '@/stores/authStore'
import { runnerApi } from '@/api/runnerApi'

const authStore = useAuthStore()

const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
function showAlert(type, title, message) {
  alert.value = { isVisible: true, type, title, message }
}

const loading  = ref(true)
const tests    = ref([])
const activeTag = ref('ALL')

const tagFilters = [
  { value: 'ALL', label: 'All',  activeBg: 'rgba(70,21,153,0.12)', activeColor: 'var(--color-primary)', countBg: 'rgba(70,21,153,0.2)' },
  { value: 'ERD', label: 'ERD',  activeBg: 'rgba(70,21,153,0.12)', activeColor: 'var(--color-primary)', countBg: 'rgba(70,21,153,0.2)' },
  { value: 'CRD', label: 'CRD',  activeBg: 'rgba(217,119,6,0.12)', activeColor: 'var(--color-warning)', countBg: 'rgba(217,119,6,0.2)' },
  { value: 'SRD', label: 'SRD',  activeBg: 'rgba(74,98,109,0.12)', activeColor: 'var(--color-info, #4a626d)', countBg: 'rgba(74,98,109,0.2)' },
]

const filteredTests = computed(() => {
  if (activeTag.value === 'ALL') return tests.value
  return tests.value.filter(t => t.scheduleTag === activeTag.value)
})

function tagCount(tag) {
  if (tag === 'ALL') return tests.value.length
  return tests.value.filter(t => t.scheduleTag === tag).length
}

function scheduleTagStyle(tag) {
  const map = {
    ERD: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
    CRD: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
    SRD: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
  }
  return map[tag] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
}

async function reschedule(test, newDate) {
  try {
    await runnerApi.rescheduleTest({ testId: test.id, runningDate: newDate })
    test.runningDate = newDate
  } catch (e) {
    showAlert('error', 'Reschedule Failed', e?.response?.data?.message ?? 'Could not reschedule.')
  }
}

async function load() {
  loading.value = true
  try {
    const data = await runnerApi.getSavedSpecimens(authStore.sectionCode)
    tests.value = Array.isArray(data) ? data : []
  } catch (e) {
    showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load saved specimens.')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
