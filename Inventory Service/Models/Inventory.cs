using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Inventory_Service.Models
{
    public class ProductMovement
    {
        public string Type { get; set; }
        public int Quantity { get; set; }
        public string Date { get; set; }
    }

    public class InventoryAlert
    {
        public int Threshold { get; set; } = 5;
        public bool IsAlert { get; set; }
    }

    public class Inventory
    {
        [BsonId]
        public string Id { get; set; }

        [Required]
        public string ProductName { get; set; }

        [Range(0, int.MaxValue)]
        public int StockLevel { get; set; }

        [Required]
        public List<ProductMovement> ProductMovement { get; set; }

        public InventoryAlert InventoryAlert { get; set; }
    }
}
