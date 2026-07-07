# OrderSystem - Event-Driven .NET Architecture

## 🎯 Goal

This project simulates a production-like distributed backend system using .NET and messaging.

It is designed to demonstrate real backend engineering concepts:

- Event-driven architecture
- At-least-once delivery
- Idempotency
- Retry and resilience strategies
- Dead Letter Queue (DLQ)
- Distributed observability and tracing
- Cloud-native deployment concepts

The goal is not complexity, but a clear understanding of distributed systems behavior.

The project focuses on backend engineering challenges:

> Building reliable services that communicate asynchronously in a distributed environment.

---

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
```

---

# ⚙️ Tech Stack

- .NET 10
- ASP.NET Core Minimal API
- MassTransit
- RabbitMQ
- PostgreSQL
- Entity Framework Core
- Docker Compose
- OpenTelemetry

---

# 🧠 Key Concepts Implemented

## 📡 Event-driven architecture

Implemented using MassTransit and RabbitMQ.

Flow:

- `Orders.Api` creates orders
- `Orders.Api` publishes `OrderCreated` events
- `Billing.Service` consumes events asynchronously

Services are decoupled through messaging.

---

## 🔁 At-least-once delivery

The system assumes that messages can be delivered more than once.

This reflects real distributed messaging behavior:

- network failures can happen
- consumers can restart
- messages can be redelivered

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

This guarantees safe message reprocessing.

---

## 🔄 Retry strategy

Implemented with MassTransit retry middleware.

Current configuration:

- exponential backoff
- multiple retry attempts
- transient failure handling

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

- Structured application logging using `Microsoft ILogger`
- HTTP request tracing in `Orders.Api`
- MassTransit message tracing
- TraceId propagation through RabbitMQ
- Service identification using OpenTelemetry resources

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

RabbitMQ

    |
    |
    ▼

Billing.Service
TraceId: X
```

The project initially implemented a manual `CorrelationId` mechanism to understand distributed context propagation.

The system now also uses native distributed tracing concepts:

- TraceId
- SpanId
- OpenTelemetry Activity model

---

# 📦 Services

## Orders.Api

Responsibilities:

- Exposes `POST /orders`
- Persists orders in PostgreSQL
- Publishes `OrderCreated` events
- Provides OpenTelemetry HTTP tracing

---

## Billing.Service

Responsibilities:

- Consumes `OrderCreated` events
- Handles duplicate messages safely
- Persists processed messages
- Provides MassTransit tracing
- Logs processing activity

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
RabbitMQ
    |
    |
Billing.Service
```

---

# 🐳 Local Infrastructure

Docker Compose provides:

- RabbitMQ with management UI
- PostgreSQL orders database
- PostgreSQL billing database

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
```

---

# 🚀 Current Status

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
✅ Distributed TraceId propagation between services  
✅ Service identification for tracing

The system currently demonstrates production-like distributed backend concepts.

---

# 📌 Next Steps

## Observability improvements

- Add local tracing visualization
- Introduce OpenTelemetry Collector
- Add Jaeger or another tracing backend

## Messaging reliability

- Implement Outbox Pattern
- Improve transactional consistency between database changes and event publishing

## Advanced resilience

- Timeout policies
- Circuit breaker patterns
- Fault handling strategies

## Kubernetes deployment

Final deployment phase:

- Container images
- Kubernetes Deployments
- Services (ClusterIP)
- ConfigMaps
- Secrets
- Health checks
- Local Kubernetes cluster

