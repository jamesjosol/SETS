import api from './axiosInstance'

const BASE_URL = '/api/User'

export const themeApi = {
  update: (theme) => api.put(`${BASE_URL}/theme`, { theme })
}
