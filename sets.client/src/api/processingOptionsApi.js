import api from './axiosInstance'

export const processingOptionsApi = {
  get: () => api.get('/api/processingoptions').then(r => r.data),
  upsert: (data) => api.put('/api/processingoptions', data),
}
