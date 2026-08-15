$conn = New-Object System.Data.SqlClient.SqlConnection('Server=DELL\SQLEXPRESS02;Database=LMS;Integrated Security=True;TrustServerCertificate=True;')
$conn.Open()
Write-Host "Connected to DB"
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'SELECT COUNT(*) FROM dbo.DiemSo'
$count = $cmd.ExecuteScalar()
Write-Host "Total grades in DiemSo: $count"

$cmd.CommandText = 'SELECT COUNT(*) FROM dbo.NguoiDung WHERE vai_tro_chinh IN (''sinh_vien'', ''hoc_sinh'')'
$users = $cmd.ExecuteScalar()
Write-Host "Total students in NguoiDung: $users"

$cmd.CommandText = 'SELECT TOP 5 ma_hoc_ky, COUNT(*) FROM dbo.DiemSo GROUP BY ma_hoc_ky'
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Host "$($reader[0]) - $($reader[1])"
}
$conn.Close()
