using Microsoft.AspNetCore.Identity;

namespace RedditCloneASP.Models;

public class RedditIdentityUser : IdentityUser {

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
}