using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;

namespace RedditCloneASP.Models;

public class RedditContext : DbContext {
    
    public RedditContext(DbContextOptions<RedditContext> options) : base(options) {}

    public DbSet<Comment> Comments { get; set; } = null!;

    public DbSet<Post> Posts { get; set; } = null!;


    // https://learn.microsoft.com/en-us/ef/core/querying/user-defined-function-mapping
    public IQueryable<Comment> GetAllCommentsForPost(string postId)
        => FromExpression(() => GetAllCommentsForPost(postId));

    public IQueryable<Comment> GetParentById(int parentId)
        => FromExpression(() => GetParentById(parentId));

    public IQueryable<Comment> GetLastChildOfParent(int parentId)
        => FromExpression(() => GetLastChildOfParent(parentId));

    public IQueryable<Comment> GetLastChildOfPost(string postId)
        => FromExpression(() => GetLastChildOfPost(postId));

    protected override void OnModelCreating(ModelBuilder modelBuilder) {

        #pragma warning disable 8604

        modelBuilder.HasDbFunction(typeof(RedditContext).GetMethod(nameof(GetAllCommentsForPost)))
            .HasName("getallcommentsforpost");

        modelBuilder.HasDbFunction(typeof(RedditContext).GetMethod(nameof(GetParentById)))
            .HasName("getparentbyid");

        modelBuilder.HasDbFunction(typeof(RedditContext).GetMethod(nameof(GetLastChildOfParent)))
            .HasName("getlastchildofparent");

        modelBuilder.HasDbFunction(typeof(RedditContext).GetMethod(nameof(GetLastChildOfPost)))
            .HasName("getlastchildofpost");

        #pragma warning restore 8604
    }
}