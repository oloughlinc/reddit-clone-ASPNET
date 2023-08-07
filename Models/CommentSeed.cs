using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace RedditCloneASP.Models;

public static class SeedData {

    public static void Initialize(IServiceProvider serviceProvider) {
        using (var context = new CommentContext(
            serviceProvider.GetRequiredService<DbContextOptions<CommentContext>>()
        )) {

            if (context.Comments.Any()) return;

            context.Comments.AddRange(
                new Comment {
                    Id = 1,
                    Poster = "JimBob23",
                    Body = "Hello from database test!!! It is nice to meet you.",
                    PostId = 0,
                    ParentId = 0,
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 2,
                    Poster = "JimBob23",
                    Body = "Hey it is me again, just replying to my own post :)",
                    PostId = 0,
                    ParentId = 1,
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = -2,
                },
                new Comment {
                    Id = 3,
                    Poster = "MarySue99",
                    Body = "You sir are an idiot, and here is why:   ",
                    PostId = 0,
                    ParentId = 2,
                    Depth = 2,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 11,
                },
                new Comment {
                    Id = 4,
                    Poster = "MarySue99",
                    Body = "Cool things I have done today: 1. eat waffle 2. add butter 3. go to park etc",
                    PostId = 0,
                    ParentId = 0,
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 5,
                    Poster = "JohnSmith123",
                    Body = "Hi Database, I am father!",
                    PostId = 0,
                    ParentId = 1,
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 23,
                },
                new Comment {
                    Id = 6,
                    Poster = "JohnSmith123",
                    Body = "Just taking my new cat out for a walk. Catch you all in a bit.",
                    PostId = 0,
                    ParentId = 0,
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 7,
                    Poster = "JimBob23",
                    Body = "Wow I wish I had a cat too! But dog is better for me.",
                    PostId = 0,
                    ParentId = 6,
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                }
            );
            context.SaveChanges();
        }

    }
}