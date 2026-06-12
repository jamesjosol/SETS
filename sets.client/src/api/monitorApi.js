import axiosInstance from './axiosInstance'

const BASE = '/api/monitor'

export const monitorApi = {
  // Developer: SETS-only server monitor snapshot (proxied from local middleware)
  // includeRequests = false skips the in-flight request list (cheaper poll)
  getSets: (includeRequests = true) =>
    axiosInstance
      .get(`${BASE}/sets`, { params: { requests: includeRequests ? 1 : 0 } })
      .then(r => r.data),
}
