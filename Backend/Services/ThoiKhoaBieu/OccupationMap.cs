namespace Backend.Services.ThoiKhoaBieu;

public sealed class OccupationMap
{
    private readonly HashSet<(int MaHocKy, int ThuTrongTuan, int MaCaHoc, int MaGiaoVien)> _teacherSlots = new();
    private readonly HashSet<(int MaHocKy, int ThuTrongTuan, int MaCaHoc, int MaLop)> _classSlots = new();
    private readonly HashSet<(int MaHocKy, int ThuTrongTuan, int MaCaHoc, int MaPhong)> _roomSlots = new();
    
    private readonly Dictionary<(int MaHocKy, int ThuTrongTuan, int MaGiaoVien), int> _teacherDailyLoad = new();
    private readonly Dictionary<(int MaHocKy, int ThuTrongTuan, int MaLop), int> _classDailyLoad = new();

    public void OccupyTeacher(int maHocKy, int thuTrongTuan, int maCaHoc, int maGiaoVien)
    {
        _teacherSlots.Add((maHocKy, thuTrongTuan, maCaHoc, maGiaoVien));
        var key = (maHocKy, thuTrongTuan, maGiaoVien);
        _teacherDailyLoad[key] = _teacherDailyLoad.GetValueOrDefault(key) + 1;
    }

    public void OccupyClass(int maHocKy, int thuTrongTuan, int maCaHoc, int maLop)
    {
        _classSlots.Add((maHocKy, thuTrongTuan, maCaHoc, maLop));
        var key = (maHocKy, thuTrongTuan, maLop);
        _classDailyLoad[key] = _classDailyLoad.GetValueOrDefault(key) + 1;
    }

    public void OccupyRoom(int maHocKy, int thuTrongTuan, int maCaHoc, int maPhong)
    {
        _roomSlots.Add((maHocKy, thuTrongTuan, maCaHoc, maPhong));
    }

    public bool IsTeacherOccupied(int maHocKy, int thuTrongTuan, int maCaHoc, int maGiaoVien)
        => _teacherSlots.Contains((maHocKy, thuTrongTuan, maCaHoc, maGiaoVien));

    public bool IsClassOccupied(int maHocKy, int thuTrongTuan, int maCaHoc, int maLop)
        => _classSlots.Contains((maHocKy, thuTrongTuan, maCaHoc, maLop));

    public bool IsRoomOccupied(int maHocKy, int thuTrongTuan, int maCaHoc, int maPhong)
        => _roomSlots.Contains((maHocKy, thuTrongTuan, maCaHoc, maPhong));

    public int GetTeacherDailyLoad(int maHocKy, int thuTrongTuan, int maGiaoVien)
        => _teacherDailyLoad.GetValueOrDefault((maHocKy, thuTrongTuan, maGiaoVien));

    public int GetClassDailyLoad(int maHocKy, int thuTrongTuan, int maLop)
        => _classDailyLoad.GetValueOrDefault((maHocKy, thuTrongTuan, maLop));

    public int TeacherCount => _teacherSlots.Count;
    public int ClassCount => _classSlots.Count;
    public int RoomCount => _roomSlots.Count;

    public void Clear()
    {
        _teacherSlots.Clear();
        _classSlots.Clear();
        _roomSlots.Clear();
        _teacherDailyLoad.Clear();
        _classDailyLoad.Clear();
    }
}
