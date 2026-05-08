<template>
  <div class="min-h-screen flex items-center justify-center p-6 relative overflow-hidden"
       style="background-color: #f8f9fa; color: #191c1d">
    <!-- Background Image (right half) -->
    <div class="absolute top-0 right-0 w-1/2 h-full hidden lg:block">
      <div class="w-full h-full relative">
        <img src="/loginbg.png"
             alt="Laboratory Background"
             class="w-full h-full object-cover opacity-90" />
        <div class="absolute inset-0"
             style="background: linear-gradient(to right, #f8f9fa, rgba(248, 249, 250, 0.4), transparent);"></div>
      </div>
    </div>

    <!-- Floating Glows -->
    <div class="absolute -bottom-24 -left-24 w-96 h-96 rounded-full pointer-events-none"
         style="background: rgba(70, 21, 153, 0.05); filter: blur(120px)"></div>
    <div class="absolute top-1/4 left-1/4 w-48 h-48 rounded-full pointer-events-none"
         style="background: rgba(205, 230, 244, 0.2); filter: blur(100px)"></div>

    <!-- Login Container -->
    <main class="w-full max-w-md relative z-10">
      <!-- Brand Header -->
      <div class="mb-12 text-left lg:pl-4">
        <h1 class="text-4xl font-bold tracking-tighter mb-2" style="color: #461599">SETS</h1>
        <p class="text-sm font-bold uppercase tracking-widest" style="color: #4a626d">
          Specimen Endorsement &amp; Tracking System
        </p>
      </div>

      <!-- Login Card -->
      <div class="p-10 rounded-2xl shadow-2xl border"
           style="
          background: rgba(248, 249, 250, 0.8);
          backdrop-filter: blur(12px);
          -webkit-backdrop-filter: blur(12px);
          border-color: rgba(203, 195, 213, 0.15);
        ">
        <header class="mb-8">
          <h2 class="text-2xl font-bold" style="color: #191c1d">
            {{ contingencyMode ? 'Contingency Login' : 'Login' }}
          </h2>
          <p class="text-sm mt-1" style="color: #7b7484">
            {{
 contingencyMode
              ? 'HCLAB is offline. Enter your User ID and the contingency password.'
              : 'Please enter your HCLAB credentials to proceed.'
            }}
          </p>
        </header>

        <!-- ── HCLAB Offline Banner ─────────────────────────────────────── -->
        <Transition name="banner-slide">
          <div v-if="hclabOffline && !contingencyMode"
               class="mb-6 p-4 rounded-2xl flex items-start gap-3"
               style="background-color: rgba(217,119,6,0.1); border: 1.5px solid rgba(217,119,6,0.35);">
            <span class="material-symbols-outlined text-lg flex-shrink-0 mt-0.5"
                  style="color: #d97706">wifi_off</span>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold uppercase tracking-widest" style="color: #d97706">HCLAB Offline</p>
              <p class="text-xs mt-0.5" style="color: #92400e">
                Cannot reach HCLAB. You may switch to Contingency Mode to continue endorsement.
              </p>
            </div>
            <button @click="enableContingency"
                    class="flex-shrink-0 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                    style="background-color: #d97706; color: #ffffff;">
              Switch
            </button>
          </div>
        </Transition>

        <!-- ── Contingency Mode Active Banner ─────────────────────────────── -->
        <Transition name="banner-slide">
          <div v-if="contingencyMode"
               class="mb-6 p-4 rounded-2xl flex items-center gap-3"
               style="background-color: rgba(217,119,6,0.1); border: 1.5px solid rgba(217,119,6,0.35);">
            <span class="material-symbols-outlined text-lg flex-shrink-0"
                  style="color: #d97706">offline_bolt</span>
            <div class="flex-1 min-w-0">
              <p class="text-xs font-bold uppercase tracking-widest" style="color: #d97706">⚡ Contingency Mode</p>
              <p class="text-xs mt-0.5" style="color: #92400e">Only Contingency tab will be accessible.</p>
            </div>
            <button @click="disableContingency"
                    class="flex-shrink-0 px-3 py-1.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all active:scale-95"
                    style="background-color: rgba(217,119,6,0.2); color: #d97706;">
              Cancel
            </button>
          </div>
        </Transition>

        <form class="space-y-6" @submit.prevent="handleLogin">

          <!-- Branch / Section Display -->
          <div class="p-4 rounded-2xl flex items-center gap-4 transition-all"
               :style="branchFieldStyle"
               :class="{ 'cursor-pointer hover:opacity-90': pcInfo.isRegistered && pcInfo.sections.length > 1 }"
               @click="openSectionPicker">
            <!-- Loading state -->
            <template v-if="pcLoading">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: #e7e8e9;">
                <span class="material-symbols-outlined text-lg animate-spin" style="color: #7b7484">progress_activity</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #7b7484">Detecting...</p>
                <p class="font-bold" style="color: #7b7484">Please wait</p>
              </div>
            </template>

            <!-- Not registered -->
            <template v-else-if="!pcInfo.isRegistered">
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: rgba(186,26,26,0.1);">
                <span class="material-symbols-outlined text-lg" style="color: #ba1a1a">warning</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #ba1a1a">Unregistered Device</p>
                <p class="font-bold text-sm" style="color: #ba1a1a">IP {{ pcInfo.ipAddress }} is not registered.</p>
              </div>
            </template>

            <!-- Registered -->
            <template v-else>
              <div class="w-10 h-10 rounded-full flex items-center justify-center" style="background-color: #cde6f4; color: #4a626d">
                <span class="material-symbols-outlined text-lg">location_on</span>
              </div>
              <div>
                <p class="text-[10px] font-bold uppercase tracking-widest" style="color: #506873">Current Section</p>
                <p class="font-bold" style="color: #191c1d">{{ selectedSection?.name ?? 'Select Section' }}</p>
              </div>
              <div class="ml-auto opacity-40">
                <span class="material-symbols-outlined text-sm" style="color: #191c1d">
                  {{ pcInfo.sections.length > 1 ? 'expand_more' : 'lock' }}
                </span>
              </div>
            </template>
          </div>

          <!-- User ID -->
          <div>
            <label class="block text-[11px] font-bold uppercase tracking-widest mb-2 ml-1"
                   style="color: #4a626d"
                   for="user-id">
              User ID
            </label>
            <div class="relative flex items-center">
              <input id="user-id"
                     v-model="form.userID"
                     type="text"
                     placeholder="Enter User ID"
                     :disabled="!pcInfo.isRegistered || pcLoading"
                     class="w-full border-none outline-none rounded-2xl py-4 px-6 transition-colors"
                     style="background-color: #e7e8e9; color: #191c1d"
                     @focus="(e) => (e.target.style.backgroundColor = '#e1e3e4')"
                     @blur="(e) => (e.target.style.backgroundColor = '#e7e8e9')" />
            </div>
          </div>

          <!-- Password -->
          <div>
            <label class="block text-[11px] font-bold uppercase tracking-widest mb-2 ml-1"
                   style="color: #4a626d"
                   for="password">
              {{ contingencyMode ? 'Contingency Password' : 'Password' }}
            </label>
            <div class="relative flex items-center">
              <input id="password"
                     v-model="form.password"
                     :type="showPassword ? 'text' : 'password'"
                     :placeholder="contingencyMode ? 'Enter contingency password' : '••••••••'"
                     :disabled="!pcInfo.isRegistered || pcLoading"
                     class="w-full border-none outline-none rounded-2xl py-4 pl-6 pr-12 transition-colors"
                     style="background-color: #e7e8e9; color: #191c1d"
                     @focus="(e) => (e.target.style.backgroundColor = '#e1e3e4')"
                     @blur="(e) => (e.target.style.backgroundColor = '#e7e8e9')" />
              <button type="button"
                      class="absolute right-4 transition-colors"
                      style="color: #4a626d"
                      :disabled="!pcInfo.isRegistered || pcLoading"
                      @mouseenter="(e) => (e.currentTarget.style.color = '#461599')"
                      @mouseleave="(e) => (e.currentTarget.style.color = '#4a626d')"
                      @click="showPassword = !showPassword">
                <span class="material-symbols-outlined text-lg">
                  {{ showPassword ? "visibility_off" : "visibility" }}
                </span>
              </button>
            </div>
          </div>

          <!-- Submit Button -->
          <button type="submit"
                  :disabled="!pcInfo.isRegistered || isLoading || pcLoading"
                  class="w-full py-4 px-6 font-bold rounded-2xl shadow-lg flex items-center justify-center gap-2 mt-4 transition-all active:scale-[0.98]"
                  :style="pcInfo.isRegistered
                    ? contingencyMode
                      ? 'background: linear-gradient(135deg, #b45309, #d97706); color: #ffffff; cursor: pointer;'
                      : 'background: linear-gradient(135deg, #461599, #5e35b1); color: #ffffff; cursor: pointer;'
                    : 'background: #e7e8e9; color: #7b7484; cursor: not-allowed;'">
            <span v-if="!isLoading" class="flex items-center gap-2 uppercase tracking-widest">
              {{ contingencyMode ? '⚡ Contingency Login' : 'Login' }}
              <span class="material-symbols-outlined text-lg">arrow_forward</span>
            </span>
            <span v-else class="uppercase tracking-widest">Authenticating...</span>
          </button>

        </form>

        <footer class="mt-10 pt-8 text-center"
                style="border-top: 1px solid rgba(203, 195, 213, 0.1)">
          <p class="text-[11px] uppercase tracking-widest leading-relaxed" style="color: rgba(73, 68, 83, 0.6)">
            Authorized Personnel Only<br />
            SETS • v1.0.0
          </p>
        </footer>
      </div>

      <div class="mt-8 flex items-center gap-3 justify-center" style="opacity: 0.5">
        <div class="h-px w-10" style="background-color: #cbc3d5"></div>
        <span class="text-[10px] font-bold uppercase tracking-tighter" style="color: #4a626d">HI-Precision Diagnostics Cebu</span>
        <div class="h-px w-10" style="background-color: #cbc3d5"></div>
      </div>
    </main>
  </div>

  <!-- Section Picker Modal -->
  <div v-if="sectionPickerVisible" class="fixed inset-0 z-50 flex items-center justify-center p-4">
    <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="sectionPickerVisible = false"></div>
    <div class="relative w-full max-w-sm rounded-2xl shadow-2xl p-8 flex flex-col gap-4 animate-modal"
         style="background-color: #ffffff;">
      <h3 class="text-lg font-bold" style="color: #191c1d">Select Section</h3>
      <p class="text-sm" style="color: #7b7484">Choose the section for this session.</p>

      <div class="flex flex-col gap-2 mt-2">
        <button v-for="section in pcInfo.sections"
                :key="section.code"
                type="button"
                class="w-full p-4 rounded-xl text-left flex items-center gap-3 transition-all"
                :style="selectedSection?.code === section.code
            ? 'background-color: rgba(70,21,153,0.1); border: 1.5px solid #461599;'
            : 'background-color: #f3f4f5; border: 1.5px solid transparent;'"
                @click="selectSection(section)">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold"
               style="background-color: #cde6f4; color: #461599;">
            {{ section.code.charAt(0) }}
          </div>
          <div>
            <p class="font-bold text-sm" style="color: #191c1d">{{ section.name }}</p>
            <p class="text-[11px]" style="color: #7b7484">{{ section.code }}</p>
          </div>
          <div v-if="selectedSection?.code === section.code" class="ml-auto">
            <span class="material-symbols-outlined text-sm" style="color: #461599">check_circle</span>
          </div>
        </button>
      </div>

      <button type="button"
              class="mt-2 w-full py-3 px-6 rounded-xl font-bold uppercase tracking-widest text-sm transition-all active:scale-[0.98]"
              style="background-color: #461599; color: #ffffff;"
              @click="sectionPickerVisible = false">
        Confirm
      </button>
    </div>
  </div>

  <!-- Alert Modal -->
  <AlertModal :isVisible="alert.isVisible"
              :type="alert.type"
              :title="alert.title"
              :message="alert.message"
              @close="alert.isVisible = false"
              @confirm="alert.isVisible = false" />

  <!-- Login Splash (unchanged) -->
  <Teleport to="body">
    <div ref="splashRef"
         class="fixed inset-0 z-[9999] flex flex-col items-center justify-center gap-3"
         style="background-color: #0d0d14; display: none;">

      <div ref="splashRing2Ref" class="absolute rounded-full"
           style="width:240px;height:240px;border:1px solid rgba(124,58,237,0.1);opacity:0;"></div>
      <div ref="splashRing1Ref" class="absolute rounded-full"
           style="width:160px;height:160px;border:1.5px solid rgba(124,58,237,0.2);opacity:0;"></div>

      <div ref="splashDot1Ref" class="absolute rounded-full"
           style="width:6px;height:6px;background:#7c3aed;top:37%;left:62%;opacity:0;"></div>
      <div ref="splashDot2Ref" class="absolute rounded-full"
           style="width:4px;height:4px;background:#7c3aed;top:63%;left:36%;opacity:0;"></div>
      <div ref="splashDot3Ref" class="absolute rounded-full"
           style="width:5px;height:5px;background:#7c3aed;top:33%;left:38%;opacity:0;"></div>

      <svg ref="splashIconRef" width="64" height="60" viewBox="0 0 60 52" fill="none"
           xmlns="http://www.w3.org/2000/svg" style="opacity:0;position:relative;z-index:2;">
        <path d="M4 8 L4 36 Q4 46 10 46 Q16 46 16 36 L16 8" stroke="#a78bfa" stroke-width="2" fill="none" />
        <line x1="2" y1="8" x2="18" y2="8" stroke="#a78bfa" stroke-width="2.5" stroke-linecap="round" />
        <path ref="splashFill1Ref" d="M4 36 L4 36 Q4 46 10 46 Q16 46 16 36 L16 36 Z" fill="#a78bfa" opacity="0.45" />

        <path d="M22 4 L22 36 Q22 46 28 46 Q34 46 34 36 L34 4" stroke="#a78bfa" stroke-width="2" fill="none" />
        <line x1="20" y1="4" x2="36" y2="4" stroke="#a78bfa" stroke-width="2.5" stroke-linecap="round" />
        <path ref="splashFill2Ref" d="M22 36 L22 36 Q22 46 28 46 Q34 46 34 36 L34 36 Z" fill="#a78bfa" opacity="0.45" />
        <circle ref="splashBubble1Ref" cx="26" cy="30" r="1.5" fill="#a78bfa" opacity="0" />
        <circle ref="splashBubble2Ref" cx="30" cy="35" r="1" fill="#a78bfa" opacity="0" />

        <path d="M40 10 L40 36 Q40 46 46 46 Q52 46 52 36 L52 10" stroke="#a78bfa" stroke-width="2" fill="none" />
        <line x1="38" y1="10" x2="54" y2="10" stroke="#a78bfa" stroke-width="2.5" stroke-linecap="round" />
        <path ref="splashFill3Ref" d="M40 36 L40 36 Q40 46 46 46 Q52 46 52 36 L52 36 Z" fill="#a78bfa" opacity="0.45" />
      </svg>

      <div style="position:relative;z-index:2;text-align:center;">
        <div ref="splashLogoRef"
             style="font-size:32px;font-weight:700;color:#ffffff;letter-spacing:-0.5px;opacity:0;">
          SETS
        </div>
        <div ref="splashTagRef"
             style="font-size:9px;font-weight:600;letter-spacing:0.18em;text-transform:uppercase;color:rgba(255,255,255,0.3);margin-top:4px;opacity:0;">
          Specimen Endorsement &amp; Tracking System
        </div>
      </div>

      <div style="position:absolute;bottom:0;left:0;width:100%;height:2px;background:rgba(124,58,237,0.15);">
        <div ref="splashBarRef" style="height:100%;background:#7c3aed;width:0%;"></div>
      </div>
    </div>
  </Teleport>
