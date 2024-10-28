using ChimeCore.Models;
using Microsoft.EntityFrameworkCore;
using ChimeCore.Data;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;

namespace ChimeCore.Routes
{
    public static class CommentsRoutes
    {
        public static void MapCommentsRoutes(this IEndpointRouteBuilder app)
        {
            var commentsRouter = app.MapGroup("/Comments").WithOpenApi().DisableAntiforgery();

            commentsRouter.MapPost("/", CreateComment).WithName("CreateComment");
            commentsRouter.MapGet("/", GetAllComments).WithName("GetAllComments");
            /* commentsRouter.MapGet("/{id}", GetComment).WithName("GetComment"); */
            commentsRouter.MapPut("/{id}", UpdateComment).WithName("UpdateComment");
            commentsRouter.MapDelete("/{id}", DeleteComment).WithName("DeleteComment");

            /* commentsRouter.MapGet("/{username}", async (string username, ApplicationDbContext ctx) => */
            /*     await ctx.Comments.Where(_ => _.By.ToLower().Contains(username.ToLower())).ToListAsync()); */
        }

        static async Task<IResult> GetAllComments(ApplicationDbContext ctx)
        {
            var comments = await ctx.Comments
                .Where(_ => _.Deleted == false)
                .OrderByDescending(_ => _.Time)
                .ToListAsync();

            return TypedResults.Ok(comments);
        }

        static async Task<IResult> CreateComment(
            [FromForm] string by,
            [FromForm] int byId,
            [FromForm] string text,
            [FromForm] int parentId,
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

                string blobName = by + " " + file.FileName;
                var blobClient = blobContainerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(file.OpenReadStream(), overwrite: true, cancellationToken);

                mediaUrl = blobClient.Uri.ToString();
            }

            var comment = new Comment(by, byId, text, parentId, kids: [], mediaUrl);

            ctx.Comments.Add(comment);
            await ctx.SaveChangesAsync();

            return TypedResults.Created($"/Comment/{comment.Id}", comment);
        }

        /* static async Task<IResult> GetComment(int id, ApplicationDbContext ctx) */
        /* { */
        /*     return await ctx.Comments.Where(_ => _.Deleted == false && _.Id == id).FirstOrDefaultAsync() is Comment comment */
        /*         ? TypedResults.Ok(comment) */
        /*         : TypedResults.NotFound("Failed to find Item with ID: " + id); */
        /* } */

        static async Task<IResult> UpdateComment(int id, Comment commentDTO, ApplicationDbContext ctx)
        {
            var comment = await ctx.Comments.FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
            if (comment is null)
            {
                return TypedResults.NotFound("Failed to find Item with ID: " + id);
            }

            comment.Text = commentDTO.Text;
            comment.Kids = commentDTO.Kids;

            await ctx.SaveChangesAsync();

            return TypedResults.Ok(comment);
        }

        static async Task<IResult> DeleteComment(int id, ApplicationDbContext ctx)
        {
            var comment = await ctx.Comments.FirstOrDefaultAsync(_ => _.Id == id && _.Deleted == false);
            if (comment is null)
            {
                return TypedResults.NotFound("Failed to find Item with ID: " + id);
            }

            comment.Deleted = true;

            await ctx.SaveChangesAsync();

            return TypedResults.NoContent();
        }
    }
}

