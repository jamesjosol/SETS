import api from './axiosInstance'

const BASE_URL = '/api/Audit'

export const auditApi = {
  getByUser: (userID, dateFrom, dateTo) =>
    api.get(`${BASE_URL}/by-user`, { params: { userID, dateFrom, dateTo } }).then(r => r.data),

  getBySpecimen: (specimenNo) =>
    api.get(`${BASE_URL}/by-specimen`, { params: { specimenNo } }).then(r => r.data),

  getByBatch: (batchNo) =>
    api.get(`${BASE_URL}/by-batch`, { params: { batchNo } }).then(r => r.data),
}
