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
            var hocKies = await _context.HocKys.ToListAsync();
            foreach (var hk in hocKies)
            {
                var existingBlocks = await _context.Blocks.Where(b => b.MaHocKy == hk.MaHocKy).ToListAsync();
                if (!existingBlocks.Any())
                {
                    int totalDays = hk.NgayKetThuc.DayNumber - hk.NgayBatDau.DayNumber;
                    int blockLength = totalDays / 5;

                    for (int i = 1; i <= 5; i++)
                    {
                        var ngayBatDau = hk.NgayBatDau.AddDays((i - 1) * blockLength);
                        var ngayKetThuc = i == 5 ? hk.NgayKetThuc : ngayBatDau.AddDays(blockLength - 1);
                        
                        _context.Blocks.Add(new Block
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

            await _context.SaveChangesAsync();

            // 2. Gán tạm cho KhoaHoc hiện có
            var khoaHocs = await _context.KhoaHocs.Where(k => k.MaBlockBatDau == null && k.MaHocKy != null).ToListAsync();
            foreach (var kh in khoaHocs)
            {
                var firstBlock = await _context.Blocks
                    .Where(b => b.MaHocKy == kh.MaHocKy && b.ThuTuBlock == 1)
                    .FirstOrDefaultAsync();

                if (firstBlock != null)
                {
                    kh.MaBlockBatDau = firstBlock.MaBlock;
                    kh.SoBlockHoc = 1;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
