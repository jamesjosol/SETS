<template>
  <AppLayout>
    <div class="max-w-4xl mx-auto">

      <!-- Page header -->
      <div class="mb-8">
        <h1 class="text-2xl font-bold" style="color: var(--color-text);">My Profile</h1>
        <p class="text-sm mt-1" style="color: var(--color-text-muted);">Your account details, section access, and activity.</p>
      </div>

      <!-- Loading skeleton -->
      <div v-if="loading" class="grid grid-cols-3 gap-6">
        <div class="col-span-1 h-72 rounded-2xl animate-pulse" style="background-color: var(--color-surface);"></div>
        <div class="col-span-2 flex flex-col gap-4">
          <div class="h-40 rounded-2xl animate-pulse" style="background-color: var(--color-surface);"></div>
          <div class="h-40 rounded-2xl animate-pulse" style="background-color: var(--color-surface);"></div>
        </div>
      </div>

      <div v-else-if="profile" class="grid grid-cols-3 gap-6">

        <!-- ── Left column: Identity card ──────────────────────────────── -->
        <div class="col-span-1 flex flex-col gap-6">

          <!-- Avatar card -->
          <div class="rounded-2xl p-6 flex flex-col items-center gap-4"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">

            <!-- Avatar -->
            <div class="relative group">
              <div class="w-24 h-24 rounded-full overflow-hidden flex items-center justify-center font-bold text-2xl flex-shrink-0"
                   style="background: var(--color-primary-gradient); color: #ffffff;">
                <img v-if="authStore.profilePicture"
                     :src="authStore.profilePicture"
                     class="w-full h-full object-cover" />
                <span v-else>{{ userInitials }}</span>
              </div>

              <!-- Upload overlay -->
              <button class="absolute inset-0 rounded-full flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity cursor-pointer"
                      style="background-color: rgba(0,0,0,0.45);"
                      @click="triggerFileInput">
                <span class="material-symbols-outlined text-white text-xl">photo_camera</span>
              </button>
              <input ref="fileInputRef" type="file" accept="image/*" class="hidden" @change="onFileSelected" />
            </div>

            <!-- Name & role -->
            <div class="text-center">
              <p class="font-bold text-base" style="color: var(--color-text);">{{ profile.userName }}</p>
              <p class="text-xs mt-0.5 font-mono uppercase tracking-widest" style="color: var(--color-text-muted);">{{ profile.userID }}</p>
              <div class="flex items-center justify-center gap-2 mt-2">
                <span v-if="profile.isAdmin"
                      class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-widest"
                      style="background-color: rgba(var(--color-primary-rgb,99,102,241),0.12); color: var(--color-primary);">
                  Administrator
                </span>
              </div>
            </div>

            <!-- Meta -->
            <div class="w-full pt-4" style="border-top: 1px solid var(--color-border);">
              <div class="flex flex-col gap-2">
                <div class="flex items-center gap-2">
                  <span class="material-symbols-outlined text-base" style="color: var(--color-text-muted); font-size: 16px;">location_city</span>
                  <span class="text-xs" style="color: var(--color-text-muted);">{{ authStore.branchCode }}</span>
                </div>
                <div class="flex items-center gap-2">
                  <span class="material-symbols-outlined text-base" style="color: var(--color-text-muted); font-size: 16px;">calendar_today</span>
                  <span class="text-xs" style="color: var(--color-text-muted);">Member since {{ formatDate(profile.created) }}</span>
                </div>
              </div>
            </div>

            <!-- Remove photo button -->
            <button v-if="authStore.profilePicture"
                    class="w-full text-xs font-bold py-2 rounded-xl transition-all"
                    style="color: var(--color-error); background-color: transparent;"
                    @mouseenter="(e) => e.currentTarget.style.backgroundColor = 'rgba(239,68,68,0.08)'"
                    @mouseleave="(e) => e.currentTarget.style.backgroundColor = 'transparent'"
                    @click="removePhoto">
              Remove Photo
            </button>
          </div>

          <!-- Appearance card -->
          <div class="rounded-2xl p-5"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
            <p class="text-xs font-bold uppercase tracking-widest mb-4" style="color: var(--color-text-muted);">Appearance</p>

            <!-- Theme -->
            <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Mode</p>
            <div class="flex items-center gap-2 mb-4">
              <button v-for="t in themes" :key="t.value"
                      class="flex-1 py-2 rounded-xl text-[10px] font-bold uppercase tracking-widest transition-all"
                      :style="authStore.theme === t.value
                        ? 'background: var(--color-primary-gradient); color: #fff;'
                        : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'"
                      @click="setTheme(t.value)">
                {{ t.label }}
              </button>
            </div>

            <!-- Accent -->
            <p class="text-[10px] font-bold uppercase tracking-widest mb-2" style="color: var(--color-text-muted);">Accent</p>
            <div class="flex items-center gap-2">
              <button v-for="a in accents" :key="a.value"
                      class="flex-1 h-7 rounded-lg transition-all relative"
                      :style="`background: ${a.gradient};`"
                      @click="setAccent(a.value)">
                <span v-if="authStore.accentColor === a.value"
                      class="material-symbols-outlined text-white absolute inset-0 flex items-center justify-center"
                      style="font-size: 14px;">check</span>
              </button>
            </div>
          </div>

        </div>

        <!-- ── Right column ─────────────────────────────────────────────── -->
        <div class="col-span-2 flex flex-col gap-6">

          <!-- Stats strip -->
          <div ref="statsRef" class="grid grid-cols-3 gap-4">
            <div class="rounded-2xl p-5 text-center"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <div class="text-3xl font-bold" style="color: var(--color-primary);">{{ profile.stats.totalBatchesEndorsed }}</div>
              <div class="text-[10px] font-bold uppercase tracking-widest mt-1" style="color: var(--color-text-muted);">Batches Endorsed</div>
            </div>
            <div class="rounded-2xl p-5 text-center"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <div class="text-3xl font-bold" style="color: var(--color-primary);">{{ profile.stats.totalBatchesReceived }}</div>
              <div class="text-[10px] font-bold uppercase tracking-widest mt-1" style="color: var(--color-text-muted);">Batches Received</div>
            </div>
            <div class="rounded-2xl p-5 text-center"
                 style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">
              <div class="text-3xl font-bold" style="color: var(--color-primary);">{{ profile.stats.totalSpecimensCompleted }}</div>
              <div class="text-[10px] font-bold uppercase tracking-widest mt-1" style="color: var(--color-text-muted);">Specimens Completed</div>
            </div>
          </div>

          <!-- Section access -->
          <div class="rounded-2xl overflow-hidden"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">

            <div class="px-6 py-4" style="border-bottom: 1px solid var(--color-border);">
              <p class="text-sm font-bold" style="color: var(--color-text);">Section Access</p>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Active sections assigned to your account.</p>
            </div>

            <div v-if="!profile.sections.length"
                 class="flex flex-col items-center justify-center py-12 gap-2">
              <span class="material-symbols-outlined text-4xl" style="color: var(--color-text-muted);">apartment</span>
              <p class="text-sm" style="color: var(--color-text-muted);">No sections assigned.</p>
            </div>

            <table v-else class="w-full text-xs">
              <thead>
                <tr style="background-color: var(--color-surface-low); border-bottom: 1.5px solid var(--color-border);">
                  <th class="px-6 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Section</th>
                  <th class="px-6 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Type</th>
                  <th class="px-6 py-3 text-left text-[10px] font-bold uppercase tracking-widest" style="color: var(--color-text-muted);">Role</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="sec in profile.sections" :key="sec.sectionCode"
                    style="border-bottom: 1px solid var(--color-border);">
                  <td class="px-6 py-3 font-bold" style="color: var(--color-text);">
                    {{ sec.sectionName }}
                    <span class="ml-1.5 font-mono text-[10px]" style="color: var(--color-text-muted);">{{ sec.sectionCode }}</span>
                  </td>
                  <td class="px-6 py-3">
                    <span class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-widest"
                          :style="categoryStyle(sec.category)">
                      {{ categoryLabel(sec.category) }}
                    </span>
                  </td>
                  <td class="px-6 py-3">
                    <span class="text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-widest"
                          :style="roleStyle(sec.roleID)">
                      {{ roleLabel(sec.roleID) }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

        </div>
      </div>

    </div>

    <!-- Crop modal -->
    <Teleport to="body">
      <Transition name="fade">
        <div v-if="cropModal.visible"
             class="fixed inset-0 z-50 flex items-center justify-center"
             style="background-color: rgba(0,0,0,0.6);">
          <div class="rounded-2xl p-6 w-[420px] flex flex-col gap-5"
               style="background-color: var(--color-surface); border: 0.5px solid var(--color-border);">

            <div>
              <p class="font-bold text-sm" style="color: var(--color-text);">Crop Profile Photo</p>
              <p class="text-xs mt-0.5" style="color: var(--color-text-muted);">Drag to reposition · Use the slider to zoom.</p>
            </div>

            <!-- Crop viewport -->
            <div class="flex justify-center">
              <div ref="cropViewportRef"
                   class="relative overflow-hidden rounded-full cursor-grab active:cursor-grabbing select-none"
                   style="width: 200px; height: 200px; border: 2.5px solid var(--color-primary); box-shadow: 0 0 0 9999px rgba(0,0,0,0.45);"
                   @mousedown="onCropMouseDown"
                   @mousemove="onCropMouseMove"
                   @mouseup="onCropMouseUp"
                   @mouseleave="onCropMouseUp"
                   @touchstart.prevent="onCropTouchStart"
                   @touchmove.prevent="onCropTouchMove"
                   @touchend="onCropMouseUp">
                <img ref="cropImgRef"
                     :src="cropModal.src"
                     class="absolute pointer-events-none"
                     :style="`
                   width: ${cropState.imgW}px;
                   height: ${cropState.imgH}px;
                   left: ${cropState.x}px;
                   top: ${cropState.y}px;
                   transform-origin: top left;
                 `"
                     draggable="false" />
              </div>
            </div>

            <!-- Zoom slider -->
            <div class="flex items-center gap-3 px-1">
              <span class="material-symbols-outlined text-base" style="color: var(--color-text-muted); font-size: 16px;">zoom_out</span>
              <input type="range"
                     class="flex-1 accent-[var(--color-primary)]"
                     min="1" max="3" step="0.01"
                     v-model.number="cropState.zoom"
                     @input="onZoomChange" />
              <span class="material-symbols-outlined text-base" style="color: var(--color-text-muted); font-size: 16px;">zoom_in</span>
            </div>

            <div class="flex gap-3">
              <button class="flex-1 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all"
                      style="background-color: var(--color-surface-low); color: var(--color-text-muted);"
                      @click="closeCropModal">
                Cancel
              </button>
              <button class="flex-1 py-2.5 rounded-xl text-xs font-bold uppercase tracking-widest transition-all flex items-center justify-center gap-2"
                      style="background: var(--color-primary-gradient); color: #fff;"
                      :disabled="saving"
                      @click="confirmCrop">
                <span v-if="saving" class="material-symbols-outlined text-sm animate-spin">progress_activity</span>
                {{ saving ? 'Saving…' : 'Use This Photo' }}
              </button>
            </div>

          </div>
        </div>
      </Transition>
    </Teleport>

    <AlertModal :isVisible="alert.visible" :type="alert.type" :title="alert.title"
                :message="alert.message" @close="alert.visible = false" />

  </AppLayout>
</template>

<script setup>
  import { ref, computed, onMounted } from 'vue'
  import { gsap } from 'gsap'
  import AppLayout from '@/components/layout/AppLayout.vue'
  import AlertModal from '@/components/common/AlertModal.vue'
  import { useAuthStore } from '@/stores/authStore'
  import { userApi } from '@/api/userApi'
  import { settingsApi } from '@/api/settingsApi'

  const authStore = useAuthStore()

  // ── State ──────────────────────────────────────────────────────────────────
  const loading = ref(true)
  const saving  = ref(false)
  const profile = ref(null)

  const fileInputRef = ref(null)
  const statsRef     = ref(null)

  const alert = ref({ visible: false, type: 'success', title: '', message: '' })

  // ── Helpers ────────────────────────────────────────────────────────────────
  const userInitials = computed(() => {
    const name = authStore.userName ?? ''
    const parts = name.trim().split(/\s+/)
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase()
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
  })

  function formatDate(dt) {
    if (!dt) return '—'
    return new Date(dt).toLocaleDateString('en-PH', { month: 'long', day: 'numeric', year: 'numeric' })
  }

  function showAlert(type, title, message) {
    alert.value = { visible: true, type, title, message }
  }

  function categoryLabel(cat) {
    return { '1': 'Endorser', '2': 'Processing', '3': 'Lab Section' }[cat] ?? cat
  }

  function categoryStyle(cat) {
    const map = {
      '1': 'background-color: rgba(22,163,74,0.1); color: #15803d;',
      '2': 'background-color: rgba(59,130,246,0.1); color: #1d4ed8;',
      '3': 'background-color: rgba(236,72,153,0.1); color: #be185d;',
    }
    return map[cat] ?? 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  function roleLabel(roleID) {
    return { 1: 'Regular', 2: 'Team Lead' }[roleID] ?? 'Staff'
  }

  function roleStyle(roleID) {
    return roleID === 2
      ? 'background-color: rgba(245,158,11,0.1); color: #b45309;'
      : 'background-color: var(--color-surface-low); color: var(--color-text-muted);'
  }

  // ── Theme / Accent (mirrors AppTopbar) ────────────────────────────────────
  const themes  = [
    { value: 0, label: '☀️' },
    { value: 1, label: '🌙' },
    { value: 2, label: '🌫️' },
  ]
  const accents = [
    { value: 0, gradient: 'linear-gradient(135deg, #461599, #5e35b1)' },
    { value: 1, gradient: 'linear-gradient(135deg, #1a6bcc, #3b82f6)' },
    { value: 2, gradient: 'linear-gradient(135deg, #0f766e, #14b8a6)' },
    { value: 3, gradient: 'linear-gradient(135deg, #be123c, #e11d48)' },
  ]

  async function setTheme(value) {
    authStore.setTheme(value)
    try { await settingsApi.updateTheme(value) } catch { /* silent */ }
  }

  async function setAccent(value) {
    authStore.setAccentColor(value)
    try { await settingsApi.updateAccentColor(value) } catch { /* silent */ }
  }


  // ── Profile picture ────────────────────────────────────────────────────────
  const VIEWPORT = 200  // px — must match the template width/height

  const cropViewportRef = ref(null)
  const cropImgRef = ref(null)

  const cropModal = ref({
    visible: false,
    src: null,
  })

  const cropState = ref({
    zoom: 1,
    x: 0,
    y: 0,
    imgW: 0,
    imgH: 0,
    naturalW: 0,
    naturalH: 0,
    dragging: false,
    lastX: 0,
    lastY: 0,
  })

  function triggerFileInput() {
    fileInputRef.value?.click()
  }

  function onFileSelected(e) {
    const file = e.target.files?.[0]
    if (!file) return
    e.target.value = ''

    const reader = new FileReader()
    reader.onload = (ev) => {
      const img = new Image()
      img.onload = () => {
        cropModal.value = { visible: true, src: ev.target.result }
        initCropState(img.naturalWidth, img.naturalHeight)
      }
      img.src = ev.target.result
    }
    reader.readAsDataURL(file)
  }

  function initCropState(natW, natH) {
    // Scale image so its smaller dimension fills the viewport at zoom=1
    const scale = VIEWPORT / Math.min(natW, natH)
    const imgW = natW * scale
    const imgH = natH * scale

    // Center it
    const x = (VIEWPORT - imgW) / 2
    const y = (VIEWPORT - imgH) / 2

    cropState.value = {
      zoom: 1,
      x, y,
      imgW, imgH,
      naturalW: natW,
      naturalH: natH,
      dragging: false,
      lastX: 0,
      lastY: 0,
    }
  }

  function onZoomChange() {
    const s = cropState.value
    const zoom = s.zoom

    // Base dimensions at zoom=1
    const baseScale = VIEWPORT / Math.min(s.naturalW, s.naturalH)
    const newW = s.naturalW * baseScale * zoom
    const newH = s.naturalH * baseScale * zoom

    // Keep center point stable while zooming
    const centerX = VIEWPORT / 2
    const centerY = VIEWPORT / 2
    const ratioX = (centerX - s.x) / s.imgW
    const ratioY = (centerY - s.y) / s.imgH

    const newX = centerX - ratioX * newW
    const newY = centerY - ratioY * newH

    s.imgW = newW
    s.imgH = newH
    s.x = clampX(newX, newW)
    s.y = clampY(newY, newH)
  }

  function clampX(x, w) {
    return Math.min(0, Math.max(VIEWPORT - w, x))
  }

  function clampY(y, h) {
    return Math.min(0, Math.max(VIEWPORT - h, y))
  }

  // ── Mouse events ──────────────────────────────────────────────────────────
  function onCropMouseDown(e) {
    cropState.value.dragging = true
    cropState.value.lastX = e.clientX
    cropState.value.lastY = e.clientY
  }

  function onCropMouseMove(e) {
    if (!cropState.value.dragging) return
    const s = cropState.value
    const dx = e.clientX - s.lastX
    const dy = e.clientY - s.lastY
    s.x = clampX(s.x + dx, s.imgW)
    s.y = clampY(s.y + dy, s.imgH)
    s.lastX = e.clientX
    s.lastY = e.clientY
  }

  function onCropMouseUp() {
    cropState.value.dragging = false
  }

  // ── Touch events ──────────────────────────────────────────────────────────
  function onCropTouchStart(e) {
    const t = e.touches[0]
    cropState.value.dragging = true
    cropState.value.lastX = t.clientX
    cropState.value.lastY = t.clientY
  }

  function onCropTouchMove(e) {
    if (!cropState.value.dragging) return
    const s = cropState.value
    const t = e.touches[0]
    const dx = t.clientX - s.lastX
    const dy = t.clientY - s.lastY
    s.x = clampX(s.x + dx, s.imgW)
    s.y = clampY(s.y + dy, s.imgH)
    s.lastX = t.clientX
    s.lastY = t.clientY
  }

  // ── Confirm crop ──────────────────────────────────────────────────────────
  async function confirmCrop() {
    saving.value = true
    try {
      const s = cropState.value
      const canvas = document.createElement('canvas')
      canvas.width = 128
      canvas.height = 128

      const img = new Image()
      img.src = cropModal.value.src

      await new Promise(r => { img.complete ? r() : (img.onload = r) })

      const ctx = canvas.getContext('2d')
      // Map viewport coords back to natural image coords
      const scale = s.naturalW / s.imgW
      const srcX = (-s.x) * scale
      const srcY = (-s.y) * scale
      const srcSize = VIEWPORT * scale

      ctx.drawImage(img, srcX, srcY, srcSize, srcSize, 0, 0, 128, 128)

      const croppedBase64 = canvas.toDataURL('image/jpeg', 0.85)
      await userApi.updateProfilePicture(croppedBase64)
      authStore.setProfilePicture(croppedBase64)
      cropModal.value.visible = false
      showAlert('success', 'Photo updated', 'Your profile photo has been saved.')
    } catch (err) {
      showAlert('error', 'Save failed', err.response?.data?.message ?? 'Could not save profile photo.')
    } finally {
      saving.value = false
    }
  }

  function closeCropModal() {
    cropModal.value.visible = false
  }

  async function removePhoto() {
    saving.value = true
    try {
      await userApi.updateProfilePicture(null)
      authStore.setProfilePicture(null)
      showAlert('success', 'Photo removed', 'Your profile photo has been removed.')
    } catch (err) {
      showAlert('error', 'Failed', err.response?.data?.message ?? 'Could not remove photo.')
    } finally {
      saving.value = false
    }
  }


  // ── GSAP stats count-up ───────────────────────────────────────────────────
  function animateStats() {
    if (!statsRef.value || !profile.value) return
    const cards = statsRef.value.querySelectorAll('.text-3xl')
    const targets = [
      profile.value.stats.totalBatchesEndorsed,
      profile.value.stats.totalBatchesReceived,
      profile.value.stats.totalSpecimensCompleted,
    ]
    cards.forEach((el, i) => {
      const obj = { val: 0 }
      gsap.to(obj, {
        val: targets[i],
        duration: 1.2,
        ease: 'power2.out',
        delay: i * 0.08,
        onUpdate: () => { el.textContent = Math.round(obj.val).toLocaleString() },
      })
    })
  }

  // ── Load ───────────────────────────────────────────────────────────────────
  onMounted(async () => {
    try {
      profile.value = await userApi.getProfile()

      // Sync profile picture into store if not already set
      if (!authStore.profilePicture && profile.value.profilePicture) {
        authStore.setProfilePicture(profile.value.profilePicture)
      }
    } catch (err) {
      showAlert('error', 'Failed to load profile', err.response?.data?.message ?? 'An error occurred.')
    } finally {
      loading.value = false
      await new Promise(r => setTimeout(r, 50))
      animateStats()
    }
  })
</script>

<style scoped>
  .fade-enter-active, .fade-leave-active {
    transition: opacity 0.2s ease;
  }

  .fade-enter-from, .fade-leave-to {
    opacity: 0;
  }
</style>
