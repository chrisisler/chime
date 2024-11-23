using ChimeCore.Data;
using ChimeCore.Routes;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string[] allowedOrigins = { "http://localhost:5173", "https://trychime.vercel.app" };

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(
        "AllowSpecifiedOrigin",
        builder => builder
            .WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddEnvironmentVariables().AddJsonFile("appsettings.Development.json");
}
var connstr = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(connstr, sqlOpts =>
    {
        sqlOpts.EnableRetryOnFailure(maxRetryCount: 3);
    }),
    ServiceLifetime.Scoped
);

builder.Services.AddSingleton<BlobServiceClient>(provider =>
    {
        var cfg = provider.GetRequiredService<IConfiguration>();
        return new BlobServiceClient(cfg["AzureBlobStorage:ConnectionString"]);
    });

/* builder.Services.AddSingleton<IConfiguration>(provider => */
/*     { */
/*         return provider.GetRequiredService<IConfiguration>(); */
/*     }); */

builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecifiedOrigin");
app.UseAntiforgery();

app.UseHttpsRedirection();

app.MapItemsRoutes();

app.MapGet("/health-check", () =>
{
    return TypedResults.Ok("hello world!");
})
.WithName("HealthCheck")
.WithOpenApi();


app.Run();
