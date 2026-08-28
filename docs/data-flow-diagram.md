# RxFlow Source Data-Flow Diagram

```mermaid
flowchart LR
    Client[Optician Client]

    subgraph API[RxFlow.Api]
      Auth[/POST /auth/token/]
      Orders[/POST /orders/]
      Override[/POST /orders/lab-override/]
      Report[/GET /reports/adhoc/]
      Health[/GET /healthz/]
      ApiScheduler[ApiOrderJobScheduler]
    end

    subgraph App[RxFlow.Application]
      SubmitSvc[OrderSubmissionService]
      RouteSvc[StaticPriorityLabRoutingService]
      Price[ComplexPricingEngine]
      ReportSvc[OrderReportService]
    end

    subgraph Infra[RxFlow.Infrastructure]
      Repo[EfOrderRepository]
      LabStore[EfLabReadStore]
      RawSql[RawSqlOrderReportQuery]
      Counter[RedisInFlightCounter]
      Publisher[KafkaOrderPublisher]
      Insurance[InsuranceConnector]
      LensCatalog[LensCatalogConnector]
      Shipping[ShippingConnector]
      Coating[CoatingConnector]
      Db[(PostgreSQL)]
      Redis[(Redis)]
      Kafka[(Redpanda / Kafka)]
    end

    subgraph Workers[RxFlow.Workers]
      Job[OrderProcessingJob]
    end

    Client --> Auth
    Client --> Orders
    Client --> Override
    Client --> Report
    Client --> Health

    Orders --> SubmitSvc
    SubmitSvc --> RouteSvc
    RouteSvc --> LabStore
    LabStore --> Db

    SubmitSvc --> Price
    Price --> Insurance
    Price --> LensCatalog
    Insurance -->|"HTTP GET /coverage/{patientId}"| InsuranceApi[(Insurance API)]
    LensCatalog -->|"HTTP GET /materials/{lensMaterial}"| CatalogApi[(Lens Catalog API)]

    SubmitSvc --> Repo
    Repo --> Db
    SubmitSvc --> ApiScheduler
    ApiScheduler --> Job

    Job --> Counter
    Counter --> Redis
    Job --> Shipping
    Job --> Coating
    Shipping -->|"HTTP POST /slots/reserve"| ShippingApi[(Shipping API)]
    Coating -->|"HTTP GET /coating/{orderId}"| CoatingApi[(Coating API)]

    Job --> Repo
    Job --> Publisher
    Publisher --> Kafka

    Override --> Db
    Report --> RawSql
    RawSql --> Db
    ReportSvc --> RawSql
```

## Source Anchors
- API endpoints and startup wiring: `RxFlow.Api/Program.cs`
- Order entry flow: `RxFlow.Application/Orders/OrderSubmissionService.cs`
- Pricing flow: `RxFlow.Application/Pricing/ComplexPricingEngine.cs`
- Lab routing flow: `RxFlow.Application/Labs/StaticPriorityLabRoutingService.cs`
- Background processing flow: `RxFlow.Workers/Processing/OrderProcessingJob.cs`
- Repository and persistence: `RxFlow.Infrastructure/Persistence/*.cs`
- Redis/Kafka/connectors/reporting: `RxFlow.Infrastructure/Redis/*.cs`, `RxFlow.Infrastructure/Messaging/*.cs`, `RxFlow.Infrastructure/Connectors/*.cs`, `RxFlow.Infrastructure/Reporting/*.cs`
