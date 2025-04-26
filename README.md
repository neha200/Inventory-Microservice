# Inventory Microservice

## Overview
This microservice manages inventory data and integrates with MongoDB and RabbitMQ.

## Features
- CRUD operations for inventory
- RabbitMQ integration for messaging
- Health check endpoint
- Swagger API documentation

## Setup
1. Install Docker and Docker Compose.
2. Run `docker-compose up` to start the service.
3. Access the API at `http://localhost:8080`.

## Endpoints
- `GET /api/inventory` - Get all inventory items
- `GET /api/inventory/{id}` - Get inventory by ID
- `POST /api/inventory` - Create a new inventory item
- `PUT /api/inventory/{id}` - Update an inventory item
- `DELETE /api/inventory/{id}` - Delete an inventory item

## General Docker Commands used
1. Build Docker Service
   ```bash
   #build without cache
   docker build . --no-cache --progress=plain

   #simple build
   docker-compose build
   ```
2. Start Docker Container
   ```bash
   docker-compose up
   ```
3. Stop Docker Container
   ```bash
   docker-compose down
   ```
4. To clear Docker cache
   ```bash
   docker builder prune -a
   ```
5. Delete all Docker Resources: To remove all unused data, including images, containers, volumes, and networks:
   ```bash
   docker system prune -a --volumes
   ```
## Note
For C# Application using RabbitMq, the container might stop after it starts (inventory-service). Need to manually start it, once RabbitMQ is up.
