using Inventory_Service.Interfaces;
using Inventory_Service.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace InventoryControllerTest
{
    public class InventoryControllerTest
    {
        private readonly Mock<IInventoryService> _mockInventoryService;
        private readonly InventoryController _controller;

        public InventoryControllerTest()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _controller = new InventoryController(_mockInventoryService.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkResult_WithListOfInventories()
        {
            // Arrange
            var inventories = new List<Inventory> { new Inventory { Id = "1", ProductName = "Item1" } };
            _mockInventoryService.Setup(service => service.GetAllAsync()).ReturnsAsync(inventories);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(inventories, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult_WithInventory()
        {
            // Arrange
            var inventory = new Inventory { Id = "1", ProductName = "Item1" };
            _mockInventoryService.Setup(service => service.GetByIdAsync("1")).ReturnsAsync(inventory);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(inventory, okResult.Value);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenInventoryDoesNotExist()
        {
            // Arrange
            _mockInventoryService.Setup(service => service.GetByIdAsync("1")).ReturnsAsync((Inventory)null);

            // Act
            var result = await _controller.GetById("1");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtActionResult_WithInventory()
        {
            // Arrange
            var inventory = new Inventory { Id = "1", ProductName = "Item1" };
            _mockInventoryService.Setup(service => service.CreateAsync(inventory)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(inventory);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(inventory, createdResult.Value);
        }

        [Fact]
        public async Task Update_ReturnsNoContentResult()
        {
            // Arrange
            var inventory = new Inventory { Id = "1", ProductName = "UpdatedItem" };
            _mockInventoryService.Setup(service => service.UpdateAsync("1", inventory)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update("1", inventory);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNoContentResult()
        {
            // Arrange
            _mockInventoryService.Setup(service => service.DeleteAsync("1")).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete("1");

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenInventoryDoesNotExist()
        {
            // Arrange
            _mockInventoryService.Setup(service => service.DeleteAsync("1")).ThrowsAsync(new KeyNotFoundException());
            // Act
            var result = await _controller.Delete("1");
            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_TracksProductMovementAndRaisesAlert()
        {
            // Arrange
            var inventory = new Inventory
            {
                Id = "1",
                ProductName = "TestProduct",
                StockLevel = 5,
                ProductMovement = new List<ProductMovement>(),
                InventoryAlert = new InventoryAlert { Threshold = 10, IsAlert = false }
            };

            var updatedInventory = new Inventory
            {
                Id = "1",
                ProductName = "TestProduct",
                StockLevel = 3, // Stock level reduced
                ProductMovement = new List<ProductMovement>(),
                InventoryAlert = new InventoryAlert { Threshold = 10, IsAlert = false }
            };

            _mockInventoryService.Setup(service => service.GetByIdAsync("1")).ReturnsAsync(inventory);
            _mockInventoryService.Setup(service => service.UpdateAsync("1", updatedInventory)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update("1", updatedInventory);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockInventoryService.Verify(service => service.UpdateAsync("1", It.Is<Inventory>(inv =>
                inv.ProductMovement.Count == 1 &&
                inv.ProductMovement[0].Type == "Reduction" &&
                inv.ProductMovement[0].Quantity == 2 &&
                inv.InventoryAlert.IsAlert == true
            )), Times.Once);
        }

    }
}