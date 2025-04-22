using System.Text.Json;
using System.Text;
using Inventory_Service.Interfaces;
using Inventory_Service.Models;
using Inventory_Service.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using RabbitMQ.Client;

namespace Inventory_MicroService.Controllers.Tests
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
        public void PublishInventoryAlert_SendsMessageToQueue()
        {
            // Arrange
            var mockChannel = new Mock<IModel>();
            var inventory = new Inventory
            {
                Id = "1",
                ProductName = "TestProduct",
                StockLevel = 5,
                InventoryAlert = new InventoryAlert { Threshold = 10, IsAlert = true }
            };

            var alertMessage = new
            {
                ProductId = inventory.Id,
                ProductName = inventory.ProductName,
                StockLevel = inventory.StockLevel,
                Alert = "Stock level below threshold"
            };
            var expectedMessageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alertMessage));

            // Act
            var service = new InventoryService(null, null, (IConnection)mockChannel.Object);
            service.PublishInventoryAlert(inventory);

            // Assert
            mockChannel.Verify(channel => channel.BasicPublish(
                "",
                "inventory_alerts",
                null,
                It.Is<byte[]>(body => body.SequenceEqual(expectedMessageBody))
            ),Times.Once);
        }

    }
}