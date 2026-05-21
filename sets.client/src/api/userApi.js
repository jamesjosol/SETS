import api from './axiosInstance'

export const userApi = {
  // ── Admin ────────────────────────────────────────────────────────────────

  getAll: () =>
    api.get('/api/user'),

  getHCLABUsers: (param) =>
    api.get('/api/user/hclab-user', { params: { param } }),

  add: (data) =>
    api.post('/api/user', data),

  update: (userID, data) =>
    api.put(`/api/user/${userID}`, data),

  toggle: (userID) =>
    api.patch(`/api/user/${userID}/toggle`),

  updateSections: (userID, data) =>
    api.put(`/api/user/${userID}/sections`, data),

  delete: (userID) =>
    api.delete(`/api/user/${userID}`),

  // ── TL (scoped to session section) ───────────────────────────────────────

  tlGetAll: () =>
    api.get('/api/user/tl'),

  tlAdd: (data) =>
    api.post('/api/user/tl', data),

  tlUpdateRole: (userID, roleID) =>
    api.put(`/api/user/tl/${userID}/role`, { roleID }),

  tlToggle: (userID) =>
    api.patch(`/api/user/tl/${userID}/toggle`),

  tlRemoveFromSection: (userID) =>
    api.delete(`/api/user/tl/${userID}/section`),

  // ── Profile ───────────────────────────────────────────────────────────────

  getProfile: () =>
    api.get('/api/user/profile').then(r => r.data),

  updateProfilePicture: (base64Image) =>
    api.put('/api/user/profile/picture', { profilePicture: base64Image }).then(r => r.data),

}
