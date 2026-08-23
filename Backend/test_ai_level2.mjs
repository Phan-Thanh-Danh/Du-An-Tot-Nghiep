async function run() {
  const loginRes = await fetch('http://localhost:5097/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      email: 'student.cntt01@lms.local',
      password: 'Test@123'
    })
  });
  const loginData = await loginRes.json();
  const token = loginData.accessToken;
  console.log('Login OK. User:', loginData.user?.hoTen, '- Token obtained.');

  const headers = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  };

  const tests = [
    { title: 'TEST 1: LỊCH HỌC / THỜI KHÓA BIỂU', prompt: 'Lịch học tuần này của tôi như thế nào?' },
    { title: 'TEST 2: CHUYÊN CẦN / ĐIỂM DANH', prompt: 'Tình hình điểm danh và chuyên cần của tôi ra sao?' },
    { title: 'TEST 3: BÀI TẬP & DEADLINE', prompt: 'Tôi còn bài tập hay bài kiểm tra nào chưa nộp không?' },
    { title: 'TEST 4: BẢNG ĐIỂM & GPA', prompt: 'Điểm số của tôi thế nào?' },
    { title: 'TEST 5: TƯ VẤN HỌC THUẬT QUA OLLAMA TỪ BẢNG ĐIỂM THẬT', prompt: 'Dựa vào kết quả học tập của tôi, môn nào tôi đạt điểm cao nhất và tôi có cần học lại môn nào không?' }
  ];

  for (const t of tests) {
    console.log(`\n=================== ${t.title} ===================`);
    console.log(`User query: "${t.prompt}"`);
    const start = Date.now();
    const res = await fetch('http://localhost:5097/api/ai/chat', {
      method: 'POST',
      headers,
      body: JSON.stringify({ message: t.prompt })
    });
    const data = await res.json();
    const duration = Date.now() - start;
    console.log(`AI Response (${duration} ms - Status: ${res.status}):\n`, data.data ? data.data.answer : JSON.stringify(data));
  }
}

run().catch(console.error);
