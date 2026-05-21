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
  {
    path: '/settings',
    name: 'EndorserSettings',
    component: () => import('../views/endorser/EndorserSettings.vue'),
    meta: { requiresAuth: true, category: '1', requiresTL: true }
  },
  {
    path: '/reports',
    name: 'EndorserReports',
    component: () => import('../views/shared/Reports.vue'),
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
  {
    path: '/receiver/issues-log',
    name: 'SpecimenIssuesLog',
    component: () => import('../views/receiver/SpecimenIssuesLog.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/receiver/settings',
    name: 'ReceiverSettings',
    component: () => import('../views/receiver/ReceiverSettings.vue'),
    meta: { requiresAuth: true, category: '2', requiresTL: true }
  },
  {
    path: '/receiver/reports',
    name: 'ReceiverReports',
    component: () => import('../views/shared/Reports.vue'),
    meta: { requiresAuth: true, category: '2' }
  },

  // ── Runner (category 3) ───────────────────────────────────────────
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
    path: '/runner/scheduled', name: 'ScheduledSpecimens',
    component: () => import('../views/runner/ScheduledSpecimens.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/assign', name: 'AssignRMT',
    component: () => import('../views/runner/AssignRMT.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/running',
    name: 'RunningSpecimens',
    component: () => import('../views/runner/RunningSpecimens.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/completed',
    name: 'CompletedSpecimens',
    component: () => import('../views/runner/CompletedSpecimens.vue'),
    meta: { requiresAuth: true, category: '3' }
  },
  {
    path: '/runner/settings',
    name: 'RunnerSettings',
    component: () => import('../views/runner/RunnerSettings.vue'),
    meta: { requiresAuth: true, category: '3', requiresTL: true }
  },
  {
    path: '/runner/reports',
    name: 'RunnerReports',
    component: () => import('../views/shared/Reports.vue'),
    meta: { requiresAuth: true, category: '3' }
  },

  // ── Shared ────────────────────────────────────────────────────────────
  {
    path: '/audit-trail',
    name: 'AuditTrail',
    component: () => import('../views/shared/AuditTrail.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  {
    path: '/receiver/audit-trail',
    name: 'ReceiverAuditTrail',
    component: () => import('../views/shared/AuditTrail.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  {
    path: '/runner/audit-trail',
    name: 'RunnerAuditTrail',
    component: () => import('../views/shared/AuditTrail.vue'),
    meta: { requiresAuth: true, category: '3' }
  },

  // ── Admin ─────────────────────────────────────────────────────────────
  {
    path: '/admin/settings',
    name: 'AdminSettings',
    component: () => import('../views/admin/AdminSettings.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  {
    path: '/admin/reports',
    name: 'AdminReports',
    component: () => import('../views/shared/Reports.vue'),
    meta: { requiresAuth: true, requiresAdmin: true }
  },
  // ── Contingency ───────────────────────────────────────────────────────
  {
    path: '/contingency/endorse',
    name: 'ContingencyEndorse',
    component: () => import('../views/contingency/ContingencyEndorse.vue'),
    meta: { requiresAuth: true, category: '1' }
  },
  {
    path: '/receiver/contingency',
    name: 'ContingencyReceive',
    component: () => import('../views/contingency/ContingencyReceive.vue'),
    meta: { requiresAuth: true, category: '2' }
  },
  // ── Shared ────────────────────────────────────────────────────────────
  {
    path: '/profile',
    name: 'MyProfile',
    component: () => import('../views/shared/MyProfile.vue'),
    meta: { requiresAuth: true }
  },
  // ── Catch-all ─────────────────────────────────────────────────────────
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

  // 1. Guest-only routes
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    return next(getDefaultRoute(authStore))
  }

  // 2. Protected routes
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next('/')
  }

  // 3. Admin-only routes
  if (to.meta.requiresAdmin && !authStore.isAdmin) {
    return next(getDefaultRoute(authStore))
  }

  // 4. TL-only routes — roleID must be 2; admin bypasses
  if (to.meta.requiresTL && !authStore.isAdmin && authStore.roleID !== 2) {
    return next(getDefaultRoute(authStore))
  }

  // 5. Category guard — admin bypasses, others must match
  if (to.meta.requiresAuth && to.meta.category && authStore.isAuthenticated) {
    if (!authStore.isAdmin && authStore.sectionCategory !== to.meta.category) {
      return next(getDefaultRoute(authStore))
    }
  }

  next()
})

router.afterEach(() => {
  NProgress.done()
})

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
