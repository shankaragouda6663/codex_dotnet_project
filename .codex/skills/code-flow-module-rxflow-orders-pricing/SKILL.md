# Code-Flow Module Overlay: RxFlow Orders + Pricing

## Purpose
Generate accurate runtime diagrams for the RxFlow order intake and pricing/processing path.

## Module Metadata
- `module_name`: `rxflow-orders-pricing`
- `module_root_paths`:
  - `RxFlow.Api/`
  - `RxFlow.Application/Orders/`
  - `RxFlow.Application/Pricing/`
  - `RxFlow.Application/Labs/`
  - `RxFlow.Workers/Processing/`
  - `RxFlow.Infrastructure/`
- `entrypoints`:
  - `POST /orders`
  - `POST /auth/token`
  - Hangfire `OrderProcessingJob.ProcessAsync`
- `core_services`:
  - `OrderSubmissionService`
  - `ComplexPricingEngine`
  - `StaticPriorityLabRoutingService`
  - `ApiOrderJobScheduler`
  - `OrderProcessingJob`
- `data_stores`:
  - PostgreSQL (`orders`, `labs`)
  - Redis (`rxflow:inflight`)
  - Kafka/Redpanda (`rxflow.orders.accepted`)
- `external_dependencies`:
  - insurance API
  - lens catalog API
  - coating API
  - shipping API
- `sensitive_data_tags`:
  - patient identifier
  - prescription values

## Focused Rules
1. Primary path begins at `POST /orders` and ends at event publication to Kafka.
2. Include async handoff through Hangfire as a first-class primary-path step.
3. Keep direct edges to partner APIs limited to pricing and worker processing nodes.
4. Move normalization and detailed pricing math into component cards.
5. Highlight raw SQL reporting path only as supporting detail, not primary path.

## Required Component Set
- Client
- RxFlow API
- OrderSubmissionService
- ComplexPricingEngine
- StaticPriorityLabRoutingService
- Hangfire Scheduler/Queue
- OrderProcessingJob
- PostgreSQL
- Redis
- Kafka/Redpanda
- Partner APIs (grouped)

## Required Evidence Files
- `RxFlow.Api/Program.cs`
- `RxFlow.Api/Background/ApiOrderJobScheduler.cs`
- `RxFlow.Application/Orders/OrderSubmissionService.cs`
- `RxFlow.Application/Pricing/ComplexPricingEngine.cs`
- `RxFlow.Application/Labs/StaticPriorityLabRoutingService.cs`
- `RxFlow.Workers/Processing/OrderProcessingJob.cs`
- `RxFlow.Infrastructure/ServiceRegistration.cs`
- `RxFlow.Infrastructure/Persistence/RxFlowDbContext.cs`
- `RxFlow.Infrastructure/Persistence/EfOrderRepository.cs`
- `RxFlow.Infrastructure/Messaging/KafkaOrderPublisher.cs`
- `RxFlow.Infrastructure/Redis/RedisInFlightCounter.cs`
- `RxFlow.Infrastructure/Connectors/InsuranceConnector.cs`
- `RxFlow.Infrastructure/Connectors/LensCatalogConnector.cs`
- `RxFlow.Infrastructure/Connectors/CoatingConnector.cs`
- `RxFlow.Infrastructure/Connectors/ShippingConnector.cs`

## Output Contract
- Output file:
  - `docs/architecture/runtime-architecture.archify.md`
- Diagram requirements:
  - 8-12 components
  - one primary path
  - explicit trust boundaries
  - supporting detail in cards

## Quality Gate
- Reject output if it adds speculative services not present in evidence files.
- Reject output if more than one end-to-end primary path is drawn.
- Reject output if trust boundaries are missing.
