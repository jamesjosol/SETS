<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-8">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Dashboard</h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        {{ today }} ·
        <span v-if="authStore.isAdmin" style="color: var(--color-primary); font-weight: 700;">ADMINISTRATOR</span>
        <span v-else style="color: var(--color-primary); font-weight: 700;">{{ authStore.sectionName }}</span>
      </p>
    </div>

    <!-- ══════════════════════════════════════════════════════════════════════
         REGULAR / TEAM LEAD VIEW
    ══════════════════════════════════════════════════════════════════════ -->
    <template v-if="!authStore.isAdmin">

      <!-- KPI Cards -->
      <div ref="kpiCardsRef" class="grid grid-cols-2 md:grid-cols-4 gap-5 mb-8">

        <router-link to="/runner/pending" class="block">
          <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="flex justify-between items-start mb-4">
              <div class="p-2 rounded-xl" style="background-color: rgba(70,21,153,0.1);">
                <span class="material-symbols-outlined" style="color: var(--color-primary);">pending_actions</span>
              </div>
              <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimens</span>
            </div>
            <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-text);">
              <span v-if="summaryLoading" class="block h-9 w-16 rounded-lg animate-pulse" style="background-color: var(--color-surface-low);"></span>
              <span v-else>{{ displayPending }}</span>
            </h3>
            <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Pending</p>
            <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-primary);"></div>
          </div>
        </router-link>

        <router-link to="/runner/scheduled" class="block">
          <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="flex justify-between items-start mb-4">
              <div class="p-2 rounded-xl" style="background-color: rgba(74,98,109,0.1);">
                <span class="material-symbols-outlined" style="color: var(--color-info, #4a626d);">event_available</span>
              </div>
              <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Tests</span>
            </div>
            <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-info, #4a626d);">
              <span v-if="summaryLoading" class="block h-9 w-16 rounded-lg animate-pulse" style="background-color: var(--color-surface-low);"></span>
              <span v-else>{{ displayScheduled }}</span>
            </h3>
            <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Scheduled</p>
            <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-info, #4a626d);"></div>
          </div>
        </router-link>

        <router-link to="/runner/running" class="block">
          <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="flex justify-between items-start mb-4">
              <div class="p-2 rounded-xl" style="background-color: rgba(217,119,6,0.1);">
                <span class="material-symbols-outlined" style="color: var(--color-warning);">labs</span>
              </div>
              <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-warning);">SELF</span>
            </div>
            <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-warning);">
              <span v-if="summaryLoading" class="block h-9 w-16 rounded-lg animate-pulse" style="background-color: var(--color-surface-low);"></span>
              <span v-else>{{ displayRunning }}</span>
            </h3>
            <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Running</p>
            <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-warning);"></div>
          </div>
        </router-link>

        <router-link to="/runner/completed" class="block">
          <div class="rounded-2xl p-6 relative overflow-hidden group cursor-pointer transition-all hover:-translate-y-0.5"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="flex justify-between items-start mb-4">
              <div class="p-2 rounded-xl" style="background-color: rgba(22,163,74,0.1);">
                <span class="material-symbols-outlined" style="color: var(--color-success, #16a34a);">check_circle</span>
              </div>
              <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-success, #16a34a);">Today</span>
            </div>
            <h3 class="text-4xl font-extrabold mb-1" style="color: var(--color-success, #16a34a);">
              <span v-if="summaryLoading" class="block h-9 w-16 rounded-lg animate-pulse" style="background-color: var(--color-surface-low);"></span>
              <span v-else>{{ displayCompleted }}</span>
            </h3>
            <p class="text-xs font-bold uppercase tracking-tighter" style="color: var(--color-text-muted);">Completed</p>
            <div class="absolute bottom-0 left-0 w-full h-0.5 transform scale-x-0 group-hover:scale-x-100 transition-transform origin-left duration-500" style="background-color: var(--color-success, #16a34a);"></div>
          </div>
        </router-link>
      </div>

      <!-- Regular Bottom Grid -->
      <div class="grid grid-cols-12 gap-6">
        <div class="col-span-12 lg:col-span-8 flex flex-col gap-6">

          <!-- My Running Tests -->
          <div ref="runningCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
              <div class="flex items-center gap-2">
                <span class="material-symbols-outlined text-base" style="color: var(--color-warning);">labs</span>
                <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">
                  {{ runningView === 'self' ? 'My Running Tests' : 'All Running Tests' }}
                </h2>
              </div>
              <div class="flex items-center gap-3">
                <!-- Self | All toggle -->
                <div class="flex rounded-xl overflow-hidden text-[10px] font-bold uppercase tracking-widest"
                     style="border: 1.5px solid var(--color-border);">
                  <button @click="runningView = 'self'"
                          class="px-3 py-1.5 transition-colors"
                          :style="runningView === 'self'
              ? 'background-color: var(--color-primary); color: #fff;'
              : 'background-color: transparent; color: var(--color-text-muted);'">
                    Self
                  </button>
                  <button @click="runningView = 'all'"
                          class="px-3 py-1.5 transition-colors"
                          :style="runningView === 'all'
              ? 'background-color: var(--color-primary); color: #fff;'
              : 'background-color: transparent; color: var(--color-text-muted);'">
                    All
                  </button>
                </div>
                <router-link to="/runner/running" class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-primary);">View All →</router-link>
              </div>
            </div>
            <div v-if="runningLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!displayedRunning.length" ref="runningEmptyRef" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">labs</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
              <p class="text-xs" style="color: var(--color-text-muted);">You have no tests currently running.</p>
            </div>
            <div v-else ref="runningListRef">
              <div v-for="specimen in displayedRunning" :key="specimen.headerId"
                   class="running-item px-6 py-4 flex flex-col gap-2" style="border-bottom: 1px solid var(--color-border);">
                <div class="flex items-center justify-between">
                  <div class="flex items-center gap-3">
                    <div class="p-2 rounded-xl" style="background-color: rgba(217,119,6,0.1);">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-warning);">labs</span>
                    </div>
                    <div>
                      <p class="text-xs font-bold font-mono" style="color: var(--color-text);">{{ specimen.specimenNo }}</p>
                      <p class="text-[10px]" style="color: var(--color-text-muted);">{{ specimen.patientName ?? '—' }}</p>
                    </div>
                  </div>
                  <div class="text-right">
                    <p class="text-xs font-semibold" style="color: var(--color-text);">{{ specimen.sampleTypeName }}</p>
                    <p class="text-[10px]" style="color: var(--color-text-muted);">({{ specimen.sampleTypeCode }})</p>
                  </div>
                </div>
                <div class="flex flex-wrap gap-2 pl-11">
                  <div v-for="test in specimen.tests" :key="test.id"
                       class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg"
                       style="background-color: rgba(217,119,6,0.08);">
                    <span class="text-[10px] font-bold font-mono" style="color: var(--color-warning);">{{ test.testCode }}</span>
                    <span class="text-[10px]" style="color: var(--color-text-muted);">{{ test.testName }}</span>
                    <span v-if="test.runAt" class="text-[10px] font-bold" style="color: var(--color-text-muted);">· {{ formatDt(test.runAt) }}</span>
                    <span v-if="runningView === 'all' && test.assignedRMT"
                          class="text-[10px] font-bold px-1.5 py-0.5 rounded"
                          style="background-color: rgba(70,21,153,0.08); color: var(--color-primary);">
                      {{ test.assignedRMT }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Due Today -->
          <div ref="dueTodayCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
              <div class="flex items-center gap-2">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">today</span>
                <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Due Today</h2>
              </div>
              <router-link to="/runner/pending" class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-primary);">View All →</router-link>
            </div>
            <div v-if="scheduledLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!dueToday.length" ref="dueTodayEmptyRef" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">event_available</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Nothing due today</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No specimens scheduled to run today.</p>
            </div>
            <div v-else ref="dueTodayListRef">
              <div v-for="specimen in dueToday" :key="specimen.headerId"
                   class="due-today-item px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
                <div class="flex items-center gap-3 min-w-0">
                  <div class="p-2 rounded-xl flex-shrink-0" style="background-color: var(--color-primary-soft);">
                    <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">labs</span>
                  </div>
                  <div class="min-w-0">
                    <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ specimen.specimenNo }}</p>
                    <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ specimen.patientName ?? '—' }}</p>
                  </div>
                </div>
                <div class="flex items-center gap-2 flex-shrink-0 flex-wrap justify-end">
                  <span v-for="tag in getDistinctTags(specimen)" :key="tag"
                        class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                        :style="scheduleTagStyle(tag)">{{ tag }}</span>
                  <span class="text-xs font-semibold" style="color: var(--color-text-muted);">
                    {{ specimen.tests.length }} test{{ specimen.tests.length !== 1 ? 's' : '' }}
                  </span>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- Regular Right col -->
        <div class="col-span-12 lg:col-span-4 flex flex-col gap-6">

          <!-- Recently Routed -->
          <div ref="routedCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center justify-between" style="border-bottom: 1px solid var(--color-border);">
              <div class="flex items-center gap-2">
                <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">move_to_inbox</span>
                <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Recently Routed</h2>
                <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(70,21,153,0.1); color: var(--color-primary);">Not yet received</span>
              </div>
              <router-link to="/runner/pending" class="text-xs font-bold uppercase tracking-widest" style="color: var(--color-primary);">View All →</router-link>
            </div>
            <div v-if="pendingLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!recentlyRouted.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">All caught up</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No unreceived specimens at the moment.</p>
            </div>
            <div v-else ref="routedListRef">
              <div v-for="item in recentlyRouted" :key="item.id"
                   class="routed-item px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
                <div class="flex items-center gap-3 min-w-0">
                  <div class="p-2 rounded-xl flex-shrink-0" style="background-color: var(--color-primary-soft);">
                    <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">labs</span>
                  </div>
                  <div class="min-w-0">
                    <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                    <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ item.patientName ?? '—' }}</p>
                  </div>
                </div>
                <div class="flex flex-col items-end flex-shrink-0 gap-0.5">
                  <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                  <span class="text-[10px]" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Completed Today -->
          <div ref="completedCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-success, #16a34a);">check_circle</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Completed Today</h2>
            </div>
            <div v-if="completedTodayLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-10 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!completedTodayList.length" class="p-8 flex flex-col items-center gap-2">
              <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted);">check_circle</span>
              <p class="text-xs font-bold" style="color: var(--color-text-muted);">None completed yet</p>
            </div>
            <div v-else ref="completedListRef">
              <div v-for="item in completedTodayList" :key="item.id"
                   class="completed-item px-6 py-3 flex items-center justify-between gap-3" style="border-bottom: 1px solid var(--color-border);">
                <div class="flex items-center gap-3 min-w-0">
                  <div class="p-2 rounded-xl flex-shrink-0" style="background-color: rgba(22,163,74,0.1);">
                    <span class="material-symbols-outlined text-sm" style="color: var(--color-success, #16a34a);">check_circle</span>
                  </div>
                  <div class="min-w-0">
                    <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                    <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ item.patientName ?? '—' }}</p>
                  </div>
                </div>
                <div class="flex flex-col items-end flex-shrink-0 gap-0.5">
                  <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold"
                        style="background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);">Released</span>
                </div>
              </div>
            </div>
          </div>

          <!-- System Status -->
          <SystemStatus />

        </div>
      </div>

    </template>

    <!-- ══════════════════════════════════════════════════════════════════════
         ADMIN VIEW
    ══════════════════════════════════════════════════════════════════════ -->
    <template v-else>

      <!-- Admin KPI Cards — per section grid -->
      <div class="mb-8">
        <div v-if="adminSummaryLoading" class="grid grid-cols-2 md:grid-cols-4 gap-5">
          <div v-for="i in 4" :key="i" class="rounded-2xl p-6 animate-pulse h-32"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);"></div>
        </div>
        <div v-else-if="!adminSectionSummaries.length" class="p-10 rounded-2xl flex flex-col items-center gap-3"
             style="background-color: var(--color-surface);">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">science</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">No active lab sections found</p>
        </div>
        <div v-else ref="adminKpiRef" class="grid gap-4"
             :style="`grid-template-columns: repeat(${Math.min(adminSectionSummaries.length, 4)}, minmax(0, 1fr));`">
          <div v-for="sec in adminSectionSummaries" :key="sec.sectionCode"
               class="admin-kpi-card rounded-2xl p-5 relative overflow-hidden transition-all hover:-translate-y-0.5"
               style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <p class="text-[10px] font-bold uppercase tracking-widest mb-3 truncate" style="color: var(--color-primary);">{{ sec.sectionName }}</p>
            <div class="grid grid-cols-4 gap-1 text-center">
              <div>
                <p class="text-xl font-extrabold" style="color: var(--color-text);">{{ sec.pending }}</p>
                <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Pending</p>
              </div>
              <div>
                <p class="text-xl font-extrabold" style="color: var(--color-info, #4a626d);">{{ sec.scheduled }}</p>
                <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Sched.</p>
              </div>
              <div>
                <p class="text-xl font-extrabold" style="color: var(--color-warning);">{{ sec.running }}</p>
                <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Running</p>
              </div>
              <div>
                <p class="text-xl font-extrabold" style="color: var(--color-success, #16a34a);">{{ sec.completedToday }}</p>
                <p class="text-[9px] font-bold uppercase tracking-tighter mt-0.5" style="color: var(--color-text-muted);">Done</p>
              </div>
            </div>
            <div class="absolute bottom-0 left-0 w-full h-0.5" style="background-color: var(--color-primary); opacity: 0.3;"></div>
          </div>
        </div>
      </div>

      <!-- Admin Bottom Grid -->
      <div class="grid grid-cols-12 gap-6">
        <div class="col-span-12 lg:col-span-8 flex flex-col gap-6">

          <!-- All Running Tests — grouped by section -->
          <div ref="adminRunningCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-warning);">labs</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">All Running Tests</h2>
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(217,119,6,0.1); color: var(--color-warning);">All Sections</span>
            </div>
            <div v-if="adminRunningLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminRunning.length" ref="adminRunningEmptyRef" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">labs</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No tests are currently running across all sections.</p>
            </div>
            <div v-else ref="adminRunningListRef">
              <div v-for="group in adminRunning" :key="group.sectionCode">
                <div class="px-6 py-2 flex items-center gap-2" style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border); border-top: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">science</span>
                  <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full ml-auto"
                        style="background-color: rgba(217,119,6,0.1); color: var(--color-warning);">
                    {{ group.specimens.reduce((sum, s) => sum + s.tests.length, 0) }} running
                  </span>
                </div>
                <div v-for="specimen in group.specimens" :key="specimen.headerId"
                     class="admin-running-item px-6 py-4 flex flex-col gap-2" style="border-bottom: 1px solid var(--color-border);">
                  <div class="flex items-center justify-between">
                    <div class="flex items-center gap-3">
                      <div class="p-2 rounded-xl" style="background-color: rgba(217,119,6,0.1);">
                        <span class="material-symbols-outlined text-sm" style="color: var(--color-warning);">labs</span>
                      </div>
                      <div>
                        <p class="text-xs font-bold font-mono" style="color: var(--color-text);">{{ specimen.specimenNo }}</p>
                        <p class="text-[10px]" style="color: var(--color-text-muted);">{{ specimen.patientName ?? '—' }}</p>
                      </div>
                    </div>
                    <div class="text-right">
                      <p class="text-xs font-semibold" style="color: var(--color-text);">{{ specimen.sampleTypeName }}</p>
                      <p class="text-[10px]" style="color: var(--color-text-muted);">({{ specimen.sampleTypeCode }})</p>
                    </div>
                  </div>
                  <div class="flex flex-wrap gap-2 pl-11">
                    <div v-for="test in specimen.tests" :key="test.id"
                         class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg"
                         style="background-color: rgba(217,119,6,0.08);">
                      <span class="text-[10px] font-bold font-mono" style="color: var(--color-warning);">{{ test.testCode }}</span>
                      <span class="text-[10px]" style="color: var(--color-text-muted);">{{ test.testName }}</span>
                      <span v-if="test.assignedRMT" class="text-[10px] font-bold" style="color: var(--color-text-muted);">· {{ test.assignedRMT }}</span>
                      <span v-if="test.runAt" class="text-[10px]" style="color: var(--color-text-muted);">· {{ formatDt(test.runAt) }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Due Today — all sections -->
          <div ref="adminDueTodayCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">today</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Due Today</h2>
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(70,21,153,0.1); color: var(--color-primary);">All Sections</span>
            </div>
            <div v-if="adminDueTodayLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminDueToday.length" ref="adminDueTodayEmptyRef" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">event_available</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Nothing due today</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No specimens scheduled across all sections today.</p>
            </div>
            <div v-else ref="adminDueTodayListRef">
              <div v-for="group in adminDueToday" :key="group.sectionCode">
                <div class="px-6 py-2 flex items-center gap-2" style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border); border-top: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">science</span>
                  <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</span>
                </div>
                <div v-for="specimen in group.specimens" :key="specimen.headerId"
                     class="admin-due-item px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
                  <div class="flex items-center gap-3 min-w-0">
                    <div class="p-2 rounded-xl flex-shrink-0" style="background-color: var(--color-primary-soft);">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">labs</span>
                    </div>
                    <div class="min-w-0">
                      <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ specimen.specimenNo }}</p>
                      <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ specimen.patientName ?? '—' }}</p>
                    </div>
                  </div>
                  <div class="flex items-center gap-2 flex-shrink-0 flex-wrap justify-end">
                    <span v-for="tag in getDistinctTags(specimen)" :key="tag"
                          class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                          :style="scheduleTagStyle(tag)">{{ tag }}</span>
                    <span class="text-xs font-semibold" style="color: var(--color-text-muted);">
                      {{ specimen.tests.length }} test{{ specimen.tests.length !== 1 ? 's' : '' }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

        </div>

        <!-- Admin Right col -->
        <div class="col-span-12 lg:col-span-4 flex flex-col gap-6">

          <!-- Recently Routed — all sections -->
          <div ref="adminRoutedCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">move_to_inbox</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Recently Routed</h2>
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(70,21,153,0.1); color: var(--color-primary);">Not yet received · All Sections</span>
            </div>
            <div v-if="adminRoutedLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminRecentlyRouted.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">inbox</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">All caught up</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No unreceived specimens across all sections.</p>
            </div>
            <div v-else ref="adminRoutedListRef">
              <div v-for="group in adminRecentlyRouted" :key="group.sectionCode">
                <div class="px-6 py-2 flex items-center gap-2" style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border); border-top: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">science</span>
                  <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full ml-auto"
                        style="background-color: rgba(70,21,153,0.1); color: var(--color-primary);">
                    {{ group.specimens.length }} specimen{{ group.specimens.length !== 1 ? 's' : '' }}
                  </span>
                </div>
                <div v-for="item in group.specimens.slice(0, 6)" :key="item.id"
                     class="admin-routed-item px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
                  <div class="flex items-center gap-3 min-w-0">
                    <div class="p-2 rounded-xl flex-shrink-0" style="background-color: var(--color-primary-soft);">
                      <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">labs</span>
                    </div>
                    <div class="min-w-0">
                      <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                      <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ item.patientName ?? '—' }}</p>
                    </div>
                  </div>
                  <div class="flex flex-col items-end flex-shrink-0 gap-0.5">
                    <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                    <span class="text-[10px]" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Completed Today — all sections -->
          <div ref="adminCompletedCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-success, #16a34a);">check_circle</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Completed Today</h2>
            </div>
            <div v-if="adminCompletedLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-10 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminCompletedToday.length" class="p-8 flex flex-col items-center gap-2">
              <span class="material-symbols-outlined text-3xl" style="color: var(--color-text-muted);">inbox</span>
              <p class="text-xs font-bold" style="color: var(--color-text-muted);">None completed yet</p>
            </div>
            <div v-else ref="adminCompletedListRef">
              <div v-for="group in adminCompletedToday" :key="group.sectionCode">
                <div class="px-6 py-2 flex items-center gap-2" style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border); border-top: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-sm" style="color: var(--color-success, #16a34a);">science</span>
                  <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-success, #16a34a);">{{ group.sectionName }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full ml-auto"
                        style="background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);">
                    {{ group.specimens.length }}
                  </span>
                </div>
                <div v-for="item in group.specimens.slice(0, 5)" :key="item.id"
                     class="admin-completed-item px-6 py-3 flex items-center justify-between gap-3" style="border-bottom: 1px solid var(--color-border);">
                  <div class="min-w-0">
                    <p class="text-xs font-bold font-mono truncate" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                    <p class="text-[10px] truncate" style="color: var(--color-text-muted);">{{ item.patientName ?? '—' }}</p>
                  </div>
                  <span class="px-2 py-0.5 rounded-full text-[10px] font-bold flex-shrink-0"
                        style="background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);">Released</span>
                </div>
              </div>
            </div>
          </div>

          <!-- System Status -->
          <SystemStatus />

        </div>
      </div>

    </template>

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
  import { ref, computed, onMounted, onUnmounted, nextTick, watch } from 'vue'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import SystemStatus from '@/components/common/SystemStatus.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'
  import { healthApi } from '@/api/healthApi'


  const authStore = useAuthStore()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Date ───────────────────────────────────────────────────────────────────

  const today = computed(() =>
    new Date().toLocaleDateString('en-US', {
      weekday: 'long', year: 'numeric', month: 'long', day: 'numeric'
    })
  )

  const todayStr = new Date().toISOString().split('T')[0]

  // ══════════════════════════════════════════════════════════════════════════
  // REGULAR / TEAM LEAD DATA
  // ══════════════════════════════════════════════════════════════════════════

  const summaryLoading = ref(true)
  const runningLoading = ref(true)
  const scheduledLoading = ref(true)
  const pendingLoading = ref(true)
  const runningView = ref('self') // 'self' | 'all'
  const allRunningSpecimens = ref([])
  const allRunningLoading = ref(false)

  const summary = ref({ pending: 0, scheduled: 0, running: 0, completedToday: 0 })
  const runningSpecimens = ref([])
  const scheduledSpecimens = ref([])
  const allPending = ref([])

  async function fetchSummary() {
    try { summary.value = await runnerApi.getDashboardSummary(authStore.sectionCode) }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load summary.') }
  }
  async function fetchRunning() {
    try {
      const d = await runnerApi.getRunningSpecimens(authStore.sectionCode, false)
      runningSpecimens.value = Array.isArray(d) ? d : []
      const all = await runnerApi.getRunningSpecimens(authStore.sectionCode, true)
      allRunningSpecimens.value = Array.isArray(all) ? all : []
    } catch (e) {
      if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load running specimens.')
    }
  }
  async function fetchScheduled() {
    try { const d = await runnerApi.getScheduledSpecimens(authStore.sectionCode); scheduledSpecimens.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load scheduled specimens.') }
  }
  async function fetchPending() {
    try { const d = await runnerApi.getPendingSpecimens(authStore.sectionCode); allPending.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load specimens.') }
  }
  async function fetchAllRunning() {
    allRunningLoading.value = true
    try {
      const d = await runnerApi.getRunningSpecimens(authStore.sectionCode)
      allRunningSpecimens.value = Array.isArray(d) ? d : []
    } catch (e) {
      if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load running specimens.')
    } finally {
      allRunningLoading.value = false
    }
  }

  const dueToday = computed(() =>
    scheduledSpecimens.value.filter(s =>
      s.tests.some(t =>
        (t.scheduleTag === 'END' || t.scheduleTag === 'CRD' || t.scheduleTag === 'SRD') && t.runningDate === todayStr
      )
    )
  )
  const recentlyRouted = computed(() => allPending.value.filter(s => !s.receivedBy).slice(0, 8))

  const completedTodayList = ref([])
  const completedTodayLoading = ref(true)

  async function fetchCompletedToday() {
    try { const d = await runnerApi.getCompletedToday(authStore.sectionCode); completedTodayList.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load completed today.') }
  }

  const displayedRunning = computed(() =>
    runningView.value === 'self'
      ? runningSpecimens.value
      : allRunningSpecimens.value
  )

  // ══════════════════════════════════════════════════════════════════════════
  // ADMIN DATA
  // ══════════════════════════════════════════════════════════════════════════

  const adminSummaryLoading = ref(true)
  const adminRunningLoading = ref(true)
  const adminRoutedLoading = ref(true)
  const adminDueTodayLoading = ref(true)
  const adminCompletedLoading = ref(true)

  const adminSectionSummaries = ref([])
  const adminRunning = ref([])
  const adminRecentlyRouted = ref([])
  const adminDueToday = ref([])
  const adminCompletedToday = ref([])

  async function fetchAdminSummary() {
    try { const d = await runnerApi.getAllSectionsSummary(); adminSectionSummaries.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load section summaries.') }
  }
  async function fetchAdminRunning() {
    try { const d = await runnerApi.getAdminRunning(); adminRunning.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load running tests.') }
  }
  async function fetchAdminRecentlyRouted() {
    try { const d = await runnerApi.getAdminRecentlyRouted(); adminRecentlyRouted.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load recently routed.') }
  }
  async function fetchAdminDueToday() {
    try { const d = await runnerApi.getAdminDueToday(); adminDueToday.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load due today.') }
  }
  async function fetchAdminCompletedToday() {
    try { const d = await runnerApi.getAdminCompletedToday(); adminCompletedToday.value = Array.isArray(d) ? d : [] }
    catch (e) { if (e?.response?.status !== 401) showAlert('error', 'Error', 'Unable to load completed today.') }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // HELPERS
  // ══════════════════════════════════════════════════════════════════════════

  function getDistinctTags(specimen) {
    const seen = new Set()
    for (const t of specimen.tests) { if (t.scheduleTag) seen.add(t.scheduleTag) }
    return ['END', 'CRD', 'SRD'].filter(tag => seen.has(tag))
  }

  function scheduleTagStyle(tag) {
    const map = {
      END: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
      CRD: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
      SRD: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
    }
    return map[tag] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', {
      month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit'
    })
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — REFS
  // ══════════════════════════════════════════════════════════════════════════

  // Regular view
  const kpiCardsRef = ref(null)
  const runningCardRef = ref(null)
  const runningListRef = ref(null)
  const runningEmptyRef = ref(null)
  const dueTodayCardRef = ref(null)
  const dueTodayListRef = ref(null)
  const dueTodayEmptyRef = ref(null)
  const routedCardRef = ref(null)
  const routedListRef = ref(null)
  const completedCardRef = ref(null)
  const completedListRef = ref(null)

  // Admin view
  const adminKpiRef = ref(null)
  const adminRunningCardRef = ref(null)
  const adminRunningListRef = ref(null)
  const adminRunningEmptyRef = ref(null)
  const adminDueTodayCardRef = ref(null)
  const adminDueTodayListRef = ref(null)
  const adminDueTodayEmptyRef = ref(null)
  const adminRoutedCardRef = ref(null)
  const adminRoutedListRef = ref(null)
  const adminCompletedCardRef = ref(null)
  const adminCompletedListRef = ref(null)

  // Count-up display values
  const displayPending = ref(0)
  const displayScheduled = ref(0)
  const displayRunning = ref(0)
  const displayCompleted = ref(0)

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — ANIMATION FUNCTIONS
  // ══════════════════════════════════════════════════════════════════════════

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

  async function animateKpiCards() {
    await nextTick()
    if (!kpiCardsRef.value) return
    const cards = kpiCardsRef.value.querySelectorAll(':scope > a > div')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, {
      opacity: 1,
      y: 0,
      duration: 0.3,
      stagger: 0.07,
      ease: 'power2.out',
    })
  }

  async function animateAdminKpiCards() {
    await nextTick()
    if (!adminKpiRef.value) return
    const cards = adminKpiRef.value.querySelectorAll('.admin-kpi-card')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, {
      opacity: 1,
      y: 0,
      duration: 0.35,
      stagger: 0.06,
      ease: 'power2.out',
    })
  }

  // Generic: animate a card container sliding in, then stagger its items
  async function animateCard(cardRef, itemSelector, delay = 0) {
    await nextTick()
    if (!cardRef.value) return
    gsap.set(cardRef.value, { opacity: 0, y: 16 })
    gsap.to(cardRef.value, { opacity: 1, y: 0, duration: 0.3, ease: 'power2.out', delay })
    const items = cardRef.value.querySelectorAll(itemSelector)
    if (items.length) {
      gsap.set(items, { opacity: 0, x: -8 })
      gsap.to(items, {
        opacity: 1,
        x: 0,
        duration: 0.2,
        stagger: 0.04,
        ease: 'power1.out',
        delay: delay + 0.14,
        clearProps: 'opacity,x',
      })
    }
  }

  async function animateEmptyState(emptyRef) {
    await nextTick()
    if (!emptyRef.value) return
    gsap.set(emptyRef.value, { scale: 0.92, opacity: 0 })
    gsap.to(emptyRef.value, { scale: 1, opacity: 1, duration: 0.32, ease: 'back.out(1.5)', clearProps: 'scale,opacity' })
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — WATCHERS (fire once on initial load, not on silent refresh)
  // ══════════════════════════════════════════════════════════════════════════

  // Regular view — KPI cards + count-up
  watch(summaryLoading, async (isLoading) => {
    if (isLoading) return
    // await animateKpiCards()
    countUp(displayPending, summary.value.pending)
    countUp(displayScheduled, summary.value.scheduled)
    countUp(displayRunning, summary.value.running)
    countUp(displayCompleted, summary.value.completedToday)
  })

  // Keep display values in sync on silent refresh (no re-animation)
  watch(summary, (val) => {
    if (summaryLoading.value) return
    displayPending.value = val.pending
    displayScheduled.value = val.scheduled
    displayRunning.value = val.running
    displayCompleted.value = val.completedToday
  }, { deep: true })

  // Running card
  watch(runningLoading, async (isLoading) => {
    if (isLoading) return
    if (displayedRunning.value.length) {
      await animateCard(runningCardRef, '.running-item')
    } else {
      await animateEmptyState(runningEmptyRef)
    }
  })

  // Due Today card
  watch(scheduledLoading, async (isLoading) => {
    if (isLoading) return
    if (dueToday.value.length) {
      await animateCard(dueTodayCardRef, '.due-today-item', 0.05)
    } else {
      await animateEmptyState(dueTodayEmptyRef)
    }
  })

  // Recently Routed card
  watch(pendingLoading, async (isLoading) => {
    if (isLoading) return
    if (recentlyRouted.value.length) {
      await animateCard(routedCardRef, '.routed-item', 0)
    }
  })

  // Completed Today card
  watch(completedTodayLoading, async (isLoading) => {
    if (isLoading) return
    if (completedTodayList.value.length) {
      await animateCard(completedCardRef, '.completed-item', 0.05)
    }
  })

  // Admin — section KPI cards
  watch(adminSummaryLoading, async (isLoading) => {
    if (isLoading) return
    await animateAdminKpiCards()
  })

  // Admin — Running card
  watch(adminRunningLoading, async (isLoading) => {
    if (isLoading) return
    if (adminRunning.value.length) {
      await animateCard(adminRunningCardRef, '.admin-running-item')
    } else {
      await animateEmptyState(adminRunningEmptyRef)
    }
  })

  // Admin — Due Today card
  watch(adminDueTodayLoading, async (isLoading) => {
    if (isLoading) return
    if (adminDueToday.value.length) {
      await animateCard(adminDueTodayCardRef, '.admin-due-item', 0.05)
    } else {
      await animateEmptyState(adminDueTodayEmptyRef)
    }
  })

  // Admin — Recently Routed card
  watch(adminRoutedLoading, async (isLoading) => {
    if (isLoading) return
    if (adminRecentlyRouted.value.length) {
      await animateCard(adminRoutedCardRef, '.admin-routed-item')
    }
  })

  // Admin — Completed Today card
  watch(adminCompletedLoading, async (isLoading) => {
    if (isLoading) return
    if (adminCompletedToday.value.length) {
      await animateCard(adminCompletedCardRef, '.admin-completed-item', 0.05)
    }
  })

  // ══════════════════════════════════════════════════════════════════════════
  // MOUNT / UNMOUNT
  // ══════════════════════════════════════════════════════════════════════════

  async function silentRefreshRegular() {
    if (!authStore.isAuthenticated) return
    await Promise.all([fetchSummary(), fetchRunning(), fetchScheduled(), fetchPending(), fetchCompletedToday()])
  }
  async function silentRefreshAdmin() {
    if (!authStore.isAuthenticated) return
    await Promise.all([
      fetchAdminSummary(), fetchAdminRunning(),
      fetchAdminRecentlyRouted(), fetchAdminDueToday(), fetchAdminCompletedToday(),
    ])
  }

  let dataInterval = null

  onMounted(async () => {
    if (!authStore.isAdmin) {
      await fetchSummary(); summaryLoading.value = false
      await fetchRunning(); runningLoading.value = false
      await fetchScheduled(); scheduledLoading.value = false
      await fetchPending(); pendingLoading.value = false
      await fetchCompletedToday(); completedTodayLoading.value = false
      dataInterval = setInterval(silentRefreshRegular, 10000)
    } else {
      await fetchAdminSummary(); adminSummaryLoading.value = false
      await fetchAdminRunning(); adminRunningLoading.value = false
      await fetchAdminRecentlyRouted(); adminRoutedLoading.value = false
      await fetchAdminDueToday(); adminDueTodayLoading.value = false
      await fetchAdminCompletedToday(); adminCompletedLoading.value = false
      dataInterval = setInterval(silentRefreshAdmin, 10000)
    }
  })

  onUnmounted(() => {
    clearInterval(dataInterval)
  })
</script>
