using ChimeCore.Data;
using ChimeCore.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connstr = String.Empty;
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
    connstr = builder.Configuration.GetConnectionString("DefaultConnection");
}
else
{
    connstr = Environment.GetEnvironmentVariable("DefaultConnection");
}
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(connstr, sqlOpts =>
    {
        sqlOpts.EnableRetryOnFailure(maxRetryCount: 3);
    }),
    ServiceLifetime.Scoped
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/health-check", () =>
{
    return "hello world!";
})
.WithName("HealthCheck")
.WithOpenApi();

// Endpoints

app.MapGet("/Chimes", (ApplicationDbContext ctx) =>
        ctx.Chimes.ToListAsync()
    )
    .WithName("GetChimes")
    .WithOpenApi();

app.MapPost("/Chimes", (Chime chime, ApplicationDbContext ctx) =>
    {
        var entry = ctx.Chimes.Add(chime);
        ctx.SaveChanges();
        return Results.Created($"/Chime/{chime.Id}", chime);
    })
    .WithName("CreateChime")
    .WithOpenApi();

app.MapGet("/Chimes/{id}", async (int id, ApplicationDbContext ctx) =>
        await ctx.Chimes.FindAsync(id) is Chime chime
            ? Results.Ok(chime)
            : Results.NotFound()
    )
    .WithName("GetChimeById")
    .WithOpenApi();


app.Run();
