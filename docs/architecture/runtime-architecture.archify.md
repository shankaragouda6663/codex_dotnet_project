# RxFlow Runtime Architecture (High-Level)

```archify
title: RxFlow Runtime Architecture
view: runtime-high-level
style: sparse-edges

boundaries:
  - id: internet
    label: Public / Client Trust Boundary
  - id: rxflow_runtime
    label: RxFlow Runtime Trust Boundary
  - id: data_plane
    label: Data Plane Trust Boundary
  - id: partners
    label: External Partner Trust Boundary

components:
  - id: client
    label: Optician Client
    boundary: internet
    type: actor
  - id: api
    label: RxFlow API (ASP.NET Core)
    boundary: rxflow_runtime
    type: service
  - id: submit
    label: OrderSubmissionService
    boundary: rxflow_runtime
    type: application-service
  - id: pricing
    label: ComplexPricingEngine
    boundary: rxflow_runtime
    type: domain-service
  - id: routing
    label: StaticPriorityLabRoutingService
    boundary: rxflow_runtime
    type: domain-service
  - id: jobs
    label: Hangfire Scheduler/Queue
    boundary: rxflow_runtime
    type: async-orchestrator
  - id: worker
    label: OrderProcessingJob
    boundary: rxflow_runtime
    type: worker
  - id: postgres
    label: PostgreSQL (orders, labs)
    boundary: data_plane
    type: datastore
  - id: redis
    label: Redis (in-flight counter)
    boundary: data_plane
    type: cache
  - id: kafka
    label: Kafka/Redpanda Topic
    boundary: data_plane
    type: event-bus
  - id: partners_api
    label: Partner APIs (insurance, lens, coating, shipping)
    boundary: partners
    type: external-api

primary_path:
  - step: 1
    from: client
    to: api
    label: POST /orders (JWT)
  - step: 2
    from: api
    to: submit
    label: Validate and submit
  - step: 3
    from: submit
    to: pricing
    label: Quote price
  - step: 4
    from: pricing
    to: partners_api
    label: Coverage + material multiplier
  - step: 5
    from: submit
    to: routing
    label: Select lab
  - step: 6
    from: submit
    to: postgres
    label: Persist accepted order
  - step: 7
    from: submit
    to: jobs
    label: Enqueue processing
  - step: 8
    from: jobs
    to: worker
    label: Execute ProcessAsync
  - step: 9
    from: worker
    to: partners_api
    label: Coating + shipment reservation
  - step: 10
    from: worker
    to: postgres
    label: Update status
  - step: 11
    from: worker
    to: kafka
    label: Publish OrderAcceptedEvent

supporting_edges:
  - from: routing
    to: postgres
    label: Read labs/capabilities
  - from: worker
    to: redis
    label: In-flight increment/decrement
```

## Component Cards

### 1) Optician Client
- Role: Calls authenticated order endpoints.
- Primary interfaces: `POST /auth/token`, `POST /orders`, `GET /orders/{id}`.
- Boundary notes: Untrusted input enters system here.

### 2) RxFlow API (ASP.NET Core)
- Role: Edge/API host, authN/authZ, request validation, endpoint mapping.
- Key files: `RxFlow.Api/Program.cs`, `RxFlow.Api/Endpoints/CreateOrderRequestModel.cs`.
- Boundary notes: First trusted boundary after JWT validation.

### 3) OrderSubmissionService
- Role: Core intake orchestration for normalization, routing, pricing, persistence, and job scheduling.
- Key file: `RxFlow.Application/Orders/OrderSubmissionService.cs`.
- Risk notes: Business correctness and idempotency behavior are concentrated here.

### 4) ComplexPricingEngine
- Role: Computes quoted price and applies expedited/discount/insurance logic.
- Key file: `RxFlow.Application/Pricing/ComplexPricingEngine.cs`.
- External touchpoint: Pulls inputs from partner APIs via connectors.

### 5) StaticPriorityLabRoutingService
- Role: Chooses lab by capability and static priority.
- Key files: `RxFlow.Application/Labs/StaticPriorityLabRoutingService.cs`, `RxFlow.Infrastructure/Labs/EfLabReadStore.cs`.
- Data note: Reads lab metadata from PostgreSQL.

### 6) Hangfire Scheduler/Queue
- Role: Asynchronous handoff from synchronous API acceptance to background fulfillment.
- Key files: `RxFlow.Api/Background/ApiOrderJobScheduler.cs`, `RxFlow.Api/Program.cs`.
- Reliability note: Retry semantics configured at job level.

### 7) OrderProcessingJob
- Role: Background fulfillment, status transition, event publication.
- Key file: `RxFlow.Workers/Processing/OrderProcessingJob.cs`.
- Operational note: Maintains in-flight counters and publishes domain events.

### 8) PostgreSQL (orders, labs)
- Role: System of record for orders and lab reference data.
- Key files: `RxFlow.Infrastructure/Persistence/RxFlowDbContext.cs`, `RxFlow.Infrastructure/Persistence/EfOrderRepository.cs`.
- Integrity note: EF migrations run at startup.

### 9) Redis (in-flight counter)
- Role: Lightweight concurrency/load signal (`rxflow:inflight`).
- Key file: `RxFlow.Infrastructure/Redis/RedisInFlightCounter.cs`.
- Risk note: Counter operations are non-transactional.

### 10) Kafka/Redpanda Topic
- Role: Emits accepted order events for downstream consumers.
- Key file: `RxFlow.Infrastructure/Messaging/KafkaOrderPublisher.cs`.
- Contract note: Topic `rxflow.orders.accepted`, JSON payload.

### 11) Partner APIs (insurance, lens, coating, shipping)
- Role: External policy/material/shipment/coating decision providers.
- Key files: `RxFlow.Infrastructure/Connectors/*.cs`.
- Trust note: Outbound HTTP with timeouts/retries; responses are partially trusted and defaulted.
