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
      <div class="grid grid-cols-2 md:grid-cols-4 gap-5 mb-8">

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
              <span v-else>{{ summary.pending }}</span>
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
              <span v-else>{{ summary.scheduled }}</span>
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
              <span v-else>{{ summary.running }}</span>
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
              <span v-else>{{ summary.completedToday }}</span>
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else-if="!displayedRunning.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">labs</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
              <p class="text-xs" style="color: var(--color-text-muted);">You have no tests currently running.</p>
            </div>
            <div v-else>
              <div v-for="specimen in displayedRunning" :key="specimen.headerId"
                   class="px-6 py-4 flex flex-col gap-2" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else-if="!dueToday.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">event_available</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Nothing due today</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No specimens scheduled to run today.</p>
            </div>
            <div v-else>
              <div v-for="specimen in dueToday" :key="specimen.headerId"
                   class="px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else>
              <div v-for="item in recentlyRouted" :key="item.id"
                   class="px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else>
              <div v-for="item in completedTodayList" :key="item.id"
                   class="px-6 py-3 flex items-center justify-between gap-3" style="border-bottom: 1px solid var(--color-border);">
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
        <div v-else class="grid gap-4"
             :style="`grid-template-columns: repeat(${Math.min(adminSectionSummaries.length, 4)}, minmax(0, 1fr));`">
          <div v-for="sec in adminSectionSummaries" :key="sec.sectionCode"
               class="rounded-2xl p-5 relative overflow-hidden transition-all hover:-translate-y-0.5"
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-warning);">labs</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">All Running Tests</h2>
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(217,119,6,0.1); color: var(--color-warning);">All Sections</span>
            </div>
            <div v-if="adminRunningLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminRunning.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">labs</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No running tests</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No tests are currently running across all sections.</p>
            </div>
            <div v-else>
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
                     class="px-6 py-4 flex flex-col gap-2" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
            <div class="px-6 py-4 flex items-center gap-2" style="border-bottom: 1px solid var(--color-border);">
              <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">today</span>
              <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text);">Due Today</h2>
              <span class="text-[10px] font-bold px-2 py-0.5 rounded-full" style="background-color: rgba(70,21,153,0.1); color: var(--color-primary);">All Sections</span>
            </div>
            <div v-if="adminDueTodayLoading" class="p-6 flex flex-col gap-3">
              <div v-for="i in 3" :key="i" class="h-12 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
            </div>
            <div v-else-if="!adminDueToday.length" class="p-10 flex flex-col items-center gap-3">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">event_available</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Nothing due today</p>
              <p class="text-xs" style="color: var(--color-text-muted);">No specimens scheduled across all sections today.</p>
            </div>
            <div v-else>
              <div v-for="group in adminDueToday" :key="group.sectionCode">
                <div class="px-6 py-2 flex items-center gap-2" style="background-color: var(--color-surface-low); border-bottom: 1px solid var(--color-border); border-top: 1px solid var(--color-border);">
                  <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">science</span>
                  <span class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</span>
                </div>
                <div v-for="specimen in group.specimens" :key="specimen.headerId"
                     class="px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else>
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
                     class="px-6 py-3 flex items-center justify-between gap-4" style="border-bottom: 1px solid var(--color-border);">
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
          <div class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
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
            <div v-else>
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
                     class="px-6 py-3 flex items-center justify-between gap-3" style="border-bottom: 1px solid var(--color-border);">
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
  import { ref, computed, onMounted, onUnmounted } from 'vue'
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
        (t.scheduleTag === 'END' || t.scheduleTag === 'CRD' ||  t.scheduleTag === 'SRD') && t.runningDate === todayStr
      )
    )
  )
  const recentlyRouted = computed(() => allPending.value.filter(s => !s.receivedBy).slice(0, 8))

  const completedTodayList = ref([])
  const completedTodayLoading = ref(true)

  async function fetchCompletedToday() {
    try { const d = await runnerApi.getCompletedToday(authStore.sectionCode); completedTodayList.value = Array.isArray(d) ? d : []

      console.log(d)
      console.log('called')
    }
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
  let statusInterval = null

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
