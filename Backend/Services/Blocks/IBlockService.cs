using Backend.DTOs.Blocks;

namespace Backend.Services.Blocks;

public interface IBlockService
{
    Task<IReadOnlyList<BlockDto>> GetByTermIdAsync(int termId, CancellationToken cancellationToken = default);
    Task<BlockDto> UpdateAsync(int blockId, UpdateBlockRequest request, CancellationToken cancellationToken = default);
}
