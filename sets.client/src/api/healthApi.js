import api from './axiosInstance'

const HEALTH_URL = '/api/health'

export const healthApi = {
  // Checks server reachability + MSSQL latency
  ping: () => {
    const start = performance.now()
    return api.get(`${HEALTH_URL}/ping`).then(r => ({
      hostLatencyMs: Math.round(performance.now() - start),
      db: r.data.db,
    }))
  },

  // Checks HCLAB (Oracle) connectivity
  hclab: () => api.get(`${HEALTH_URL}/hclab`).then(r => r.data),
}
