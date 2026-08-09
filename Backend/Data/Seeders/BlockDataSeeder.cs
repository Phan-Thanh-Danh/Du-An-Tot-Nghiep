using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Data.Seeders
{
    public class BlockDataSeeder
    {
        private readonly ApplicationDbContext _context;

        public BlockDataSeeder(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // 1. Sinh 5 Block cho mỗi HocKy hiện có
            var hocKies = await _context.HocKys
                .AsNoTracking()
                .Select(hk => new
                {
                    hk.MaHocKy,
                    hk.NgayBatDau,
                    hk.NgayKetThuc
                })
                .ToListAsync();
            var existingTermIds = await _context.Blocks
                .AsNoTracking()
                .Select(block => block.MaHocKy)
                .Distinct()
                .ToHashSetAsync();
            var newBlocks = new List<Block>();

            foreach (var hk in hocKies)
            {
                if (!existingTermIds.Contains(hk.MaHocKy))
                {
                    int totalDays = hk.NgayKetThuc.DayNumber - hk.NgayBatDau.DayNumber;
                    int blockLength = Math.Max(1, totalDays / 5);

                    for (int i = 1; i <= 5; i++)
                    {
                        var ngayBatDau = hk.NgayBatDau.AddDays((i - 1) * blockLength);
                        var ngayKetThuc = i == 5 ? hk.NgayKetThuc : ngayBatDau.AddDays(blockLength - 1);

                        newBlocks.Add(new Block
                        {
                            MaHocKy = hk.MaHocKy,
                            ThuTuBlock = i,
                            TenBlock = $"Block {i}",
                            NgayBatDau = ngayBatDau,
                            NgayKetThuc = ngayKetThuc
                        });
                    }
                }
            }

            if (newBlocks.Count > 0)
            {
                _context.Blocks.AddRange(newBlocks);
                await _context.SaveChangesAsync();
            }

            // 2. Gán tạm cho KhoaHoc hiện có
            var firstBlockByTerm = await _context.Blocks
                .AsNoTracking()
                .Where(block => block.ThuTuBlock == 1)
                .ToDictionaryAsync(block => block.MaHocKy, block => block.MaBlock);
            var khoaHocs = await _context.KhoaHocs
                .Where(khoaHoc => khoaHoc.MaBlockBatDau == null && khoaHoc.MaHocKy != null)
                .ToListAsync();
            var changedCourseCount = 0;
            foreach (var kh in khoaHocs)
            {
                if (kh.MaHocKy.HasValue && firstBlockByTerm.TryGetValue(kh.MaHocKy.Value, out var firstBlockId))
                {
                    kh.MaBlockBatDau = firstBlockId;
                    kh.SoBlockHoc = 1;
                    changedCourseCount++;
                }
            }

            if (changedCourseCount > 0)
                await _context.SaveChangesAsync();
        }
    }
}
