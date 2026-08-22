$loginResponse = Invoke-RestMethod -Uri "http://localhost:5097/api/auth/login" -Method Post -ContentType "application/json" -Body '{"email":"student.cntt01@lms.local","password":"Test@123"}'
$token = $loginResponse.data.token
$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "=================== TEST 1: LICH HOC ===================" -ForegroundColor Cyan
$body1 = '{"message":"Lịch học tuần này của tôi như thế nào?"}'
$r1 = Invoke-RestMethod -Uri "http://localhost:5097/api/ai/chat" -Method Post -Headers $headers -ContentType "application/json" -Body $body1
Write-Host $r1.answer
Write-Host "Processing time: $($r1.processingTimeMs) ms" -ForegroundColor Green

Write-Host "`n=================== TEST 2: CHUYEN CAN ===================" -ForegroundColor Cyan
$body2 = '{"message":"Tình hình điểm danh và chuyên cần của tôi ra sao?"}'
$r2 = Invoke-RestMethod -Uri "http://localhost:5097/api/ai/chat" -Method Post -Headers $headers -ContentType "application/json" -Body $body2
Write-Host $r2.answer
Write-Host "Processing time: $($r2.processingTimeMs) ms" -ForegroundColor Green

Write-Host "`n=================== TEST 3: BAI TAP & DEADLINE ===================" -ForegroundColor Cyan
$body3 = '{"message":"Tôi còn bài tập hay bài kiểm tra nào chưa nộp không?"}'
$r3 = Invoke-RestMethod -Uri "http://localhost:5097/api/ai/chat" -Method Post -Headers $headers -ContentType "application/json" -Body $body3
Write-Host $r3.answer
Write-Host "Processing time: $($r3.processingTimeMs) ms" -ForegroundColor Green

Write-Host "`n=================== TEST 4: BANG DIEM & GPA ===================" -ForegroundColor Cyan
$body4 = '{"message":"Điểm số và GPA của tôi thế nào?"}'
$r4 = Invoke-RestMethod -Uri "http://localhost:5097/api/ai/chat" -Method Post -Headers $headers -ContentType "application/json" -Body $body4
Write-Host $r4.answer
Write-Host "Processing time: $($r4.processingTimeMs) ms" -ForegroundColor Green
