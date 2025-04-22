using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class RabbitMqPublisher
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqPublisher(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    public void PublishInventoryUpdated(string productId, int newStock)
    {
        var factory = new ConnectionFactory() { HostName = _settings.Host };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: _settings.Queue, durable: false, exclusive: false, autoDelete: false);

        var payload = new
        {
            product_id = productId,
            new_stock = newStock
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

        channel.BasicPublish(exchange: "", routingKey: _settings.Queue, body: body);
        Console.WriteLine($"[Producer] Sent: {JsonSerializer.Serialize(payload)}");
    }
}
