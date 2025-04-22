using Inventory_Service.Models;

namespace Inventory_Service.Interfaces
{
    public interface IInventoryService
    {
            Task<List<Inventory>> GetAllAsync();
            Task<Inventory> GetByIdAsync(string id);
            Task CreateAsync(Inventory inventory);
            Task UpdateAsync(string id, Inventory inventory);
            Task DeleteAsync(string id);
    }
}
