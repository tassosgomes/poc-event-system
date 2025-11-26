const normalizeBaseUrl = (value?: string) => {
  if (!value) {
    return ''
  }

  return value.endsWith('/') ? value.slice(0, -1) : value
}

export const appConfig = {
  apiBaseUrl: normalizeBaseUrl(import.meta.env.VITE_API_BASE_URL as string | undefined),
}
