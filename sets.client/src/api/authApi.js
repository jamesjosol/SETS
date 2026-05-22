// src/api/authApi.js
import api from './axiosInstance'

const BASE_URL = '/api/Auth'

export const authApi = {
  login: (data) => api.post(`${BASE_URL}/login`, data),
  getPCInfo: () => api.get(`${BASE_URL}/pc-info`),
  logout: () => api.post(`${BASE_URL}/logout`),
  contingencyLogin: (data) => api.post(`${BASE_URL}/contingency-login`, data),
  getVersion: () => api.get(`${BASE_URL}/version`),
}
