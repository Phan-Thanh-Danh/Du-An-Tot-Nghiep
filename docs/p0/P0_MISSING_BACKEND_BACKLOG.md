# P0 Backend Backlog - Missing & Partial Endpoints

This document tracks endpoints that are either completely missing or partially implemented but lacking core business logic (e.g. `NotImplementedException`, stub methods, or unverified database access).

## Missing Capabilities

### 1. Content Council publishes a quiz
- **Capability ID**: CAP-QZ-001
- **Role**: `HoiDongQuanLyNoiDung`
- **Expected Route**: `POST /api/quizzes/{id}/publish`
- **Issue**: Endpoint is completely missing from the backend routing and controller logic.

## Partial / Stub Capabilities

### 2. Parent initiates payment
- **Capability ID**: CAP-PAY-002
- **Role**: `Parent`
- **Current Route**: `POST /api/parent/payment`
- **Controller**: `ParentController`
- **Issue**: Hoàn thiện payment gateway transaction và redirect flow. Mặc dù endpoint có khai báo, logic phía trong chưa hoàn tất các side-effect về cổng thanh toán và xác nhận giao dịch.
