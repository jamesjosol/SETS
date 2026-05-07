import api from './axiosInstance'

export const pcApi = {
  // ── Admin ────────────────────────────────────────────────────────────────

  getAll: () =>
    api.get('/api/pc'),

  add: (data) =>
    api.post('/api/pc', data),

  update: (id, data) =>
    api.put(`/api/pc/${id}`, data),

  toggle: (id) =>
    api.patch(`/api/pc/${id}/toggle`),

  updateSections: (id, data) =>
    api.put(`/api/pc/${id}/sections`, data),

  delete: (id) =>
    api.delete(`/api/pc/${id}`),

  // ── TL (scoped to session section) ───────────────────────────────────────

  tlGetAll: () =>
    api.get('/api/pc/tl'),

  tlAdd: (data) =>
    api.post('/api/pc/tl', data),

  tlUpdate: (id, data) =>
    api.put(`/api/pc/tl/${id}`, data),

  tlToggle: (id) =>
    api.patch(`/api/pc/tl/${id}/toggle`),

  tlRemoveSection: (id) =>
    api.delete(`/api/pc/tl/${id}/section`),
}
