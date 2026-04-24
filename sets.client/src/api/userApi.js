import api from './axiosInstance'

export const userApi = {
  // GET all users with section assignments
  getAll: () =>
    api.get('/api/user'),

  // GET all user from hclab
  getHCLABUsers: (param) =>
    api.get('/api/user/hclab-user', { params: { param } }),

  // POST register new user
  add: (data) =>
    api.post('/api/user', data),

  // PUT update user info (name, isAdmin, active)
  update: (userID, data) =>
    api.put(`/api/user/${userID}`, data),

  // PATCH toggle active/inactive
  toggle: (userID) =>
    api.patch(`/api/user/${userID}/toggle`),

  // PUT replace all section assignments
  updateSections: (userID, data) =>
    api.put(`/api/user/${userID}/sections`, data),
}
