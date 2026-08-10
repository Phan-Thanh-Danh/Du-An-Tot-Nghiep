-- Seed ma tran ky nang giang vien (GiaoVienMonHoc) - campus 3 (HCM) cho Smart Timetable GA
-- P26 bo nguyen vong -> GA chon giang vien theo MucDoPhuHop. Script upsert (them/sua, khong xoa).
-- Chay tren may co DB LMS: sqlcmd -S localhost -d LMS -E -C -i docs/sql/seed-giao-vien-skill.sql

SET NOCOUNT ON;

MERGE GiaoVienMonHoc AS target
USING (
    VALUES
        -- Nguyen Van Lap Trinh (13)
        (13, 6, 90, 0, 5), (13, 4, 85, 0, 4), (13, 3, 70, 0, 3), (13, 34, 65, 0, 2),
        -- P12 Test Giang Vien (14)
        (14, 6, 95, 1, 6), (14, 4, 60, 0, 2),
        -- Tran Thi Giang Vien (15)
        (15, 4, 85, 1, 5), (15, 3, 75, 0, 3), (15, 34, 70, 0, 3),
        -- Tran Thi Thiet Ke (16)
        (16, 6, 80, 0, 4), (16, 4, 90, 1, 6), (16, 34, 60, 0, 2),
        -- Le Van Marketing (17)
        (17, 34, 95, 1, 8), (17, 6, 55, 0, 2), (17, 3, 60, 0, 2),
        -- Nguyen Van An (18)
        (18, 4, 100, 1, 9), (18, 6, 70, 0, 3), (18, 2, 95, 1, 7),
        -- Tran Thi Binh (19)
        (19, 4, 95, 1, 7), (19, 3, 70, 0, 3),
        -- Pham Minh Cuong (20)
        (20, 3, 95, 1, 6), (20, 6, 75, 0, 4), (20, 2, 70, 0, 3),
        -- Do Thi Dung (21)
        (21, 3, 85, 1, 5), (21, 4, 60, 0, 2),
        -- Le Thi Em (22)
        (22, 34, 90, 1, 6), (22, 6, 65, 0, 3)
) AS source (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, la_mon_chinh, so_nam_kinh_nghiem)
ON target.ma_giao_vien = source.ma_giao_vien AND target.ma_mon_hoc = source.ma_mon_hoc
WHEN MATCHED THEN
    UPDATE SET
        muc_do_phu_hop = source.muc_do_phu_hop,
        la_mon_chinh = source.la_mon_chinh,
        so_nam_kinh_nghiem = source.so_nam_kinh_nghiem,
        so_lan_da_day = so_lan_da_day + 1,
        con_hoat_dong = 1,
        ngay_cap_nhat = SYSUTCDATETIME()
WHEN NOT MATCHED THEN
    INSERT (ma_giao_vien, ma_mon_hoc, muc_do_phu_hop, so_lan_da_day, so_nam_kinh_nghiem, la_mon_chinh, con_hoat_dong, ngay_tao)
    VALUES (source.ma_giao_vien, source.ma_mon_hoc, source.muc_do_phu_hop, 0, source.so_nam_kinh_nghiem, source.la_mon_chinh, 1, SYSUTCDATETIME());

SELECT 'Cap nhat hoan tat. So dong hien co: ', COUNT(*) FROM GiaoVienMonHoc WHERE con_hoat_dong = 1;