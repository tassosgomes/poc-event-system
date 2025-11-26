export const formatReleaseDate = (value?: string | null) => {
  if (!value) {
    return '-'
  }

  const [year, month, day] = value.split('-')
  if (!year || !month || !day) {
    return value
  }

  return `${day}/${month}/${year}`
}

export const formatViews = (value?: number | null) => {
  if (value === null || value === undefined) {
    return '--'
  }

  return new Intl.NumberFormat('pt-BR').format(value)
}
