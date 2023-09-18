using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RedditCloneASP.Models;

namespace RedditCloneASP.Auth;

public class AuthDbContext : IdentityDbContext<RedditIdentityUser> {

    public AuthDbContext(DbContextOptions options) : base(options) { 
    }

    protected override void OnModelCreating(ModelBuilder builder) {
        base.OnModelCreating(builder);
    }
}