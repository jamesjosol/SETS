import api from './axiosInstance'

const BASE_URL = '/api/User'

export const themeApi = {
  update: (theme, accentColor) => api.put(`${BASE_URL}/theme`, { theme, accentColor })
}
