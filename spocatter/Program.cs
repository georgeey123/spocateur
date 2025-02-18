using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Text.Json;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapPost("/api/prune", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    var jsonDocument = JsonDocument.Parse(body);
    var url = jsonDocument.RootElement.GetProperty("url").GetString();

    if (string.IsNullOrEmpty(url))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = "Invalid URL" });
        return;
    }

    var prunedUrl = PruneUrl(url);

    await context.Response.WriteAsJsonAsync(new { prunedUrl });
});

app.MapFallbackToFile("index.html");

app.Run();

static string PruneUrl(string url)
{
    var uri = new Uri(url);
    return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
}