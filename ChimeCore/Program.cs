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

var chimesRouter = app.MapGroup("/Chimes");

chimesRouter.MapGet("/", GetAllChimes).WithName("GetAllChimes").WithOpenApi();
chimesRouter.MapPost("/", CreateChime).WithName("CreateChime").WithOpenApi();
chimesRouter.MapGet("/{id}", GetChimeById).WithName("GetChimeById").WithOpenApi();
chimesRouter.MapPut("/{id}", UpdateChime).WithName("UpdateChime").WithOpenApi();
chimesRouter.MapDelete("/{id}", DeleteChime).WithName("DeleteChime").WithOpenApi();

/* chimesRouter.MapGet("/{username}", async (string username, ApplicationDbContext ctx) => */
/*     await ctx.Chimes.Where(_ => _.By.ToLower().Contains(username.ToLower())).ToListAsync()); */

static async Task<IResult> GetAllChimes(ApplicationDbContext ctx)
{
    return TypedResults.Ok(await ctx.Chimes.ToListAsync());
}

static async Task<IResult> CreateChime(Chime chime, ApplicationDbContext ctx)
{
    ctx.Chimes.Add(chime);
    await ctx.SaveChangesAsync();

    return TypedResults.Created($"/Chime/{chime.Id}", chime);
}

static async Task<IResult> GetChimeById(int id, ApplicationDbContext ctx)
{
    return await ctx.Chimes.FindAsync(id) is Chime chime
        ? TypedResults.Ok(chime)
        : TypedResults.NotFound();
}

static async Task<IResult> UpdateChime(int id, Chime fields, ApplicationDbContext ctx)
{
    var chime = await ctx.Chimes.FindAsync(id);
    if (chime is null)
    {
        return TypedResults.NotFound();
    }

    chime.Text = fields.Text;
    chime.MediaUrl = fields.MediaUrl;
    chime.Kids = fields.Kids;

    // this works? `FindAsync` returns a proxied obj???
    await ctx.SaveChangesAsync();

    return TypedResults.Ok(chime);
}

static async Task<IResult> DeleteChime(int id, ApplicationDbContext ctx)
{
    var chime = await ctx.Chimes.FindAsync(id);
    if (chime is null)
    {
        return TypedResults.NotFound();
    }

    chime.Deleted = true;

    await ctx.SaveChangesAsync();

    return TypedResults.NoContent();
}

app.Run();
