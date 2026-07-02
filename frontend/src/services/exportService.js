import * as XLSX from 'xlsx'

export function exportToExcel(data, filename = 'export.xlsx', sheetName = 'Sheet1') {
  if (!data || data.length === 0) return
  const ws = XLSX.utils.json_to_sheet(data)
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, sheetName)
  XLSX.writeFile(wb, filename)
}

export function triggerPrint() {
  window.print()
}
