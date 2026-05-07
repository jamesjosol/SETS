import api from './axiosInstance'

const BASE_URL = '/api/Transaction'
const BATCH_URL = '/api/Batch'

export const transactionApi = {
  getOrder: (specimenNo) => api.get(`${BASE_URL}/hclaborder/${specimenNo}`),
  getJobOrder: (labNo) => api.get(`${BASE_URL}/joborder/${labNo}`).then(r => r.data),
  endorse: (payload) => api.post(`${BATCH_URL}/endorse`, payload),
  checkSpecimen: (specimenNo) => api.get(`${BATCH_URL}/checkspecimen/${specimenNo}`),
}
