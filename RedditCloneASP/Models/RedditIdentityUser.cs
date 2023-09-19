using Microsoft.AspNetCore.Identity;

namespace RedditCloneASP.Models;

public class RedditIdentityUser : IdentityUser {

    public string? RefreshToken { get; set; }
    public DateTimeOffset? RefreshTokenExpiry { get; set; }
}