/**
 * P17 Smart Feature Demo Smoke Script (Playwright/Puppeteer Stub)
 * 
 * Mục đích: Script này tự động hóa hoặc hướng dẫn kiểm thử luồng Smart Timetable
 * và Smart Course Allocation trên UI, xác nhận các endpoint đã được tích hợp đúng
 * và các mutation an toàn chạy thành công.
 * 
 * Lưu ý: API được gọi trong script này phải sử dụng dữ liệu P17_DEMO_* từ db.
 */

const { chromium } = require('playwright'); // Hoặc puppeteer, tùy thuộc môi trường.
const fs = require('fs');

async function runSmartFeatureSmoke() {
    console.log("🚀 Bắt đầu P17 Smart Feature Smoke Test...");
    const results = {
        smartTimetable: 'PENDING',
        courseAllocation: 'PENDING',
        errors: []
    };

    try {
        const browser = await chromium.launch({ headless: true });
        const context = await browser.newContext();
        const page = await context.newPage();

        // Giả lập login
        await page.goto('http://localhost:5173/login');
        await page.fill('input[type="email"]', 'p17demo_giaovu@lms.local');
        await page.fill('input[type="password"]', 'Test@123');
        await page.click('button[type="submit"]');
        await page.waitForURL('**/giaovu/dashboard');

        // 1. Kiểm tra Smart Course Allocation
        console.log("▶️ Kiểm thử Smart Course Allocation...");
        await page.goto('http://localhost:5173/giaovu/courses');
        
        // Bắt request POST /api/courses/bulk-assign
        const allocationPromise = page.waitForResponse(response => 
            response.url().includes('/api/courses/bulk-assign') && response.request().method() === 'POST'
        );
        
        // Tìm và click nút Bulk Assign/Gợi ý tự động
        // (Trong môi trường thực, chúng ta sẽ select rows trước khi click)
        const bulkAssignBtn = await page.$('text="Phân công hàng loạt"');
        if (bulkAssignBtn) {
            await bulkAssignBtn.click();
            await page.click('text="Thực hiện phân công"');
            const allocationRes = await allocationPromise;
            if (allocationRes.ok()) {
                results.courseAllocation = 'PASS';
                console.log("✅ Smart Course Allocation PASS");
            } else {
                results.courseAllocation = 'FAIL';
                results.errors.push(`Course allocation trả về status ${allocationRes.status()}`);
            }
        } else {
            console.warn("⚠️ Không tìm thấy nút Phân công hàng loạt (có thể cần mock selector chính xác hơn)");
            results.courseAllocation = 'MANUAL_VERIFY_NEEDED';
        }

        // 2. Kiểm tra Smart Timetable
        console.log("▶️ Kiểm thử Smart Timetable...");
        await page.goto('http://localhost:5173/giaovu/schedule');
        
        // Bắt request POST /api/thoi-khoa-bieu/generate
        const generatePromise = page.waitForResponse(response => 
            response.url().includes('/api/thoi-khoa-bieu/generate') && response.request().method() === 'POST'
        );
        
        const generateBtn = await page.$('text="Xếp lịch tự động"');
        if (generateBtn) {
            await generateBtn.click();
            const generateRes = await generatePromise;
            if (generateRes.ok()) {
                console.log("✅ Smart Timetable Generate PASS");
                
                // Qua màn Pending để test Publish
                await page.goto('http://localhost:5173/giaovu/schedule/pending');
                const publishPromise = page.waitForResponse(response => 
                    response.url().includes('/api/thoi-khoa-bieu/publish') && response.request().method() === 'POST'
                );
                
                // Click the first pending draft
                const firstDraft = await page.$('.surface-card.cursor-pointer');
                if (firstDraft) {
                    await firstDraft.click();
                    const publishBtn = await page.$('text="Phê duyệt & Xuất bản"');
                    if (publishBtn) {
                        await publishBtn.click();
                        const publishRes = await publishPromise;
                        if (publishRes.ok()) {
                            results.smartTimetable = 'PASS';
                            console.log("✅ Smart Timetable Publish PASS");
                        } else {
                            results.smartTimetable = 'FAIL';
                            results.errors.push(`Publish draft trả về status ${publishRes.status()}`);
                        }
                    }
                }
            } else {
                results.smartTimetable = 'FAIL';
                results.errors.push(`Generate draft trả về status ${generateRes.status()}`);
            }
        } else {
            console.warn("⚠️ Không tìm thấy nút Xếp lịch tự động");
            results.smartTimetable = 'MANUAL_VERIFY_NEEDED';
        }

        await browser.close();
    } catch (e) {
        console.error("❌ Smoke test gặp lỗi:", e);
        results.errors.push(e.message);
    }

    // Ghi kết quả
    fs.writeFileSync('docs/artifacts/p17-smart-feature-demo/smoke-results-p17.json', JSON.stringify(results, null, 2));
    console.log("📊 Đã ghi kết quả vào smoke-results-p17.json");
}

if (require.main === module) {
    runSmartFeatureSmoke();
}

module.exports = { runSmartFeatureSmoke };
