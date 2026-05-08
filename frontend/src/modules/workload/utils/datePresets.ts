export type DatePreset = 'today' | 'tomorrow' | 'future7days' | 'last7days'

export function getDatePresetRange(preset: DatePreset) {
  const now = new Date()
  const start = new Date(now)
  const end = new Date(now)

  if (preset === 'tomorrow') {
    start.setDate(start.getDate() + 1)
    end.setDate(end.getDate() + 1)
  }

  if (preset === 'future7days') {
    start.setDate(start.getDate() + 1)
    end.setDate(end.getDate() + 7)
  }

  if (preset === 'last7days') {
    start.setDate(start.getDate() - 6)
  }

  return {
    dateStart: formatDate(start),
    dateEnd: formatDate(end),
  }
}

function formatDate(date: Date) {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}
