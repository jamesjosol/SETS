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
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  {
    path: '/endorsement/new',
    name: 'NewEndorsement',
    component: () => import('../views/NewEndorsement.vue'),
    meta: { requiresAuth: true, category: '1' }
  },

  // ── Receiver (category 2) ──────────────────────────────────────────
  {
    path: '/receiver/dashboard',
    name: 'ReceiverDashboard',
    component: () => import('../views/ReceiverDashboard.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/receive',
    name: 'ReceiveEndorsement',
    component: () => import('../views/ReceiveEndorsement.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/incoming',
    name: 'IncomingSpecimens',
    component: () => import('../views/IncomingSpecimens.vue'),
    meta: { requiresAuth: true, category: '2' }
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
