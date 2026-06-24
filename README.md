# OrderSystem - Event-Driven .NET Architecture

## 🎯 Goal

This project simulates a production-like distributed backend system using .NET and messaging.

It is designed to demonstrate real backend engineering concepts:

- Event-driven architecture
- At-least-once delivery
- Idempotency
- Retry and resilience strategies
- Dead Letter Queue (DLQ)
- Basic observability using CorrelationId

The goal is not complexity, but clear understanding of distributed systems behavior.

---

## 🧱 Architecture

Orders.Api → RabbitMQ → Billing.Service → PostgreSQL

---

## ⚙️ Tech Stack

- .NET 10
- MassTransit
- RabbitMQ
- PostgreSQL
- Entity Framework Core
- Docker Compose

---

## 🧠 Key Concepts Implemented

### 📡 Event-driven architecture
- Orders.Api publishes `OrderCreated` events
- Billing.Service consumes events asynchronously

### 🔁 At-least-once delivery
- Messages may be delivered more than once
- System is designed to handle duplicates safely

### 🧾 Idempotency
- Implemented using `ProcessedMessages` table
- Ensures each order is processed only once

### 🔄 Retry strategy
- Exponential backoff retry policy via MassTransit
- Handles transient infrastructure failures

### ☠️ Dead Letter Queue (DLQ)
- Failed messages are routed to `billing-service_error`
- Allows inspection of poison messages

### 🔍 Observability (basic)
- CorrelationId generated in Orders.Api
- Propagated through message payload
- Used in Billing.Service logs to trace execution flow

---

## 📦 Services

### Orders.Api
- Exposes `POST /orders`
- Creates orders in PostgreSQL
- Generates a CorrelationId per request
- Publishes `OrderCreated` event to RabbitMQ

### Billing.Service
- Consumes `OrderCreated` events
- Ensures idempotent processing
- Persists processed messages in PostgreSQL
- Logs CorrelationId for traceability

---

## 🔄 Message Flow

POST /orders
→ Orders.Api (CorrelationId generated)
→ RabbitMQ (OrderCreated event)
→ Billing.Service (consumer)
→ PostgreSQL (ProcessedMessages stored)

---

## 🚀 Current Status

- Event-driven architecture implemented and working
- RabbitMQ messaging configured
- PostgreSQL persistence working
- Idempotency fully implemented
- Retry policy with exponential backoff enabled
- Dead Letter Queue active
- CorrelationId propagation implemented (basic observability)
- End-to-end flow validated

---

## 📌 Next Steps

- Replace manual CorrelationId propagation with MassTransit headers
- Introduce structured logging (Serilog or equivalent)
- Improve observability with lightweight distributed tracing
- Prepare Kubernetes deployment (local cluster)