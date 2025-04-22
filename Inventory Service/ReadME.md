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
3. Access the API at `http://localhost:8001`.

## Endpoints
- `GET /api/inventory` - Get all inventory items
- `GET /api/inventory/{id}` - Get inventory by ID
- `POST /api/inventory` - Create a new inventory item
- `PUT /api/inventory/{id}` - Update an inventory item
- `DELETE /api/inventory/{id}` - Delete an inventory item