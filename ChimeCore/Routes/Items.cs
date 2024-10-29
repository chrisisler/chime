using ChimeCore.Models;
using Microsoft.EntityFrameworkCore;
using ChimeCore.Data;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace ChimeCore.Routes
{
    public static class ItemsRoutes
    {
        public static void MapItemsRoutes(this IEndpointRouteBuilder app)
        {
            var itemsRouter = app.MapGroup("/v0/Items").WithOpenApi().DisableAntiforgery();

            itemsRouter.MapPost("/", CreateItem).WithName("CreateItem");
            itemsRouter.MapGet("/", GetAllItems).WithName("GetAllItems");
            itemsRouter.MapGet("/{id}", GetItemById).WithName("GetItemById");
            itemsRouter.MapPut("/{id}", UpdateItem).WithName("UpdateItem");
            itemsRouter.MapDelete("/{id}", DeleteItem).WithName("DeleteItem");

            // TODO filters
            static async Task<IResult> GetAllItems(ApplicationDbContext ctx, CancellationToken cancellationToken)
            {
                return TypedResults.Ok(
                    await ctx.Items
                        .Where(_ => _.Deleted == false)
                        .OrderByDescending(_ => _.Time)
                        .ToListAsync(cancellationToken)
                );
            }

            static async Task<IResult> CreateItem(
                [FromForm] string by,
                [FromForm] int byId,
                [FromForm] string text,
                [FromForm] int? parentId,
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

                    await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

                    var blobClient = blobContainerClient.GetBlobClient(file.FileName);

                    await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true, cancellationToken);

                    mediaUrl = blobClient.Uri.ToString();
                }

                Item item = parentId == null
                    ? new Chime(by, byId, text, [], mediaUrl)
                    : new Comment(by, byId, text, parentId, [], mediaUrl);

                ctx.Items.Add(item);
                await ctx.SaveChangesAsync(cancellationToken);

                if (item.Type == "comment" && item.ParentId is int _parentId)
                {
                    Item? parentChime = await ctx.Items.FindAsync(
                        _parentId,
                        cancellationToken
                    );
                    if (parentChime == null)
                    {
                        return TypedResults.Problem("Creating comment using invalid parent ID");
                    }
                    /* var _item = await ctx.Items.FindAsync(item, cancellationToken); */

                    parentChime.Kids = parentChime.Kids.ToList().Append(item.Id).ToArray();
                    ctx.Items.Update(parentChime);

                    await ctx.SaveChangesAsync(cancellationToken);
                }

                return TypedResults.Created($"/Item/{item.Id}", item);
            }

            static async Task<IResult> GetItemById(int id, ApplicationDbContext ctx, CancellationToken cancellationToken)
            {
                return await ctx.Items.Where(_ => _.Deleted == false && _.Id == id).Distinct().FirstOrDefaultAsync(cancellationToken) is Item item
                    ? TypedResults.Ok(item)
                    : TypedResults.NotFound("Failed to find Item with ID: " + id);
            }

            static async Task<IResult> UpdateItem(
                int id,
                ItemDTO itemDTO,
                ApplicationDbContext ctx,
                CancellationToken cancellationToken
            )
            {
                var item = await ctx.Items.Distinct().FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false, cancellationToken);
                if (item is null)
                {
                    return TypedResults.NotFound("Failed to find Item with ID: " + id);
                }

                item.Text = itemDTO.Text;
                item.Kids = itemDTO.Kids;

                await ctx.SaveChangesAsync(cancellationToken);

                return TypedResults.Ok(item);
            }

            static async Task<IResult> DeleteItem(int id, ApplicationDbContext ctx, CancellationToken cancellationToken)
            {
                var item = await ctx.Items.Distinct().FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false, cancellationToken);
                if (item is null)
                {
                    return TypedResults.NotFound("Failed to find Item with ID: " + id);
                }

                item.Deleted = true;

                await ctx.SaveChangesAsync(cancellationToken);

                return TypedResults.NoContent();
            }

        }
    }
}
