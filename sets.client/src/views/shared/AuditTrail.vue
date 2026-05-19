<template>
  <AppLayout>

    <!-- Page Header -->
    <div ref="pageHeaderRef" class="mb-6">
      <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Audit Trail</h1>
      <p class="text-sm mt-1" style="color: var(--color-text-muted);">
        <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">
          {{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}
        </span>
        · {{ authStore.branchCode }} · System activity log
      </p>
    </div>

    <!-- Tab Navigation -->
    <div ref="tabNavRef" class="flex gap-1 mb-6 p-1 rounded-2xl w-fit"
         style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
      <button v-for="tab in visibleTabs"
              :key="tab.key"
              class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
              :style="activeTab === tab.key
                ? 'background-color: var(--color-primary); color: #ffffff; box-shadow: 0 2px 8px var(--color-shadow);'
                : 'color: var(--color-text-muted);'"
              @click="activeTab = tab.key">
        <span class="material-symbols-outlined text-sm">{{ tab.icon }}</span>
        {{ tab.label }}
      </button>
    </div>

    <!-- ── BY USER ──────────────────────────────────────────────────────── -->
    <template v-if="activeTab === 'by-user'">

      <!-- Filter Bar -->
      <div ref="byUserFilterRef" class="rounded-2xl p-5 mb-5 flex items-end gap-4 flex-wrap"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">User ID</label>
          <div class="relative">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
                  style="color: var(--color-text-muted);">person</span>
            <input v-model="userFilter.userID"
                   type="text"
                   placeholder="e.g. JDELACRUZ"
                   class="pl-9 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none border transition-all w-44"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border-color: var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'"
                   @input="e => userFilter.userID = e.target.value.toUpperCase()"
                   @keydown.enter="loadByUser" />
          </div>
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Date From</label>
          <DatePicker v-model="userFilter.dateFrom"
                      placeholder="Date From"
                      :max-date="userFilter.dateTo" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Date To</label>
          <DatePicker v-model="userFilter.dateTo"
                      placeholder="Date To"
                      :min-date="userFilter.dateFrom" />
        </div>

        <div class="flex items-center gap-2 pb-0.5">
          <button class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                  style="background: var(--color-primary-gradient); color: #ffffff;"
                  @click="loadByUser">
            <span class="material-symbols-outlined text-sm"
                  :class="{ 'animate-spin': byUserLoading }">
              {{ byUserLoading ? 'progress_activity' : 'search' }}
            </span>
            List
          </button>
          <button class="flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="clearByUser">
            <span class="material-symbols-outlined text-sm">filter_list_off</span>
            Clear
          </button>
        </div>
      </div>

      <!-- Table Card -->
      <div ref="byUserCardRef" class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center justify-between"
             style="border-bottom: 1px solid var(--color-surface-low);">
          <h2 class="text-base font-bold" style="color: var(--color-text);">Results</h2>
          <span v-if="!byUserLoading && !byUserError && byUserSearched"
                class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ byUserLogs.length }} record{{ byUserLogs.length !== 1 ? 's' : '' }}
            <template v-if="byUserTotalPages > 1">
              · Page {{ byUserPage }} of {{ byUserTotalPages }}
            </template>
          </span>
        </div>
        <div v-if="byUserLoading" class="px-8 py-16 flex items-center justify-center gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
          <span class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</span>
        </div>
        <div v-else-if="byUserError" class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-error);">error_outline</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">{{ byUserError }}</p>
          <button class="mt-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                  @click="loadByUser">
            Retry
          </button>
        </div>
        <div v-else-if="!byUserSearched || byUserLogs.length === 0"
             ref="byUserEmptyRef"
             class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">
            {{ byUserSearched ? 'inbox' : 'manage_search' }}
          </span>
          <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
            {{ byUserSearched ? 'No records found' : 'Enter a User ID and date range, then click List.' }}
          </p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Event</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Lab No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">From</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">To</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">User ID</th>
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Date &amp; Time</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in byUserPaginated" :key="log.id"
                  class="by-user-row cursor-pointer transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                  @click="drawerLog = log">
                <td class="px-8 py-4">
                  <div class="flex items-center gap-2">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                          :style="getEventStyle(log.eventCode)">
                      <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getEventDot(log.eventCode)}`"></span>
                      {{ log.eventLabel }}
                    </span>
                    <span v-if="log.isOutsideTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside TAT</span>
                    <span v-if="log.isOutsideProcTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside Proc TAT</span>
                  </div>
                </td>
                <td class="px-4 py-4 font-mono text-sm font-bold" style="color: var(--color-primary);">{{ log.specimenNo ?? '—' }}</td>
                <td class="px-4 py-4">
                  <p v-if="log.patientName" class="text-sm font-bold" style="color: var(--color-text);">{{ log.patientName }}</p>
                  <p v-if="log.pid" class="text-xs" style="color: var(--color-text-muted);">{{ log.pid }}</p>
                  <span v-if="!log.patientName" style="color: var(--color-text-muted);">—</span>
                </td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.fromLocationName ?? '—' }}</td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.toLocationName ?? '—' }}</td>
                <td class="px-4 py-4">
                  <span class="text-xs font-bold px-2.5 py-1 rounded-lg"
                        :style="log.userID === 'MIDDLEWARE'
                          ? 'background-color: rgba(37,99,235,0.08); color: #2563eb;'
                          : 'background-color: var(--color-surface-low); color: var(--color-text);'">
                    {{ log.userID }}
                  </span>
                </td>
                <td class="px-8 py-4 text-xs" style="color: var(--color-text-muted);">{{ formatDateTime(log.loggedAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!byUserLoading && !byUserError && byUserTotalPages > 1"
             class="px-8 py-4 flex items-center justify-between"
             style="border-top: 1px solid var(--color-surface-low);">
          <button :disabled="byUserPage === 1"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="byUserPage--">
            <span class="material-symbols-outlined text-sm">chevron_left</span>Previous
          </button>
          <div class="flex items-center gap-1.5">
            <button v-for="p in byUserPageNumbers" :key="p"
                    class="w-8 h-8 rounded-lg text-xs font-bold transition-all"
                    :style="p === byUserPage
                      ? 'background-color: var(--color-primary); color: #fff;'
                      : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                    @click="byUserPage = p">
              {{ p }}
            </button>
          </div>
          <button :disabled="byUserPage === byUserTotalPages"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="byUserPage++">
            Next<span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </template>

    <!-- ── BY LAB NO. ───────────────────────────────────────────────────── -->
    <template v-if="activeTab === 'by-specimen'">

      <!-- Filter Bar -->
      <div class="rounded-2xl p-5 mb-5 flex items-end gap-4 flex-wrap"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Lab No. / Specimen No.</label>
          <div class="relative">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
                  style="color: var(--color-text-muted);">barcode_scanner</span>
            <input v-model="specimenFilter.specimenNo"
                   type="text"
                   placeholder="Scan or type specimen no."
                   class="pl-9 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none border transition-all w-64"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border-color: var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'"
                   @keydown.enter="loadBySpecimen" />
          </div>
        </div>
        <div class="flex items-center gap-2 pb-0.5">
          <button class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                  style="background: var(--color-primary-gradient); color: #ffffff;"
                  @click="loadBySpecimen">
            <span class="material-symbols-outlined text-sm" :class="{ 'animate-spin': bySpecimenLoading }">
              {{ bySpecimenLoading ? 'progress_activity' : 'search' }}
            </span>
            List
          </button>
          <button class="flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="clearBySpecimen">
            <span class="material-symbols-outlined text-sm">filter_list_off</span>Clear
          </button>
        </div>
      </div>

      <!-- Table Card -->
      <div ref="bySpecimenCardRef" class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center justify-between"
             style="border-bottom: 1px solid var(--color-surface-low);">
          <h2 class="text-base font-bold" style="color: var(--color-text);">Results</h2>
          <span v-if="!bySpecimenLoading && !bySpecimenError && bySpecimenSearched"
                class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ bySpecimenLogs.length }} record{{ bySpecimenLogs.length !== 1 ? 's' : '' }}
            <template v-if="bySpecimenTotalPages > 1">
              · Page {{ bySpecimenPage }} of {{ bySpecimenTotalPages }}
            </template>
          </span>
        </div>
        <div v-if="bySpecimenLoading" class="px-8 py-16 flex items-center justify-center gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
          <span class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</span>
        </div>
        <div v-else-if="bySpecimenError" class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-error);">error_outline</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">{{ bySpecimenError }}</p>
          <button class="mt-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                  @click="loadBySpecimen">
            Retry
          </button>
        </div>
        <div v-else-if="!bySpecimenSearched || bySpecimenLogs.length === 0"
             ref="bySpecimenEmptyRef"
             class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">
            {{ bySpecimenSearched ? 'inbox' : 'manage_search' }}
          </span>
          <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
            {{ bySpecimenSearched ? 'No records found' : 'Enter a specimen number and click List.' }}
          </p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Event</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Lab No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">From</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">To</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">User ID</th>
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Date &amp; Time</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in bySpecimenPaginated" :key="log.id"
                  class="by-specimen-row cursor-pointer transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                  @click="drawerLog = log">
                <td class="px-8 py-4">
                  <div class="flex items-center gap-2">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                          :style="getEventStyle(log.eventCode)">
                      <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getEventDot(log.eventCode)}`"></span>
                      {{ log.eventLabel }}
                    </span>
                    <span v-if="log.isOutsideTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside TAT</span>
                    <span v-if="log.isOutsideProcTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside Proc TAT</span>
                  </div>
                </td>
                <td class="px-4 py-4 font-mono text-sm font-bold" style="color: var(--color-primary);">{{ log.specimenNo ?? '—' }}</td>
                <td class="px-4 py-4">
                  <p v-if="log.patientName" class="text-sm font-bold" style="color: var(--color-text);">{{ log.patientName }}</p>
                  <p v-if="log.pid" class="text-xs" style="color: var(--color-text-muted);">{{ log.pid }}</p>
                  <span v-if="!log.patientName" style="color: var(--color-text-muted);">—</span>
                </td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.fromLocationName ?? '—' }}</td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.toLocationName ?? '—' }}</td>
                <td class="px-4 py-4">
                  <span class="text-xs font-bold px-2.5 py-1 rounded-lg"
                        :style="log.userID === 'MIDDLEWARE'
                          ? 'background-color: rgba(37,99,235,0.08); color: #2563eb;'
                          : 'background-color: var(--color-surface-low); color: var(--color-text);'">
                    {{ log.userID }}
                  </span>
                </td>
                <td class="px-8 py-4 text-xs" style="color: var(--color-text-muted);">{{ formatDateTime(log.loggedAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!bySpecimenLoading && !bySpecimenError && bySpecimenTotalPages > 1"
             class="px-8 py-4 flex items-center justify-between"
             style="border-top: 1px solid var(--color-surface-low);">
          <button :disabled="bySpecimenPage === 1"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="bySpecimenPage--">
            <span class="material-symbols-outlined text-sm">chevron_left</span>Previous
          </button>
          <div class="flex items-center gap-1.5">
            <button v-for="p in bySpecimenPageNumbers" :key="p"
                    class="w-8 h-8 rounded-lg text-xs font-bold transition-all"
                    :style="p === bySpecimenPage
                      ? 'background-color: var(--color-primary); color: #fff;'
                      : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                    @click="bySpecimenPage = p">
              {{ p }}
            </button>
          </div>
          <button :disabled="bySpecimenPage === bySpecimenTotalPages"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="bySpecimenPage++">
            Next<span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </template>

    <!-- ── BATCH DETAILS ────────────────────────────────────────────────── -->
    <template v-if="activeTab === 'by-batch'">

      <!-- Filter Bar -->
      <div ref="byBatchFilterRef" class="rounded-2xl p-5 mb-5 flex items-end gap-4 flex-wrap"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Batch No.</label>
          <div class="relative">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
                  style="color: var(--color-text-muted);">inventory_2</span>
            <input v-model="batchFilter.batchNo"
                   type="text"
                   placeholder="e.g. WA26-00001"
                   class="pl-9 pr-4 py-2.5 rounded-xl text-sm font-medium outline-none border transition-all w-56"
                   style="background-color: var(--color-surface-low); color: var(--color-text); border-color: var(--color-border);"
                   @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                   @blur="e => e.target.style.borderColor = 'var(--color-border)'"
                   @keydown.enter="loadByBatch" />
          </div>
        </div>
        <div class="flex items-center gap-2 pb-0.5">
          <button class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                  style="background: var(--color-primary-gradient); color: #ffffff;"
                  @click="loadByBatch">
            <span class="material-symbols-outlined text-sm" :class="{ 'animate-spin': byBatchLoading }">
              {{ byBatchLoading ? 'progress_activity' : 'search' }}
            </span>
            List
          </button>
          <button class="flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="clearByBatch">
            <span class="material-symbols-outlined text-sm">filter_list_off</span>Clear
          </button>
        </div>
      </div>

      <!-- Table Card -->
      <div ref="byBatchCardRef" class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center justify-between"
             style="border-bottom: 1px solid var(--color-surface-low);">
          <h2 class="text-base font-bold" style="color: var(--color-text);">Results</h2>
          <span v-if="!byBatchLoading && !byBatchError && byBatchSearched"
                class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ byBatchLogs.length }} record{{ byBatchLogs.length !== 1 ? 's' : '' }}
            <template v-if="byBatchTotalPages > 1">
              · Page {{ byBatchPage }} of {{ byBatchTotalPages }}
            </template>
          </span>
        </div>
        <div v-if="byBatchLoading" class="px-8 py-16 flex items-center justify-center gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
          <span class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</span>
        </div>
        <div v-else-if="byBatchError" class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-error);">error_outline</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">{{ byBatchError }}</p>
          <button class="mt-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                  @click="loadByBatch">
            Retry
          </button>
        </div>
        <div v-else-if="!byBatchSearched || byBatchLogs.length === 0"
             ref="byBatchEmptyRef"
             class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">
            {{ byBatchSearched ? 'inbox' : 'manage_search' }}
          </span>
          <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
            {{ byBatchSearched ? 'No records found' : 'Enter a batch number and click List.' }}
          </p>
        </div>
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Event</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Lab No.</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">From</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">To</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">User ID</th>
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Date &amp; Time</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="log in byBatchPaginated" :key="log.id"
                  class="by-batch-row cursor-pointer transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                  @click="drawerLog = log">
                <td class="px-8 py-4">
                  <div class="flex items-center gap-2">
                    <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1 w-fit"
                          :style="getEventStyle(log.eventCode)">
                      <span class="w-1.5 h-1.5 rounded-full" :style="`background-color: ${getEventDot(log.eventCode)}`"></span>
                      {{ log.eventLabel }}
                    </span>
                    <span v-if="log.isOutsideTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside TAT</span>
                    <span v-if="log.isOutsideProcTat"
                          class="px-2 py-0.5 rounded-full text-[9px] font-bold uppercase tracking-tight"
                          style="background-color: rgba(239,68,68,0.10); color: var(--color-error);">Outside Proc TAT</span>
                  </div>
                </td>
                <td class="px-4 py-4 font-mono text-sm font-bold" style="color: var(--color-primary);">{{ log.specimenNo ?? '—' }}</td>
                <td class="px-4 py-4">
                  <p v-if="log.patientName" class="text-sm font-bold" style="color: var(--color-text);">{{ log.patientName }}</p>
                  <p v-if="log.pid" class="text-xs" style="color: var(--color-text-muted);">{{ log.pid }}</p>
                  <span v-if="!log.patientName" style="color: var(--color-text-muted);">—</span>
                </td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.fromLocationName ?? '—' }}</td>
                <td class="px-4 py-4 text-xs" style="color: var(--color-text-muted);">{{ log.toLocationName ?? '—' }}</td>
                <td class="px-4 py-4">
                  <span class="text-xs font-bold px-2.5 py-1 rounded-lg"
                        :style="log.userID === 'MIDDLEWARE'
                          ? 'background-color: rgba(37,99,235,0.08); color: #2563eb;'
                          : 'background-color: var(--color-surface-low); color: var(--color-text);'">
                    {{ log.userID }}
                  </span>
                </td>
                <td class="px-8 py-4 text-xs" style="color: var(--color-text-muted);">{{ formatDateTime(log.loggedAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!byBatchLoading && !byBatchError && byBatchTotalPages > 1"
             class="px-8 py-4 flex items-center justify-between"
             style="border-top: 1px solid var(--color-surface-low);">
          <button :disabled="byBatchPage === 1"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="byBatchPage--">
            <span class="material-symbols-outlined text-sm">chevron_left</span>Previous
          </button>
          <div class="flex items-center gap-1.5">
            <button v-for="p in byBatchPageNumbers" :key="p"
                    class="w-8 h-8 rounded-lg text-xs font-bold transition-all"
                    :style="p === byBatchPage
                      ? 'background-color: var(--color-primary); color: #fff;'
                      : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                    @click="byBatchPage = p">
              {{ p }}
            </button>
          </div>
          <button :disabled="byBatchPage === byBatchTotalPages"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="byBatchPage++">
            Next<span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </template>

    <!-- ── TAT CYCLE LOG ────────────────────────────────────────────────── -->
    <template v-if="activeTab === 'tat-cycle'">

      <!-- Filter Bar -->
      <div class="rounded-2xl p-5 mb-5 flex items-end gap-4 flex-wrap"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <!-- Section dropdown — admin only -->
        <div v-if="authStore.isAdmin" class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Section</label>
          <div class="relative">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-base pointer-events-none"
                  style="color: var(--color-text-muted);">outbox</span>
            <select v-model="tatCycleFilter.sectionCode"
                    class="pl-9 pr-8 py-2.5 rounded-xl text-sm font-medium outline-none border transition-all appearance-none w-52"
                    style="background-color: var(--color-surface-low); color: var(--color-text); border-color: var(--color-border);"
                    @focus="e => e.target.style.borderColor = 'var(--color-primary)'"
                    @blur="e => e.target.style.borderColor = 'var(--color-border)'">
              <option value="">All Sections</option>
              <option v-for="s in endorsingSections" :key="s.code" :value="s.code">{{ s.name }}</option>
            </select>
            <span class="material-symbols-outlined absolute right-2.5 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
                  style="color: var(--color-text-muted);">expand_more</span>
          </div>
        </div>

        <!-- Section label — endorser (read-only context) -->
        <div v-else class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Section</label>
          <div class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold"
               style="background-color: var(--color-primary-soft); color: var(--color-primary);">
            <span class="material-symbols-outlined text-base">outbox</span>
            {{ authStore.sectionName }}
          </div>
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Date From</label>
          <DatePicker v-model="tatCycleFilter.dateFrom"
                      placeholder="Date From"
                      :max-date="tatCycleFilter.dateTo" />
        </div>

        <div class="flex flex-col gap-1.5">
          <label class="text-[10px] font-bold uppercase tracking-widest"
                 style="color: var(--color-text-muted);">Date To</label>
          <DatePicker v-model="tatCycleFilter.dateTo"
                      placeholder="Date To"
                      :min-date="tatCycleFilter.dateFrom" />
        </div>

        <div class="flex items-center gap-2 pb-0.5">
          <button class="flex items-center gap-2 px-5 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                  style="background: var(--color-primary-gradient); color: #ffffff;"
                  @click="loadTatCycleLogs">
            <span class="material-symbols-outlined text-sm" :class="{ 'animate-spin': tatCycleLoading }">
              {{ tatCycleLoading ? 'progress_activity' : 'search' }}
            </span>
            List
          </button>
          <button class="flex items-center gap-1.5 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="clearTatCycleLogs">
            <span class="material-symbols-outlined text-sm">filter_list_off</span>Clear
          </button>
        </div>
      </div>

      <!-- Summary Cards — visible only when results are loaded -->
      <div v-if="tatCycleSearched && !tatCycleLoading && !tatCycleError && tatCycleLogs.length > 0"
           ref="tatSummaryCardsRef"
           class="grid grid-cols-2 gap-4 mb-5" :class="authStore.isAdmin ? 'md:grid-cols-4' : 'md:grid-cols-4'">

        <!-- Total Cycles -->
        <div class="rounded-2xl p-5 flex items-center gap-4"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-primary-soft);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-primary);">cycle</span>
          </div>
          <div>
            <p class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">{{ displayTatTotal }}</p>
            <p class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Total Cycles</p>
          </div>
        </div>

        <!-- Within TAT -->
        <div class="rounded-2xl p-5 flex items-center gap-4"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-success-soft);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-success);">check_circle</span>
          </div>
          <div>
            <p class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">{{ displayTatWithin }}</p>
            <p class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Within TAT</p>
          </div>
        </div>

        <!-- Outside TAT -->
        <div class="rounded-2xl p-5 flex items-center gap-4"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: rgba(239,68,68,0.10);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-error);">timer_off</span>
          </div>
          <div>
            <p class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">{{ displayTatOutside }}</p>
            <p class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Outside TAT</p>
          </div>
        </div>

        <!-- Appealed -->
        <div class="rounded-2xl p-5 flex items-center gap-4"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
          <div class="w-11 h-11 rounded-xl flex items-center justify-center flex-shrink-0"
               style="background-color: var(--color-warning-soft);">
            <span class="material-symbols-outlined text-lg" style="color: var(--color-warning);">do_not_disturb_on</span>
          </div>
          <div>
            <p class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">{{ displayTatAppealed }}</p>
            <p class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Appealed</p>
          </div>
        </div>
      </div>

      <!-- Table Card -->
      <div ref="tatCycleCardRef" class="rounded-2xl overflow-hidden"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <div class="px-8 py-5 flex items-center justify-between"
             style="border-bottom: 1px solid var(--color-surface-low);">
          <div>
            <h2 class="text-base font-bold" style="color: var(--color-text);">TAT Cycle Log</h2>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
              Each cycle represents the interval between consecutive batch endorsements.
            </p>
          </div>
          <span v-if="!tatCycleLoading && !tatCycleError && tatCycleSearched"
                class="text-xs font-bold" style="color: var(--color-text-muted);">
            {{ tatCycleLogs.length }} cycle{{ tatCycleLogs.length !== 1 ? 's' : '' }}
            <template v-if="tatCycleTotalPages > 1">
              · Page {{ tatCyclePage }} of {{ tatCycleTotalPages }}
            </template>
          </span>
        </div>

        <!-- Loading -->
        <div v-if="tatCycleLoading" class="px-8 py-16 flex items-center justify-center gap-3">
          <span class="material-symbols-outlined animate-spin" style="color: var(--color-text-muted);">progress_activity</span>
          <span class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Loading...</span>
        </div>

        <!-- Error -->
        <div v-else-if="tatCycleError" class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-error);">error_outline</span>
          <p class="text-sm font-bold" style="color: var(--color-text-muted);">{{ tatCycleError }}</p>
          <button class="mt-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest"
                  style="background-color: var(--color-primary-soft); color: var(--color-primary);"
                  @click="loadTatCycleLogs">
            Retry
          </button>
        </div>

        <!-- Empty -->
        <div v-else-if="!tatCycleSearched || tatCycleLogs.length === 0"
             ref="tatCycleEmptyRef"
             class="px-8 py-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">
            {{ tatCycleSearched ? 'inbox' : 'timer' }}
          </span>
          <p class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">
            {{ tatCycleSearched ? 'No cycles found for the selected range.' : 'Select a date range and click List.' }}
          </p>
        </div>

        <!-- Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-left">
            <thead>
              <tr style="background-color: var(--color-bg);">
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Result</th>
                <th v-if="authStore.isAdmin" class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Section</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Cycle Start</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Cycle End</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Elapsed</th>
                <th class="px-4 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Batch No.</th>
                <th class="px-8 py-4 text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Appealed By</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="cycle in tatCyclePaginated" :key="cycle.id"
                  class="tat-cycle-row transition-colors"
                  style="border-top: 1px solid var(--color-surface-low);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">

                <!-- Result badge -->
                <td class="px-8 py-4">
                  <span class="px-3 py-1 rounded-full text-[10px] font-bold uppercase tracking-tight flex items-center gap-1.5 w-fit"
                        :style="getTatResultStyle(cycle.result)">
                    <span class="w-1.5 h-1.5 rounded-full flex-shrink-0"
                          :style="`background-color: ${getTatResultDot(cycle.result)}`"></span>
                    {{ getTatResultLabel(cycle.result) }}
                  </span>
                </td>

                <!-- Section (admin only) -->
                <td v-if="authStore.isAdmin" class="px-4 py-4">
                  <p class="text-sm font-bold" style="color: var(--color-text);">{{ cycle.sectionName }}</p>
                  <p class="text-[10px] font-mono mt-0.5" style="color: var(--color-text-muted);">{{ cycle.sectionCode }}</p>
                </td>

                <!-- Cycle Start -->
                <td class="px-4 py-4">
                  <p class="text-sm font-medium" style="color: var(--color-text);">{{ formatDateTime(cycle.cycleStart) }}</p>
                </td>

                <!-- Cycle End -->
                <td class="px-4 py-4">
                  <p v-if="cycle.cycleEnd" class="text-sm font-medium" style="color: var(--color-text);">
                    {{ formatDateTime(cycle.cycleEnd) }}
                  </p>
                  <span v-else class="px-2.5 py-1 rounded-lg text-[10px] font-bold uppercase tracking-tight"
                        style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                    In Progress
                  </span>
                </td>

                <!-- Elapsed -->
                <td class="px-4 py-4">
                  <span v-if="cycle.elapsedMinutes != null"
                        class="font-mono text-sm font-bold"
                        :style="cycle.result === 'Outside' ? 'color: var(--color-error);' : 'color: var(--color-text);'">
                    {{ formatElapsed(cycle.elapsedMinutes) }}
                  </span>
                  <span v-else style="color: var(--color-text-muted);">—</span>
                </td>

                <!-- Batch No. -->
                <td class="px-4 py-4">
                  <span v-if="cycle.batchNo"
                        class="font-mono text-xs font-bold px-2.5 py-1 rounded-lg"
                        style="background-color: var(--color-surface-low); color: var(--color-primary);">
                    {{ cycle.batchNo }}
                  </span>
                  <span v-else style="color: var(--color-text-muted);">—</span>
                </td>

                <!-- Appealed By -->
                <td class="px-8 py-4">
                  <span v-if="cycle.appealedBy"
                        class="text-xs font-bold px-2.5 py-1 rounded-lg"
                        style="background-color: var(--color-warning-soft); color: var(--color-warning);">
                    {{ cycle.appealedBy }}
                  </span>
                  <span v-else style="color: var(--color-text-muted);">—</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Pagination -->
        <div v-if="!tatCycleLoading && !tatCycleError && tatCycleTotalPages > 1"
             class="px-8 py-4 flex items-center justify-between"
             style="border-top: 1px solid var(--color-surface-low);">
          <button :disabled="tatCyclePage === 1"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="tatCyclePage--">
            <span class="material-symbols-outlined text-sm">chevron_left</span>Previous
          </button>
          <div class="flex items-center gap-1.5">
            <button v-for="p in tatCyclePageNumbers" :key="p"
                    class="w-8 h-8 rounded-lg text-xs font-bold transition-all"
                    :style="p === tatCyclePage
                      ? 'background-color: var(--color-primary); color: #fff;'
                      : 'color: var(--color-text-muted); background-color: var(--color-surface-low);'"
                    @click="tatCyclePage = p">
              {{ p }}
            </button>
          </div>
          <button :disabled="tatCyclePage === tatCycleTotalPages"
                  class="flex items-center gap-1.5 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all disabled:opacity-40"
                  style="color: var(--color-text-muted); background-color: var(--color-surface-low);"
                  @click="tatCyclePage++">
            Next<span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </template>

    <!-- ── DETAIL DRAWER ────────────────────────────────────────────────── -->
    <Transition name="fade">
      <div v-if="drawerLog"
           class="fixed inset-0 z-[60]"
           style="background-color: rgba(0,0,0,0.4);"
           @click="drawerLog = null" />
    </Transition>

    <Transition name="slide">
      <div v-if="drawerLog"
           class="fixed top-0 right-0 h-full z-[70] flex flex-col overflow-y-auto"
           style="width: 420px; background-color: var(--color-surface); border-left: 1px solid var(--color-border); box-shadow: -4px 0 24px rgba(0,0,0,0.12);">

        <!-- Header -->
        <div class="px-6 py-5 flex items-center justify-between flex-shrink-0"
             style="border-bottom: 1px solid var(--color-border);">
          <div>
            <p class="text-[10px] font-bold uppercase tracking-widest mb-1" style="color: var(--color-text-muted);">Audit Entry</p>
            <p class="text-base font-extrabold" style="color: var(--color-text);">{{ drawerLog.eventLabel }}</p>
          </div>
          <button class="p-2 rounded-xl transition-all"
                  style="color: var(--color-text-muted);"
                  @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                  @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                  @click="drawerLog = null">
            <span class="material-symbols-outlined">close</span>
          </button>
        </div>

        <!-- Body -->
        <div class="p-6 flex flex-col flex-1">
          <div class="mb-5">
            <span class="px-3 py-1.5 rounded-full text-xs font-bold uppercase tracking-tight flex items-center gap-1.5 w-fit"
                  :style="getEventStyle(drawerLog.eventCode)">
              <span class="w-2 h-2 rounded-full" :style="`background-color: ${getEventDot(drawerLog.eventCode)}`"></span>
              {{ drawerLog.eventLabel }}
            </span>
          </div>
          <div v-if="drawerLog.tests"
               class="flex flex-col gap-1 py-3"
               style="border-bottom: 1px solid var(--color-surface-low);">
            <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Tests</p>
            <div class="flex flex-wrap gap-2">
              <span v-for="test in drawerLog.tests.split(',')" :key="test"
                    class="flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold"
                    style="background-color: var(--color-surface-low); color: var(--color-text);">
                <span class="font-extrabold text-[10px] px-1.5 py-0.5 rounded-md"
                      style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                  {{ test.split(':')[0].trim() }}
                </span>
                {{ test.split(':')[1]?.trim() ?? test.trim() }}
              </span>
            </div>
          </div>
          <template v-for="field in drawerFields" :key="field.label">
            <div v-if="field.value && field.value != '—'"
                 class="flex flex-col gap-1 py-3"
                 style="border-bottom: 1px solid var(--color-surface-low);">
              <p class="text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">{{ field.label }}</p>
              <p class="text-sm" :class="field.extra" :style="`color: ${field.color};`">{{ field.value }}</p>
            </div>
          </template>
        </div>
      </div>
    </Transition>

  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted, nextTick, watch } from 'vue'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import DatePicker from '@/components/common/DatePicker.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { useGlobalAlert } from '@/composables/useGlobalAlert'
  import { auditApi } from '@/api/auditApi'
  import { tatApi } from '@/api/tatApi'
  import { sectionApi } from '@/api/sectionApi'

  const authStore = useAuthStore()
  const { showAlert } = useGlobalAlert()

  // ── Tabs ───────────────────────────────────────────────────────────────────

  const allTabs = [
    { key: 'by-user', label: 'By User', icon: 'person_search', restricted: false },
    { key: 'by-specimen', label: 'By Lab No.', icon: 'barcode_scanner', restricted: false },
    { key: 'by-batch', label: 'Batch Details', icon: 'inventory_2', restricted: false },
    { key: 'tat-cycle', label: 'TAT Cycle Log', icon: 'timer', restricted: true },
  ]

  // TAT Cycle tab is only visible to endorsers (category 1) and admins
  const visibleTabs = computed(() =>
    allTabs.filter(t => !t.restricted || authStore.isAdmin || authStore.sectionCategory === '1')
  )

  const activeTab = ref('by-user')

  // ── Helpers ────────────────────────────────────────────────────────────────

  function toInputDate(date) {
    return date.toISOString().slice(0, 10)
  }

  function defaultDateFrom() {
    const d = new Date()
    d.setDate(d.getDate() - 7)
    return toInputDate(d)
  }

  function defaultDateTo() {
    return toInputDate(new Date())
  }

  function todayDate() {
    return toInputDate(new Date())
  }

  function formatDateTime(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-US', {
      month: 'short', day: 'numeric', year: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: true
    })
  }

  function formatElapsed(minutes) {
    if (minutes == null) return '—'
    const totalMins = Math.round(Number(minutes))
    const h = Math.floor(totalMins / 60)
    const m = totalMins % 60
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}`
  }

  function getEventStyle(eventCode) {
    const map = {
      ENDORSED: 'background-color: var(--color-primary-soft); color: var(--color-primary);',
      ENDORSED_14DAYS: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      ENDORSED_DUPLICATE: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      PROC_RECEIVED: 'background-color: var(--color-success-soft); color: var(--color-success);',
      SPECIMEN_CANCELLED: 'background-color: rgba(239,68,68,0.10); color: var(--color-error);',
      SECTION_RECEIVED: 'background-color: var(--color-success-soft); color: var(--color-success);',
      SPECIMEN_STORED: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
      TEST_SCHEDULED: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
      TEST_RESCHEDULED: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      RESULT_RELEASED: 'background-color: var(--color-success-soft); color: var(--color-success);',
      SCHEDULE_DUE: 'background-color: rgba(37,99,235,0.08); color: #2563eb;',
      TEST_RUN: 'background-color: rgba(124,58,237,0.08); color: #7c3aed;',
      SPECIMEN_CANCELLED_SECTION: 'background-color: rgba(239,68,68,0.10); color: var(--color-error);',
      TEST_ABORTED: 'background-color: rgba(217,119,6,0.1); color: var(--color-warning);',
      SPECIMEN_FLAGGED: 'background-color: rgba(239,68,68,0.10); color: var(--color-error);',
      FLAGGED_SPECIMEN_RECEIVED: 'background-color: rgba(239,68,68,0.10); color: var(--color-error);',
    }
    return map[eventCode] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function getEventDot(eventCode) {
    const map = {
      ENDORSED: 'var(--color-primary)',
      ENDORSED_14DAYS: 'var(--color-warning)',
      ENDORSED_DUPLICATE: 'var(--color-warning)',
      PROC_RECEIVED: 'var(--color-success)',
      SPECIMEN_CANCELLED: 'var(--color-error)',
      SECTION_RECEIVED: 'var(--color-success)',
      SPECIMEN_STORED: '#2563eb',
      TEST_SCHEDULED: '#2563eb',
      TEST_RESCHEDULED: 'var(--color-warning)',
      RESULT_RELEASED: 'var(--color-success)',
      SCHEDULE_DUE: '#2563eb',
      TEST_RUN: '#7c3aed',
      SPECIMEN_CANCELLED_SECTION: 'var(--color-error)',
      TEST_ABORTED: 'var(--color-warning)',
      SPECIMEN_FLAGGED: 'var(--color-error)',
      FLAGGED_SPECIMEN_RECEIVED: 'var(--color-error)',
    }
    return map[eventCode] ?? 'var(--color-text-muted)'
  }

  // ── TAT Result helpers ─────────────────────────────────────────────────────

  function getTatResultStyle(result) {
    const map = {
      Within: 'background-color: var(--color-success-soft); color: var(--color-success);',
      Outside: 'background-color: rgba(239,68,68,0.10); color: var(--color-error);',
      Appealed: 'background-color: var(--color-warning-soft); color: var(--color-warning);',
      EndOfDay: 'background-color: var(--color-surface-low); color: var(--color-text-muted);',
    }
    return map[result] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function getTatResultDot(result) {
    const map = {
      Within: 'var(--color-success)',
      Outside: 'var(--color-error)',
      Appealed: 'var(--color-warning)',
      EndOfDay: 'var(--color-text-muted)',
    }
    return map[result] ?? 'var(--color-text-muted)'
  }

  function getTatResultLabel(result) {
    const map = {
      Within: 'Within TAT',
      Outside: 'Outside TAT',
      Appealed: 'Appealed',
      EndOfDay: 'End of Day',
    }
    return map[result] ?? result ?? '—'
  }

  function buildPageNumbers(current, total) {
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
    let start = Math.max(1, current - 2)
    let end = Math.min(total, start + 4)
    start = Math.max(1, end - 4)
    return Array.from({ length: end - start + 1 }, (_, i) => start + i)
  }

  const PAGE_SIZE = 20

  // ── Drawer ─────────────────────────────────────────────────────────────────

  const drawerLog = ref(null)

  const drawerFields = computed(() => {
    if (!drawerLog.value) return []
    const l = drawerLog.value
    return [
      { label: 'Lab No.', value: l.specimenNo, extra: 'font-mono font-bold', color: 'var(--color-primary)' },
      { label: 'Batch No.', value: l.batchNo, extra: 'font-mono font-bold', color: 'var(--color-primary)' },
      { label: 'Patient Name', value: l.patientName, extra: 'font-semibold', color: 'var(--color-text)' },
      { label: 'Patient ID', value: l.pid, extra: '', color: 'var(--color-text-muted)' },
      { label: 'From', value: l.fromLocationName, extra: '', color: 'var(--color-text)' },
      { label: 'To', value: l.toLocationName, extra: '', color: 'var(--color-text)' },
      { label: 'Status', value: l.status, extra: '', color: 'var(--color-text)' },
      { label: 'Running Date', value: formatDateTime(l.runningDate), extra: '', color: 'var(--color-text)' },
      { label: 'Run At', value: formatDateTime(l.runAt), extra: '', color: 'var(--color-text-muted)' },
      { label: 'Transaction Date', value: formatDateTime(l.txDate), extra: '', color: 'var(--color-warning)' },
      { label: 'Remarks', value: l.remarks, extra: '', color: 'var(--color-text)' },
      { label: 'TAT Status', value: l.isOutsideTat ? 'Outside TAT' : null, extra: 'font-bold', color: 'var(--color-error)' },
      { label: 'Proc TAT Status', value: l.isOutsideProcTat ? 'Outside Processing TAT' : null, extra: 'font-bold', color: 'var(--color-error)' },
      { label: 'Logged By', value: l.userID, extra: 'font-bold', color: l.userID === 'MIDDLEWARE' ? '#2563eb' : 'var(--color-text)' },
      { label: 'Date & Time', value: formatDateTime(l.loggedAt), extra: '', color: 'var(--color-text-muted)' },
    ]
  })

  // ── By User ────────────────────────────────────────────────────────────────

  const userFilter = ref({ userID: '', dateFrom: defaultDateFrom(), dateTo: defaultDateTo() })
  const byUserLogs = ref([])
  const byUserLoading = ref(false)
  const byUserError = ref(null)
  const byUserSearched = ref(false)
  const byUserPage = ref(1)

  const byUserTotalPages = computed(() => Math.max(1, Math.ceil(byUserLogs.value.length / PAGE_SIZE)))
  const byUserPaginated = computed(() => byUserLogs.value.slice((byUserPage.value - 1) * PAGE_SIZE, byUserPage.value * PAGE_SIZE))
  const byUserPageNumbers = computed(() => buildPageNumbers(byUserPage.value, byUserTotalPages.value))

  async function loadByUser() {
    if (!userFilter.value.userID.trim()) { showAlert('warning', 'Missing Field', 'Please enter a User ID.'); return }
    byUserLoading.value = true; byUserError.value = null; byUserSearched.value = true; byUserPage.value = 1
    try {
      byUserLogs.value = await auditApi.getByUser(userFilter.value.userID.trim(), userFilter.value.dateFrom, userFilter.value.dateTo)
    } catch (err) {
      byUserError.value = err.response?.data?.message ?? 'Failed to load audit logs.'
      showAlert('error', 'Load Error', byUserError.value)
    } finally { byUserLoading.value = false }
  }

  function clearByUser() {
    userFilter.value = { userID: '', dateFrom: defaultDateFrom(), dateTo: defaultDateTo() }
    byUserLogs.value = []; byUserError.value = null; byUserSearched.value = false; byUserPage.value = 1
  }

  // ── By Specimen ────────────────────────────────────────────────────────────

  const specimenFilter = ref({ specimenNo: '' })
  const bySpecimenLogs = ref([])
  const bySpecimenLoading = ref(false)
  const bySpecimenError = ref(null)
  const bySpecimenSearched = ref(false)
  const bySpecimenPage = ref(1)

  const bySpecimenTotalPages = computed(() => Math.max(1, Math.ceil(bySpecimenLogs.value.length / PAGE_SIZE)))
  const bySpecimenPaginated = computed(() => bySpecimenLogs.value.slice((bySpecimenPage.value - 1) * PAGE_SIZE, bySpecimenPage.value * PAGE_SIZE))
  const bySpecimenPageNumbers = computed(() => buildPageNumbers(bySpecimenPage.value, bySpecimenTotalPages.value))

  async function loadBySpecimen() {
    if (!specimenFilter.value.specimenNo.trim()) { showAlert('warning', 'Missing Field', 'Please enter a specimen number.'); return }
    bySpecimenLoading.value = true; bySpecimenError.value = null; bySpecimenSearched.value = true; bySpecimenPage.value = 1
    try {
      bySpecimenLogs.value = await auditApi.getBySpecimen(specimenFilter.value.specimenNo.trim())
    } catch (err) {
      bySpecimenError.value = err.response?.data?.message ?? 'Failed to load audit logs.'
      showAlert('error', 'Load Error', bySpecimenError.value)
    } finally { bySpecimenLoading.value = false }
  }

  function clearBySpecimen() {
    specimenFilter.value = { specimenNo: '' }
    bySpecimenLogs.value = []; bySpecimenError.value = null; bySpecimenSearched.value = false; bySpecimenPage.value = 1
  }

  // ── By Batch ───────────────────────────────────────────────────────────────

  const batchFilter = ref({ batchNo: '' })
  const byBatchLogs = ref([])
  const byBatchLoading = ref(false)
  const byBatchError = ref(null)
  const byBatchSearched = ref(false)
  const byBatchPage = ref(1)

  const byBatchTotalPages = computed(() => Math.max(1, Math.ceil(byBatchLogs.value.length / PAGE_SIZE)))
  const byBatchPaginated = computed(() => byBatchLogs.value.slice((byBatchPage.value - 1) * PAGE_SIZE, byBatchPage.value * PAGE_SIZE))
  const byBatchPageNumbers = computed(() => buildPageNumbers(byBatchPage.value, byBatchTotalPages.value))

  async function loadByBatch() {
    if (!batchFilter.value.batchNo.trim()) { showAlert('warning', 'Missing Field', 'Please enter a batch number.'); return }
    byBatchLoading.value = true; byBatchError.value = null; byBatchSearched.value = true; byBatchPage.value = 1
    try {
      byBatchLogs.value = await auditApi.getByBatch(batchFilter.value.batchNo.trim())
    } catch (err) {
      byBatchError.value = err.response?.data?.message ?? 'Failed to load audit logs.'
      showAlert('error', 'Load Error', byBatchError.value)
    } finally { byBatchLoading.value = false }
  }

  function clearByBatch() {
    batchFilter.value = { batchNo: '' }
    byBatchLogs.value = []; byBatchError.value = null; byBatchSearched.value = false; byBatchPage.value = 1
  }

  // ── TAT Cycle Log ──────────────────────────────────────────────────────────

  const endorsingSections = ref([])

  const tatCycleFilter = ref({
    sectionCode: '',
    dateFrom: todayDate(),
    dateTo: todayDate(),
  })

  const tatCycleLogs = ref([])
  const tatCycleLoading = ref(false)
  const tatCycleError = ref(null)
  const tatCycleSearched = ref(false)
  const tatCyclePage = ref(1)

  const tatCycleTotalPages = computed(() => Math.max(1, Math.ceil(tatCycleLogs.value.length / PAGE_SIZE)))
  const tatCyclePaginated = computed(() => tatCycleLogs.value.slice((tatCyclePage.value - 1) * PAGE_SIZE, tatCyclePage.value * PAGE_SIZE))
  const tatCyclePageNumbers = computed(() => buildPageNumbers(tatCyclePage.value, tatCycleTotalPages.value))

  const tatCycleSummary = computed(() => {
    const logs = tatCycleLogs.value
    return {
      total: logs.length,
      within: logs.filter(c => c.result === 'Within').length,
      outside: logs.filter(c => c.result === 'Outside').length,
      appealed: logs.filter(c => c.result === 'Appealed').length,
    }
  })

  async function loadEndorsingSections() {
    if (!authStore.isAdmin) return
    try {
      const res = await sectionApi.getAll()
      endorsingSections.value = res.data.filter(s => s.category === '1' && s.active)
    } catch {
      endorsingSections.value = []
    }
  }

  async function loadTatCycleLogs() {
    tatCycleLoading.value = true; tatCycleError.value = null; tatCycleSearched.value = true; tatCyclePage.value = 1
    try {
      const params = {
        dateFrom: tatCycleFilter.value.dateFrom,
        dateTo: tatCycleFilter.value.dateTo,
      }
      if (authStore.isAdmin && tatCycleFilter.value.sectionCode) {
        params.sectionCode = tatCycleFilter.value.sectionCode
      }
      tatCycleLogs.value = await tatApi.getCycleLogs(params)
    } catch (err) {
      tatCycleError.value = err.response?.data?.message ?? 'Failed to load TAT cycle logs.'
      showAlert('error', 'Load Error', tatCycleError.value)
    } finally { tatCycleLoading.value = false }
  }

  function clearTatCycleLogs() {
    tatCycleFilter.value = { sectionCode: '', dateFrom: todayDate(), dateTo: todayDate() }
    tatCycleLogs.value = []; tatCycleError.value = null; tatCycleSearched.value = false; tatCyclePage.value = 1
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — REFS
  // ══════════════════════════════════════════════════════════════════════════

  const pageHeaderRef = ref(null)
  const tabNavRef = ref(null)

  // By User
  const byUserFilterRef = ref(null)
  const byUserCardRef = ref(null)
  const byUserEmptyRef = ref(null)

  // By Specimen
  const bySpecimenCardRef = ref(null)
  const bySpecimenEmptyRef = ref(null)

  // By Batch
  const byBatchCardRef = ref(null)
  const byBatchEmptyRef = ref(null)

  // TAT Cycle
  const tatCycleCardRef = ref(null)
  const tatCycleEmptyRef = ref(null)
  const tatSummaryCardsRef = ref(null)

  // TAT summary count-up display values
  const displayTatTotal = ref(0)
  const displayTatWithin = ref(0)
  const displayTatOutside = ref(0)
  const displayTatAppealed = ref(0)

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — ANIMATION FUNCTIONS
  // ══════════════════════════════════════════════════════════════════════════

  async function animatePageEnter() {
    await nextTick()
    const els = [pageHeaderRef.value, tabNavRef.value, byUserFilterRef.value].filter(Boolean)
    if (!els.length) return
    gsap.set(els, { opacity: 0, y: 16 })
    gsap.to(els, {
      opacity: 1,
      y: 0,
      duration: 0.3,
      stagger: 0.07,
      ease: 'power2.out',
    })
  }

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

  async function animateSummaryCards() {
    await nextTick()
    if (!tatSummaryCardsRef.value) return
    const cards = tatSummaryCardsRef.value.querySelectorAll(':scope > div')
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

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — WATCHERS
  // ══════════════════════════════════════════════════════════════════════════

  watch(byUserLoading, async (isLoading) => {
    if (isLoading) return
    if (byUserLogs.value.length) {
      await animateCard(byUserCardRef, '.by-user-row')
    } else if (byUserSearched.value) {
      await animateEmptyState(byUserEmptyRef)
    }
  })

  watch(bySpecimenLoading, async (isLoading) => {
    if (isLoading) return
    if (bySpecimenLogs.value.length) {
      await animateCard(bySpecimenCardRef, '.by-specimen-row')
    } else if (bySpecimenSearched.value) {
      await animateEmptyState(bySpecimenEmptyRef)
    }
  })

  watch(byBatchLoading, async (isLoading) => {
    if (isLoading) return
    if (byBatchLogs.value.length) {
      await animateCard(byBatchCardRef, '.by-batch-row')
    } else if (byBatchSearched.value) {
      await animateEmptyState(byBatchEmptyRef)
    }
  })

  watch(tatCycleLoading, async (isLoading) => {
    if (isLoading) return
    if (tatCycleLogs.value.length) {
      await animateSummaryCards()
      countUp(displayTatTotal, tatCycleSummary.value.total)
      countUp(displayTatWithin, tatCycleSummary.value.within)
      countUp(displayTatOutside, tatCycleSummary.value.outside)
      countUp(displayTatAppealed, tatCycleSummary.value.appealed)
      await animateCard(tatCycleCardRef, '.tat-cycle-row', 0.05)
    } else if (tatCycleSearched.value) {
      await animateEmptyState(tatCycleEmptyRef)
    }
  })

  // ── Init ───────────────────────────────────────────────────────────────────

  onMounted(() => {
    loadEndorsingSections()
    animatePageEnter()
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
