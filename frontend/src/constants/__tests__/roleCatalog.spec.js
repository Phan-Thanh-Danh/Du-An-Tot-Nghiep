import { describe, it, expect } from 'vitest'
import { normalizeRole } from '../roleCatalog'

describe('roleCatalog', () => {
  describe('normalizeRole', () => {
    it('normalizes standard roles to canonical casing', () => {
      expect(normalizeRole('admin')).toBe('Admin')
      expect(normalizeRole('Student')).toBe('Student')
      expect(normalizeRole('teacher')).toBe('Teacher')
    })

    it('normalizes aliases to exact canonical casing', () => {
      expect(normalizeRole('staff')).toBe('AcademicStaff')
      expect(normalizeRole('bgh')).toBe('Principal')
      expect(normalizeRole('contentcouncil')).toBe('HoiDongQuanLyNoiDung')
      expect(normalizeRole('lecturer')).toBe('Teacher')
    })

    it('handles unexpected formats gracefully', () => {
      expect(normalizeRole('  StAff  ')).toBe('AcademicStaff')
      expect(normalizeRole('UNKNOWN')).toBe('unknown')
      expect(normalizeRole(null)).toBe('')
      expect(normalizeRole(undefined)).toBe('')
    })
  })
})
