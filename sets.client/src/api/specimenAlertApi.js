import api from './axiosInstance'

const BASE_URL = '/api/SpecimenAlert'

export const specimenAlertApi = {

  setAlert: (data) =>
    api.post(`${BASE_URL}/set`, data).then(r => r.data),

  clearAlert: (data) =>
    api.post(`${BASE_URL}/clear`, data).then(r => r.data),

}
