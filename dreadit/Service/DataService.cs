using Microsoft.EntityFrameworkCore;

using Data;
using Model;

namespace Service;

public class DataService
{
    private BoardContext db {get; }

    public DataService(BoardContext db) {
        this.db = db;
    }

    public void SeedData() 
    {
        User user = db.Users!.FirstOrDefault()!;
        if (user == null) {
            db.Users!.Add(new User {Username = "Mikkel"});
            db.Users.Add(new User {Username = "Anna"});
            db.SaveChanges();
        }

        Post post = db.Posts!.FirstOrDefault()!;
        if(post == null) {
            var users = db.Users!.ToList(); // Hent alle brugere fra databasen

        for (int i = 0; i < 5; i++)
        foreach (var bruger in users)
        {
            db.Posts!.Add(new Post
            {
                Title = $"Post {i + 1} af {bruger.Username}", 
                Content = $"Indhold af post {i + 1} af {bruger.Username}",
                Upvotes = 0,
                Downvotes = 0,
                User = bruger
            });
        }
        }
        db.SaveChanges();

        //ds        
    }

    //Get all posts
     public List<Post> GetPosts() {
        return db.Posts!.Include(p => p.User).ToList();
    }

    // Get specifik post
    public Post GetPost(int id) {
        return db.Posts!.Include(p => p.User).FirstOrDefault(p => p.Id == id)!;
    }

    //  HTTP Post, opret en ny post
    public async Task<Post> CreatePost(string title, string content, int upvotes, int downvotes, int userId)
    {
        var user = await db.Users!.FirstOrDefaultAsync(u => u.Id == userId);
        var post = new Post
        {
            Title = title,
            Content = content,
            Upvotes = upvotes,
            Downvotes = downvotes,
            User = user

        };
        
        db.Posts!.Add(post);
        await db.SaveChangesAsync();
        return post;
    }

    // post, comment

    public async Task<Post> AddComment(int postId, string content, int upvotes, int downvotes, int userId)
    {
        var user = await db.Users!.FirstOrDefaultAsync(u => u.Id == userId);
        var post = await db.Posts!.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == postId);
        var comment = new Comment
        {
            Content = content,
            Upvotes = upvotes,
            Downvotes = downvotes,
            User = user
        };
    
        post!.Comments.Add(comment);
        await db.SaveChangesAsync();
        return post;
    }

    // Put, post upvote
    public async Task<Post> PostUpvote(int postId)
    {
        var post = await db.Posts!.FirstOrDefaultAsync(p => p.Id == postId);
        post!.Upvotes ++;
        await db.SaveChangesAsync();
        return post;
    }

    // Put, post downvote
    public async Task<Post> PostDownvote(int postId)
    {
        var post = await db.Posts!.FirstOrDefaultAsync(p => p.Id == postId);
        post!.Downvotes ++;
        await db.SaveChangesAsync();
        return post;
    }

    //Put, comment upvote
    public async Task<Post> CommentUpvote(int postId, int commentId)
    {
        var post = await db.Posts!.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == postId);
        var comment = post!.Comments.FirstOrDefault(c => c.Id == commentId);
        comment!.Upvotes ++;
        await db.SaveChangesAsync();
        return post;
    }

    //Put, comment downvote
    public async Task<Post> CommentDownvote(int postId, int commentId)
    {
        var post = await db.Posts!.Include(p => p.Comments).FirstOrDefaultAsync(p => p.Id == postId);
        var comment = post!.Comments.FirstOrDefault(c => c.Id == commentId);
        comment!.Downvotes ++;
        await db.SaveChangesAsync();
        return post!;
    }
}       