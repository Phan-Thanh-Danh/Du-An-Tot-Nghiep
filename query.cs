using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        string connStr = "Server=DELL\\SQLEXPRESS02;Database=LMS;Integrated Security=True;TrustServerCertificate=True;";
        using (SqlConnection conn = new SqlConnection(connStr)) {
            conn.Open();
            SqlCommand cmd = new SqlCommand("SELECT TOP 5 ma_ky_luat, trang_thai, tieu_de, ngay_hieu_luc, ngay_het_hieu_luc FROM HoSoKyLuat ORDER BY ma_ky_luat DESC", conn);
            using (SqlDataReader reader = cmd.ExecuteReader()) {
                while (reader.Read()) {
                    Console.WriteLine($"{reader["ma_ky_luat"]} | {reader["trang_thai"]} | {reader["tieu_de"]} | {reader["ngay_hieu_luc"]} | {reader["ngay_het_hieu_luc"]}");
                }
            }
        }
    }
}
