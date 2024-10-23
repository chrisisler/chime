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

app.MapGet("/Chimes", async (ApplicationDbContext ctx) =>
        await ctx.Chimes.ToListAsync()
    )
    .WithName("GetAllChimes")
    .WithOpenApi();

/* app.MapGet("/Chimes/{username}", async (string username, ApplicationDbContext ctx) => */ 
/*     await ctx.Chimes.Where(_ => _.By.ToLower().Contains(username.ToLower())).ToListAsync()); */

app.MapPost("/Chimes", async (Chime chime, ApplicationDbContext ctx) =>
    {
        ctx.Chimes.Add(chime);
        await ctx.SaveChangesAsync();

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

app.MapPut("/Chimes/{id}", async (int id, Chime fields, ApplicationDbContext ctx) =>
    {
        var chime = await ctx.Chimes.FindAsync(id);
        if (chime is null)
        {
            return Results.NotFound();
        }

        chime.Text = fields.Text;
        chime.MediaUrl = fields.MediaUrl;
        chime.Kids = fields.Kids;

        // this works? `FindAsync` returns a proxied obj???
        await ctx.SaveChangesAsync();

        return Results.Ok(chime);
    })
    .WithName("UpdateChime")
    .WithOpenApi();

app.MapDelete("/Chimes/{id}", async (int id, ApplicationDbContext ctx) =>
    {
        var chime = await ctx.Chimes.FindAsync(id);
        if (chime is null)
        {
            return Results.NotFound();
        }

        chime.Deleted = true;

        await ctx.SaveChangesAsync();

        return Results.NoContent();
    })
    .WithName("DeleteChime")
    .WithOpenApi();

app.Run();
