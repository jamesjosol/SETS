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
    <div v-if="!authStore.isAdmin" ref="kpiCardsRef" class="grid grid-cols-1 md:grid-cols-4 gap-6 mb-8">
      <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex justify-between items-start mb-4">
          <div class="p-2 rounded-xl" style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined" style="color: var(--color-primary);">clinical_notes</span>
          </div>
          <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Today</span>
        </div>
        <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
          <span v-if="loading">—</span><span v-else>{{ displayTotalEndorsed }}</span>
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
          <span v-if="loading">—</span><span v-else>{{ displayPending }}</span>
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
          <span v-if="loading">—</span><span v-else>{{ displayReceived }}</span>
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
          <span v-if="loading">—</span><span v-else>{{ displayOutsideTAT }}</span>
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
      <div v-else ref="adminKpiRef">
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

    <!-- TAT Countdown Bar — Endorser only -->
    <div v-if="!authStore.isAdmin && (tatCycle.hasOpenCycle || outboundTat.enabled)"
         class="mb-6 grid gap-4"
         :class="outboundTat.enabled ? 'grid-cols-1 lg:grid-cols-2' : 'grid-cols-1'">

      <!-- ── Local TAT column ── -->
      <div v-if="tatCycle.hasOpenCycle"
           class="flex items-center gap-4 px-5 py-3 rounded-xl"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <!-- Icon + Label -->
        <div class="flex items-center gap-2 flex-shrink-0">
          <span class="material-symbols-outlined text-base"
                :class="tatExceeded ? 'animate-pulse' : ''"
                :style="tatColorStyle">
            {{ tatExceeded ? 'timer_off' : 'timer' }}
          </span>
          <p class="text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted);">
            {{ outboundTat.enabled ? 'Local TAT' : 'Next Endorsement Due' }}
          </p>
        </div>

        <!-- Progress bar -->
        <div class="flex-1 h-1 rounded-full overflow-hidden"
             style="background-color: var(--color-surface-low);">
          <div class="h-full rounded-full transition-all duration-1000"
               :style="`width: ${tatProgressPct}%; ${tatBarStyle}`"></div>
        </div>

        <!-- Countdown -->
        <p class="text-xs font-extrabold font-mono flex-shrink-0 tabular-nums"
           :style="tatColorStyle">
          {{ tatExceeded ? 'EXCEEDED' : tatCountdown }}
        </p>

        <!-- Divider -->
        <div class="h-4 w-px flex-shrink-0" style="background-color: var(--color-border);"></div>

        <!-- Appeal Button -->
        <button v-if="tatCycle.canAppeal"
                class="flex-shrink-0 flex items-center gap-1.5 text-[10px] font-bold uppercase tracking-widest transition-all active:scale-95 cursor-pointer"
                :class="tatLoading ? 'opacity-60 pointer-events-none' : ''"
                style="color: var(--color-text-muted);"
                @mouseenter="e => e.currentTarget.style.color = 'var(--color-text)'"
                @mouseleave="e => e.currentTarget.style.color = 'var(--color-text-muted)'"
                @click="confirmAppeal">
          <span class="material-symbols-outlined text-sm">do_not_disturb_on</span>
          Nothing to Endorse
        </button>

        <!-- Appeal not available hint -->
        <p v-else
           class="flex-shrink-0 text-[10px] font-bold uppercase tracking-widest"
           style="color: var(--color-text-muted);">
          Appeal after TAT breach
        </p>

      </div>

      <!-- Local TAT placeholder when no open cycle but outbound is shown -->
      <div v-else-if="outboundTat.enabled"
           class="flex items-center gap-3 px-5 py-3 rounded-xl"
           style="background-color: var(--color-surface);
                  box-shadow: 0 1px 3px var(--color-shadow);
                  border: 1px solid var(--color-border);">
        <span class="material-symbols-outlined text-base"
              style="color: var(--color-text-muted);">timer</span>
        <p class="text-[10px] font-bold uppercase tracking-widest"
           style="color: var(--color-text-muted);">Local TAT</p>
        <p class="text-xs font-bold ml-auto" style="color: var(--color-text-muted);">No active cycle</p>
      </div>

      <!-- ── Outbound TAT column ── -->
      <template v-if="outboundTat.enabled">

        <!-- Inside a window -->
        <div v-if="outboundTat.currentWindow"
             class="flex items-center gap-4 px-5 py-3 rounded-xl"
             :style="`background-color: var(--color-surface);
             box-shadow: 0 1px 3px var(--color-shadow);
             border: 1px solid ${
             outboundWindowSecondsRemaining <= 0
                 ? 'var(--color-error)'
                 : outboundTat.hasEndorsedThisWindow
                   ? 'var(--color-success)'
                   : outboundWindowProgressPct <= 25
                     ? 'var(--color-warning)'
                     : 'var(--color-border)'
             };`">

          <div class="flex items-center gap-2 flex-shrink-0">
            <span class="material-symbols-outlined text-base"
                  :style="outboundWindowColorStyle">alt_route</span>
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">
              Outbound TAT · {{ outboundTat.currentWindow.windowStart }}–{{ outboundTat.currentWindow.windowEnd }}
            </p>
          </div>

          <div class="flex-1 h-1 rounded-full overflow-hidden"
               style="background-color: var(--color-surface-low);">
            <div class="h-full rounded-full transition-all duration-1000"
                 :style="`width: ${outboundWindowProgressPct}%; ${outboundWindowBarStyle}`"></div>
          </div>

          <p class="text-xs font-extrabold font-mono flex-shrink-0 tabular-nums"
             :style="outboundWindowColorStyle">
            {{ outboundWindowCountdown }}
          </p>

        </div>

        <!-- Between windows — countdown to next -->
        <div v-else-if="outboundTat.nextWindow"
             class="flex items-center gap-4 px-5 py-3 rounded-xl"
             style="background-color: var(--color-surface);
                    box-shadow: 0 1px 3px var(--color-shadow);
                    border: 1px solid var(--color-border);">

          <div class="flex items-center gap-2 flex-shrink-0">
            <span class="material-symbols-outlined text-base"
                  style="color: var(--color-text-muted);">schedule</span>
            <p class="text-[10px] font-bold uppercase tracking-widest"
               style="color: var(--color-text-muted);">
              Next Outbound Window · {{ outboundTat.nextWindow.windowStart }}
            </p>
          </div>

          <div class="flex-1 h-1 rounded-full overflow-hidden"
               style="background-color: var(--color-surface-low);">
            <div class="h-full w-full rounded-full"
                 style="background-color: var(--color-surface-low);"></div>
          </div>

          <p class="text-xs font-extrabold font-mono flex-shrink-0 tabular-nums"
             style="color: var(--color-text-muted);">
            {{ outboundNextWindowCountdown }}
          </p>

        </div>

        <!-- No more windows today -->
        <div v-else
             class="flex items-center gap-3 px-5 py-3 rounded-xl"
             style="background-color: var(--color-surface);
                    box-shadow: 0 1px 3px var(--color-shadow);
                    border: 1px solid var(--color-border);">
          <span class="material-symbols-outlined text-base"
                style="color: var(--color-text-muted);">alt_route</span>
          <p class="text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted);">Outbound TAT</p>
          <p class="text-xs font-bold ml-auto" style="color: var(--color-text-muted);">
            {{ outboundTat.hasWindowsToday ? 'No more windows today' : 'No windows configured' }}
          </p>
        </div>

      </template>

    </div>

    <!-- Appeal Confirm Modal -->
    <ConfirmModal :isVisible="appealConfirm.visible"
                  type="warning"
                  icon="do_not_disturb_on"
                  title="Nothing to Endorse?"
                  message="This will log an appeal and reset the TAT timer. Only proceed if there are genuinely no specimens to endorse at this time."
                  confirmText="Yes, Submit Appeal"
                  cancelText="Cancel"
                  @confirm="handleAppeal"
                  @close="appealConfirm.visible = false" />

    <!-- Main Grid -->
    <div class="grid grid-cols-12 gap-6">

      <!-- Recent Batches Table -->
      <div class="col-span-12 lg:col-span-8">
        <div ref="tableCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Table Header -->
          <div class="px-8 py-5 flex justify-between items-center" style="border-bottom: 1px solid var(--color-surface-low);">
            <div class="flex items-center gap-3">
              <h2 class="text-base font-bold" style="color: var(--color-text);">Today's Batches</h2>
              <span v-if="recentBatches.length > 0 && recentBatchPages > 1"
                    class="text-xs font-bold" style="color: var(--color-text-muted);">
                {{ recentBatchPage }}/{{ recentBatchPages }}
              </span>
            </div>
            <button class="text-xs font-bold uppercase tracking-widest transition-all"
                    style="color: var(--color-primary);"
                    @click="router.push('/endorsements')">
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
              <tbody ref="tableBodyRef">
                <tr v-for="batch in paginatedRecentBatches" :key="batch.batchNo"
                    class="cursor-pointer transition-colors"
                    style="border-top: 1px solid var(--color-surface-low);"
                    @mouseenter="(e) => (e.currentTarget.style.backgroundColor = 'var(--color-surface-low)')"
                    @mouseleave="(e) => (e.currentTarget.style.backgroundColor = 'transparent')"
                    @click="openDrawer(batch.batchNo)">
                  <td class="px-8 py-4">
                    <div class="flex items-center gap-2">
                      <span class="font-mono text-sm font-bold" style="color: var(--color-primary);">
                        {{ batch.batchNo }}
                      </span>

                      <!-- Unposted indicator -->
                      <div v-if="batch.hasUnpostedSpecimens"
                           class="relative group flex-shrink-0">
                        <span class="material-symbols-outlined"
                              style="font-size: 14px; color: var(--color-warning);">cloud_off</span>
                        <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1.5 px-2.5 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest whitespace-nowrap shadow-lg pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity z-50"
                             style="background-color: var(--color-warning); color: #ffffff;">
                          Unposted specimen(s) to destination
                        </div>
                      </div>

                      <!-- Flagged indicator -->
                      <div v-if="batch.hasFlaggedSpecimens"
                           class="relative group flex-shrink-0">
                        <span class="material-symbols-outlined"
                              style="font-size: 14px; color: orangered; font-variation-settings: 'FILL' 1;">flag</span>
                        <div class="absolute bottom-full left-1/2 -translate-x-1/2 mb-1.5 px-2.5 py-1.5 rounded-lg text-[10px] font-bold uppercase tracking-widest whitespace-nowrap shadow-lg pointer-events-none opacity-0 group-hover:opacity-100 transition-opacity z-50"
                             style="background-color: orangered; color: #ffffff;">
                          Has required action specimen(s)
                        </div>
                      </div>
                    </div>
                  </td>
                  <td v-if="authStore.isAdmin" class="px-4 py-4 text-sm" style="color: var(--color-text);">{{ batch.location }}</td>
                  <td class="px-4 py-4 text-sm" style="color: var(--color-text-muted);">{{ formatDateTime(batch.endorsed) }}</td>
                  <td class="px-4 py-4 text-xs font-bold" style="color: var(--color-text);">{{ batch.endorsedBy }}</td>
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

            <!-- Pagination -->
            <div v-if="recentBatchPages > 1"
                 class="px-8 py-3 flex items-center justify-between"
                 style="border-top: 1px solid var(--color-surface-low);">
              <button :disabled="recentBatchPage === 1"
                      class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                      style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                      @click="recentBatchPage--">
                <span class="material-symbols-outlined text-sm">chevron_left</span>
                Prev
              </button>
              <div class="flex items-center gap-1">
                <button v-for="p in recentBatchPages"
                        :key="p"
                        class="w-7 h-7 rounded-lg text-xs font-bold transition-all"
                        :style="p === recentBatchPage
              ? 'background-color: var(--color-primary); color: #fff;'
              : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                        @click="recentBatchPage = p">
                  {{ p }}
                </button>
              </div>
              <button :disabled="recentBatchPage === recentBatchPages"
                      class="flex items-center gap-1 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                      style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                      @click="recentBatchPage++">
                Next
                <span class="material-symbols-outlined text-sm">chevron_right</span>
              </button>
            </div>
          </div>

        </div>
      </div>

      <!-- Right Panel -->
      <div class="col-span-12 lg:col-span-4 space-y-6">

        <!-- Daily Batch Flow -->
        <div ref="flowCardRef" class="rounded-2xl p-6" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <h2 class="text-xs font-bold uppercase tracking-widest mb-6" style="color: var(--color-text);">Daily Batch Flow</h2>
          <div ref="flowBarsContainerRef" class="h-48 flex items-end justify-between gap-2 px-2">
            <div v-for="(bar, i) in flowBars" :key="i"
                 class="relative w-full h-full flex flex-col items-center justify-end group/bar">
              <span class="absolute -top-5 text-[10px] font-bold opacity-0 group-hover/bar:opacity-100 transition-opacity"
                    style="color: var(--color-primary);">{{ bar.count }}</span>
              <div class="w-full rounded-t-lg origin-bottom"
                   :data-bar-index="i"
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
        <div class="col-span-12 lg:col-span-4">
          <SystemStatus />
        </div>

      </div>
    </div>

    <!-- ── Batch Detail Drawer ─────────────────────────────────────────── -->
    <BatchDetailDrawer :isOpen="drawerOpen"
                       :loading="drawerLoading"
                       :data="drawerData"
                       :allowFlag="true"
                       @close="closeDrawer"
                       @specimen-flagged="onSpecimenFlagged"
                       @specimen-unflagged="onSpecimenUnflagged" />

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
  import { ref, computed, onMounted, onUnmounted, watch, nextTick } from 'vue'
  import { useRouter } from 'vue-router'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { batchApi } from '@/api/batchApi'
  import { healthApi } from '@/api/healthApi'
  import { tatApi } from '@/api/tatApi'
  import { tatOutboundApi } from '@/api/tatOutboundApi'
  import AlertModal from '@/components/common/AlertModal.vue'
  import SystemStatus from '@/components/common/SystemStatus.vue'
  import BatchDetailDrawer from '@/components/common/BatchDetailDrawer.vue'
  import ConfirmModal from '@/components/common/ConfirmModal.vue'

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

  const displayTotalEndorsed = ref(0)
  const displayPending = ref(0)
  const displayReceived = ref(0)
  const displayOutsideTAT = ref(0)

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
  const weeklyFlow = ref([])

  // ── Recent Batches Pagination ──────────────────────────────────────────────

  const recentBatchPage = ref(1)
  const RECENT_PAGE_SIZE = 8

  const recentBatchPages = computed(() =>
    Math.max(1, Math.ceil(recentBatches.value.length / RECENT_PAGE_SIZE))
  )

  const paginatedRecentBatches = computed(() => {
    const start = (recentBatchPage.value - 1) * RECENT_PAGE_SIZE
    return recentBatches.value.slice(start, start + RECENT_PAGE_SIZE)
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
        console.error('Drawer fetch error:', err)
      }
    } finally {
      drawerLoading.value = false
    }
  }

  function closeDrawer() {
    drawerOpen.value = false
    fetchTableData()
    drawerData.value = null
  }

  // ── Flag / Unflag handlers ─────────────────────────────────────────────────

  function onSpecimenFlagged({ specimenNo, batchNo, flagReason, flaggedBy, flaggedAt }) {
    if (drawerData.value?.specimens) {
      const sp = drawerData.value.specimens.find(s => s.specimenNo === specimenNo)
      if (sp) {
        sp.isFlagged = true
        sp.flagReason = flagReason
        sp.flaggedBy = flaggedBy
        sp.flaggedAt = flaggedAt
      }
    }
    const batch = recentBatches.value.find(b => b.batchNo === batchNo)
    if (batch) batch.hasFlaggedSpecimens = true
  }

  function onSpecimenUnflagged({ specimenNo, batchNo }) {
    if (drawerData.value?.specimens) {
      const sp = drawerData.value.specimens.find(s => s.specimenNo === specimenNo)
      if (sp) {
        sp.isFlagged = false
        sp.flagReason = null
        sp.flaggedBy = null
        sp.flaggedAt = null
      }
    }
    const batch = recentBatches.value.find(b => b.batchNo === batchNo)
    if (batch && drawerData.value?.specimens) {
      batch.hasFlaggedSpecimens = drawerData.value.specimens.some(s => s.isFlagged)
    }
  }

  // ── GSAP Refs ──────────────────────────────────────────────────────────────

  const kpiCardsRef = ref(null)
  const adminKpiRef = ref(null)
  const tableCardRef = ref(null)
  const tableBodyRef = ref(null)
  const flowCardRef = ref(null)
  const flowBarsContainerRef = ref(null)

  // ── GSAP Animations ────────────────────────────────────────────────────────

  function countUp(displayRef, target) {
    const obj = { val: 0 }
    gsap.killTweensOf(obj)
    gsap.to(obj, {
      val: target,
      duration: 0.75,
      ease: 'power2.out',
      onUpdate: () => { displayRef.value = Math.round(obj.val) },
    })
  }

  async function animateAdminKpiCards() {
    await nextTick()
    if (!adminKpiRef.value) return
    const cards = adminKpiRef.value.querySelectorAll('.rounded-2xl')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, { opacity: 1, y: 0, duration: 0.35, stagger: 0.05, ease: 'power2.out' })
  }

  async function animateTableEntrance() {
    await nextTick()
    if (!tableCardRef.value) return
    gsap.set(tableCardRef.value, { opacity: 0, y: 16 })
    gsap.to(tableCardRef.value, { opacity: 1, y: 0, duration: 0.3, ease: 'power2.out' })
    if (tableBodyRef.value) {
      const rows = tableBodyRef.value.querySelectorAll('tr')
      if (rows.length) {
        gsap.set(rows, { opacity: 0, x: -8 })
        gsap.to(rows, { opacity: 1, x: 0, duration: 0.3, stagger: 0.05, ease: 'power1.out', delay: 0.18 })
      }
    }
  }

  async function animateTableRows() {
    await nextTick()
    if (!tableBodyRef.value) return
    const rows = tableBodyRef.value.querySelectorAll('tr')
    if (!rows.length) return
    gsap.set(rows, { opacity: 0, x: -6 })
    gsap.to(rows, { opacity: 1, x: 0, duration: 0.18, stagger: 0.025, ease: 'power1.out' })
  }

  async function animateFlowBars() {
    await nextTick()
    if (!flowBarsContainerRef.value) return
    const bars = flowBarsContainerRef.value.querySelectorAll('[data-bar-index]')
    if (!bars.length) return
    if (flowCardRef.value) {
      gsap.set(flowCardRef.value, { opacity: 0, y: 16 })
      gsap.to(flowCardRef.value, { opacity: 1, y: 0, duration: 0.3, ease: 'power2.out' })
    }
    gsap.set(bars, { scaleY: 0, opacity: 0 })
    gsap.to(bars, { scaleY: 1, opacity: 1, duration: 0.45, stagger: 0.06, ease: 'back.out(1.4)', delay: 0.15 })
  }

  // ── Fetch ──────────────────────────────────────────────────────────────────

  let refreshInterval = null
  let outboundTatRefreshInterval = null

  onMounted(async () => {
    await fetchKPIs()
    loading.value = false

    await fetchTableData()
    tableLoading.value = false

    await loadTatCycle()
    await loadOutboundTatWindow()

    refreshInterval = setInterval(silentRefresh, 5000)
    tickInterval = setInterval(() => { nowTick.value = Date.now() }, 1000)
    outboundTatRefreshInterval = setInterval(loadOutboundTatWindow, 10000)
  })

  onUnmounted(() => {
    clearInterval(refreshInterval)
    clearInterval(tickInterval)
    clearInterval(outboundTatRefreshInterval)
  })

  // ── Watchers ───────────────────────────────────────────────────────────────

  watch(loading, async (isLoading) => {
    if (!isLoading) {
      if (authStore.isAdmin) {
        await animateAdminKpiCards()
      } else {
        countUp(displayTotalEndorsed, summary.value.totalEndorsed)
        countUp(displayPending, summary.value.pending)
        countUp(displayReceived, summary.value.received)
        countUp(displayOutsideTAT, summary.value.outsideTAT)
      }
    }
  })

  watch(tableLoading, async (isLoading) => {
    if (isLoading) return
    if (recentBatches.value.length > 0) await animateTableEntrance()
    if (weeklyFlow.value.length > 0) await animateFlowBars()
  })

  watch(recentBatchPage, async () => {
    await animateTableRows()
  })

  const flowAnimatedOnce = ref(false)
  watch(weeklyFlow, async (newVal, oldVal) => {
    if (!newVal?.length) return
    if (!flowAnimatedOnce.value) { flowAnimatedOnce.value = true; return }
    const hasChanged = JSON.stringify(newVal) !== JSON.stringify(oldVal)
    if (!hasChanged) return
    await animateFlowBars()
  }, { deep: true })

  // ── Data fetch ─────────────────────────────────────────────────────────────

  async function fetchKPIs() {
    try {
      if (authStore.isAdmin) {
        allSections.value = await batchApi.getAllSectionsSummary()
      } else {
        summary.value = await batchApi.getDashboardSummary(authStore.sectionCode)
      }
    } catch (err) {
      if (err.response?.status === 401) {
        showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
      } else {
        console.error('Dashboard KPI fetch error:', err)
      }
    }
  }

  async function fetchTableData() {
    try {
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
        console.error('Dashboard table fetch error:', err)
      }
    }
  }

  async function silentRefresh() {
    if (!authStore.isAuthenticated) return
    await Promise.all([fetchKPIs(), fetchTableData(), loadTatCycle()])
  }

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

  // ── Weekly flow ────────────────────────────────────────────────────────────

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

  // ── TAT Countdown ──────────────────────────────────────────────────────────

  const tatCycle = ref({ hasOpenCycle: false, cycleStart: null, thresholdMins: null, canAppeal: false })
  const nowTick = ref(Date.now())
  const tatLoading = ref(false)
  let tickInterval = null

  async function loadTatCycle() {
    if (authStore.isAdmin) return
    try {
      tatCycle.value = await tatApi.getOpenCycle(authStore.sectionCode)
    } catch {
      tatCycle.value = { hasOpenCycle: false, cycleStart: null, thresholdMins: null, canAppeal: false }
    }
  }

  const appealConfirm = ref({ visible: false })

  function confirmAppeal() { appealConfirm.value.visible = true }

  async function handleAppeal() {
    if (tatLoading.value) return
    tatLoading.value = true
    try {
      await tatApi.appeal(authStore.sectionCode)
      await loadTatCycle()
    } catch (err) {
      console.error('Appeal failed:', err)
    } finally {
      tatLoading.value = false
    }
  }

  const tatSecondsRemaining = computed(() => {
    if (!tatCycle.value.hasOpenCycle || !tatCycle.value.cycleStart || !tatCycle.value.thresholdMins) return null
    const start = new Date(tatCycle.value.cycleStart).getTime()
    const threshold = tatCycle.value.thresholdMins * 60 * 1000
    const elapsed = nowTick.value - start
    return Math.floor((threshold - elapsed) / 1000)
  })

  const tatCountdown = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return null
    if (secs <= 0) return 'EXCEEDED'
    const h = Math.floor(secs / 3600)
    const m = Math.floor((secs % 3600) / 60)
    const s = secs % 60
    if (h > 0) return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  })

  const tatProgressPct = computed(() => {
    if (!tatCycle.value.thresholdMins || tatSecondsRemaining.value === null) return 0
    const total = tatCycle.value.thresholdMins * 60
    const remaining = Math.max(tatSecondsRemaining.value, 0)
    return Math.round((remaining / total) * 100)
  })

  const tatColorStyle = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return ''
    if (secs <= 0) return 'color: var(--color-error);'
    const pct = tatProgressPct.value
    if (pct <= 25) return 'color: var(--color-warning);'
    return 'color: var(--color-success);'
  })

  const tatBarStyle = computed(() => {
    const secs = tatSecondsRemaining.value
    if (secs === null) return ''
    if (secs <= 0) return 'background-color: var(--color-error);'
    const pct = tatProgressPct.value
    if (pct <= 25) return 'background-color: var(--color-warning);'
    return 'background-color: var(--color-success);'
  })

  const tatExceeded = computed(() => tatSecondsRemaining.value !== null && tatSecondsRemaining.value <= 0)

  // ── Outbound TAT ──────────────────────────────────────────────────────────

  const outboundTat = ref({
    enabled: false,
    appealEnabled: false,
    currentWindow: null,
    nextWindow: null,
    hasWindowsToday: false,
    serverTime: null,
  })

  async function loadOutboundTatWindow() {
    if (authStore.isAdmin) return
    try {
      outboundTat.value = await tatOutboundApi.getCurrentWindow()
    } catch {
      outboundTat.value = {
        enabled: false,
        appealEnabled: false,
        currentWindow: null,
        nextWindow: null,
        hasWindowsToday: false,
        serverTime: null,
      }
    }
  }

  const outboundWindowSecondsRemaining = computed(() => {
    if (!outboundTat.value.currentWindow) return null
    const end = new Date(outboundTat.value.currentWindow.windowEndFull).getTime()
    return Math.floor((end - nowTick.value) / 1000)
  })

  const outboundNextWindowSecondsUntil = computed(() => {
    if (!outboundTat.value.nextWindow) return null
    const start = new Date(outboundTat.value.nextWindow.windowStartFull).getTime()
    return Math.floor((start - nowTick.value) / 1000)
  })

  function formatOutboundCountdown(secs) {
    if (secs === null) return null
    if (secs <= 0) return '00:00'
    const h = Math.floor(secs / 3600)
    const m = Math.floor((secs % 3600) / 60)
    const s = secs % 60
    if (h > 0) return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
  }

  const outboundWindowCountdown = computed(() =>
    formatOutboundCountdown(outboundWindowSecondsRemaining.value)
  )

  const outboundNextWindowCountdown = computed(() =>
    formatOutboundCountdown(outboundNextWindowSecondsUntil.value)
  )

  const outboundWindowProgressPct = computed(() => {
    if (!outboundTat.value.currentWindow) return 0
    const [sh, sm] = outboundTat.value.currentWindow.windowStart.split(':').map(Number)
    const [eh, em] = outboundTat.value.currentWindow.windowEnd.split(':').map(Number)
    const totalMins = (eh * 60 + em) - (sh * 60 + sm)
    if (totalMins <= 0) return 0
    const remaining = Math.max(outboundWindowSecondsRemaining.value ?? 0, 0)
    return Math.round((remaining / (totalMins * 60)) * 100)
  })

  const outboundWindowColorStyle = computed(() => {
    const secs = outboundWindowSecondsRemaining.value
    if (secs === null) return 'color: var(--color-text-muted);'
    if (secs <= 0) return 'color: var(--color-error);'
    // Already endorsed this window — stay green
    if (outboundTat.value.hasEndorsedThisWindow) return 'color: var(--color-success);'
    if (outboundWindowProgressPct.value <= 25) return 'color: var(--color-warning);'
    return 'color: var(--color-success);'
  })

  const outboundWindowBarStyle = computed(() => {
    const secs = outboundWindowSecondsRemaining.value
    if (secs === null) return 'background-color: var(--color-text-muted);'
    if (secs <= 0) return 'background-color: var(--color-error);'
    if (outboundTat.value.hasEndorsedThisWindow) return 'background-color: var(--color-success);'
    if (outboundWindowProgressPct.value <= 25) return 'background-color: var(--color-warning);'
    return 'background-color: var(--color-success);'
  })

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
