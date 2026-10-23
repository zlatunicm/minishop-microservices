# MiniShop — a Minimal Microservices Example (.NET 8)

The finale (**part 11**) of a hands-on C# practice series. Three tiny, independently runnable processes that demonstrate the core microservice ideas **with zero infrastructure** — no Docker, no queues, nothing to install beyond the .NET SDK:

```
                        ┌─────────────────────┐
   client (curl)  ───►  │  Gateway (YARP)      │   http://localhost:5100
                        │  /products* /orders* │
                        └────────┬─────────┬───┘
                                 │         │
                    ┌────────────▼──┐   ┌──▼─────────────┐
                    │ ProductService │◄──┤  OrderService  │
                    │     :5101      │   │     :5102      │
                    │  owns products │   │  owns orders   │
                    └────────────────┘   └────────────────┘
                         (OrderService calls ProductService
                          over HTTP to validate & price orders)
```

## What it demonstrates

| Concept | Where |
|---|---|
| One service = one process that **owns its data** | each service has a private in-memory list |
| **Service-to-service HTTP** call | `OrderService` validates products via `IHttpClientFactory` |
| **API gateway** as the single entry point | `Gateway` with [YARP](https://microsoft.github.io/reverse-proxy/) routing by path |
| Contracts, not shared code | `OrderService` has its own copy of the `Product` record shape |
| **Resilience basics** | timeout on the client + `503 Service Unavailable` when the dependency is down |
| Health endpoints | `GET /health` on both services |

## Running (three terminals)

```bash
dotnet run --project ProductService    # terminal 1 → :5101
dotnet run --project OrderService      # terminal 2 → :5102
dotnet run --project Gateway           # terminal 3 → :5100
```

## Try it

```bash
curl http://localhost:5100/products
curl -X POST http://localhost:5100/orders \
     -H "Content-Type: application/json" \
     -d '{"productId": 2, "quantity": 3}'
# → {"id":1,"productId":2,"productName":"kolac","quantity":3,"total":10.50}

curl http://localhost:5100/orders

# resilience: stop ProductService (Ctrl+C in terminal 1), then POST again:
# → 503 {"title":"Product service nedostupan", ...}
```

## Where a real system goes next

This example stops deliberately short. The next steps in production systems, in rough order:

1. **Configuration/service discovery** instead of hardcoded `localhost` ports (env vars → Consul/Kubernetes DNS → .NET Aspire).
2. **Resilience policies** — retries with backoff, circuit breakers (`Microsoft.Extensions.Http.Resilience`).
3. **A database per service** (each service its own schema/DB — never shared).
4. **Async messaging** for events (RabbitMQ/Kafka): "OrderPlaced" published, others subscribe — removes the synchronous coupling.
5. **Observability** — structured logs, distributed tracing (OpenTelemetry), metrics.
6. **Containers & orchestration** — Dockerfiles, compose, then Kubernetes when scale demands it.

## Further reading

- [.NET microservices architecture e-book](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/)
- [YARP documentation](https://microsoft.github.io/reverse-proxy/)
- [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis)

## The series

← Previous: [10 Files & JSON](https://github.com/zlatunicm/csharp-files-json-examples)

| Part | Topic |
|---|---|
| 01 | [Collections](https://github.com/zlatunicm/csharp-collections-examples) |
| 02 | [LINQ](https://github.com/zlatunicm/csharp-linq-examples) |
| 03 | [Delegates & events](https://github.com/zlatunicm/csharp-delegates-events-examples) |
| 04 | [Async/await](https://github.com/zlatunicm/csharp-async-examples) |
| 05 | [Generics](https://github.com/zlatunicm/csharp-generics-examples) |
| 06 | [Types & memory](https://github.com/zlatunicm/csharp-types-memory-examples) |
| 07 | [Exceptions](https://github.com/zlatunicm/csharp-exceptions-examples) |
| 08 | [Modern C# 8–12](https://github.com/zlatunicm/csharp-modern-examples) |
| 09 | [Strings & text](https://github.com/zlatunicm/csharp-strings-examples) |
| 10 | [Files & JSON](https://github.com/zlatunicm/csharp-files-json-examples) |
| 11 | **MiniShop microservices (finale)** (this repo) |

## Author

**Matej Zlatunić**

## License

Released under the [MIT License](LICENSE). Feel free to use it for learning.
