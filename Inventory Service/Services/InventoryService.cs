using System.Text.Json;
using System.Text;
using Inventory_Service.Interfaces;
using Inventory_Service.Models;
using MongoDB.Driver;
using RabbitMQ.Client;
using MongoDB.Bson;

namespace Inventory_Service.Services
{

    public class InventoryService : IInventoryService
    {
        private readonly IMongoCollection<Inventory> _inventoryCollection;
        private readonly IModel _rabbitMqChannel;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IConfiguration configuration, IMongoClient mongoClient, IConnection rabbitMqConnection)
        {
            _logger = new Logger<InventoryService>(new LoggerFactory());

            // MongoDB connection setup and collection initialization
            var database = mongoClient.GetDatabase(configuration.GetValue<string>("MongoDb:Database"));
            _inventoryCollection = database.GetCollection<Inventory>("Inventories");

            // RabbitMQ channel setup
            _rabbitMqChannel = rabbitMqConnection.CreateModel();
            _rabbitMqChannel.QueueDeclare(queue: "inventory_alerts", durable: true, exclusive: false, autoDelete: false, arguments: null);

            _logger.LogInformation("InventoryService initialized.");
        }

        public async Task<List<Inventory>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all inventory records.");
            return await _inventoryCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Inventory> GetByIdAsync(string id)
        {
            _logger.LogInformation("Fetching inventory record with ID: {Id}", id);
            var inventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
            return inventory;
        }

        public async Task CreateAsync(Inventory inventory)
        {
            _logger.LogInformation("Creating new inventory record for product: {ProductName}", inventory.ProductName);
            if (string.IsNullOrEmpty(inventory.Id))
            {
                inventory.Id = ObjectId.GenerateNewId().ToString();
            }
            await _inventoryCollection.InsertOneAsync(inventory);
        }

        public async Task UpdateAsync(string id, Inventory inventory)
        {
            _logger.LogInformation("Updating inventory record with ID: {Id}", id);
            var existingInventory = await _inventoryCollection.Find(i => i.Id == id).FirstOrDefaultAsync();

            if (existingInventory == null)
            {
                _logger.LogError("Inventory with ID: {Id} not found for update.", id);
                throw new KeyNotFoundException("Inventory not found to update.");
            }

            TrackProductMovement(existingInventory, inventory);

            // Check and raise inventory alert
            if (inventory.StockLevel < inventory.InventoryAlert.Threshold)
            {
                _logger.LogWarning("Stock level below threshold for product: {ProductName}", inventory.ProductName);
                inventory.InventoryAlert.IsAlert = true;
                PublishInventoryAlert(inventory);
            }
            else
            {
                inventory.InventoryAlert.IsAlert = false;
            }

            var result = await _inventoryCollection.ReplaceOneAsync(i => i.Id == id, inventory);
            if (result.ModifiedCount == 0)
            {
                _logger.LogError("Failed to update inventory with ID: {Id}", id);
            }
        }

        public async Task DeleteAsync(string id)
        {
            _logger.LogInformation("Deleting inventory record with ID: {Id}", id);
            var result = await _inventoryCollection.DeleteOneAsync(i => i.Id == id);
            if (result.DeletedCount == 0)
            {
                _logger.LogError("Failed to delete inventory with ID: {Id}", id);
                throw new KeyNotFoundException("Inventory not found to delete.");
            }
        }

        public void TrackProductMovement(Inventory existingInventory, Inventory updatedInventory)
        {
            var productMovement = new ProductMovement
            {
                Type = updatedInventory.StockLevel > existingInventory.StockLevel ? "Addition" : "Reduction",
                Quantity = Math.Abs(updatedInventory.StockLevel - existingInventory.StockLevel),
                Date = DateTime.UtcNow.ToString("o")
            };

            updatedInventory.ProductMovement.Add(productMovement);
            _logger.LogInformation("Tracked product movement: {Type}, Quantity: {Quantity}", productMovement.Type, productMovement.Quantity);
        }

        public void PublishInventoryAlert(Inventory inventory)
        {
            var alertMessage = new
            {
                ProductId = inventory.Id,
                ProductName = inventory.ProductName,
                StockLevel = inventory.StockLevel,
                Alert = "Stock level below threshold"
            };

            var messageBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(alertMessage));
            _rabbitMqChannel.BasicPublish(exchange: "", routingKey: "inventory_alerts", basicProperties: null, body: messageBody);
            _logger.LogInformation("Published inventory alert for product: {ProductName}", inventory.ProductName);
        }
    }
}
