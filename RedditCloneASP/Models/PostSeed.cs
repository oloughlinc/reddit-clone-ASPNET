using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace RedditCloneASP.Models;

public static class PostSeed {

    public static void Initialize(IServiceProvider serviceProvider) {
        using (var context = new RedditContext(
            serviceProvider.GetRequiredService<DbContextOptions<RedditContext>>()
        )) {

            if (context.Posts.Any()) return;

            context.Posts.AddRange(
                new Post {
                    Id = 1,
                    Title = "Welcome to Fake Reddit!",
                    Body = "This project was designed by me, Craig, and it uses a .NET 7 backend with PostgreSql as an exercise. Check my github for more stuff!",
                    Sub = "",
                    Link = "https://github.com/oloughlinc",
                    Poster = "oloughlinc",
                    PostDate = DateTimeOffset.UtcNow,
                    ReplyCount = 7,
                    Upsends = 99,
                },
                new Post {
                    Id = 2,
                    Title = "Cannot BELIEVE what happen to me today...",
                    Body = "I met a duck. Discuss.",
                    Sub = "",
                    Link = "",
                    Poster = "duck_lover",
                    PostDate = DateTimeOffset.UtcNow,
                    ReplyCount = 4,
                    Upsends = 0,
                },
                new Post {
                    Id = 3,
                    Title = "ELI5: How can this thing do this stuff",
                    Body = "I just started learning about it. Please be nice.",
                    Sub = "",
                    Link = "",
                    Poster = "JimBob23",
                    PostDate = DateTimeOffset.UtcNow,
                    ReplyCount = 3,
                    Upsends = 0,
                }
            );
            context.SaveChanges();
        }

    }
}