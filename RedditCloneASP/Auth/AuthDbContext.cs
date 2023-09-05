using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RedditCloneASP.Auth;

public class AuthDbContext : IdentityDbContext<IdentityUser> {

    public AuthDbContext(DbContextOptions options) : base(options) { 
    }
}