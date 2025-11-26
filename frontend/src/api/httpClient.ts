import axios from 'axios'
import { appConfig } from '../config'

export const httpClient = axios.create({
  baseURL: appConfig.apiBaseUrl || undefined,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10_000,
})
