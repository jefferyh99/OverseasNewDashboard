export function formatRiskLabel(status: string) {
  return status === 'overdue' ? '已超时' : '即将超时'
}

export function formatTimeValueLabel(label: string) {
  if (!label) return ''

  const trimmed = label.trim()
  let prefix = ''
  let body = trimmed

  if (trimmed.startsWith('remaining ')) {
    prefix = '剩余 '
    body = trimmed.slice('remaining '.length)
  } else if (trimmed.startsWith('overdue ')) {
    prefix = '已超时 '
    body = trimmed.slice('overdue '.length)
  }

  return `${prefix}${body}`
    .replace(/(\d+(?:\.\d+)?)d\b/g, '$1 天')
    .replace(/(\d+(?:\.\d+)?)h\b/g, '$1 小时')
    .replace(/(\d+(?:\.\d+)?)m\b/g, '$1 分钟')
}
