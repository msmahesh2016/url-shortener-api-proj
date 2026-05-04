using Microsoft.EntityFrameworkCore;
using url_shortener_api.Data;
using url_shortener_api.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SqlServer")));

var dbPath = @"C:\temp\urls.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("allow", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("allow");

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

string GenerateShortCode()
{
    const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();

    return new string(Enumerable.Repeat(chars, 6)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}


async Task<string> GenerateUniqueCode(AppDbContext db)
{
    string code;
    do
    {
        code = GenerateShortCode();
    }
    while (await db.Urls.AnyAsync(x => x.ShortCode == code));

    return code;
}


app.MapPost("/api/url", async (AppDbContext db, UrlMapping req) =>
{
    req.ShortCode = await GenerateUniqueCode(db);
    req.CreatedAt = DateTime.UtcNow;

    db.Urls.Add(req);
    await db.SaveChangesAsync();

    return Results.Ok(req);
});

app.MapGet("/api/url/public", async (AppDbContext db) =>
{
    return await db.Urls
        .Where(x => !x.IsPrivate)
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();
});

app.MapGet("/api/url/search", async (AppDbContext db, string term) =>
{
    return await db.Urls
        .Where(x => x.OriginalUrl.Contains(term))
        .ToListAsync();
});

app.MapDelete("/api/url/{id}", async (AppDbContext db, int id) =>
{
    var url = await db.Urls.FindAsync(id);
    if (url == null) return Results.NotFound();

    db.Urls.Remove(url);
    await db.SaveChangesAsync();

    return Results.Ok();
});

app.MapGet("/{code}", async (AppDbContext db, string code) =>
{
    var url = await db.Urls.FirstOrDefaultAsync(x => x.ShortCode == code);

    if (url == null)
        return Results.NotFound();

    url.ClickCount++;
    await db.SaveChangesAsync();

    return Results.Redirect(url.OriginalUrl);
});

app.Run();