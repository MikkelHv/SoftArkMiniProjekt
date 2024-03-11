using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;


using Data;
using Service;
using Model;


var builder = WebApplication.CreateBuilder(args);

// Sætter CORS så API'en kan bruges fra andre domæner
var AllowSomeStuff = "_AllowSomeStuff";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowSomeStuff, builder => {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

// Tilføj DbContext factory som service.
builder.Services.AddDbContext<BoardContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ContextSQLite")));
    
// Tilføj DataService så den kan bruges i endpoints
builder.Services.AddScoped<DataService>();

var app = builder.Build();

// Seed data hvis nødvendigt.
using (var scope = app.Services.CreateScope())
{
    var dataService = scope.ServiceProvider.GetRequiredService<DataService>();
    var db = scope.ServiceProvider.GetRequiredService<BoardContext>(); // Tilføj dette
    dataService.SeedData(); // Fylder data på, hvis databasen er tom. Ellers ikke.
}

app.UseHttpsRedirection();
app.UseCors(AllowSomeStuff);

// Middlware der kører før hver request. Sætter ContentType for alle responses til "JSON".
app.Use(async (context, next) =>
{
    context.Response.ContentType = "application/json; charset=utf-8";
    await next(context);
});

// DataService fås via "Dependency Injection" (DI)
//  Get, tjek om der er forbindelse, "Hello World"
app.MapGet("/", (DataService service) =>
{
    return new { message = "Hello World!" };
});

//  Get alle posts
app.MapGet("/api/posts/", (DataService service) =>
{
    return service.GetPosts();
    
});

//  Get specifik post via ID
app.MapGet("/api/posts/{id}", (DataService service, int id) =>
{
    return service.GetPost(id);
});

// Post, opret ny opslag
// Se Notes.txt for json format til postman
app.MapPost("/api/posts", async (DataService service, PostData postData) =>
{
    var createdPost = await service.CreatePost(postData.Title, postData.Content, postData.Upvotes, postData.Downvotes, postData.User.Id);
    return Results.Created($"/api/posts/{createdPost.Id}", createdPost);
});


// Post, tilføj comments til opslag
// Se Notes.txt for json format til postman
app.MapPost("/api/posts/{id}/comments/", async (DataService service, int id, CommentData commentData) =>
{
    var updatedPost = await service.AddComment(id, commentData.Content, commentData.Upvotes, commentData.Downvotes, commentData.User.Id);
    return Results.Created($"/api/posts/{id}/comments", updatedPost);

});

// Put, post upvote
app.MapPut("/api/posts/{id}/upvote/", async (DataService service, int id) =>
{
    var upvotedPost = await service.PostUpvote(id);
    return Results.Ok(upvotedPost);
});

// put, post downvote
app.MapPut("/api/posts/{id}/downvote/", async (DataService service, int id) =>
{
    var downvotedPost = await service.PostDownvote(id);
    return Results.Ok(downvotedPost);
});

//put, comment upvotes
app.MapPut("/api/posts/{id}/comments/{commentId}/upvote", async (DataService service, int id, int commentId) =>
{
    var upvotedComment = await service.CommentUpvote(id, commentId);
    return Results.Ok(upvotedComment);
});

// put, comment downvote
app.MapPut("/api/posts/{id}/comments/{commentId}/downvote", async (DataService service, int id, int commentId) =>
{
    var downvotedComment = await service.CommentDownvote(id, commentId);
    return Results.Ok(downvotedComment);
});


app.Run();

public record PostData(string Title, string Content, int Upvotes, int Downvotes, UserData User);
public record UserData(int Id, string Username);
public record CommentData(string Content, int Upvotes, int Downvotes, UserData User);
