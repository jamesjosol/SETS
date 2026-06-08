import axiosInstance from './axiosInstance'

const BASE = '/api/changelog'

export const changelogApi = {
  // Admin: get all entries
  getAll: () =>
    axiosInstance.get(BASE).then(r => r.data),

  // Any user: get latest entry + existence flag
  getLatest: () =>
    axiosInstance.get(`${BASE}/latest`).then(r => r.data),

  // Any user: check if current user has seen a version
  hasSeen: (version) =>
    axiosInstance.get(`${BASE}/seen`, { params: { version } }).then(r => r.data),

  // Admin: create entry
  create: (payload) =>
    axiosInstance.post(BASE, payload).then(r => r.data),

  // Admin: delete entry
  delete: (id) =>
    axiosInstance.delete(`${BASE}/${id}`).then(r => r.data),

  // Any user: mark version as seen
  markSeen: (version) =>
    axiosInstance.post(`${BASE}/seen`, { version }).then(r => r.data),
}
