// ─────────────────────────────────────────────────────────────────────────────
//  PRODUCT SERVICE  —  http://localhost:5101
//
//  One microservice = one small, independently runnable process that OWNS its
//  data. Nobody reaches into our product list directly — they ask over HTTP.
//  This is an ASP.NET Core "minimal API": routing + JSON with almost no code.
// ─────────────────────────────────────────────────────────────────────────────

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// This service's PRIVATE data. In a real system this would be its own database
// (one database PER service — sharing a DB couples services at the hip).
var products = new List<Product>
{
    new(1, "kava",    1.70m),
    new(2, "kolac",   3.50m),
    new(3, "sendvic", 4.20m),
    new(4, "sok",     2.00m),
};

// GET /products — the whole catalog.
app.MapGet("/products", () => products);

// GET /products/3 — one product, or 404. Route constraint '{id:int}' rejects
// junk like /products/abc before our code even runs.
app.MapGet("/products/{id:int}", (int id) =>
    products.FirstOrDefault(p => p.Id == id) is { } product
        ? Results.Ok(product)
        : Results.NotFound(new { error = $"proizvod {id} ne postoji" }));

// Health endpoint — orchestrators (and our gateway's curiosity) ping this.
app.MapGet("/health", () => new { status = "healthy", service = "products" });

app.Run("http://localhost:5101");

// The service's contract. 'record' = perfect JSON DTO (project 08).
record Product(int Id, string Name, decimal Price);
