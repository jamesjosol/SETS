import { createRouter, createWebHistory } from 'vue-router'
import NProgress from 'nprogress'
import { useAuthStore } from '@/stores/authStore'
import Login from '../views/Login.vue'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: Login,
    meta: { requiresGuest: true }  // only for non-logged in users
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true }  // protected
  },
  {
    path: '/endorsement/new',
    name: 'NewEndorsement',
    component: () => import('../views/NewEndorsement.vue'),
    meta: { requiresAuth: true }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  NProgress.start()

  const authStore = useAuthStore()

  // redirect to dashboard if already logged in
  if (to.meta.requiresGuest && authStore.isAuthenticated) {
    return next('/dashboard')
  }

  // redirect to login if not authenticated
  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    return next('/')
  }

  next()
})

router.afterEach(() => {
  NProgress.done()
})

export default router
