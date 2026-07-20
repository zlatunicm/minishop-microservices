// ─────────────────────────────────────────────────────────────────────────────
//  API GATEWAY (YARP)  —  http://localhost:5100
//
//  The single door into the system. Clients talk ONLY to the gateway; it
//  forwards by path:   /products*  ->  ProductService (5101)
//                      /orders*    ->  OrderService  (5102)
//
//  Why a gateway: one public address & TLS endpoint, central place for auth,
//  rate limiting, logging, CORS — and services can move/scale behind it
//  without any client noticing. Routing rules live in appsettings.json.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// A tiny landing page so hitting the gateway root explains the system:
app.MapGet("/", () => Results.Text(
    """
    MiniShop gateway (YARP)
      GET  /products        -> ProductService (5101)
      GET  /products/{id}   -> ProductService (5101)
      GET  /orders          -> OrderService  (5102)
      POST /orders          -> OrderService  (5102)
    """));

app.MapReverseProxy();      // everything else: forwarded per appsettings.json

app.Run("http://localhost:5100");
