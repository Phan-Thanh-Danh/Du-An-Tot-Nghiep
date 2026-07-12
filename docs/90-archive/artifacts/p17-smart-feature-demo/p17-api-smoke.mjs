// p17-api-smoke.mjs
// Node.js script to smoke test the Smart Timetable APIs.
// Run this via: node docs/artifacts/p17-smart-feature-demo/p17-api-smoke.mjs

import fs from 'fs'

const API_BASE = 'http://127.0.0.1:5097'
const CREDENTIALS = {
  email: 'p12test_staff01@lms.local',
  password: 'Test@123'
}

async function login() {
  const res = await fetch(`${API_BASE}/api/auth/login`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(CREDENTIALS)
  })
  if (!res.ok) throw new Error(`Login failed: ${res.status}`)
  const data = await res.json()
  console.log("Login response:", data)
  return data.accessToken || data.token
}

async function runSmoke() {
  const results = {
    courseAllocation: 'PENDING',
    smartTimetable: 'PENDING',
    errors: [],
    details: {}
  }

  try {
    console.log('1. Logging in...')
    const token = await login()
    const headers = {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    }

    console.log('2a. Fetching a course to get maHocKy and maDonVi...')
    const courseRes = await fetch(`${API_BASE}/api/courses?PageSize=1`, { headers })
    const courseData = await courseRes.json()
    const firstCourse = courseData.data?.items?.[0]
    const maHocKy = firstCourse ? firstCourse.maHocKy : 1
    const maDonVi = firstCourse ? firstCourse.maDonVi : 1
    const maMonHoc = firstCourse ? firstCourse.maMonHoc : 'M001'
    const maGiaoVien = firstCourse ? firstCourse.maGiaoVien : 'GV001'
    
    console.log(`Using maHocKy: ${maHocKy}, maDonVi: ${maDonVi}, maMonHoc: ${maMonHoc}, maGiaoVien: ${maGiaoVien}`)

    console.log('2b. Checking Course Allocation (bulk-assign)...')
    const bulkRes = await fetch(`${API_BASE}/api/courses/bulk-assign`, {
      method: 'POST',
      headers,
      body: JSON.stringify({
        maHocKy: maHocKy,
        maLopIds: [],
        maGiaoVien: maGiaoVien,
        maMonHoc: maMonHoc
      })
    })
    
    // We expect it to run or return a validation error (if empty), but not 404/500
    if (bulkRes.status === 404 || bulkRes.status >= 500) {
      results.courseAllocation = 'FAIL'
      results.errors.push(`bulk-assign returned ${bulkRes.status}`)
    } else {
      results.courseAllocation = 'PASS'
    }

    // 2. Check Smart Timetable
    console.log('3. Generating Smart Timetable Draft...')
    let generatedDraftId = null;
    let successfulMaHocKy = maHocKy;
    
    // Try multiple terms to find one with courses to schedule
    for (let term of [1, 2, 3, 4, 5, 6, maHocKy]) {
      console.log(`- Trying maHocKy: ${term}...`)
      const genRes = await fetch(`${API_BASE}/api/thoi-khoa-bieu/generate`, {
        method: 'POST',
        headers,
        body: JSON.stringify({
          maHocKy: term,
          maDonVi: maDonVi,
          tongTheHe: 10,
          kichThuocQuanThe: 10,
          maKhoaHocFilter: []
        })
      })

      if (genRes.ok) {
        const genData = await genRes.json()
        generatedDraftId = genData.data?.draftId
        if (generatedDraftId) {
          console.log(`  -> Generate success! draftId: ${generatedDraftId}`)
          successfulMaHocKy = term
          break;
        }
      } else if (genRes.status === 400) {
        console.log(`  -> Term ${term} has no courses to schedule.`)
      } else {
        throw new Error(`Generate failed: ${genRes.status} ${await genRes.text()}`)
      }
    }

    if (!generatedDraftId) {
      console.log('WARNING: Could not generate draft. Seed data might not have pending courses.')
    }
    results.details.generatedDraftId = generatedDraftId

    console.log('4. Listing Drafts...')
    const listRes = await fetch(`${API_BASE}/api/thoi-khoa-bieu/drafts?maHocKy=${successfulMaHocKy}&maDonVi=${maDonVi}`, { headers })
    if (!listRes.ok) throw new Error(`List drafts failed: ${listRes.status}`)
    const listData = await listRes.json()
    console.log(`- Found ${listData.data?.length} drafts`)

    console.log('5. Checking Conflicts Batch...')
    const checkRes = await fetch(`${API_BASE}/api/thoi-khoa-bieu/check-xung-dot-batch`, {
      method: 'POST',
      headers,
      body: JSON.stringify({
        maHocKy: successfulMaHocKy,
        maDonVi: maDonVi,
        items: [] // Empty list for smoke
      })
    })
    if (!checkRes.ok) throw new Error(`Check conflicts failed: ${checkRes.status}`)
    console.log(`- Check conflicts success`)

    if (generatedDraftId) {
      console.log('6. Publishing Draft...')
      const pubRes = await fetch(`${API_BASE}/api/thoi-khoa-bieu/publish`, {
        method: 'POST',
        headers,
        body: JSON.stringify({ draftId: generatedDraftId })
      })
      if (!pubRes.ok) throw new Error(`Publish failed: ${pubRes.status} ${await pubRes.text()}`)
      console.log(`- Publish success`)
      results.smartTimetable = 'PASS'
    } else {
      // If we couldn't generate a draft, we still mark it as PASS_WITH_WARNINGS if API is verified
      results.smartTimetable = 'PASS_WITH_WARNINGS (No Seed Data)'
    }

  } catch (err) {
    console.error('Smoke test error:', err.message)
    results.smartTimetable = 'FAIL'
    results.errors.push(err.message)
  }

  fs.writeFileSync('docs/artifacts/p17-smart-feature-demo/p17-api-smoke-results.json', JSON.stringify(results, null, 2))
  console.log('\nResults saved to p17-api-smoke-results.json')
  console.log('FINAL STATUS:', results.smartTimetable === 'PASS' && results.courseAllocation === 'PASS' ? 'SUCCESS' : 'FAILED')
}

runSmoke()
