<template>
  <AppLayout>

    <!-- Page Header -->
    <div class="mb-6 flex items-center justify-between flex-wrap gap-3">
      <div>
        <h1 class="text-2xl font-extrabold tracking-tight" style="color: var(--color-text);">Completed Specimens</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">
          <span :style="authStore.isAdmin ? 'color: var(--color-primary); font-weight: 700;' : ''">
            {{ authStore.isAdmin ? 'ADMINISTRATOR' : authStore.sectionName }}
          </span> · {{ authStore.branchCode }}
        </p>
      </div>

      <!-- Date Filters + Refresh -->
      <div class="flex items-center gap-3 flex-wrap">
        <DatePicker v-model="dateFrom"
                    placeholder="Date From"
                    :max-date="dateTo"
                    @change="load" />
        <span class="text-xs font-bold" style="color: var(--color-text-muted);">to</span>
        <DatePicker v-model="dateTo"
                    placeholder="Date To"
                    :min-date="dateFrom"
                    @change="load" />
        <button class="flex items-center gap-2 px-4 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-[0.98]"
                style="background: var(--color-primary-gradient); color: #fff;"
                @click="load">
          <span class="material-symbols-outlined text-sm">refresh</span>
          Refresh
        </button>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════════════
         REGULAR / TEAM LEAD VIEW  (unchanged)
    ══════════════════════════════════════════════════════════════════════ -->
    <template v-if="!authStore.isAdmin">

      <!-- Search + count -->
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
          <span class="material-symbols-outlined text-sm" style="color: var(--color-success, #16a34a);">task_alt</span>
          {{ filteredSpecimens.length }} specimen{{ filteredSpecimens.length !== 1 ? 's' : '' }}
        </div>
      </div>

      <!-- Table Card -->
      <div ref="tableCardRef" class="rounded-2xl overflow-hidden" style="background-color: var(--color-surface); box-shadow: 0 1px 3px var(--color-shadow);">

        <!-- Loading skeleton -->
        <div v-if="loading" class="p-6 flex flex-col gap-3">
          <div v-for="i in 5" :key="i" class="h-14 rounded-xl animate-pulse"
               style="background-color: var(--color-surface-low);"></div>
        </div>

        <!-- Empty state -->
        <div v-else-if="!filteredSpecimens.length" ref="emptyRef" class="p-16 flex flex-col items-center gap-3">
          <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">task_alt</span>
          <p class="text-sm font-bold" style="color: var(--color-text);">No completed specimens</p>
          <p class="text-xs" style="color: var(--color-text-muted);">No specimens have been completed in this date range.</p>
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
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Completed</th>
                <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Completed By</th>
              </tr>
            </thead>
            <tbody ref="tableBodyRef">
              <template v-for="item in paginatedSpecimens" :key="item.headerId">

                <!-- Main row -->
                <tr class="transition-colors cursor-pointer"
                    :data-specimenno="item.specimenNo"
                    :style="expandedId === item.headerId
                      ? 'background-color: var(--color-primary-soft);'
                      : 'background-color: transparent;'"
                    @mouseenter="e => { if (expandedId !== item.headerId) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                    @mouseleave="e => { if (expandedId !== item.headerId) e.currentTarget.style.backgroundColor = 'transparent' }"
                    @click="toggleExpand(item)">
                  <td class="px-4 py-3">
                    <span class="material-symbols-outlined text-sm transition-transform duration-200"
                          :style="{ color: 'var(--color-text-muted)', transform: expandedId === item.headerId ? 'rotate(90deg)' : 'rotate(0deg)' }">
                      chevron_right
                    </span>
                  </td>
                  <td class="px-4 py-3">
                    <div class="flex items-center gap-1.5">
                      <span class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</span>
                      <span v-if="item.isOnSite"
                            class="material-symbols-outlined cursor-default"
                            style="color: var(--color-warning); font-size: 14px;"
                            title="On-Site / Mission">
                        location_on
                      </span>
                    </div>
                  </td>
                  <td class="px-4 py-3">
                    <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName || '—' }}</p>
                    <p v-if="item.pid" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.pid }}</p>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                    <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                  </td>

                  <td class="px-4 py-3">
                    <span class="text-xs font-semibold" style="color: var(--color-success, #16a34a);">{{ formatDt(item.completed) }}</span>
                  </td>
                  <td class="px-4 py-3">
                    <span class="text-xs" style="color: var(--color-text-muted);">{{ item.completedBy || '—' }}</span>
                  </td>
                </tr>

                <!-- Expanded test rows -->
                <Transition name="expand">
                  <tr v-if="expandedId === item.headerId" :key="`exp-${item.headerId}`">
                    <td colspan="7" class="px-0 py-0">
                      <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                           style="border: 1.5px solid var(--color-border);">
                        <table class="w-full text-xs">
                          <thead>
                            <tr style="background-color: var(--color-surface-low);">
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Run At</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Released By</th>
                              <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Released On</th>
                            </tr>
                          </thead>
                          <tbody>
                            <tr v-for="test in item.tests" :key="test.id"
                                style="border-top: 1px solid var(--color-border);">
                              <td class="px-4 py-2.5">
                                <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                              </td>
                              <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                              <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT || '—' }}</td>
                              <td class="px-4 py-2" style="color: var(--color-text-muted);">{{ formatDt(test.runAt) || '—' }}</td>
                              <td class="px-4 py-2" style="color: var(--color-text-muted);">{{ test.releasedBy || '—' }}</td>
                              <td class="px-4 py-2" style="color: var(--color-text-muted);">{{ formatDt(test.releasedOn) || '—' }}</td>
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

          <!-- Pagination — v-model binding (fixes missing modelValue warning) -->
          <div v-if="filteredSpecimens.length > pageSize"
               class="px-6 py-4"
               style="border-top: 1px solid var(--color-border);">
            <AppPagination v-model="currentPage"
                           :total="filteredSpecimens.length"
                           :page-size="pageSize" />
          </div>
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
          <span class="material-symbols-outlined text-sm" style="color: var(--color-success, #16a34a);">task_alt</span>
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
        <span class="material-symbols-outlined text-5xl" style="color: var(--color-text-muted);">task_alt</span>
        <p class="text-sm font-bold" style="color: var(--color-text);">No completed specimens</p>
        <p class="text-xs" style="color: var(--color-text-muted);">No specimens have been completed in this date range.</p>
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
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Received</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Completed</th>
                    <th class="px-4 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Completed By</th>
                  </tr>
                </thead>
                <tbody>
                  <template v-for="item in adminPagedSpecimens(group)" :key="item.headerId">

                    <!-- Main row -->
                    <tr class="transition-colors cursor-pointer"
                        :data-specimenno="item.specimenNo"
                        :style="adminExpandedKey === `${group.sectionCode}-${item.headerId}`
                          ? 'background-color: var(--color-primary-soft);'
                          : 'background-color: transparent;'"
                        @mouseenter="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.headerId}`) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                        @mouseleave="e => { if (adminExpandedKey !== `${group.sectionCode}-${item.headerId}`) e.currentTarget.style.backgroundColor = 'transparent' }"
                        @click="adminToggleExpand(group.sectionCode, item)"
                        style="border-top: 1px solid var(--color-border);">
                      <td class="px-4 py-3">
                        <span class="material-symbols-outlined text-sm transition-transform duration-200"
                              :style="{ color: 'var(--color-text-muted)', transform: adminExpandedKey === `${group.sectionCode}-${item.headerId}` ? 'rotate(90deg)' : 'rotate(0deg)' }">
                          chevron_right
                        </span>
                      </td>
                      <td class="px-4 py-3">
                        <div class="flex items-center gap-1.5">
                          <span class="font-bold font-mono text-xs" style="color: var(--color-text);">{{ item.specimenNo }}</span>
                          <span v-if="item.isOnSite"
                                class="material-symbols-outlined cursor-default"
                                style="color: var(--color-warning); font-size: 14px;"
                                title="On-Site / Mission">
                            location_on
                          </span>
                        </div>
                      </td>
                      <td class="px-4 py-3">
                        <p class="font-semibold text-xs" style="color: var(--color-text);">{{ item.patientName || '—' }}</p>
                        <p v-if="item.pid" class="text-[10px]" style="color: var(--color-text-muted);">{{ item.pid }}</p>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs font-semibold" style="color: var(--color-text);">{{ item.sampleTypeName }}</span>
                        <span class="text-[10px] ml-1.5" style="color: var(--color-text-muted);">({{ item.sampleTypeCode }})</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs" style="color: var(--color-text-muted);">{{ formatDt(item.received) }}</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs font-semibold" style="color: var(--color-success, #16a34a);">{{ formatDt(item.completed) }}</span>
                      </td>
                      <td class="px-4 py-3">
                        <span class="text-xs" style="color: var(--color-text-muted);">{{ item.completedBy || '—' }}</span>
                      </td>
                    </tr>

                    <!-- Expanded test rows -->
                    <Transition name="expand">
                      <tr v-if="adminExpandedKey === `${group.sectionCode}-${item.headerId}`"
                          :key="`exp-${group.sectionCode}-${item.headerId}`">
                        <td colspan="7" class="px-0 py-0">
                          <div class="mx-4 mb-3 rounded-xl overflow-hidden"
                               style="border: 1.5px solid var(--color-border);">
                            <table class="w-full text-xs">
                              <thead>
                                <tr style="background-color: var(--color-surface-low);">
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Code</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Test Name</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Assigned RMT</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Run At</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Released By</th>
                                  <th class="px-4 py-2 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Released On</th>
                                </tr>
                              </thead>
                              <tbody>
                                <tr v-for="test in item.tests" :key="test.id"
                                    style="border-top: 1px solid var(--color-border);">
                                  <td class="px-4 py-2.5">
                                    <span class="font-mono font-bold" style="color: var(--color-text);">{{ test.testCode }}</span>
                                  </td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text);">{{ test.testName }}</td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ test.assignedRMT || '—' }}</td>
                                  <td class="px-4 py-2.5" style="color: var(--color-text-muted);">{{ formatDt(test.runAt) }}</td>
                                  <td class="px-4 py-2" style="color: var(--color-text-muted);">{{ test.releasedBy || '—' }}</td>
                                  <td class="px-4 py-2" style="color: var(--color-text-muted);">{{ formatDt(test.releasedOn) || '—' }}</td>
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

              <!-- ── Pagination footer ───────────────────────────────────── -->
              <div v-if="adminGroupPageCount(group) > 1"
                   class="px-4 py-3 flex items-center justify-between gap-3 flex-wrap"
                   style="border-top: 1.5px solid var(--color-border);">
                <p class="text-[11px]" style="color: var(--color-text-muted);">
                  Showing
                  <span class="font-bold" style="color: var(--color-text);">
                    {{ adminPageStart(group) }}–{{ adminPageEnd(group) }}
                  </span>
                  of
                  <span class="font-bold" style="color: var(--color-text);">
                    {{ group.filteredSpecimens.length }}
                  </span>
                </p>
                <div class="flex items-center gap-1">
                  <!-- Prev -->
                  <button class="px-2.5 py-1.5 rounded-lg text-xs font-bold transition-all disabled:opacity-30 disabled:cursor-not-allowed"
                          style="color: var(--color-text-muted);"
                          :disabled="adminCurrentPage(group.sectionCode) === 1"
                          @mouseenter="e => { if (adminCurrentPage(group.sectionCode) > 1) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                          @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                          @click="adminSetPage(group.sectionCode, adminCurrentPage(group.sectionCode) - 1)">
                    <span class="material-symbols-outlined text-sm">chevron_left</span>
                  </button>

                  <template v-for="pg in adminVisiblePages(group)" :key="pg">
                    <button v-if="pg !== '...'"
                            class="min-w-[28px] px-2 py-1.5 rounded-lg text-xs font-bold transition-all"
                            :style="pg === adminCurrentPage(group.sectionCode)
                              ? 'background: var(--color-primary-gradient); color: #fff;'
                              : 'color: var(--color-text-muted);'"
                            @mouseenter="e => { if (pg !== adminCurrentPage(group.sectionCode)) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                            @mouseleave="e => { if (pg !== adminCurrentPage(group.sectionCode)) e.currentTarget.style.backgroundColor = 'transparent' }"
                            @click="adminSetPage(group.sectionCode, pg)">
                      {{ pg }}
                    </button>
                    <span v-else class="px-1 text-xs" style="color: var(--color-text-muted);">…</span>
                  </template>

                  <!-- Next -->
                  <button class="px-2.5 py-1.5 rounded-lg text-xs font-bold transition-all disabled:opacity-30 disabled:cursor-not-allowed"
                          style="color: var(--color-text-muted);"
                          :disabled="adminCurrentPage(group.sectionCode) === adminGroupPageCount(group)"
                          @mouseenter="e => { if (adminCurrentPage(group.sectionCode) < adminGroupPageCount(group)) e.currentTarget.style.backgroundColor = 'var(--color-surface-low)' }"
                          @mouseleave="e => e.currentTarget.style.backgroundColor = 'transparent'"
                          @click="adminSetPage(group.sectionCode, adminCurrentPage(group.sectionCode) + 1)">
                    <span class="material-symbols-outlined text-sm">chevron_right</span>
                  </button>
                </div>
              </div>

            </div>
          </Transition>

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
  import { ref, computed, onMounted, nextTick, watch } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import AppPagination from '@/components/common/AppPagination.vue'
  import DatePicker from '@/components/common/DatePicker.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { runnerApi } from '@/api/runnerApi'

  const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  // ── Alert ─────────────────────────────────────────────────────────────────────

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  // ── Date range (default: today) ───────────────────────────────────────────────

  //function todayStr() {
  //  return new Date().toISOString().slice(0, 10)
  //}

  function todayStr() {
    const d = new Date()
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
  }

  const dateFrom = ref(todayStr())
  const dateTo = ref(todayStr())

  // ══════════════════════════════════════════════════════════════════════════
  // REGULAR / TEAM LEAD
  // ══════════════════════════════════════════════════════════════════════════

  const loading = ref(true)
  const specimens = ref([])
  const searchQuery = ref('')
  const expandedId = ref(null)
  const currentPage = ref(1)
  const pageSize = 15

  const filteredSpecimens = computed(() => {
    const q = searchQuery.value.toLowerCase()
    if (!q) return specimens.value
    return specimens.value.filter(s =>
      s.specimenNo?.toLowerCase().includes(q) ||
      s.patientName?.toLowerCase().includes(q) ||
      s.pid?.toLowerCase().includes(q)
    )
  })

  const paginatedSpecimens = computed(() => {
    const start = (currentPage.value - 1) * pageSize
    return filteredSpecimens.value.slice(start, start + pageSize)
  })

  // Reset page + expanded row when search changes
  watch(searchQuery, () => {
    currentPage.value = 1
    expandedId.value = null
  })

  // Reset expanded row and re-animate rows when page changes via AppPagination
  watch(currentPage, async () => {
    expandedId.value = null
    await animateTableRows()
  })

  function toggleExpand(item) {
    expandedId.value = expandedId.value === item.headerId ? null : item.headerId
  }

  // ══════════════════════════════════════════════════════════════════════════
  // ADMIN — data
  // ══════════════════════════════════════════════════════════════════════════

  const adminLoading = ref(true)
  const adminGroups = ref([])
  const adminSearchQuery = ref('')
  const adminExpandedKey = ref(null)
  const collapsedSections = ref(new Set())

  // ── Pagination ────────────────────────────────────────────────────────────
  const ADMIN_PAGE_SIZE = 50

  const adminPageMap = ref(new Map())

  function adminCurrentPage(sectionCode) {
    return adminPageMap.value.get(sectionCode) ?? 1
  }

  function adminSetPage(sectionCode, page) {
    const next = new Map(adminPageMap.value)
    next.set(sectionCode, page)
    adminPageMap.value = next
    // Close expanded row if it's no longer on the new page
    if (adminExpandedKey.value?.startsWith(`${sectionCode}-`)) {
      adminExpandedKey.value = null
    }
  }

  function adminGroupPageCount(group) {
    return Math.ceil(group.filteredSpecimens.length / ADMIN_PAGE_SIZE)
  }

  function adminPageStart(group) {
    return (adminCurrentPage(group.sectionCode) - 1) * ADMIN_PAGE_SIZE + 1
  }

  function adminPageEnd(group) {
    return Math.min(adminCurrentPage(group.sectionCode) * ADMIN_PAGE_SIZE, group.filteredSpecimens.length)
  }

  function adminPagedSpecimens(group) {
    const page = adminCurrentPage(group.sectionCode)
    const start = (page - 1) * ADMIN_PAGE_SIZE
    return group.filteredSpecimens.slice(start, start + ADMIN_PAGE_SIZE)
  }

  function adminVisiblePages(group) {
    const total = adminGroupPageCount(group)
    const current = adminCurrentPage(group.sectionCode)
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)

    const pages = new Set([1, total, current])
    if (current > 1) pages.add(current - 1)
    if (current < total) pages.add(current + 1)

    const sorted = Array.from(pages).sort((a, b) => a - b)
    const result = []
    for (let i = 0; i < sorted.length; i++) {
      if (i > 0 && sorted[i] - sorted[i - 1] > 1) result.push('...')
      result.push(sorted[i])
    }
    return result
  }

  // Search: reset pages + uncollapse matching sections
  watch(adminSearchQuery, (q) => {
    adminPageMap.value = new Map()
    if (!q) return
    const next = new Set(collapsedSections.value)
    adminGroups.value.forEach(group => {
      const hasMatch = group.specimens.some(s =>
        s.specimenNo?.toLowerCase().includes(q) ||
        s.patientName?.toLowerCase().includes(q) ||
        s.pid?.toLowerCase().includes(q)
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
            s.pid?.toLowerCase().includes(q)
          )
          : group.specimens,
      }))
      .filter(group => group.filteredSpecimens.length > 0)
  })

  const adminTotalCount = computed(() =>
    adminFilteredGroups.value.reduce((sum, g) => sum + g.filteredSpecimens.length, 0)
  )

  function adminToggleExpand(sectionCode, item) {
    const key = `${sectionCode}-${item.headerId}`
    adminExpandedKey.value = adminExpandedKey.value === key ? null : key
  }

  function toggleCollapse(sectionCode) {
    const next = new Set(collapsedSections.value)
    next.has(sectionCode) ? next.delete(sectionCode) : next.add(sectionCode)
    collapsedSections.value = next
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — REFS
  // ══════════════════════════════════════════════════════════════════════════

  const tableCardRef = ref(null)
  const tableBodyRef = ref(null)
  const emptyRef = ref(null)
  const adminGroupsRef = ref(null)
  const adminEmptyRef = ref(null)

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — ANIMATION FUNCTIONS
  // ══════════════════════════════════════════════════════════════════════════

  async function animateTableEntrance() {
    await nextTick()
    if (!tableCardRef.value) return
    gsap.set(tableCardRef.value, { opacity: 0, y: 16 })
    gsap.to(tableCardRef.value, { opacity: 1, y: 0, duration: 0.28, ease: 'power2.out' })
    if (tableBodyRef.value) {
      const rows = tableBodyRef.value.querySelectorAll('tr')
      if (rows.length) {
        gsap.set(rows, { opacity: 0, x: -8 })
        gsap.to(rows, {
          opacity: 1, x: 0, duration: 0.18, stagger: 0.025,
          ease: 'power1.out', delay: 0.14, clearProps: 'opacity,x',
        })
      }
    }
  }

  async function animateTableRows() {
    await nextTick()
    if (!tableBodyRef.value) return
    const rows = tableBodyRef.value.querySelectorAll('tr')
    if (!rows.length) return
    gsap.set(rows, { opacity: 0, x: -6 })
    gsap.to(rows, {
      opacity: 1, x: 0, duration: 0.18, stagger: 0.025,
      ease: 'power1.out', clearProps: 'opacity,x',
    })
  }

  async function animateEmptyState(emptyRefArg) {
    await nextTick()
    if (!emptyRefArg.value) return
    gsap.set(emptyRefArg.value, { scale: 0.92, opacity: 0 })
    gsap.to(emptyRefArg.value, {
      scale: 1, opacity: 1, duration: 0.32,
      ease: 'back.out(1.5)', clearProps: 'scale,opacity',
    })
  }

  // Admin: animate group cards only — no per-row stagger
  async function animateAdminGroups() {
    await nextTick()
    if (!adminGroupsRef.value) return
    const cards = adminGroupsRef.value.querySelectorAll('.admin-group-card')
    if (!cards.length) return
    gsap.set(cards, { opacity: 0, y: 20 })
    gsap.to(cards, {
      opacity: 1, y: 0, duration: 0.3, stagger: 0.07, ease: 'power2.out',
    })
  }

  async function runHighlight() {
    const specimenNo = route.query.highlight
    if (!specimenNo) return

    // Regular view: jump to the correct page first
    if (!authStore.isAdmin) {
      const idx = filteredSpecimens.value.findIndex(s => s.specimenNo === specimenNo)
      if (idx !== -1) {
        const targetPage = Math.floor(idx / pageSize) + 1
        if (currentPage.value !== targetPage) {
          currentPage.value = targetPage
          await nextTick()
        }
      }
    }

    // Admin view: jump to the correct page within the right section
    if (authStore.isAdmin) {
      for (const group of adminFilteredGroups.value) {
        const idx = group.filteredSpecimens.findIndex(s => s.specimenNo === specimenNo)
        if (idx === -1) continue
        if (collapsedSections.value.has(group.sectionCode)) {
          const next = new Set(collapsedSections.value)
          next.delete(group.sectionCode)
          collapsedSections.value = next
        }
        const targetPage = Math.floor(idx / ADMIN_PAGE_SIZE) + 1
        if (adminCurrentPage(group.sectionCode) !== targetPage) {
          adminSetPage(group.sectionCode, targetPage)
        }
        break
      }
      await nextTick()
    }

    const row = document.querySelector(`tr[data-specimenno="${specimenNo}"]`)
    if (!row) return

    row.scrollIntoView({ behavior: 'smooth', block: 'center' })

    await new Promise(r => setTimeout(r, 350))
    gsap.set(row, { backgroundColor: 'transparent' })
    gsap.to(row, {
      backgroundColor: 'var(--color-primary-soft)',
      duration: 0.5,
      ease: 'sine.inOut',
      yoyo: true,
      repeat: 3,
      repeatDelay: 0.12,
      onComplete: () => gsap.set(row, { clearProps: 'backgroundColor' }),
    })

    router.replace({ query: {} })
  }

  // ══════════════════════════════════════════════════════════════════════════
  // GSAP — WATCHERS
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
      currentPage.value = 1
      try {
        const data = await runnerApi.getCompletedSpecimens(dateFrom.value, dateTo.value)
        specimens.value = Array.isArray(data) ? data : []
      } catch (e) {
        if (e?.response?.status === 401) {
          showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
        } else {
          showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load completed specimens.')
        }
      } finally {
        loading.value = false
      }
    } else {
      adminLoading.value = true
      adminExpandedKey.value = null
      adminPageMap.value = new Map()
      try {
        const data = await runnerApi.getAdminCompleted(dateFrom.value, dateTo.value)
        adminGroups.value = Array.isArray(data) ? data : []
      } catch (e) {
        if (e?.response?.status === 401) {
          showAlert('error', 'Session Expired', 'Your session has expired. Please log in again.')
        } else {
          showAlert('error', 'Load Failed', e?.response?.data?.message ?? 'Could not load completed specimens.')
        }
      } finally {
        adminLoading.value = false
      }
    }
  }

  onMounted(() => load())

  // ══════════════════════════════════════════════════════════════════════════
  // FORMATTERS
  // ══════════════════════════════════════════════════════════════════════════

  function formatDt(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleString('en-PH', { month: 'short', day: '2-digit', hour: '2-digit', minute: '2-digit' })
  }
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
</style>
