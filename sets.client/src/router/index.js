import { createRouter, createWebHistory } from 'vue-router'
import NProgress from 'nprogress'
import { useAuthStore } from '@/stores/authStore'
import Login from '../views/Login.vue'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: Login,
    meta: { requiresGuest: true }
  },

  // ── Endorser (category 1) ──────────────────────────────────────────
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/endorser/Dashboard.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  {
    path: '/endorsement/new',
    name: 'NewEndorsement',
    component: () => import('../views/endorser/NewEndorsement.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  {
    path: '/endorsements',
    name: 'Endorsements',
    component: () => import('../views/endorser/Endorsements.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  // ── Receiver (category 2) ──────────────────────────────────────────
  {
    path: '/receiver/dashboard',
    name: 'ReceiverDashboard',
    component: () => import('../views/receiver/ReceiverDashboard.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/receive',
    name: 'ReceiveEndorsement',
    component: () => import('../views/receiver/ReceiveEndorsement.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/incoming',
    name: 'IncomingSpecimens',
    component: () => import('../views/receiver/IncomingSpecimens.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/received-batches',
    name: 'ReceivedBatches',
    component: () => import('../views/receiver/ReceivedBatches.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  // ── Runner ───────────────────────────────────
  {
    path: '/runner/dashboard',
    name: 'RunnerDashboard',
    component: () => import('../views/runner/RunnerDashboard.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/pending',
    name: 'PendingSpecimens',
    component: () => import('../views/runner/PendingSpecimens.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/saved', name: 'SavedSpecimens',
    component: () => import('../views/runner/SavedSpecimens.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/assign', name: 'AssignRMT',
    component: () => import('../views/runner/AssignRMT.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  // ── Catch-all: redirect to login ───────────────────────────────────
  {
    path: '/:pathMatch(.*)*',
    redirect: '/'
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  NProgress.start()

  const authStore = useAuthStore()

  // 1. Guest-only routes — redirect to home if already logged in
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    return next(getDefaultRoute(authStore))
  }

  // 2. Protected routes — redirect to login if not authenticated
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next('/')
  }

  // 3. Category guard — admin bypasses, others must match
  if (to.meta.requiresAuth && to.meta.category && authStore.isAuthenticated) {
    if (!authStore.isAdmin && authStore.sectionCategory !== to.meta.category) {
      // Redirect to their own default route instead of blocking
      return next(getDefaultRoute(authStore))
    }
  }

  next()
})

router.afterEach(() => {
  NProgress.done()
})

// ── Helper: resolve default route per category ─────────────────────────────
export function getDefaultRoute(authStore) {
  if (authStore.isAdmin) return '/dashboard'
  switch (authStore.sectionCategory) {
    case '1': return '/dashboard'
    case '2': return '/receiver/dashboard'
    case '3': return '/runner/dashboard'
    default: return '/'
  }
}

export default router
