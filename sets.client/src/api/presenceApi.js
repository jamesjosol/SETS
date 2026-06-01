import api from './axiosInstance'

const BASE_URL = '/api/Presence'

export const presenceApi = {
  getOnlineUsers: () => api.get(BASE_URL),
}
