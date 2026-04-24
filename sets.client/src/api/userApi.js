import axios from 'axios'

export const userApi = {
  // GET all users with section assignments
  getAll: () =>
    axios.get('/api/user'),

  // GET all user from hclab
  getHCLABUsers: (param) =>
    axios.get('/api/user/hclab-user', { params: { param } }),

  // POST register new user
  add: (data) =>
    axios.post('/api/user', data),

  // PUT update user info (name, isAdmin, active)
  update: (userID, data) =>
    axios.put(`/api/user/${userID}`, data),

  // PATCH toggle active/inactive
  toggle: (userID) =>
    axios.patch(`/api/user/${userID}/toggle`),

  // PUT replace all section assignments
  updateSections: (userID, data) =>
    axios.put(`/api/user/${userID}/sections`, data),
}
