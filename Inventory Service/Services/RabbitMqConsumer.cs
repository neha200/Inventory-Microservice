using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

public class RabbitMqConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqConsumer(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    private void InitRabbitMQ()
    {
        var factory = new ConnectionFactory() { HostName = _settings.Host };

        // Establish connection and channel
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declare the queue
        _channel.QueueDeclare(
            queue: _settings.Queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        InitRabbitMQ();

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                Console.WriteLine($"[Consumer] Received: {message}");

                // TODO: Deserialize and process the message  
                // Example: var data = JsonSerializer.Deserialize<YourMessageType>(message);  
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Consumer] Error processing message: {ex.Message}");
            }

            await Task.CompletedTask; // Ensure the lambda returns a Task  
        };

        // Start consuming messages  
        _channel.BasicConsume(
            queue: _settings.Queue,
            autoAck: true,
            consumer: consumer
        );

        // Keep the task running until the service is stopped  
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        // Dispose resources properly
        if (_channel != null && _channel.IsOpen)
        {
            _channel.Close();
            _channel.Dispose();
        }

        if (_connection != null && _connection.IsOpen)
        {
            _connection.Close();
            _connection.Dispose();
        }

        base.Dispose();
    }
}
