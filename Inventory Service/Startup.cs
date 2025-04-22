using Inventory_Service.Interfaces;
using Inventory_Service.Services;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace Inventory_Service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Configure services
        public void ConfigureServices(IServiceCollection services)
        {
            // Add RabbitMQ settings from configuration
            services.Configure<RabbitMqSettings>(Configuration.GetSection("RabbitMQ"));

            // Add MongoDB client and services
            services.AddSingleton<IMongoClient, MongoClient>(sp =>
                new MongoClient(Configuration.GetConnectionString("MongoDb")));

            services.AddScoped(sp =>
            {
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                var databaseName = Configuration.GetValue<string>("MongoDb:Database");
                if (string.IsNullOrEmpty(databaseName))
                {
                    throw new ArgumentNullException(nameof(databaseName), "Database name cannot be null or empty.");
                }
                return mongoClient.GetDatabase(databaseName);
            });

            services.AddScoped<IInventoryService, InventoryService>();

            // Add RabbitMQ Publisher
            services.AddSingleton<RabbitMqPublisher>();

            // Add RabbitMQ Consumer (Background Service)
            services.AddHostedService<RabbitMqConsumer>();

            // Add Swagger for API documentation
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Add Health Checks
            services.AddHealthChecks();

            // Add Logging
            services.AddLogging();

            services.AddSingleton<IConnection>(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var rabbitMqSettings = configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>();

                var factory = new ConnectionFactory
                {
                    HostName = rabbitMqSettings.Host,
                    UserName = rabbitMqSettings.User ?? "guest",
                    Password = rabbitMqSettings.Password ?? "guest",
                    Port = rabbitMqSettings.Port ?? 5672
                };

                return factory.CreateConnection();
            });

            // Add Controllers
            services.AddControllers(); // <-- This is the required addition
        }

        // Configure middleware
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1"));

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // <-- Ensure controllers are mapped here
                endpoints.MapHealthChecks("/health");
            });
        }
    }
}
