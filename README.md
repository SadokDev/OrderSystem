# OrderSystem - Event-Driven .NET Architecture

## 🎯 Goal
Simulate a production-like distributed system using .NET, RabbitMQ, and PostgreSQL.

## 🧱 Architecture

Order API → RabbitMQ → Billing Service → PostgreSQL

## ⚙️ Tech Stack
- .NET 10
- MassTransit
- RabbitMQ
- PostgreSQL
- Entity Framework Core

## 🧠 Key Concepts Implemented
- Event-driven architecture
- At-least-once delivery
- Idempotent consumers
- Retry policies (exponential backoff)
- Dead Letter Queue (DLQ)

## 📦 Services

### Orders.Api
- Creates orders
- Publishes OrderCreated event

### Billing.Service
- Consumes OrderCreated
- Processes idempotently
- Stores processed messages

## 🔄 Messaging Flow

POST /orders → RabbitMQ → Billing Service → PostgreSQL

## 🚀 Current Status
- System functional
- Retry policy implemented
- Idempotence implemented
- DLQ active

## 📌 Next Step
- CorrelationId propagation
- Structured logging
- Observability improvements