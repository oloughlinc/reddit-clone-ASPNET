using Microsoft.EntityFrameworkCore;

namespace RedditCloneASP.Models;

public class RedditContext : DbContext {
    
    public RedditContext(DbContextOptions<RedditContext> options) : base(options) {}

    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
}