<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Dashboard</h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        {{ today }} · <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span>
      </p>
    </div>

    <!-- KPI Cards — Regular / Team Lead -->
    <div v-if="!authStore.isAdmin" class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-primary);">clinical_notes</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Today</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ summary.totalEndorsed }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Total Batches Endorsed</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-primary);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-warning-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-warning);">pending_actions</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-warning);">Awaiting</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-warning);">
          <span v-if="loading">—</span><span v-else>{{ summary.pending }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Pending Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-warning);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-success-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-success);">inventory</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-success);">Completed</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ summary.received }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Received Batches</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-success);"></div>
      </div>
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-error-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-error);">timer_off</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-error);">TAT</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-error);">
          <span v-if="loading">—</span><span v-else>{{ summary.outsideTAT }}</span>
        </h3>
        <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Outside TAT</p>
        <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-error);"></div>
      </div>
    </div>

    <!-- KPI Cards — Admin -->
    <div v-else class="mb-8">
      <div v-if="loading" class="grid grid-cols-1 md:grid-cols-4 gap-6">
        <div v-for="n in 4" :key="n" class="rounded-2xl p-6 animate-pulse"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow); height: 130px;"></div>
      </div>
      <div v-else>
        <div v-for="group in adminGroups" :key="group.category" class="mb-6">
          <div class="flex items-center gap-2 mb-3">
            <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">{{ group.icon }}</span>
            <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ group.label }}</p>
          </div>
          <div class="grid gap-4" :style="`grid-template-columns: repeat(${Math.min(group.sections.length, 4)}, minmax(0, 1fr));`">
            <div v-for="sec in group.sections" :key="sec.sectionCode"
                 class="rounded-2xl p-5 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
                 style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
              <p class="text-[10px] font-bold uppercase tracking-widest mb-3 truncate" style="color: var(--color-primary);">{{ sec.sectionName }}</p>
              <div class="grid grid-cols-4 gap-2">
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-text);">{{ sec.totalEndorsed }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Total</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-warning);">{{ sec.pending }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Pending</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-success);">{{ sec.received }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Received</p>
                </div>
                <div class="text-center">
                  <p class="text-xl font-extrabold" style="color: var(--color-error);">{{ sec.outsideTAT }}</p>
                  <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Outside TAT</p>
                </div>
              </div>
              <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-primary);"></div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Main Grid -->
    <div class="grid grid-cols-12 gap-6">

      <!-- Recent Batches Table -->
      <div class="col-span-12 lg:col-span-8">
        <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Table Header -->
          <div class="px-8 py-5 flex justify-between items-center" style="border-bottom: 1px solid var(--color-surface-low);">
            <h2 class="text-base font-bold" style="color: var(--color-text);">Recent Batches</h2>
            <button class="text-xs font-bold uppercase tracking-widest transition-all"
                    style="color: var(--color-primary);"
                    @click="router.push('/batch-monitoring')">
              View All
            </button>
          </div>

          <!-- Loading -->
          <div v-if="tableLoading" class="px-8 py-12 flex items-center justify-center gap-3">
            <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
            <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</p>
          </div>

          <!-- Empty -->
          <div v-else-if="recentBatches.length === 0" class="px-8 py-12 flex flex-col items-center justify-center gap-2">
            <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
            <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">No batches endorsed today</p>
          </div>

          <!-- Table -->
          <div v-else class="overflow-x-auto">
            <table class="w-full text-left">
              <thead>
                <tr style="background-color: var(--color-bg);">
                  <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Batch No.</th>
                  <th v-if="authStore.isAdmin" class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Endorsed By</th>
                  <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Destination</th>
                  <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="batch in recentBatches" :key="batch.batchNo"
                    class="cursor-pointer transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="openDrawer(batch.batchNo)">
                  <td class="px-8 py-4 font-mono text-xs font-bold" style="color: var(--color-primary);">{{ batch.batchNo }}</td>
                  <td v-if="authStore.isAdmin" class="px-4 py-4 text-sm" style="color: var(--color-text);">{{ batch.location }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ formatDateTime(batch.endorsed) }}</td>
                  <td class="px-4 py-4 text-sm font-bold" style="color: var(--color-text);">{{ batch.endorsedBy }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ batch.destination }}</td>
                  <td class="px-8 py-4">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                          :style="getBatchStatusStyle(batch.status)">
                      <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getBatchStatusDot(batch.status)}`"></span>
                      {{ getBatchStatusLabel(batch.status) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

        </div>
      </div>

      <!-- Right Panel -->
      <div class="col-span-12 lg:col-span-4 space-y-6">

        <!-- Daily Batch Flow -->
        <div class="rounded-2xl p-6" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-6" style="color: var(--color-text);">Daily Batch Flow</h2>
          <div class="h-48 flex items-end justify-between gap-2 px-2">
            <div v-for="(bar, i) in flowBars" :key="i"
                 class="relative w-full h-full flex flex-col items-center justify-end group/bar">
              <!-- Count tooltip -->
              <span class="absolute -top-5 text-[10px] font-bold opacity-0 group-hover/bar:opacity-100 transition-opacity"
                    style="color: var(--color-primary);">{{ bar.count }}</span>
              <!-- Bar -->
              <div class="w-full rounded-t-lg transition-all duration-500"
                   :style="`height: ${bar.height}%; background-color: ${bar.active ? 'var(--color-primary)' : 'var(--color-primary-soft)'};`"
                   @mouseenter="(e) => { if (!bar.active) e.currentTarget.style.opacity = '0.7'; }"
                   @mouseleave="(e) => { if (!bar.active) e.currentTarget.style.opacity = '1'; }">
              </div>
            </div>
          </div>
          <div class="flex justify-between mt-4">
            <span v-for="bar in flowBars" :key="bar.day"
                  class="text-[10px] font-bold uppercase tracking-widest"
                  :style="bar.active ? 'color: var(--color-primary);' : 'color: var(--color-text-muted);'">
              {{ bar.day }}
            </span>
          </div>
        </div>

        <!-- System Status -->
        <div class="rounded-2xl p-6" style="background-color: var(--color-surface-low);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-5" style="color: var(--color-text);">System Status</h2>
          <div class="space-y-3">
            <div v-for="status in systemStatus" :key="status.label"
                 class="flex items-center justify-between p-3 rounded-xl"
                 style="background-color: var(--color-surface);">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-lg" :style="`color: ${status.iconColor}`">{{ status.icon }}</span>
                <span class="text-xs font-bold" style="color: var(--color-text-muted);">{{ status.label }}</span>
              </div>
              <div class="flex flex-col items-end">
                <span class="text-[10px] font-bold px-2 py-0.5 rounded uppercase" :style="getStatusBadgeStyle(status.state)">{{ status.state }}</span>
                <span v-if="status.note" class="text-[8px] mt-0.5" style="color: var(--color-text-muted);">{{ status.note }}</span>
              </div>
            </div>
          </div>
          <div class="mt-5 pt-4 flex justify-between text-[10px] font-bold uppercase tracking-widest"
               style="border-top: 1px solid var(--color-border); color: var(--color-text-muted);">
            <span>Last Global Sync</span>
            <span>2 mins ago</span>
          </div>
        </div>

      </div>
    </div>

    <!-- ── Batch Detail Drawer ─────────────────────────────────────────── -->
    <!-- Backdrop -->
    <transition name="fade">
      <div v-if="drawerOpen"
           class="fixed inset-0 z-[60]"
           style="background-color: rgba(0,0,0,0.4);"
           @click="closeDrawer"></div>
    </transition>

    <!-- Drawer Panel -->
    <transition name="slide">
      <div v-if="drawerOpen"
           class="fixed top-0 right-0 h-full z-[70] flex flex-col"
           style="width: 480px; background-color: var(--color-surface); border-left: 1px solid var(--color-border); box-shadow: -4px 0 24px rgba(0,0,0,0.12);">

        <!-- Drawer Header -->
        <div class="px-6 py-5 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
          <div>
            <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Batch Detail</p>
            <p class="text-base font-extrabold font-mono" style="color: var(--color-primary);">
              {{ drawerLoading ? '...' : drawerData?.batchNo }}
            </p>
          </div>
          <button class="p-2 rounded-xl transition-all"
                  style="color: var(--color-text-muted);"
                  @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                  @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                  @click="closeDrawer">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>

        <!-- Drawer Loading -->
        <div v-if="drawerLoading" class="flex-1 flex items-center justify-center gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
          <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</p>
        </div>

        <template v-else-if="drawerData">

          <!-- Batch Info -->
          <div class="px-6 py-4 space-y-3" style="border-bottom: 1px solid var(--color-border);">
            <div class="grid grid-cols-2 gap-3">
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Location</p>
                <p class="text-sm font-bold" style="color: var(--color-text);">{{ drawerData.location }}</p>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Destination</p>
                <p class="text-sm font-bold" style="color: var(--color-text);">{{ drawerData.destination }}</p>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Endorsed By</p>
                <p class="text-sm font-bold" style="color: var(--color-text);">{{ drawerData.endorsedBy }}</p>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Date & Time</p>
                <p class="text-sm font-bold" style="color: var(--color-text);">{{ formatDateTime(drawerData.endorsed) }}</p>
              </div>
            </div>
            <div>
              <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Status</p>
              <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                    :style="getBatchStatusStyle(drawerData.status)">
                <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getBatchStatusDot(drawerData.status)}`"></span>
                {{ getBatchStatusLabel(drawerData.status) }}
              </span>
            </div>
          </div>

          <!-- Tabs -->
          <div class="flex px-6 pt-4 gap-1" style="border-bottom: 1px solid var(--color-border);">
            <button v-for="tab in ['Specimens', 'Non-Barcoded']" :key="tab"
                    class="px-4 py-2 text-xs font-bold uppercase tracking-widest rounded-t-lg transition-all"
                    :style="drawerTab === tab
                      ? 'color: var(--color-primary); border-bottom: 2px solid var(--color-primary); margin-bottom: -1px;'
                      : 'color: var(--color-text-muted);'"
                    @click="drawerTab = tab">
              {{ tab }}
              <span class="ml-1.5 text-[10px] px-1.5 py-0.5 rounded-full font-bold"
                    :style="drawerTab === tab ? 'background-color: var(--color-primary-soft); color: var(--color-primary);' : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'">
                {{ tab === 'Specimens' ? drawerData.specimens.length : drawerData.nonBarcoded.length }}
              </span>
            </button>
          </div>

          <!-- Tab Content -->
          <div class="flex-1 overflow-y-auto px-6 py-4">

            <!-- Specimens Tab -->
            <template v-if="drawerTab === 'Specimens'">
              <div v-if="drawerData.specimens.length === 0"
                   class="flex flex-col items-center justify-center py-12 gap-2">
                <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted);">biotech</span>
                <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">No specimens</p>
              </div>
              <div v-else class="space-y-2">
                <div v-for="sp in drawerData.specimens" :key="sp.id"
                     class="rounded-xl p-4"
                     style="background-color: var(--color-surface-low);">
                  <div class="flex justify-between items-start mb-2">
                    <p class="text-xs font-bold font-mono" style="color: var(--color-primary);">{{ sp.specimenNo }}</p>
                    <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                          :style="sp.status === 'R'
                            ? 'background-color: var(--color-success-soft); color: var(--color-success);'
                            : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                      {{ sp.status === 'R' ? 'Received' : 'Pending' }}
                    </span>
                  </div>
                  <p class="text-sm font-bold mb-0.5" style="color: var(--color-text);">{{ sp.patientName }}</p>
                  <div class="flex items-center gap-3 mt-1">
                    <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ sp.sampleTypeName }}</span>
                    <span class="text-[10px]" style="color: var(--color-text-muted);">·</span>
                    <span class="text-[10px] font-mono" style="color: var(--color-text-muted);">{{ sp.labNo }}</span>
                  </div>
                </div>
              </div>
            </template>

            <!-- Non-Barcoded Tab -->
            <template v-else>
              <div v-if="drawerData.nonBarcoded.length === 0"
                   class="flex flex-col items-center justify-center py-12 gap-2">
                <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted);">description</span>
                <p class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">No non-barcoded items</p>
              </div>
              <div v-else class="space-y-2">
                <div v-for="nb in drawerData.nonBarcoded" :key="nb.itemID"
                     class="rounded-xl p-4 flex items-center justify-between"
                     style="background-color: var(--color-surface-low);">
                  <div>
                    <p class="text-sm font-bold mb-0.5" style="color: var(--color-text);">{{ nb.description }}</p>
                    <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
                      Qty: {{ nb.quantity }}
                    </span>
                  </div>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase"
                        :style="nb.status === 'R'
                          ? 'background-color: var(--color-success-soft); color: var(--color-success);'
                          : 'background-color: var(--color-warning-soft); color: var(--color-warning);'">
                    {{ nb.status === 'R' ? 'Received' : 'Pending' }}
                  </span>
                </div>
              </div>
            </template>

          </div>
        </template>

      </div>
    </transition>

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
  import { useRouter } from 'vue-router'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { batchApi } from '@/api/batchApi'
  import AlertModal from '@/components/common/AlertModal.vue'

  const authStore = useAuthStore()
  const router = useRouter()

  // ── Alert ──────────────────────────────────────────────────────────────────

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  const today = computed(() =>
    new Date().toLocaleDateString('en-US', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric',
    })
  )

  // ── KPI State ──────────────────────────────────────────────────────────────

  const loading = ref(true)

  const summary = ref({ totalEndorsed: 0, pending: 0, received: 0, outsideTAT: 0 })

  const allSections = ref([])

  const adminGroups = computed(() => {
    const categoryMap = {
      '1': { label: 'Phlebo / Send-In', icon: 'vaccines', category: '1' },
      '2': { label: 'Processing', icon: 'labs', category: '2' },
      '3': { label: 'Laboratory', icon: 'science', category: '3' },
    }
    const groups = {}
    for (const sec of allSections.value) {
      const cat = sec.sectionCategory
      if (!groups[cat]) {
        groups[cat] = { ...categoryMap[cat] ?? { label: `Category ${cat}`, icon: 'category', category: cat }, sections: [] }
      }
      groups[cat].sections.push(sec)
    }
    return Object.values(groups).sort((a, b) => a.category.localeCompare(b.category))
  })

  // ── Recent Batches State ───────────────────────────────────────────────────

  const tableLoading = ref(true)
  const recentBatches = ref([])

  // ── Drawer State ───────────────────────────────────────────────────────────

  const drawerOpen = ref(false)
  const drawerLoading = ref(false)
  const drawerData = ref(null)
  const drawerTab = ref('Specimens')

  async function openDrawer(batchNo) {
    drawerOpen.value = true
    drawerLoading.value = true
    drawerTab.value = 'Specimens'
    drawerData.value = null
    try {
      drawerData.value = await batchApi.getBatchDetail(batchNo)
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Drawer fetch error:', err)
      }
    } finally {
      drawerLoading.value = false
    }
  }

  function closeDrawer() {
    drawerOpen.value = false
    drawerData.value = null
  }

  // ── Fetch ──────────────────────────────────────────────────────────────────

  onMounted(async () => {
    try {
      // KPI cards
      if (authStore.isAdmin) {
        const data = await batchApi.getAllSectionsSummary()
        allSections.value = data
      } else {
        const data = await batchApi.getDashboardSummary(authStore.sectionCode)
        summary.value = data
      }
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Dashboard KPI fetch error:', err)
      }
    } finally {
      loading.value = false
    }

    try {
      // Recent batches + weekly flow in parallel
      if (authStore.isAdmin) {
        const [recentData, flowData] = await Promise.all([
          batchApi.getAllSectionsRecentBatches(),
          batchApi.getAllSectionsWeeklyFlow(),
        ])
        recentBatches.value = recentData
        weeklyFlow.value = flowData
      } else {
        const [recentData, flowData] = await Promise.all([
          batchApi.getRecentBatches(authStore.sectionCode),
          batchApi.getWeeklyFlow(authStore.sectionCode),
        ])
        recentBatches.value = recentData
        weeklyFlow.value = flowData
      }
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Recent batches fetch error:', err)
      }
    } finally {
      tableLoading.value = false
    }
  })

  // ── Helpers ────────────────────────────────────────────────────────────────

  function formatDateTime(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-US', {
      month: 'short', day: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: true,
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

  function getStatusBadgeStyle(state) {
    const map = {
      Online: 'background-color: var(--color-success-soft); color: var(--color-success);',
      Delayed: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      Offline: 'background-color: var(--color-error-soft); color: var(--color-error);',
    }
    return map[state] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  // ── Static placeholders ────────────────────────────────────────────────────

  const weeklyFlow = ref([])

  const flowBars = computed(() => {
    if (weeklyFlow.value.length === 0) return []
    const max = Math.max(...weeklyFlow.value.map(d => d.count), 1)
    return weeklyFlow.value.map(d => ({
      height: Math.max(Math.round((d.count / max) * 100), 4),
      active: d.isToday,
      count: d.count,
      day: d.day,
    }))
  })

  const systemStatus = [
    { label: 'Lab Connectivity', icon: 'router', iconColor: '#059669', state: 'Online', note: null },
    { label: 'Tracking Relay', icon: 'satellite_alt', iconColor: '#059669', state: 'Online', note: null },
    { label: 'Endorsement API', icon: 'api', iconColor: '#d97706', state: 'Delayed', note: '140ms latency' },
  ]
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
