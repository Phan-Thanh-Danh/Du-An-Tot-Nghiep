export async function exportBghToExcel(data, filename, sheetName) {
  const { exportToExcel } = await import('@/services/exportService.js')
  return exportToExcel(data, filename, sheetName)
}

export function printBghPage() {
  window.print()
}
