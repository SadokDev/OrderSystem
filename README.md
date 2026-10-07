# OrderSystem - Event-Driven .NET Architecture

## 🎯 Goal

This project simulates a production-like distributed backend system using .NET and messaging.

It is designed to demonstrate practical backend engineering concepts:

* Event-driven architecture
* At-least-once delivery
* Idempotency
* Retry and failure handling
* Dead Letter Queue (DLQ)
* Distributed observability and tracing
* Cloud-native deployment concepts

The goal is not complexity, but a clear understanding of distributed systems behavior and the challenges of building reliable services that communicate asynchronously.


# 🧱 Architecture

```text
                    OrderCreated Event

Orders.Api  ------------------------------> RabbitMQ
    |                                         |
    |                                         |
    |                                         ▼
    |                                Billing.Service
    |
    ▼
PostgreSQL                              PostgreSQL
(orders db)                            (billing db)


Distributed tracing:

Orders.Api
    |
    |
OpenTelemetry Collector
    |
    |
Jaeger
```

---

# ⚙️ Tech Stack

* .NET 10
* ASP.NET Core Minimal API
* MassTransit
* RabbitMQ
* PostgreSQL
* Entity Framework Core
* Docker Compose
* OpenTelemetry
* OpenTelemetry Collector
* Jaeger

---
# 📦 Outbox Pattern

The system uses the Transactional Outbox Pattern to improve consistency between database persistence and event publishing.

Without an Outbox, the following sequence could lead to an inconsistent state:

```text
Database
   |
   | Order saved
   |
   X Application failure
   |
RabbitMQ
   |
   | Event never published
```

The current implementation persists the order and the corresponding event in the same database operation:

```text
Orders.Api
     |
     ▼
PostgreSQL
┌──────────────────────┐
│ Order                │
│ OutboxMessage        │
└──────────┬───────────┘
           |
           | SaveChangesAsync()
           |
           ▼
       Transaction
```

A background publisher periodically reads pending messages from the `OutboxMessages` table and publishes them through MassTransit:

```text
OutboxMessages
      |
      | ProcessedOnUtc IS NULL
      ▼
OutboxPublisher
      |
      ▼
MassTransit
      |
      ▼
RabbitMQ
      |
      ▼
Billing.Service
```

After successful publication, the message is marked with `ProcessedOnUtc`.

The Outbox Publisher runs as a hosted background service and polls for pending messages periodically.

### Delivery semantics

The Outbox Pattern does not provide exactly-once delivery.

A failure can occur after the message has been published to RabbitMQ but before `ProcessedOnUtc` is persisted:

```text
Publish to RabbitMQ
       |
       | success
       ▼
Application failure
       |
       X
ProcessedOnUtc not updated
```

The message may therefore be published again after recovery.

For this reason, the consumer remains idempotent.

This gives the system the following reliability model:

```text
Outbox
   |
   | prevents event loss
   ▼
RabbitMQ
   |
   | at-least-once delivery
   ▼
Billing.Service
   |
   | idempotent processing
   ▼
ProcessedMessages
```

This design favors reliable delivery and recovery over exactly-once processing.

---



# 🧠 Key Concepts Implemented

## 📡 Event-driven architecture

Implemented using MassTransit and RabbitMQ.

Flow:

* `Orders.Api` creates orders
* `Orders.Api` stores `OrderCreated` events in the Transactional Outbox
* The Outbox Publisher publishes events to RabbitMQ
* `Billing.Service` consumes events asynchronously

Services are decoupled through messaging.

---

## 🔁 At-least-once delivery

The system assumes that messages can be delivered more than once.

This reflects real distributed messaging behavior:

* network failures can happen
* consumers can restart
* messages can be redelivered

The consumer is designed to handle duplicates safely.

---

## 🧾 Idempotency

Implemented using a `ProcessedMessages` table.

The consumer checks whether an order has already been processed.

Current strategy:

```text
OrderId uniqueness
        +
ProcessedMessages persistence
```

This allows duplicate messages to be detected and safely ignored.

---

