process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';
(async () => {
  const loginRes = await fetch('https://localhost:7150/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'admin@edulms.local', password: '123456' })
  });
  const loginData = await loginRes.json();
  const token = loginData.data?.accessToken;
  if (!token) {
    console.log('Login failed:', loginData);
    return;
  }
  const subRes = await fetch('https://localhost:7150/api/master-data/subjects', {
    headers: { 'Authorization': 'Bearer ' + token }
  });
  const subData = await subRes.json();
  console.log(JSON.stringify(subData, null, 2));
})();
