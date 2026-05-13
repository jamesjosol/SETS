<template>
  <AppLayout>
    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Pending Specimens</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">{{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}</span> · {{ authStore.branchCode }}
        </p>
      </div>
      <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
              style="background: var(--color-primary-gradient); color: #fff;"
              @click="load">
        <span class="material-symbols-outlined text-sm">refresh</span>
        Refresh
      </button>
    </div>

    <!-- ═════════════════════════════════════════════════════════════════════
       REGULAR / TEAM LEAD VIEW
  ══════════════════════════════════════════════════════════════════════ -->
    <template v-if="!authStore.isAdmin">

      <!-- Search -->
      <div class="mb-4 flex gap-3 flex-wrap">
        <div class="relative flex-1 min-w-[200px]">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
                style="color: var(--color-text-muted);">search</span>
          <input v-model="searchQuery"
                 type="text"
                 placeholder="Search specimen no., patient name..."
                 class="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                 style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        </div>
        <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
             style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">pending_actions</span>
          {{ filteredSpecimens.length }} specimen{{ filteredSpecimens.length !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- Table -->
      <div ref="tableCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <!-- Loading skeleton -->
        <div v-if="loading" class="p-6 flex flex-col gap-3">
          <div v-for="i in 5" :key="i" class="h-14 rounded-xl animate-pulse" style="background-color: var(--color-surface-low);"></div>
        </div>

        <!-- Empty state -->
        <div v-else-if="!filteredSpecimens.length" ref="emptyRef" class="p-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">inbox</span>
          <p class="text-sm font-bold" style="color: var(--color-text);">No pending specimens</p>
          <p class="text-xs" style="color: var(--color-text-muted);">All specimens for this section have been processed.</p>
        </div>

        <!-- Table -->
        <div v-else class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr style="border-bottom: 1.5px solid var(--color-border);">
                <th class="w-8 px-4 py-3"></th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Routed</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
                <th class="px-4 py-3 w-10"></th>
              </tr>
            </thead>
            <tbody ref="tableBodyRef">
              <template v-for="item in filteredSpecimens" :key="item.id">
                <!-- Main row -->
                <tr class="transition-colors cursor-pointer"
                    :data-specimenno="item.specimenNo"
                    :style="expandedId === item.id
                      ? 'background-color: var(--color-primary-soft);'
                      : 'background-color: transparent;'"
                    @mouseenter="e => { if (expandedId !== item.id) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                    @mouseleave="e => { if (expandedId !== item.id) e.currentTarget.style.backgroundColor = 'transparent' }"
                    @click="toggleExpand(item)">
                  <td class="px-4 py-3">
                    <span class="material-symbols-outlined text-sm transition-transform duration-200"
                          :style="{ color: 'var(--color-text-muted)', transform: expandedId === item.id ? 'rotate(90deg)' : 'rotate(0deg)' }">
                      chevron_right
                    </span>
                  </td>
                  <td class="px-4 py-3">
                    <div class="flex items-center gap-1.5">
                      <span class="font-bold font-mono" style="color: var(--color-text);">{{ item.specimenNo }}</span>
                      <span v-if="item.isOnSite"
                            class="material-symbols-outlined cursor-default"
                            style="color: var(--color-warning); font-size: 16px;"
                            title="On-Site / Mission">
                        location_on
                      </span>
                    </div>
                  </td>
                  <td class="px-4 py-3">
                    <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName ?? '—' }}</p>
                    <p v-if="item.patientID" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.patientID }}</p>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                    <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                  </td>
                  <td class="px-4 py-3">
                    <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                    <span v-else class="text-xs italic" style="color: var(--color-text-muted);">Not yet received</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                          :style="headerStatusStyle(item.status)">
                      {{ headerStatusLabel(item.status) }}
                    </span>
                  </td>
                  <td class="px-4 py-3" @click.stop>
                    <button class="p-1.5 rounded-lg transition-all"
                            :style="item.remarks
                              ? 'color: var(--color-warning);'
                              : 'color: var(--color-text-muted); opacity: 0.4;'"
                            :disabled="!item.remarks"
                            @click="openRemarks(item)">
                      <span class="material-symbols-outlined text-sm">
                        {{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                      </span>
                    </button>
                  </td>
                  <!-- Cancel — TL only (Regular view) -->
                  <td v-if="authStore.isTL" class="px-3 py-3" @click.stop>
                    <button class="p-1.5 rounded-lg transition-all group/cancel relative"
                            style="color: var(--color-text-muted);"
                            title="Cancel Specimen"
                            @mouseenter="e => e.currentTarget.style.cssText = 'color: var(--color-error, #dc2626); background-color: rgba(220,38,38,0.08);'"
                            @mouseleave="e => e.currentTarget.style.cssText = 'color: var(--color-text-muted);'"
                            @click="openCancelModal(item)">
                      <span class="material-symbols-outlined text-sm">do_not_disturb_on</span>
                    </button>
                  </td>
                  <td v-else class="px-3 py-3"></td>
                </tr>

                <!-- Expanded test rows -->
                <Transition name="expand">
                  <tr v-if="expandedId === item.id" :key="`exp-${item.id}`">
                    <td colspan="9" class="px-0 py-0">
                      <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                           style="border: 1.5px solid var(--color-border);">
                        <div v-if="testsLoading" class="p-4 flex flex-col gap-2">
                          <div v-for="j in 3" :key="j" class="h-8 rounded-lg animate-pulse"
                               style="background-color: var(--color-surface-low);"></div>
                        </div>
                        <table v-else class="w-full text-xs">
                          <thead>
                            <tr style="background-color: var(--color-surface-low);">
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="test in expandedTests" :key="test.id"
                                style="border-top: 1px solid var(--color-border);">
                              <td class="px-4 py-2.5">
                                <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                              </td>
                              <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                              <td class="px-4 py-2.5">
                                <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                      :style="testStatusStyle(test.status)">
                                  {{ testStatusLabel(test.status) }}
                                </span>
                              </td>
                              <td class="px-4 py-2.5">
                                <span v-if="test.scheduleTag" class="font-bold" :style="scheduleTagStyle(test.scheduleTag)">
                                  {{ test.scheduleTag }}
                                  <span v-if="test.runningDate" style="color: var(--color-text-muted);"> — {{ test.runningDate }}</span>
                                </span>
                                <span v-else style="color: var(--color-text-muted);">—</span>
                              </td>
                              <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT ?? '—' }}</td>
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
        </div>
      </div>

    </template>

    <!-- ══════════════════════════════════════════════════════════════════════
       ADMIN VIEW — all sections grouped
  ══════════════════════════════════════════════════════════════════════ -->
    <template v-else>

      <!-- Search + count -->
      <div class="mb-4 flex gap-3 flex-wrap">
        <div class="relative flex-1 min-w-[200px]">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-sm pointer-events-none"
                style="color: var(--color-text-muted);">search</span>
          <input v-model="adminSearchQuery"
                 type="text"
                 placeholder="Search specimen no., patient name..."
                 class="w-full pl-9 pr-4 py-2.5 rounded-xl text-sm outline-none transition-all"
                 style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text);" />
        </div>
        <div class="px-4 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2"
             style="background-color: var(--color-surface); border: 1.5px solid var(--color-border); color: var(--color-text-muted);">
          <span class="material-symbols-outlined text-sm" style="color: var(--color-primary);">pending_actions</span>
          {{ adminTotalCount }} specimen{{ adminTotalCount !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- Loading -->
      <div v-if="adminLoading" class="flex flex-col gap-4">
        <div v-for="i in 3" :key="i" class="rounded-2xl p-6 animate-pulse"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow); height: 120px;"></div>
      </div>

      <!-- Empty -->
      <div v-else-if="!adminFilteredGroups.length"
           ref="adminEmptyRef"
           class="rounded-2xl p-16 flex flex-col items-center gap-3"
           style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">inbox</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No pending specimens</p>
        <p class="text-xs" style="color: var(--color-text-muted);">All sections are clear.</p>
      </div>

      <!-- Grouped tables -->
      <div v-else ref="adminGroupsRef" class="flex flex-col gap-5">
        <div v-for="group in adminFilteredGroups" :key="group.sectionCode"
             class="admin-group-card rounded-2xl overflow-hidden"
             style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

          <!-- Section header -->
          <div class="px-6 py-3 flex items-center gap-3 cursor-pointer select-none"
               :data-sectioncode="group.sectionCode"
               style="background-color: var(--color-primary-soft); border-bottom: 1.5px solid var(--color-border);"
               @click="toggleCollapse(group.sectionCode)">
            <span class="material-symbols-outlined text-base" style="color: var(--color-primary);">science</span>
            <h2 class="text-sm font-bold uppercase tracking-widest" style="color: var(--color-primary);">{{ group.sectionName }}</h2>
            <span class="ml-auto text-[10px] font-bold px-2.5 py-1 rounded-full"
                  style="background-color: rgba(70,21,153,0.15); color: var(--color-primary);">
              {{ group.filteredSpecimens.length }} specimen{{ group.filteredSpecimens.length !== 1 ? 's' : '' }}
            </span>
            <!-- Chevron -->
            <span class="material-symbols-outlined text-sm transition-transform"
                  :style="collapsedSections.has(group.sectionCode)
          ? 'color: var(--color-primary); transform: rotate(-90deg);'
          : 'color: var(--color-primary); transform: rotate(0deg);'">
              expand_more
            </span>
          </div>

          <!-- Table -->
          <Transition name="expand">
            <div v-show="!collapsedSections.has(group.sectionCode)" class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr style="border-bottom: 1.5px solid var(--color-border);">
                    <th class="w-8 px-4 py-3"></th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Specimen No.</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Patient</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Sample Type</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Routed</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Remarks</th>
                    <th class="px-4 py-3 w-10"></th>
                  </tr>
                </thead>
                <tbody>
                  <template v-for="item in group.filteredSpecimens" :key="item.id">

                    <!-- Main row -->
                    <tr class="transition-colors cursor-pointer"
                        :data-specimenno="item.specimenNo"
                        :style="adminExpandedKey === `${group.sectionCode}-${item.id}`
                        ? 'background-color: var(--color-primary-soft);'
                        : 'background-color: transparent;'"
                        @mouseenter="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.id}`) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                        @mouseleave="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.id}`) e.currentTarget.style.backgroundColor = 'transparent' }"
                        @click="adminToggleExpand(group.sectionCode, item)"
                        style="border-top: 1px solid var(--color-border);">
                      <td class="px-4 py-3">
                        <span class="material-symbols-outlined text-sm transition-transform duration-200"
                              :style="{ color: 'var(--color-text-muted)', transform: adminExpandedKey === `${group.sectionCode}-${item.id}` ? 'rotate(90deg)' : 'rotate(0deg)' }">
                          chevron_right
                        </span>
                      </td>
                      <td class="px-4 py-3">
                        <div class="flex items-center gap-1.5">
                          <p class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</p>
                          <span v-if="item.isOnSite"
                                class="material-symbols-outlined cursor-default"
                                style="color: var(--color-warning); font-size: 16px;"
                                title="On-Site / Mission">
                            location_on
                          </span>
                        </div>
                      </td>
                      <td class="px-4 py-3">
                        <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName ?? '—' }}</p>
                        <p v-if="item.patientID" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.patientID }}</p>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                        <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.routed) }}</span>
                      </td>
                      <td class="px-4 py-3">
                        <span v-if="item.received" class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                        <span v-else class="text-xs italic" style="color: var(--color-text-muted);">Not yet received</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="px-2.5 py-1 rounded-full text-[10px] font-bold uppercase tracking-widest"
                              :style="headerStatusStyle(item.status)">
                          {{ headerStatusLabel(item.status) }}
                        </span>
                      </td>
                      <td class="px-4 py-3" @click.stop>
                        <button class="p-1.5 rounded-lg transition-all"
                                :style="item.remarks
                                ? 'color: var(--color-warning);'
                                : 'color: var(--color-text-muted); opacity: 0.4;'"
                                :disabled="!item.remarks"
                                @click="openRemarks(item)">
                          <span class="material-symbols-outlined text-sm">
                            {{ item.remarks ? 'chat_bubble' : 'chat_bubble_outline' }}
                          </span>
                        </button>
                      </td>
                      <td class="px-3 py-3" @click.stop>
                        <button class="p-1.5 rounded-lg transition-all group/cancel relative"
                                style="color: var(--color-text-muted);"
                                title="Cancel Specimen"
                                @mouseenter="e => e.currentTarget.style.cssText = 'color: var(--color-error, #dc2626); background-color: rgba(220,38,38,0.08);'"
                                @mouseleave="e => e.currentTarget.style.cssText = 'color: var(--color-text-muted);'"
                                @click="openCancelModal(item)">
                          <span class="material-symbols-outlined text-sm">do_not_disturb_on</span>
                        </button>
                      </td>
                    </tr>

                    <!-- Expanded test rows -->
                    <Transition name="expand">
                      <tr v-if="adminExpandedKey === `${group.sectionCode}-${item.id}`"
                          :key="`exp-${group.sectionCode}-${item.id}`">
                        <td colspan="9" class="px-0 py-0">
                          <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                               style="border: 1.5px solid var(--color-border);">
                            <div v-if="adminTestsLoading" class="p-4 flex flex-col gap-2">
                              <div v-for="j in 3" :key="j" class="h-8 rounded-lg animate-pulse"
                                   style="background-color: var(--color-surface-low);"></div>
                            </div>
                            <table v-else class="w-full text-xs">
                              <thead>
                                <tr style="background-color: var(--color-surface-low);">
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Status</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Schedule</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                                </tr>
                              </thead>
                              <tbody>
                                <tr v-for="test in adminExpandedTests" :key="test.id"
                                    style="border-top: 1px solid var(--color-border);">
                                  <td class="px-4 py-2.5">
                                    <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                                  </td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                                  <td class="px-4 py-2.5">
                                    <span class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-widest"
                                          :style="testStatusStyle(test.status)">
                                      {{ testStatusLabel(test.status) }}
                                    </span>
                                  </td>
                                  <td class="px-4 py-2.5">
                                    <span v-if="test.scheduleTag" class="font-bold" :style="scheduleTagStyle(test.scheduleTag)">
                                      {{ test.scheduleTag }}
                                      <span v-if="test.runningDate" style="color: var(--color-text-muted);"> — {{ test.runningDate }}</span>
                                    </span>
                                    <span v-else style="color: var(--color-text-muted);">—</span>
                                  </td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT ?? '—' }}</td>
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
            </div>
          </Transition>

        </div>
      </div>

    </template>

    <CancelSpecimenModal :isVisible="cancelModal.visible"
                         :specimenNo="cancelModal.specimenNo"
                         :loading="cancelModal.loading"
                         @confirm="submitCancel"
                         @close="cancelModal.visible = false" />

    <RemarkViewer :isVisible="remarkViewer.visible"
                  title="Specimen Remarks"
                  :text="remarkViewer.text"
                  @close="remarkViewer.visible = false" />

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
  import { useRoute, useRouter } from 'vue-router'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import RemarkViewer from '@/components/common/RemarkViewer.vue'
  import CancelSpecimenModal from '@/components/common/CancelSpecimenModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'

  const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  const remarkViewer = ref({ visible: false, text: '' })

  function openRemarks(item) {
    remarkViewer.value = { visible: true, text: item.remarks ?? '' }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // CANCEL SPECIMEN
  // ══════════════════════════════════════════════════════════════════════════

  const cancelModal = ref({
    visible: false,
    headerId: null,
    specimenNo: '',
    sectionCode: '',
    isOnSite: false,
    reason: '',
    loading: false,
  })

  function openCancelModal(item) {
    cancelModal.value = {
      visible: true,
      headerId: item.id,
      specimenNo: item.specimenNo,
      sectionCode: item.sectionCode,
      isOnSite: item.isOnSite ?? false,
      reason: '',
      loading: false,
    }
  }

async function submitCancel(reason) {
  cancelModal.value.loading = true
  try {
    await runnerApi.cancelSpecimen({
      headerId:   cancelModal.value.headerId,
      specimenNo: cancelModal.value.specimenNo,
      sectionCode: cancelModal.value.sectionCode,
      reason,                          // ← comes from the emit now
      userID:     authStore.userID,
      isOnSite:   cancelModal.value.isOnSite,
    })
    cancelModal.value.visible = false
    showAlert('success', 'Specimen Cancelled', `Specimen ${cancelModal.value.specimenNo} has been cancelled.`)
    await silentRefresh()
  } catch (e) {
    showAlert('error', 'Cancel Failed', e?.response?.data?.message ?? 'Could not cancel specimen.')
  } finally {
    cancelModal.value.loading = false
  }
}

  // ══════════════════════════════════════════════════════════════════════════
  // REGULAR / TEAM LEAD
  // ══════════════════════════════════════════════════════════════════════════

  const loading = ref(true)
  const testsLoading = ref(false)
  const specimens = ref([])
  const searchQuery = ref('')
  const expandedId = ref(null)

  const expandedTests = ref([])

  const filteredSpecimens = computed(() => {
    const q = searchQuery.value.toLowerCase()
    if (!q) return specimens.value
    return specimens.value.filter(s =>
      s.specimenNo?.toLowerCase().includes(q) ||
      s.patientName?.toLowerCase().includes(q) ||
      s.patientID?.toLowerCase().includes(q)
    )
  })

  async function toggleExpand(item) {
    if (expandedId.value === item.id) {
      expandedId.value = null
      expandedTests.value = []
      return
    }
    expandedId.value = item.id
    expandedTests.value = []
    testsLoading.value = true
    try {
      expandedTests.value = await runnerApi.getTestsByHeader(item.id, item.isOnSite)
    } catch (e) {
      showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load tests.')
    } finally {
      testsLoading.value = false
    }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // ADMIN
  // ══════════════════════════════════════════════════════════════════════════

  const adminLoading = ref(true)
  const adminTestsLoading = ref(false)
  const adminGroups = ref([])
  const adminSearchQuery = ref('')
  const adminExpandedKey = ref(null)   // format: `${sectionCode}-${itemId}`
  const adminExpandedTests = ref([])
  const collapsedSections = ref(new Set())

  watch(adminSearchQuery, (q) => {
    if (!q) return
    const next = new Set(collapsedSections.value)
    adminGroups.value.forEach(group => {
      const hasMatch = group.specimens.some(s =>
        s.specimenNo?.toLowerCase().includes(q) ||
        s.patientName?.toLowerCase().includes(q) ||
        s.patientID?.toLowerCase().includes(q)
      )
      if (hasMatch) next.delete(group.sectionCode)
    })
    collapsedSections.value = next
  })


  const adminFilteredGroups = computed(() => {
    const q = adminSearchQuery.value.toLowerCase()
    return adminGroups.value
      .map(group => ({
        ...group,
        filteredSpecimens: q
          ? group.specimens.filter(s =>
            s.specimenNo?.toLowerCase().includes(q) ||
            s.patientName?.toLowerCase().includes(q) ||
            s.patientID?.toLowerCase().includes(q)
          )
          : group.specimens,
      }))
      .filter(group => group.filteredSpecimens.length > 0)
  })

  const adminTotalCount = computed(() =>
    adminFilteredGroups.value.reduce((sum, g) => sum + g.filteredSpecimens.length, 0)
  )

  async function adminToggleExpand(sectionCode, item) {
    const key = `${sectionCode}-${item.id}`
    if (adminExpandedKey.value === key) {
      adminExpandedKey.value = null
      adminExpandedTests.value = []
      return
    }
    adminExpandedKey.value = key
    adminExpandedTests.value = []
    adminTestsLoading.value = true
    try {
      adminExpandedTests.value = await runnerApi.getTestsByHeader(item.id, item.isOnSite)
    } catch (e) {
      showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load tests.')
    } finally {
      adminTestsLoading.value = false
    }
  }

  function toggleCollapse(sectionCode) {
    const next = new Set(collapsedSections.value)
    if (next.has(sectionCode)) next.delete(sectionCode)
    else next.add(sectionCode)
    collapsedSections.value = next
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — REFS
  // ══════════════════════════════════════════════════════════════════════════

  // Regular view
  const tableCardRef = ref(null)
  const tableBodyRef = ref(null)
  const emptyRef = ref(null)

  // Admin view
  const adminGroupsRef = ref(null)
  const adminEmptyRef = ref(null)

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — ANIMATION FUNCTIONS
  // ══════════════════════════════════════════════════════════════════════════

  async function animateTableEntrance() {
    await nextTick()
    if (!tableCardRef.value) return
    // Slide the whole card in
    gsap.set(tableCardRef.value, { opacity: 0, y: 16 })
    gsap.to(tableCardRef.value, { opacity: 1, y: 0, duration: 0.28, ease: 'power2.out' })
    // Stagger rows
    if (tableBodyRef.value) {
      const rows = tableBodyRef.value.querySelectorAll('tr')
      if (rows.length) {
        gsap.set(rows, { opacity: 0, x: -8 })
        gsap.to(rows, {
          opacity: 1,
          x: 0,
          duration: 0.18,
          stagger: 0.025,
          ease: 'power1.out',
          delay: 0.14,
          clearProps: 'opacity,x',
        })
      }
    }
  }

  async function animateEmptyState(emptyRefArg) {
    await nextTick()
    if (!emptyRefArg.value) return
    gsap.set(emptyRefArg.value, { scale: 0.92, opacity: 0 })
    gsap.to(emptyRefArg.value, {
      scale: 1,
      opacity: 1,
      duration: 0.32,
      ease: 'back.out(1.5)',
      clearProps: 'scale,opacity',
    })
  }

  async function animateAdminGroups() {
    await nextTick()
    if (!adminGroupsRef.value) return
    const cards = adminGroupsRef.value.querySelectorAll('.admin-group-card')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, {
      opacity: 1,
      y: 0,
      duration: 0.3,
      stagger: 0.07,
      ease: 'power2.out',
    })
    // Stagger table rows within each card
    cards.forEach((card, i) => {
      const rows = card.querySelectorAll('tbody tr')
      if (!rows.length) return
      gsap.set(rows, { opacity: 0, x: -6 })
      gsap.to(rows, {
        opacity: 1,
        x: 0,
        duration: 0.15,
        stagger: 0.02,
        ease: 'power1.out',
        delay: 0.18 + i * 0.05,
        clearProps: 'opacity,x',
      })
    })
  }

  // ── Highlight — fired after data loads when ?highlight=<specimenNo> is present
  async function runHighlight() {
    const specimenNo = route.query.highlight
    if (!specimenNo) return

    await nextTick()

    // Find the row across both regular and admin tables
    const row = document.querySelector(`tr[data-specimenno="${specimenNo}"]`)
    if (!row) return

    // For admin view: make sure the parent section card isn't collapsed
    const groupCard = row.closest('.admin-group-card')
    if (groupCard) {
      // Find which sectionCode this card belongs to by reading its header text
      // We resolve it by checking collapsedSections against the card's header
      const headerEl = groupCard.querySelector('[data-sectioncode]')
      if (headerEl) {
        const sc = headerEl.dataset.sectioncode
        if (collapsedSections.value.has(sc)) {
          const next = new Set(collapsedSections.value)
          next.delete(sc)
          collapsedSections.value = next
          await nextTick()
        }
      }
    }

    // Scroll into view with a little offset
    row.scrollIntoView({ behavior: 'smooth', block: 'center' })

    // Double pulse: flash the background twice smoothly
    await new Promise(r => setTimeout(r, 350)) // let scroll settle
    gsap.set(row, { backgroundColor: 'transparent' })
    gsap.to(row, {
      backgroundColor: 'var(--color-primary-soft)',
      duration: 0.5,
      ease: 'sine.inOut',
      yoyo: true,
      repeat: 3,         // 4 steps total = 2 full in→out pulses
      repeatDelay: 0.12,
      onComplete: () => gsap.set(row, { clearProps: 'backgroundColor' }),
    })

    // Clean the query param so a hard refresh doesn't re-fire
    router.replace({ query: {} })
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — WATCHERS
  // Watches both loading flags so animations fire on initial load AND
  // every manual Refresh button press.
  // ══════════════════════════════════════════════════════════════════════════

  watch(loading, async (isLoading) => {
    if (isLoading) return
    if (filteredSpecimens.value.length) {
      await animateTableEntrance()
      await runHighlight()
    } else {
      await animateEmptyState(emptyRef)
    }
  })

  watch(adminLoading, async (isLoading) => {
    if (isLoading) return
    if (adminFilteredGroups.value.length) {
      await animateAdminGroups()
      await runHighlight()
    } else {
      await animateEmptyState(adminEmptyRef)
    }
  })

  // ══════════════════════════════════════════════════════════════════════════
  // LOAD
  // ══════════════════════════════════════════════════════════════════════════

  async function load() {
    if (!authStore.isAdmin) {
      loading.value = true
      expandedId.value = null
      expandedTests.value = []
      try {
        const data = await runnerApi.getPendingSpecimens(authStore.sectionCode)
        specimens.value = Array.isArray(data) ? data : []
      } catch (e) {
        showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load specimens.')
      } finally {
        loading.value = false
      }
    } else {
      adminLoading.value = true
      adminExpandedKey.value = null
      adminExpandedTests.value = []
      try {
        const data = await runnerApi.getAdminPending()
        adminGroups.value = Array.isArray(data) ? data : []
      } catch (e) {
        showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load specimens.')
      } finally {
        adminLoading.value = false
      }
    }
  }

  async function silentRefresh() {
    try {
      if (!authStore.isAdmin) {
        const data = await runnerApi.getPendingSpecimens(authStore.sectionCode)
        specimens.value = Array.isArray(data) ? data : []
      } else {
        const data = await runnerApi.getAdminPending()
        adminGroups.value = Array.isArray(data) ? data : []
      }
    } catch {
      // fail silently
    }
  }

  // ══════════════════════════════════════════════════════════════════════════
  // FORMATTERS
  // ══════════════════════════════════════════════════════════════════════════

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' })
  }

  function headerStatusLabel(s) {
    return { P: 'Pending', S: 'Saved', C: 'Completed' }[s] ?? s
  }

  function headerStatusStyle(s) {
    const map = {
      P: 'background-color: rgba(70,21,153,0.1); color: var(--color-primary);',
      S: 'background-color: rgba(74,98,109,0.1); color: var(--color-info, #4a626d);',
      C: 'background-color: rgba(22,163,74,0.1); color: var(--color-success, #16a34a);',
    }
    return map[s] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
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

  function scheduleTagStyle(tag) {
    const map = {
      ERD: 'color: var(--color-primary);',
      CRD: 'color: var(--color-warning);',
      SRD: 'color: var(--color-info, #4a626d);',
    }
    return map[tag] ?? 'color: var(--color-text-muted);'
  }

  // ══════════════════════════════════════════════════════════════════════════
  // MOUNT / UNMOUNT
  // ══════════════════════════════════════════════════════════════════════════

  let refreshInterval = null

  onMounted(() => {
    load()
    refreshInterval = setInterval(silentRefresh, 10000)
  })

  onUnmounted(() => {
    clearInterval(refreshInterval)
  })
</script>
