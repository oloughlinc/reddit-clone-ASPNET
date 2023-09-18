using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Drawing.Text;
using RedditCloneASP.Models;

namespace RedditCloneASP.Auth;

public static class AuthSeed {

    public static async void Initialize(UserManager<RedditIdentityUser> userManager) {

        if (userManager.Users.Count() >= 4) return;

        await userManager.CreateAsync(
            new RedditIdentityUser()
            {
                UserName = "JimBob23",
                Email = "mymeail1@email.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            "SuperSecret123!"
        );

        await userManager.CreateAsync(
            new RedditIdentityUser()
            {
                UserName = "MarySue99",
                Email = "mymeail12@email.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            "SuperSecret123!"
        );

        await userManager.CreateAsync(
            new RedditIdentityUser()
            {
                UserName = "JohnSmith123",
                Email = "mymeail123@email.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            "SuperSecret123!"
        );
        await userManager.CreateAsync(
            new RedditIdentityUser()
            {
                UserName = "oloughlinc",
                Email = "craig.oloughlin@hotmail.com",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString()
            },
            "SuperSecret123!"
        );

    }
}

