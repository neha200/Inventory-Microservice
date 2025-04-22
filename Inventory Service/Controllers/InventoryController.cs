using Inventory_Service.Interfaces;
using Inventory_Service.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var inventories = await _inventoryService.GetAllAsync();
        return Ok(inventories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var inventory = await _inventoryService.GetByIdAsync(id);
        if (inventory == null) return NotFound();
        return Ok(inventory);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Inventory inventory)
    {
        await _inventoryService.CreateAsync(inventory);
        return Created();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, Inventory inventory)
    {
        await _inventoryService.UpdateAsync(id, inventory);
        return Accepted();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }
}
