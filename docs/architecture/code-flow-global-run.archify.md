# Code-Flow Global Skill Run: RxFlow

```archify
title: RxFlow Code Flow (Global Skill Run)
view: runtime-code-flow
style: sparse-edges

boundaries:
  - id: public
    label: Public / Client Trust Boundary
  - id: runtime
    label: Application Runtime Trust Boundary
  - id: data
    label: Data Plane Trust Boundary
  - id: external
    label: External Partner Trust Boundary

components:
  - id: client
    label: Optician Client
    boundary: public
    type: actor
  - id: api
    label: RxFlow API
    boundary: runtime
    type: service
  - id: submission
    label: OrderSubmissionService
    boundary: runtime
    type: application-service
  - id: pricing
    label: ComplexPricingEngine
    boundary: runtime
    type: domain-service
  - id: routing
    label: LabRoutingService
    boundary: runtime
    type: domain-service
  - id: jobs
    label: Hangfire Scheduler
    boundary: runtime
    type: async-orchestrator
  - id: worker
    label: OrderProcessingJob
    boundary: runtime
    type: worker
  - id: postgres
    label: PostgreSQL
    boundary: data
    type: datastore
  - id: redis
    label: Redis
    boundary: data
    type: cache
  - id: kafka
    label: Kafka/Redpanda
    boundary: data
    type: event-bus
  - id: partners
    label: Partner APIs
    boundary: external
    type: external-api

primary_path:
  - step: 1
    from: client
    to: api
    label: POST /orders (JWT)
  - step: 2
    from: api
    to: submission
    label: Validate and submit
  - step: 3
    from: submission
    to: pricing
    label: Compute quote
  - step: 4
    from: pricing
    to: partners
    label: Insurance + lens lookup
  - step: 5
    from: submission
    to: routing
    label: Choose lab
  - step: 6
    from: submission
    to: postgres
    label: Persist accepted order
  - step: 7
    from: submission
    to: jobs
    label: Enqueue processing
  - step: 8
    from: jobs
    to: worker
    label: Run async processing
  - step: 9
    from: worker
    to: partners
    label: Coating + shipping reservation
  - step: 10
    from: worker
    to: postgres
    label: Update order status
  - step: 11
    from: worker
    to: kafka
    label: Publish OrderAcceptedEvent

supporting_edges:
  - from: routing
    to: postgres
    label: Read lab capabilities
  - from: worker
    to: redis
    label: In-flight counter
```

## Component Cards

### RxFlow API
- Handles auth, validation, and endpoint mapping.
- Entrypoints: `/auth/token`, `/orders`, `/orders/{id}`, `/reports/adhoc`.

### OrderSubmissionService
- Normalizes inputs, orchestrates pricing/routing, persists, schedules job.
- Main correctness hotspot for intake flow.

### ComplexPricingEngine
- Computes quoted price from multiplier, insurance adjustment, discount, expedited charge.
- Uses partner connectors.

### LabRoutingService
- Reads labs by capability and selects by static priority.

### Hangfire Scheduler + OrderProcessingJob
- Async fulfillment path.
- Job updates status and emits event.

### PostgreSQL
- Source of truth for `orders` and `labs`.

### Redis
- Tracks in-flight processing counter.

### Kafka/Redpanda
- Carries `rxflow.orders.accepted` events.

### Partner APIs
- Insurance, lens catalog, coating, and shipping integrations.