</template>

<script setup>
  import { ref, computed, onMounted, nextTick } from 'vue'
  import { gsap } from 'gsap'
  import { useRouter } from 'vue-router'
  import NProgress from 'nprogress'
  import { authApi } from '@/api/authApi'
  import { healthApi } from '@/api/healthApi'
  import { useAuthStore } from '@/stores/authStore'
  import { getDefaultRoute } from '@/router'
  import AlertModal from '@/components/common/AlertModal.vue'
  import { useTheme } from '@/composables/useTheme'

  // ── Splash refs (unchanged) ────────────────────────────────────────────────
  const splashRef = ref(null)
  const splashLogoRef = ref(null)
  const splashTagRef = ref(null)
  const splashRing1Ref = ref(null)
  const splashRing2Ref = ref(null)
  const splashDot1Ref = ref(null)
  const splashDot2Ref = ref(null)
  const splashDot3Ref = ref(null)
  const splashIconRef = ref(null)
  const splashFill1Ref = ref(null)
  const splashFill2Ref = ref(null)
  const splashFill3Ref = ref(null)
  const splashBubble1Ref = ref(null)
  const splashBubble2Ref = ref(null)
  const splashBarRef = ref(null)

  const { applyTheme, applyAccent } = useTheme()
  const router = useRouter()
  const authStore = useAuthStore()

  // ── State ──────────────────────────────────────────────────────────────────
  const showPassword = ref(false)
  const isLoading = ref(false)
  const pcLoading = ref(true)
  const sectionPickerVisible = ref(false)

  // ── Contingency ────────────────────────────────────────────────────────────
  const hclabOffline = ref(false)   // true when pre-login HCLAB check fails
  const contingencyMode = ref(false)   // user switched to contingency login

  function enableContingency() { contingencyMode.value = true; form.value.password = '' }
  function disableContingency() { contingencyMode.value = false; form.value.password = '' }

  // ── PC / Section ───────────────────────────────────────────────────────────
  const pcInfo = ref({ isRegistered: false, ipAddress: '', sections: [] })
  const selectedSection = ref(null)

  const alert = ref({ isVisible: false, type: 'error', title: '', message: '' })
  const form = ref({ userID: '', password: '' })

  const branchFieldStyle = computed(() => {
    if (pcLoading.value) return 'background-color: #f3f4f5;'
    if (!pcInfo.value.isRegistered) return 'background-color: rgba(186,26,26,0.08); border: 1.5px solid rgba(186,26,26,0.3);'
    return 'background-color: #f3f4f5;'
  })

  function showAlert(type, title, message) {
    alert.value = { isVisible: true, type, title, message }
  }

  function openSectionPicker() {
    if (pcInfo.value.isRegistered && pcInfo.value.sections.length > 1)
      sectionPickerVisible.value = true
  }

  function selectSection(section) {
    selectedSection.value = section
  }

  // ── Mount: PC info then HCLAB pre-check ───────────────────────────────────
  onMounted(async () => {
    try {
      const response = await authApi.getPCInfo()
      pcInfo.value = response.data

      if (pcInfo.value.isRegistered && pcInfo.value.sections.length > 0) {
        selectedSection.value = pcInfo.value.sections[0]

        // Check HCLAB reachability using the branch from PC info
        const branch = pcInfo.value.sections[0].branchCode
        try {
          const hclab = await healthApi.hclabPreLogin(branch)
          hclabOffline.value = !hclab.online
        } catch {
          hclabOffline.value = true
        }
      }
    } catch {
      pcInfo.value = { isRegistered: false, ipAddress: 'Unknown', sections: [] }
    } finally {
      pcLoading.value = false
    }
  })

  // ── Login handler — branches on contingencyMode ────────────────────────────
  async function handleLogin() {
    if (!pcInfo.value.isRegistered) return

    if (!form.value.userID || !form.value.password) {
      showAlert('warning', 'Missing Fields', 'Please enter your User ID and Password.')
      return
    }

    if (!selectedSection.value) {
      showAlert('warning', 'No Section', 'Please select a section to proceed.')
      return
    }

    contingencyMode.value ? await handleContingencyLogin() : await handleNormalLogin()
  }

  async function handleNormalLogin() {
    isLoading.value = true
    NProgress.start()
    try {
      const response = await authApi.login({
        userID: form.value.userID.toUpperCase(),
        password: form.value.password,
        branch: selectedSection.value.branchCode,
        sectionCode: selectedSection.value.code,
      })

      const data = response.data
      if (!data.success) {
        showAlert('error', 'Login Failed', data.message || 'Invalid username or password.')
        NProgress.done()
        return
      }

      authStore.setUser(
        { userID: data.data.userID, userName: data.data.userName, isAdmin: data.data.isAdmin, theme: data.data.theme, accentColor: data.data.accentColor },
        data.data.branchCode,
        { code: data.data.sectionCode, name: data.data.sectionName, roleID: data.data.roleID, branchCode: data.data.branchCode, category: data.data.sectionCategory }
      )

      applyTheme(data.data.theme)
      applyAccent(data.data.accentColor)
      await playLoginSplash()
      router.push(getDefaultRoute(authStore))
      await nextTick(); await nextTick()
      if (splashRef.value) splashRef.value.style.display = 'none'

    } catch (err) {
      if (err.response) {
        showAlert('error', 'Login Failed', err.response?.data?.message || 'Invalid username or password.')
      } else {
        showAlert('error', 'Connection Error', 'Unable to connect to the server. Please try again.')
      }
      NProgress.done()
    } finally {
      isLoading.value = false
    }
  }

  async function handleContingencyLogin() {
    isLoading.value = true
    NProgress.start()
    try {
      const response = await authApi.contingencyLogin({
        userID: form.value.userID.toUpperCase(),
        password: form.value.password,
        branch: selectedSection.value.branchCode,
        sectionCode: selectedSection.value.code,
      })

      const data = response.data
      if (!data.success) {
        showAlert('error', 'Login Failed', data.message)
        NProgress.done()
        return
      }

      // isContingency = true — 4th param
      authStore.setUser(
        { userID: data.data.userID, userName: data.data.userName, isAdmin: false, theme: data.data.theme, accentColor: data.data.accentColor },
        data.data.branchCode,
        { code: data.data.sectionCode, name: data.data.sectionName, roleID: data.data.roleID, branchCode: data.data.branchCode, category: data.data.sectionCategory },
        true
      )

      applyTheme(data.data.theme)
      applyAccent(data.data.accentColor)
      await playLoginSplash()

      // Route to contingency page only
      const route = data.data.sectionCategory === '1'
        ? '/contingency/endorse'
        : '/receiver/contingency'
      router.push(route)

      await nextTick(); await nextTick()
      if (splashRef.value) splashRef.value.style.display = 'none'

    } catch (err) {
      showAlert('error', 'Login Failed', err.response?.data?.message ?? 'Invalid contingency password.')
      NProgress.done()
    } finally {
      isLoading.value = false
    }
  }

  // ── Splash animation (unchanged) ───────────────────────────────────────────
  function playLoginSplash() {
    return new Promise((resolve) => {
      const splash = splashRef.value
      const ring1 = splashRing1Ref.value
      const ring2 = splashRing2Ref.value
      const dots = [splashDot1Ref.value, splashDot2Ref.value, splashDot3Ref.value]
      const icon = splashIconRef.value
      const fill1 = splashFill1Ref.value
      const fill2 = splashFill2Ref.value
      const fill3 = splashFill3Ref.value
      const bubble1 = splashBubble1Ref.value
      const bubble2 = splashBubble2Ref.value
      const logo = splashLogoRef.value
      const tag = splashTagRef.value
      const bar = splashBarRef.value

      splash.style.display = 'flex'

      const tl = gsap.timeline({ onComplete: resolve })

      tl.fromTo(splash, { opacity: 0 }, { opacity: 1, duration: 0.2, ease: 'power2.out' })
        .fromTo(ring2, { opacity: 0, scale: 0.5 }, { opacity: 1, scale: 1, duration: 0.35, ease: 'power2.out' }, '-=0.1')
        .fromTo(ring1, { opacity: 0, scale: 0.5 }, { opacity: 1, scale: 1, duration: 0.3, ease: 'power2.out' }, '-=0.3')
        .fromTo(dots, { opacity: 0, scale: 0 }, { opacity: 1, scale: 1, duration: 0.2, stagger: 0.05, ease: 'back.out(2)' }, '-=0.2')
        .fromTo(icon, { opacity: 0, y: 10 }, { opacity: 1, y: 0, duration: 0.25, ease: 'power3.out' }, '-=0.1')
        .to(fill1, { attr: { d: 'M4 28 L4 36 Q4 46 10 46 Q16 46 16 36 L16 28 Z' }, duration: 0.35, ease: 'power2.out' }, '-=0.05')
        .to(fill2, { attr: { d: 'M22 20 L22 36 Q22 46 28 46 Q34 46 34 36 L34 20 Z' }, duration: 0.35, ease: 'power2.out' }, '-=0.3')
        .to(fill3, { attr: { d: 'M40 22 L40 36 Q40 46 46 46 Q52 46 52 36 L52 22 Z' }, duration: 0.35, ease: 'power2.out' }, '-=0.3')
        .fromTo([bubble1, bubble2], { attr: { opacity: 0 } }, { attr: { opacity: 0.7 }, duration: 0.2, stagger: 0.07, ease: 'power2.out' }, '-=0.1')
        .fromTo(logo, { opacity: 0, y: 10 }, { opacity: 1, y: 0, duration: 0.25, ease: 'power3.out' }, '-=0.1')
        .fromTo(tag, { opacity: 0 }, { opacity: 1, duration: 0.2 }, '-=0.2')
        .to({}, { duration: 0.25 })
        .fromTo(bar, { width: '0%' }, { width: '100%', duration: 0.45, ease: 'power2.inOut' })
    })
  }
</script>

<style>
  body {
    font-family: 'Manrope', sans-serif;
  }

  #nprogress .bar {
    background: #461599 !important;
    height: 3px;
  }

  #nprogress .peg {
    box-shadow: 0 0 10px #461599, 0 0 5px #461599 !important;
  }

  #nprogress .spinner-icon {
    border-top-color: #461599 !important;
    border-left-color: #461599 !important;
  }

  .animate-modal {
    animation: modalIn 0.25s cubic-bezier(0.34, 1.56, 0.64, 1);
  }

  @keyframes modalIn {
    from {
      opacity: 0;
      transform: scale(0.85) translateY(20px);
    }

    to {
      opacity: 1;
      transform: scale(1) translateY(0);
    }
  }

  .banner-slide-enter-active,
  .banner-slide-leave-active {
    transition: all 0.25s ease;
  }

  .banner-slide-enter-from,
  .banner-slide-leave-to {
    opacity: 0;
    transform: translateY(-8px);
  }
</style>
