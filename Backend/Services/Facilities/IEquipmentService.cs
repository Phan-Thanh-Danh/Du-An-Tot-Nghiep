using Backend.DTOs.Facilities;

namespace Backend.Services.Facilities;

public interface IEquipmentService
{
    Task<List<EquipmentDto>> GetEquipmentByRoomIdAsync(int roomId);
    Task<EquipmentDto> CreateEquipmentAsync(CreateEquipmentDto dto);
    Task<EquipmentDto> UpdateEquipmentAsync(int id, UpdateEquipmentDto dto);
    Task DeleteEquipmentAsync(int id);
}
