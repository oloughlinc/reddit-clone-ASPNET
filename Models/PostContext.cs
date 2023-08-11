using Microsoft.EntityFrameworkCore;

namespace RedditCloneASP.Models;

public class PostContext : DbContext {
    
    public PostContext(DbContextOptions<PostContext> options) : base(options) {}

    public DbSet<Post> Posts { get; set; } = null!;
}