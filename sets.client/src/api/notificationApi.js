import api from './axiosInstance'

const BASE_URL = '/api/notification'

export const notificationApi = {
  getForUser: () =>
    api.get(BASE_URL).then(r => r.data),

  markRead: (notifID) =>
    api.patch(`${BASE_URL}/${notifID}/read`).then(r => r.data),

  markAllRead: () =>
    api.patch(`${BASE_URL}/read-all`).then(r => r.data),

  reportMiddlewareIssue: () =>
    api.post(`${BASE_URL}/middleware-issue`).then(r => r.data),
}