## 🔄 Retry strategy

Implemented with MassTransit retry middleware.

Current configuration:

* exponential backoff
* multiple retry attempts
* transient failure handling

Example:

```text
Failure
   |
Retry 1
   |
Retry 2
   |
Retry 3
   |
Success or Dead Letter Queue
```

---

## ☠️ Dead Letter Queue (DLQ)

Failed messages are routed to an error queue.

Current behavior:

```text
billing-service
        |
        |
        X
        |
        ▼
billing-service_error
```

This allows inspection of poison messages.

---

# 🔍 Observability

Implemented using OpenTelemetry.

Current capabilities:

* Structured application logging using `Microsoft ILogger`
* HTTP request tracing in `Orders.Api`
* MassTransit message tracing
* Distributed TraceId propagation through RabbitMQ
* Service identification using OpenTelemetry resources
* Local trace visualization using Jaeger

Services:

```text
orders-api
billing-service
```

Distributed tracing flow:

```text
HTTP Request

    |
    |
    ▼

Orders.Api

TraceId: X

    |
    |
    ▼

RabbitMQ / MassTransit

    |
    |
    ▼

Billing.Service

TraceId: X
```

The project initially implemented a manual `CorrelationId` mechanism to understand business-level message correlation.

The system now also uses native distributed tracing concepts:

* TraceId
* SpanId
* OpenTelemetry Activity model

CorrelationId and TraceId serve different purposes:

* `CorrelationId` identifies the business operation and message flow
* `TraceId` identifies the technical distributed execution flow

---

# 📦 Services

## Orders.Api

Responsibilities:

* Exposes `POST /orders`
* Persists orders in PostgreSQL
* Publishes `OrderCreated` events
* Provides OpenTelemetry HTTP and MassTransit tracing

---

## Billing.Service

Responsibilities:

* Consumes `OrderCreated` events
* Handles duplicate messages safely
* Persists processed messages
* Provides MassTransit consumer tracing
* Logs processing activity

---

# 🔄 Message Flow

```text
POST /orders

      |
      ▼

Orders.Api

      |
      | OrderCreated event
      |
      ▼

RabbitMQ

      |
      ▼

Billing.Service

      |
      ▼

ProcessedMessages PostgreSQL table
```

Distributed trace:

```text
TraceId

Orders.Api
    |
    |
MassTransit / RabbitMQ
    |
    |
Billing.Service
```

---

# 🐳 Local Infrastructure

Docker Compose provides:

* RabbitMQ with management UI
* PostgreSQL orders database
* PostgreSQL billing database
* OpenTelemetry Collector
* Jaeger tracing backend

Local development environment:

```text
Docker Compose
       |
       |
       +-- RabbitMQ
       |
       +-- PostgreSQL Orders DB
       |
       +-- PostgreSQL Billing DB
       |
       +-- OpenTelemetry Collector
       |
       +-- Jaeger
```

---

# 🚀 Current Status

The core distributed backend is implemented and validated locally.

Implemented:

✅ Event-driven architecture  
✅ RabbitMQ messaging  
✅ MassTransit integration  
✅ PostgreSQL persistence  
✅ Idempotent consumer  
✅ Retry policy  
✅ Dead Letter Queue  
✅ CorrelationId propagation  
✅ OpenTelemetry integration  
✅ OpenTelemetry Collector integration  
✅ Jaeger tracing backend  
✅ Distributed TraceId propagation between services  
✅ Service identification for tracing  
✅ Transactional Outbox Pattern  
✅ Background Outbox Publisher  
✅ Docker Compose local infrastructure  
✅ End-to-end validation  

The system demonstrates the core reliability and observability mechanisms of a small distributed backend.

---

# 📌 Next Steps

The application-level feature set is considered complete.

The remaining phase is the deployment of the existing system to a local Kubernetes environment.

## Kubernetes deployment

Planned:

* Container images
* Kubernetes Deployments
* Services (`ClusterIP`)
* ConfigMaps
* Secrets
* Liveness and readiness probes
* Local Kubernetes cluster


