// ─────────────────────────────────────────────────────────────────────────────
//  ORDER SERVICE  —  http://localhost:5102
//
//  Owns ORDERS — and knows NOTHING about products except how to ASK the
//  Product service over HTTP. That call is the heart of microservices:
//  service-to-service communication instead of a shared database.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);

// A named HttpClient from the factory — the production-correct way (pooled
// handlers, no socket exhaustion; see project 06's HttpClient note).
builder.Services.AddHttpClient("products", client =>
{
    // In real systems this address comes from configuration / service
    // discovery (Consul, k8s DNS...) — hardcoded here to stay focused.
    client.BaseAddress = new Uri("http://localhost:5101");
    client.Timeout = TimeSpan.FromSeconds(3);       // never trust the network
});

var app = builder.Build();

// This service's PRIVATE data: the orders.
var orders = new List<Order>();
var nextId = 1;

// GET /orders — everything ordered so far.
app.MapGet("/orders", () => orders);

// POST /orders  { "productId": 2, "quantity": 3 }
// The interesting one: we VALIDATE the product by calling the other service.
app.MapPost("/orders", async (NewOrder request, IHttpClientFactory httpFactory) =>
{
    if (request.Quantity is < 1 or > 100)
        return Results.BadRequest(new { error = "kolicina mora biti 1-100" });

    var client = httpFactory.CreateClient("products");

    // ── The service-to-service call ──────────────────────────────────────────
    HttpResponseMessage response;
    try
    {
        response = await client.GetAsync($"/products/{request.ProductId}");
    }
    catch (HttpRequestException)
    {
        // The dependency is DOWN. A microservice must answer something sane —
        // 503 tells the caller "try later", not a mystery 500.
        return Results.Problem(
            title: "Product service nedostupan",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (!response.IsSuccessStatusCode)
        return Results.BadRequest(new { error = $"proizvod {request.ProductId} ne postoji" });

    var product = (await response.Content.ReadFromJsonAsync<Product>())!;

    // Price is computed HERE, from data the owning service gave us NOW —
    // we don't cache someone else's truth without a reason.
    var order = new Order(nextId++, product.Id, product.Name, request.Quantity,
                          Total: product.Price * request.Quantity);
    orders.Add(order);

    return Results.Created($"/orders/{order.Id}", order);
});

app.MapGet("/orders/{id:int}", (int id) =>
    orders.FirstOrDefault(o => o.Id == id) is { } order
        ? Results.Ok(order)
        : Results.NotFound());

app.MapGet("/health", () => new { status = "healthy", service = "orders" });

app.Run("http://localhost:5102");

// Contracts. NewOrder = what clients SEND; Order = what we STORE and return.
record NewOrder(int ProductId, int Quantity);
record Order(int Id, int ProductId, string ProductName, int Quantity, decimal Total);

// A local copy of the Product CONTRACT — services share JSON shapes, not code.
// (Bigger systems generate these from OpenAPI or keep a contracts package.)
record Product(int Id, string Name, decimal Price);
