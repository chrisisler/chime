using ChimeCore.Models;
using Microsoft.EntityFrameworkCore;
using ChimeCore.Data;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace ChimeCore.Routes
{
    public static class ChimesRoutes
    {
        public static void MapChimesRoutes(this IEndpointRouteBuilder app)
        {
            var chimesRouter = app.MapGroup("/Chimes").WithOpenApi().DisableAntiforgery();

            chimesRouter.MapPost("/", CreateChime).WithName("CreateChime");
            chimesRouter.MapGet("/", GetAllChimes).WithName("GetAllChimes");
            chimesRouter.MapGet("/{id}", GetChimeById).WithName("GetChimeById");
            chimesRouter.MapPut("/{id}", UpdateChime).WithName("UpdateChime");
            chimesRouter.MapDelete("/{id}", DeleteChime).WithName("DeleteChime");

            /* chimesRouter.MapGet("/{username}", async (string username, ApplicationDbContext ctx) => */
            /*     await ctx.Chimes.Where(_ => _.By.ToLower().Contains(username.ToLower())).ToListAsync()); */

            static async Task<IResult> GetAllChimes(ApplicationDbContext ctx, CancellationToken cancellationToken)
            {
                return TypedResults.Ok(
                    await ctx.Chimes
                        .Where(_ => _.Deleted == false)
                        .OrderByDescending(_ => _.Time)
                        .ToListAsync(cancellationToken)
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

                var res = ctx.Chimes.Add(chime);
                await ctx.SaveChangesAsync(cancellationToken);

                return TypedResults.Created($"/Chime/{chime.Id}", chime);
            }

            static async Task<IResult> GetChimeById(int id, ApplicationDbContext ctx)
            {
                return await ctx.Chimes.Where(_ => _.Deleted == false && _.Id == id).FirstOrDefaultAsync() is Chime chime
                    ? TypedResults.Ok(chime)
                    : TypedResults.NotFound("Failed to find Item with ID: " + id);
            }

            static async Task<IResult> UpdateChime(
                int id,
                ChimeDTO chimeDTO,
                ApplicationDbContext ctx,
                CancellationToken cancellationToken
            )
            {
                var chime = await ctx.Chimes.FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
                if (chime is null)
                {
                    return TypedResults.NotFound("Failed to find Item with ID: " + id);
                }

                chime.Text = chimeDTO.Text;
                chime.Kids = chimeDTO.Kids;

                await ctx.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok(chime);
            }

            static async Task<IResult> DeleteChime(int id, ApplicationDbContext ctx, CancellationToken cancellationToken)
            {
                var chime = await ctx.Chimes.FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
                if (chime is null)
                {
                    return TypedResults.NotFound("Failed to find Item with ID: " + id);
                }

                chime.Deleted = true;

                await ctx.SaveChangesAsync(cancellationToken);

                return TypedResults.NoContent();
            }

        }
    }
}
