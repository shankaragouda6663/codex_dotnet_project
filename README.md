# RxFlow Training Repository

RxFlow simulates a prescription-to-lens order flow with routing, pricing, background processing, and shipment integration.

## Quick start

```bash
docker compose up -d
```

```bash
dotnet restore --configfile NuGet.Config
```

```bash
dotnet build -warnaserror
```

```bash
dotnet test
```

## Development

Run the API:

```bash
dotnet run --project RxFlow.Api/RxFlow.Api.csproj
```

## Useful commands

```bash
dotnet ef database update --project RxFlow.Infrastructure --startup-project RxFlow.Api
```

```bash
dotnet run --project RxFlow.Legacy.Console/RxFlow.Legacy.Console.csproj
```