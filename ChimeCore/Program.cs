using ChimeCore.Data;
using ChimeCore.Models;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string[] allowedOrigins = { "http://localhost:5173" };

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(
        "AllowSpecifiedOrigin",
        builder => builder
            .WithOrigins(allowedOrigins)
            /* .AllowAnyOrigin() // testing */
            .AllowAnyMethod()
            .AllowAnyHeader()
    );
});

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

app.MapGet("/health-check", () =>
{
    return TypedResults.Ok("hello world!");
})
.WithName("HealthCheck")
.WithOpenApi();

var chimesRouter = app.MapGroup("/Chimes").WithOpenApi().DisableAntiforgery();

chimesRouter.MapPost("/", CreateChime).WithName("CreateChime");
chimesRouter.MapGet("/", GetAllChimes).WithName("GetAllChimes");
chimesRouter.MapGet("/{id}", GetChimeById).WithName("GetChimeById");
chimesRouter.MapPut("/{id}", UpdateChime).WithName("UpdateChime");
chimesRouter.MapDelete("/{id}", DeleteChime).WithName("DeleteChime");

/* chimesRouter.MapGet("/{username}", async (string username, ApplicationDbContext ctx) => */
/*     await ctx.Chimes.Where(_ => _.By.ToLower().Contains(username.ToLower())).ToListAsync()); */

static async Task<IResult> GetAllChimes(ApplicationDbContext ctx)
{
    return TypedResults.Ok(
        await ctx.Chimes.Where(_ => _.Deleted == false).OrderByDescending(_ => _.Time).ToListAsync()
    );
}

static async Task<IResult> CreateChime(
    [FromForm] string by,
    [FromForm] int byId,
    [FromForm] string text,
    IFormFile? file,
    ApplicationDbContext ctx,
    BlobServiceClient blobServiceClient,
    IConfiguration cfg,
    CancellationToken cancellationToken
)
{
    string? mediaUrl = null;

    // upload and get shareable url from azure storage
    if (file != null)
    {
        var containerName = cfg["AzureBlobStorage:ContainerName"];
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.CreateIfNotExistsAsync();

        var blobClient = blobContainerClient.GetBlobClient(file.FileName);

        await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true, cancellationToken);

        mediaUrl = blobClient.Uri.ToString();
    }

    var chime = new Chime(by, byId, text, kids: [], mediaUrl);

    ctx.Chimes.Add(chime);
    await ctx.SaveChangesAsync();

    return TypedResults.Created($"/Chime/{chime.Id}", chime);
}

static async Task<IResult> GetChimeById(int id, ApplicationDbContext ctx)
{
    return await ctx.Chimes.Where(_ => _.Deleted == false && _.Id == id).FirstOrDefaultAsync() is Chime chime
        ? TypedResults.Ok(chime)
        : TypedResults.NotFound("Failed to find Item with ID: " + id);
}

static async Task<IResult> UpdateChime(int id, ChimeDTO chimeDTO, ApplicationDbContext ctx)
{
    var chime = await ctx.Chimes.SingleOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
    if (chime is null)
    {
        return TypedResults.NotFound("Failed to find Item with ID: " + id);
    }

    chime.Text = chimeDTO.Text;
    chime.Kids = chimeDTO.Kids;

    await ctx.SaveChangesAsync();

    return TypedResults.Ok(chime);
}

static async Task<IResult> DeleteChime(int id, ApplicationDbContext ctx)
{
    var chime = await ctx.Chimes.SingleOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
    if (chime is null)
    {
        return TypedResults.NotFound("Failed to find Item with ID: " + id);
    }

    chime.Deleted = true;

    await ctx.SaveChangesAsync();

    return TypedResults.NoContent();
}

app.Run();
