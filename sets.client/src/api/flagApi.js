import api from './axiosInstance'

const BASE_URL = '/api/Flag'

export const flagApi = {
  flagSpecimen: (data) =>
    api.patch(`${BASE_URL}/specimen`, data).then(r => r.data),

  unflagSpecimen: (data) =>
    api.patch(`${BASE_URL}/specimen/unflag`, data).then(r => r.data),
}
