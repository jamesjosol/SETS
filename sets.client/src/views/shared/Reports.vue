<template>
  <AppLayout>
    <div class="flex h-full" style="min-height: calc(100vh - 64px);">

      <!-- ── Sidebar ──────────────────────────────────────────────────────── -->
      <aside class="w-56 flex-shrink-0 flex flex-col overflow-y-auto"
             style="border-right: 1px solid var(--color-border); background-color: var(--color-surface);">

        <div class="px-4 pt-5 pb-3 text-[10px] font-bold uppercase tracking-widest"
             style="color: var(--color-text-muted); border-bottom: 0.5px solid var(--color-border);">Reports</div>

        <nav class="flex flex-col gap-0.5 px-2 py-3">
          <template v-for="item in reportItems" :key="item.key">

            <!-- Locked item -->
            <div v-if="!item.hasAccess"
                 class="flex items-start gap-2.5 px-3 py-2.5 rounded-xl cursor-not-allowed select-none"
                 style="opacity: 0.35;">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    style="color: var(--color-text-muted); font-size: 17px;">lock</span>
              <div>
                <div class="text-xs font-medium" style="color: var(--color-text-muted);">{{ item.label }}</div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </div>

            <!-- Accessible item -->
            <button v-else
                    class="w-full flex items-start gap-2.5 px-3 py-2.5 rounded-xl text-left transition-all cursor-pointer"
                    :style="activeReport === item.key
                      ? 'background-color: var(--color-surface-low); border-left: 2.5px solid var(--color-primary);'
                      : 'border-left: 2.5px solid transparent;'"
                    @click="selectReport(item.key)">
              <span class="material-symbols-outlined flex-shrink-0 mt-0.5"
                    :style="`font-size: 17px; color: ${activeReport === item.key ? 'var(--color-primary)' : 'var(--color-text-muted)'}`">
                {{ item.icon }}
              </span>
              <div>
                <div class="text-xs font-medium"
                     :style="`color: ${activeReport === item.key ? 'var(--color-text)' : 'var(--color-text-muted)'}`">
                  {{ item.label }}
                </div>
                <div class="text-[10px] mt-0.5" style="color: var(--color-text-muted);">{{ item.sublabel }}</div>
                <div class="flex flex-wrap gap-1 mt-1.5">
                  <span v-for="role in item.roles" :key="role"
                        class="text-[9px] font-medium px-1.5 py-0.5 rounded"
                        :class="roleChipClass(role)">{{ role }}</span>
                </div>
              </div>
            </button>

          </template>
        </nav>
      </aside>

      <!-- ── Main panel ───────────────────────────────────────────────────── -->
      <main class="flex-1 flex flex-col overflow-hidden" style="background-color: var(--color-bg);">

        <!-- ══ BATCH SUMMARY ═══════════════════════════════════════════════ -->
        <template v-if="activeReport === 'batch-summary'">

          <!-- Header -->
          <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div>
              <h1 class="text-base font-bold" style="color: var(--color-text);">Batch Summary Report</h1>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                Per-batch TAT for endorsement and completion · Excel export
              </p>
            </div>
            <button :disabled="!bsResult || bsExporting"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="bsExportExcel">
              <span class="material-symbols-outlined text-sm">download</span>
              {{ bsExporting ? 'Exporting…' : 'Export Excel' }}
            </button>
          </div>

          <!-- Filters -->
          <div class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

            <div v-if="authStore.isAdmin || authStore.sectionCategory === '2'" class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Location</label>
              <select v-model="bsFilters.locationCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 140px;">
                <option value="">ALL</option>
                <option v-for="s in endorsingSections" :key="s.code" :value="s.code">{{ s.name }}</option>
              </select>
            </div>

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
                     style="color: var(--color-text-muted);">Date from</label>
              <DatePicker v-model="bsFilters.dateFrom"
                          placeholder="Date from"
                          :max-date="bsFilters.dateTo" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date to</label>
              <DatePicker v-model="bsFilters.dateTo"
                          placeholder="Date to"
                          :min-date="bsFilters.dateFrom" />
            </div>

            <button :disabled="bsLoading"
                    class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="bsGenerate">
              <span class="material-symbols-outlined text-sm" :class="bsLoading ? 'animate-spin' : ''">
                {{ bsLoading ? 'progress_activity' : 'search' }}
              </span>
              {{ bsLoading ? 'Loading…' : 'Generate' }}
            </button>
          </div>

          <!-- Content area -->
          <div class="flex-1 overflow-y-auto px-7 py-6">

            <div v-if="!bsResult && !bsLoading"
                 class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">table_chart</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
              <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
            </div>

            <div v-else-if="bsLoading" class="flex flex-col gap-3">
              <div class="grid grid-cols-5 gap-3 mb-2">
                <div v-for="i in 5" :key="i" class="h-16 rounded-xl animate-pulse"
                     style="background-color: var(--color-surface);"></div>
              </div>
              <div v-for="i in 6" :key="i" class="h-10 rounded-xl animate-pulse"
                   style="background-color: var(--color-surface);"></div>
            </div>

            <template v-else-if="bsResult">
              <!-- Summary strip -->
              <div class="grid grid-cols-5 gap-3 mb-5">
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-primary);">{{ bsResult.totalBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Total batches</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-success);">{{ bsResult.completedBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Completed</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-2xl font-bold" style="color: var(--color-warning);">{{ bsResult.pendingBatches }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Pending / Partial</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-xl font-bold" style="color: var(--color-text);">{{ bsResult.avgTatEndorsement ?? '—' }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Avg TAT endorsement</div>
                </div>
                <div class="rounded-xl p-4 text-center"
                     style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                  <div class="text-xl font-bold" style="color: var(--color-text);">{{ bsResult.avgTatCompletion ?? '—' }}</div>
                  <div class="text-[10px] font-bold uppercase tracking-widest mt-0.5" style="color: var(--color-text-muted);">Avg TAT completion</div>
                </div>
              </div>

              <div v-if="!bsResult.rows.length"
                   class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
                   style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
                <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
                <p class="text-sm font-bold" style="color: var(--color-text);">No batches found</p>
                <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
              </div>

              <div v-else class="rounded-2xl overflow-hidden"
                   style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
                <div class="overflow-x-auto">
                  <table class="w-full text-xs">
                    <thead>
                      <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Batch no.</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed by</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Received</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Received by</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Completed</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Temp</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">TAT (endorse)</th>
                        <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">TAT (complete)</th>
                      </tr>
                    </thead>
                    <tbody>
                      <tr v-for="row in bsResult.rows" :key="row.batchNo"
                          style="border-bottom: 0.5px solid var(--color-border);"
                          @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                          @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                        <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">{{ row.batchNo }}</td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ formatDt(row.endorsed) }}</td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.endorsedBy }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.procReceived ? formatDt(row.procReceived) : '—' }}</td>
                        <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.receivedBy ?? '—' }}</td>
                        <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.completed ? formatDt(row.completed) : '—' }}</td>
                        <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.temp ?? '—' }}</td>
                        <td class="px-4 py-2.5">
                          <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                :style="statusStyle(row.status)">{{ row.status }}</span>
                        </td>
                        <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.tatEndorsement ?? '—' }}</td>
                        <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.tatCompletion ?? '—' }}</td>
                      </tr>
                    </tbody>
                    <tfoot>
                      <tr style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low);">
                        <td colspan="9" class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                            style="color: var(--color-text-muted);">
                          Average TAT
                        </td>
                        <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">{{ bsResult.avgTatEndorsement ?? '—' }}</td>
                        <td class="px-4 py-2.5 text-xs font-bold" style="color: var(--color-text);">{{ bsResult.avgTatCompletion ?? '—' }}</td>
                      </tr>
                    </tfoot>
                  </table>
                </div>
              </div>
            </template>
          </div>
        </template>

        <!-- ══ SPECIMEN RECEIPT LABORATORY SECTION ════════════════════════ -->
        <template v-else-if="activeReport === 'specimen-receipt-section'">

          <!-- Header -->
          <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div>
              <h1 class="text-base font-bold" style="color: var(--color-text);">Specimen Receipt Laboratory Section</h1>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                TAT from processing receipt to lab section receipt · Excel export
              </p>
            </div>
            <button :disabled="!srsRows.length || srsExporting"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="srsExportExcel">
              <span class="material-symbols-outlined text-sm">download</span>
              {{ srsExporting ? 'Exporting…' : 'Export Excel' }}
            </button>
          </div>

          <!-- Filters -->
          <div class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

            <!-- Lab Section — dropdown for Admin/Processing, static chip for Lab -->
            <div v-if="authStore.isAdmin || authStore.sectionCategory === '2'" class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Lab Section</label>
              <select v-model="srsFilters.sectionCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 160px;">
                <option value="">ALL</option>
                <option v-for="s in labSections" :key="s.code" :value="s.code">{{ s.name }}</option>
              </select>
            </div>

            <div v-else class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Lab Section</label>
              <div class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-sm font-bold"
                   style="background-color: var(--color-primary-soft); color: var(--color-primary);">
                <span class="material-symbols-outlined text-base">biotech</span>
                {{ authStore.sectionName }}
              </div>
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date from</label>
              <DatePicker v-model="srsFilters.dateFrom"
                          placeholder="Date from"
                          :max-date="srsFilters.dateTo" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date to</label>
              <DatePicker v-model="srsFilters.dateTo"
                          placeholder="Date to"
                          :min-date="srsFilters.dateFrom" />
            </div>

            <button :disabled="srsLoading"
                    class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="srsGenerate">
              <span class="material-symbols-outlined text-sm" :class="srsLoading ? 'animate-spin' : ''">
                {{ srsLoading ? 'progress_activity' : 'search' }}
              </span>
              {{ srsLoading ? 'Loading…' : 'Generate' }}
            </button>
          </div>

          <!-- Content area -->
          <div class="flex-1 overflow-y-auto px-7 py-6">

            <div v-if="!srsGenerated && !srsLoading"
                 class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">schedule_send</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
              <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
            </div>

            <div v-else-if="srsLoading" class="flex flex-col gap-3">
              <div v-for="i in 8" :key="i" class="h-10 rounded-xl animate-pulse"
                   style="background-color: var(--color-surface);"></div>
            </div>

            <div v-else-if="srsGenerated && !srsRows.length"
                 class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No records found</p>
              <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
            </div>

            <div v-else-if="srsRows.length" class="rounded-2xl overflow-hidden"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
              <div class="overflow-x-auto">
                <table class="w-full text-xs">
                  <thead>
                    <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Batch no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Specimen no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Patient name</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Sample type</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Proc. received</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Routed by</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Section received</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Received by (section)</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Lab section</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">TAT (section)</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, idx) in srsRows" :key="idx"
                        style="border-bottom: 0.5px solid var(--color-border);"
                        @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                        @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                      <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">{{ row.batchNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location ?? '—' }}</td>
                      <td class="px-4 py-2.5 font-mono font-medium whitespace-nowrap" style="color: var(--color-text);">{{ row.specimenNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.patientName ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.sampleTypeName ?? '—' }}</td>
                      <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.procReceived ? formatDt(row.procReceived) : '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.routedBy ?? '—' }}</td>
                      <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.sectionReceived ? formatDt(row.sectionReceived) : '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.sectionReceivedBy ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.labSection ?? '—' }}</td>
                      <td class="px-4 py-2.5 font-medium" style="color: var(--color-text);">{{ row.tatSection ?? '—' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                   style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low); color: var(--color-text-muted);">
                {{ srsRows.length }} record{{ srsRows.length !== 1 ? 's' : '' }}
              </div>
            </div>

          </div>
        </template>

        <!-- ══ DUPLICATE ENDORSEMENT ═════════════════════════════════════════ -->
        <template v-else-if="activeReport === 'duplicate-endorsement'">

          <!-- Header -->
          <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div>
              <h1 class="text-base font-bold" style="color: var(--color-text);">Duplicate Endorsement Report</h1>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                Re-endorsed specimens with paired first and second endorsement · Excel export
              </p>
            </div>
            <button :disabled="!deRows.length || deExporting"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="deExportExcel">
              <span class="material-symbols-outlined text-sm">download</span>
              {{ deExporting ? 'Exporting…' : 'Export Excel' }}
            </button>
          </div>

          <!-- Filters -->
          <div class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

            <!-- Location — dropdown for Admin/Processing, static chip for Endorser -->
            <div v-if="authStore.isAdmin || authStore.sectionCategory === '2'" class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Location</label>
              <select v-model="deFilters.locationCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 140px;">
                <option value="">ALL</option>
                <option v-for="s in endorsingSections" :key="s.code" :value="s.code">{{ s.name }}</option>
              </select>
            </div>

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
                     style="color: var(--color-text-muted);">Date from</label>
              <DatePicker v-model="deFilters.dateFrom" placeholder="Date from" :max-date="deFilters.dateTo" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date to</label>
              <DatePicker v-model="deFilters.dateTo" placeholder="Date to" :min-date="deFilters.dateFrom" />
            </div>

            <button :disabled="deLoading"
                    class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="deGenerate">
              <span class="material-symbols-outlined text-sm" :class="deLoading ? 'animate-spin' : ''">
                {{ deLoading ? 'progress_activity' : 'search' }}
              </span>
              {{ deLoading ? 'Loading…' : 'Generate' }}
            </button>
          </div>

          <!-- Content -->
          <div class="flex-1 overflow-y-auto px-7 py-6">

            <div v-if="!deGenerated && !deLoading"
                 class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">content_copy</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
              <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
            </div>

            <div v-else-if="deLoading" class="flex flex-col gap-3">
              <div v-for="i in 6" :key="i" class="h-10 rounded-xl animate-pulse"
                   style="background-color: var(--color-surface);"></div>
            </div>

            <div v-else-if="deGenerated && !deRows.length"
                 class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No duplicate endorsements found</p>
              <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
            </div>

            <div v-else-if="deRows.length" class="rounded-2xl overflow-hidden"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
              <div class="overflow-x-auto">
                <table class="w-full text-xs">
                  <thead>
                    <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Batch no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Specimen no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Patient name</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">1st Endorsed</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">By (1st)</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">2nd Endorsed</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">By (2nd)</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, idx) in deRows" :key="idx"
                        style="border-bottom: 0.5px solid var(--color-border);"
                        @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                        @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                      <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">{{ row.batchNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location ?? '—' }}</td>
                      <td class="px-4 py-2.5 font-mono font-medium whitespace-nowrap" style="color: var(--color-text);">{{ row.specimenNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.patientName ?? '—' }}</td>
                      <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.firstEndorsedAt ? formatDt(row.firstEndorsedAt) : '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.firstEndorsedBy ?? '—' }}</td>
                      <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.secondEndorsedAt ? formatDt(row.secondEndorsedAt) : '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.secondEndorsedBy ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.reason ?? '—' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                   style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low); color: var(--color-text-muted);">
                {{ deRows.length }} record{{ deRows.length !== 1 ? 's' : '' }}
              </div>
            </div>

          </div>
        </template>

        <!-- ══ BEYOND 14 DAYS ════════════════════════════════════════════════ -->
        <template v-else-if="activeReport === 'beyond-14-days'">

          <!-- Header -->
          <div class="px-7 py-5 flex items-center justify-between flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <div>
              <h1 class="text-base font-bold" style="color: var(--color-text);">Transaction Beyond 14 Days Report</h1>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">
                Specimens endorsed beyond the 14-day transaction window · Excel export
              </p>
            </div>
            <button :disabled="!b14Rows.length || b14Exporting"
                    class="flex items-center gap-2 px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-40 disabled:cursor-not-allowed"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="b14ExportExcel">
              <span class="material-symbols-outlined text-sm">download</span>
              {{ b14Exporting ? 'Exporting…' : 'Export Excel' }}
            </button>
          </div>

          <!-- Filters -->
          <div class="px-7 py-4 flex flex-wrap gap-4 items-end flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">

            <div v-if="authStore.isAdmin || authStore.sectionCategory === '2'" class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Location</label>
              <select v-model="b14Filters.locationCode"
                      class="text-xs px-3 py-2 rounded-xl outline-none transition-all cursor-pointer"
                      style="background-color: var(--color-surface-low); border: 1px solid var(--color-border); color: var(--color-text); min-width: 140px;">
                <option value="">ALL</option>
                <option v-for="s in endorsingSections" :key="s.code" :value="s.code">{{ s.name }}</option>
              </select>
            </div>

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
                     style="color: var(--color-text-muted);">Date from</label>
              <DatePicker v-model="b14Filters.dateFrom" placeholder="Date from" :max-date="b14Filters.dateTo" />
            </div>

            <div class="flex flex-col gap-1.5">
              <label class="text-[10px] font-bold uppercase tracking-widest"
                     style="color: var(--color-text-muted);">Date to</label>
              <DatePicker v-model="b14Filters.dateTo" placeholder="Date to" :min-date="b14Filters.dateFrom" />
            </div>

            <button :disabled="b14Loading"
                    class="flex items-center gap-2 px-5 py-2 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95 disabled:opacity-50"
                    style="background: var(--color-primary-gradient); color: #ffffff;"
                    @click="b14Generate">
              <span class="material-symbols-outlined text-sm" :class="b14Loading ? 'animate-spin' : ''">
                {{ b14Loading ? 'progress_activity' : 'search' }}
              </span>
              {{ b14Loading ? 'Loading…' : 'Generate' }}
            </button>
          </div>

          <!-- Content -->
          <div class="flex-1 overflow-y-auto px-7 py-6">

            <div v-if="!b14Generated && !b14Loading"
                 class="flex flex-col items-center justify-center h-64 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">calendar_clock</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">Set filters and click Generate</p>
              <p class="text-xs" style="color: var(--color-text-muted);">The report will appear here</p>
            </div>

            <div v-else-if="b14Loading" class="flex flex-col gap-3">
              <div v-for="i in 6" :key="i" class="h-10 rounded-xl animate-pulse"
                   style="background-color: var(--color-surface);"></div>
            </div>

            <div v-else-if="b14Generated && !b14Rows.length"
                 class="flex flex-col items-center justify-center h-40 gap-3 rounded-2xl"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">search_off</span>
              <p class="text-sm font-bold" style="color: var(--color-text);">No records found</p>
              <p class="text-xs" style="color: var(--color-text-muted);">Try adjusting the filters</p>
            </div>

            <div v-else-if="b14Rows.length" class="rounded-2xl overflow-hidden"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border); box-shadow: 0 1px 3px var(--color-shadow);">
              <div class="overflow-x-auto">
                <table class="w-full text-xs">
                  <thead>
                    <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Batch no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Location</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Specimen no.</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Patient name</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest whitespace-nowrap" style="color: var(--color-text-muted);">Endorsed by</th>
                      <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Reason</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr v-for="(row, idx) in b14Rows" :key="idx"
                        style="border-bottom: 0.5px solid var(--color-border);"
                        @mouseenter="e => e.currentTarget.style.backgroundColor = 'var(--color-surface-low)'"
                        @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'">
                      <td class="px-4 py-2.5 font-medium whitespace-nowrap" style="color: var(--color-primary);">{{ row.batchNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.location ?? '—' }}</td>
                      <td class="px-4 py-2.5 font-mono font-medium whitespace-nowrap" style="color: var(--color-text);">{{ row.specimenNo ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.patientName ?? '—' }}</td>
                      <td class="px-4 py-2.5 whitespace-nowrap" style="color: var(--color-text);">{{ row.endorsedAt ? formatDt(row.endorsedAt) : '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text);">{{ row.endorsedBy ?? '—' }}</td>
                      <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ row.reason ?? '—' }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
              <div class="px-4 py-2.5 text-[10px] font-bold uppercase tracking-widest"
                   style="border-top: 1.5px solid var(--color-border); background-color: var(--color-surface-low); color: var(--color-text-muted);">
                {{ b14Rows.length }} record{{ b14Rows.length !== 1 ? 's' : '' }}
              </div>
            </div>

          </div>
        </template>

        <!-- ══ COMING SOON ════════════════════════════════════════════════════ -->
        <template v-else>
          <div class="px-7 py-5 flex-shrink-0"
               style="border-bottom: 1px solid var(--color-border); background-color: var(--color-surface);">
            <h1 class="text-base font-bold" style="color: var(--color-text);">{{ activeItem?.label }}</h1>
            <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">{{ activeItem?.sublabel }}</p>
          </div>
          <div class="flex-1 flex flex-col items-center justify-center gap-4">
            <span class="material-symbols-outlined text-6xl" style="color: var(--color-text-muted);">engineering</span>
            <p class="text-sm font-bold" style="color: var(--color-text);">This report is in the build queue</p>
            <p class="text-xs text-center max-w-xs" style="color: var(--color-text-muted);">
              Pending clarification or implementation. Check back once all requirements are confirmed.
            </p>
          </div>
        </template>

      </main>
    </div>

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
  import DatePicker from '@/components/common/DatePicker.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { reportApi } from '@/api/reportApi'
  import { sectionApi } from '@/api/sectionApi'

  const authStore = useAuthStore()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Helpers ───────────────────────────────────────────────────────────────────

  function formatDt(val) {
    if (!val) return '—'
    return new Date(val).toLocaleString('en-PH', {
      month: '2-digit', day: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', hour12: false
    })
  }

  function statusStyle(status) {
    switch (status) {
      case 'Complete': return 'background-color: rgba(29,158,117,0.12); color: var(--color-success);'
      case 'Partial': return 'background-color: rgba(59,130,246,0.1);  color: var(--color-info);'
      case 'Pending': return 'background-color: rgba(217,119,6,0.1);   color: var(--color-warning);'
      case 'Cancelled': return 'background-color: rgba(239,68,68,0.1);   color: var(--color-error);'
      default: return 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
    }
  }

  function roleChipClass(role) {
    switch (role) {
      case 'All roles': return 'bg-teal-50   text-teal-800'
      case 'Endorser': return 'bg-green-50  text-green-800'
      case 'Endorser/Processing': return 'bg-cyan-50   text-cyan-800'
      case 'Processing': return 'bg-blue-50   text-blue-800'
      case 'Processing/Lab': return 'bg-indigo-50 text-indigo-800'
      case 'Lab': return 'bg-rose-50   text-rose-800'
      case 'TL': return 'bg-amber-50  text-amber-800'
      case 'Admin': return 'bg-purple-50 text-purple-800'
      default: return 'bg-gray-100  text-gray-600'
    }
  }

  // ── Access helpers ────────────────────────────────────────────────────────────

  const isTLorAdmin = computed(() => authStore.isAdmin || authStore.roleID === 2)
  const isProcessing = computed(() => authStore.isAdmin || authStore.roleID === 2 || authStore.sectionCategory === '2')

  // ── Report nav items ──────────────────────────────────────────────────────────

  const reportItems = computed(() => [
    {
      key: 'unprocessed-specimen',
      label: 'Unprocessed Specimen',
      sublabel: 'ERD / CRD / SRD per section',
      icon: 'science',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-endorsed',
      label: 'Specimen Not Endorsed',
      sublabel: 'Flagged during endorsement',
      icon: 'warning',
      roles: ['All roles'],
      hasAccess: true,
    },
    {
      key: 'specimen-not-received',
      label: 'Specimen Not Received',
      sublabel: 'Pending / unprocessed',
      icon: 'inventory_2',
      roles: ['Processing', 'TL', 'Admin'],
      hasAccess: isProcessing.value,
    },
    {
      key: 'test-management',
      label: 'Test Management',
      sublabel: 'Running days, TAT, status',
      icon: 'biotech',
      roles: ['Lab', 'TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
    {
      key: 'batch-summary',
      label: 'Batch Summary',
      sublabel: 'TAT endorsement & completion',
      icon: 'table_chart',
      roles: ['Endorser/Processing', 'TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
    {
      key: 'specimen-receipt-section',
      label: 'Specimen Receipt Laboratory Section',
      sublabel: 'Processing → Lab section TAT',
      icon: 'schedule_send',
      roles: ['Processing/Lab', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '2' || authStore.sectionCategory === '3',
    },
    {
      key: 'duplicate-endorsement',
      label: 'Duplicate Endorsement',
      sublabel: 'Re-endorsed specimens',
      icon: 'content_copy',
      roles: ['Endorser/Processing', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '1' || authStore.sectionCategory === '2',
    },
    {
      key: 'beyond-14-days',
      label: 'Beyond 14 Days',
      sublabel: 'Old transaction flags',
      icon: 'calendar_clock',
      roles: ['Endorser/Processing', 'Admin'],
      hasAccess: authStore.isAdmin || authStore.sectionCategory === '1' || authStore.sectionCategory === '2',
    },
    {
      key: 'monthly-summary',
      label: 'Monthly Summary',
      sublabel: 'Aggregate per location',
      icon: 'bar_chart',
      roles: ['TL', 'Admin'],
      hasAccess: isTLorAdmin.value,
    },
  ])

  const firstAccessible = computed(() =>
    reportItems.value.find(i => i.hasAccess)?.key ?? 'batch-summary'
  )

  const activeReport = ref(null)
  const activeItem = computed(() => reportItems.value.find(i => i.key === activeReport.value))

  function selectReport(key) {
    if (activeReport.value === key) return
    activeReport.value = key
  }

  // ── Reference data ────────────────────────────────────────────────────────────

  const endorsingSections = ref([])   // Category 1
  const labSections = ref([])   // Category 3

  async function loadReferenceData() {
    try {
      const res = await sectionApi.getAll()
      const active = res.data.filter(s => s.active)
      endorsingSections.value = active.filter(s => s.category === '1')
      labSections.value = active.filter(s => s.category === '3')
    } catch (e) {
      console.error(e)
    }
  }

  // ── Batch Summary ─────────────────────────────────────────────────────────────

  const today = new Date().toISOString().slice(0, 10)

  const bsFilters = ref({
    locationCode: authStore.sectionCategory === '1' ? authStore.sectionCode : '',
    dateFrom: today,
    dateTo: today,
  })
  const bsLoading = ref(false)
  const bsExporting = ref(false)
  const bsResult = ref(null)

  async function bsGenerate() {
    bsLoading.value = true
    bsResult.value = null
    try {
      bsResult.value = await reportApi.getBatchSummary({
        locationCode: bsFilters.value.locationCode || null,
        dateFrom: bsFilters.value.dateFrom,
        dateTo: bsFilters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      bsLoading.value = false
    }
  }

  async function bsExportExcel() {
    if (!bsResult.value) return
    bsExporting.value = true
    try {
      await reportApi.exportBatchSummaryExcel({
        locationCode: bsFilters.value.locationCode || null,
        dateFrom: bsFilters.value.dateFrom,
        dateTo: bsFilters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      bsExporting.value = false
    }
  }

  // ── Specimen Receipt Laboratory Section ───────────────────────────────────────

  const srsFilters = ref({
    sectionCode: authStore.sectionCategory === '3' ? authStore.sectionCode : '',
    dateFrom: today,
    dateTo: today,
  })
  const srsLoading = ref(false)
  const srsExporting = ref(false)
  const srsRows = ref([])
  const srsGenerated = ref(false)

  async function srsGenerate() {
    srsLoading.value = true
    srsRows.value = []
    srsGenerated.value = false
    try {
      srsRows.value = await reportApi.getSpecimenReceiptSection({
        sectionCode: srsFilters.value.sectionCode || null,
        dateFrom: srsFilters.value.dateFrom,
        dateTo: srsFilters.value.dateTo,
      })
      srsGenerated.value = true
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      srsLoading.value = false
    }
  }

  async function srsExportExcel() {
    if (!srsRows.value.length) return
    srsExporting.value = true
    try {
      await reportApi.exportSpecimenReceiptSectionExcel({
        sectionCode: srsFilters.value.sectionCode || null,
        locationCode: srsFilters.value.locationCode || null,
        dateFrom: srsFilters.value.dateFrom,
        dateTo: srsFilters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      srsExporting.value = false
    }
  }

  // ── Duplicate Endorsement ─────────────────────────────────────────────────────
  const deFilters = ref({
    locationCode: authStore.sectionCategory === '1' ? authStore.sectionCode : '',
    dateFrom: today,
    dateTo: today,
  })
  const deLoading = ref(false)
  const deExporting = ref(false)
  const deRows = ref([])
  const deGenerated = ref(false)

  async function deGenerate() {
    deLoading.value = true
    deRows.value = []
    deGenerated.value = false
    try {
      deRows.value = await reportApi.getDuplicateEndorsements({
        locationCode: deFilters.value.locationCode || null,
        dateFrom: deFilters.value.dateFrom,
        dateTo: deFilters.value.dateTo,
      })
      deGenerated.value = true
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      deLoading.value = false
    }
  }

  async function deExportExcel() {
    if (!deRows.value.length) return
    deExporting.value = true
    try {
      await reportApi.exportDuplicateEndorsementsExcel({
        locationCode: deFilters.value.locationCode || null,
        dateFrom: deFilters.value.dateFrom,
        dateTo: deFilters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      deExporting.value = false
    }
  }

  // ── Beyond 14 Days ────────────────────────────────────────────────────────────
  const b14Filters = ref({
    locationCode: authStore.sectionCategory === '1' ? authStore.sectionCode : '',
    dateFrom: today,
    dateTo: today,
  })
  const b14Loading = ref(false)
  const b14Exporting = ref(false)
  const b14Rows = ref([])
  const b14Generated = ref(false)

  async function b14Generate() {
    b14Loading.value = true
    b14Rows.value = []
    b14Generated.value = false
    try {
      b14Rows.value = await reportApi.getBeyond14Days({
        locationCode: b14Filters.value.locationCode || null,
        dateFrom: b14Filters.value.dateFrom,
        dateTo: b14Filters.value.dateTo,
      })
      b14Generated.value = true
    } catch (err) {
      showAlert('error', 'Failed to load report', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      b14Loading.value = false
    }
  }

  async function b14ExportExcel() {
    if (!b14Rows.value.length) return
    b14Exporting.value = true
    try {
      await reportApi.exportBeyond14DaysExcel({
        locationCode: b14Filters.value.locationCode || null,
        dateFrom: b14Filters.value.dateFrom,
        dateTo: b14Filters.value.dateTo,
      })
    } catch (err) {
      showAlert('error', 'Export failed', err.response?.data?.message ?? 'Could not generate the Excel file.')
    } finally {
      b14Exporting.value = false
    }
  }

  // ── Lifecycle ─────────────────────────────────────────────────────────────────

  onMounted(() => {
    activeReport.value = firstAccessible.value
    loadReferenceData()
  })
</script>
