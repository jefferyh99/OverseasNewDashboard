export function buildCsv(
  headers: string[],
  rows: Array<Array<string | number | null | undefined>>,
) {
  const escapeCell = (value: string | number | null | undefined) =>
    `"${String(value ?? '').replaceAll('"', '""')}"`

  const headerLine = headers.map(escapeCell).join(',')
  const rowLines = rows.map((row) => row.map(escapeCell).join(','))
  return [headerLine, ...rowLines].join('\n')
}

export function downloadCsv(options: {
  fileName: string
  headers: string[]
  rows: Array<Array<string | number | null | undefined>>
}) {
  const csv = buildCsv(options.headers, options.rows)
  const blob = new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8;' })
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')

  anchor.href = url
  anchor.download = options.fileName
  document.body.appendChild(anchor)
  anchor.click()
  document.body.removeChild(anchor)

  URL.revokeObjectURL(url)
}
