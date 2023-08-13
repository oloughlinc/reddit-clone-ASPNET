using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace RedditCloneASP.Models;

public static class CommentSeed {

    public static void Initialize(IServiceProvider serviceProvider) {
        using (var context = new RedditContext(
            serviceProvider.GetRequiredService<DbContextOptions<RedditContext>>()
        )) {

            if (context.Comments.Any()) return;

            context.Comments.AddRange(
                new Comment {
                    Id = 1,
                    Poster = "JimBob23",
                    Body = "Hello from database test!!! It is nice to meet you.",
                    PostId = 1,
                    Path = "1.1",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 2,
                    Poster = "JimBob23",
                    Body = "Hey it is me again, just replying to my own post :)",
                    PostId = 1,
                    Path = "1.1.1",
                    ParentPath = "0.1",
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = -2,
                },
                new Comment {
                    Id = 3,
                    Poster = "MarySue99",
                    Body = "You sir are an idiot, and here is why:   ",
                    PostId = 1,
                    Path = "1.1.1.1",
                    ParentPath = "0.1.2",
                    Depth = 2,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 11,
                },
                new Comment {
                    Id = 4,
                    Poster = "MarySue99",
                    Body = "Cool things I have done today: 1. eat waffle 2. add butter 3. go to park etc",
                    PostId = 1,
                    Path = "1.2",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 5,
                    Poster = "JohnSmith123",
                    Body = "Hi Database, I am father!",
                    PostId = 1,
                    Path = "1.1.2",
                    ParentPath = "0.1",
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 23,
                },
                new Comment {
                    Id = 6,
                    Poster = "JohnSmith123",
                    Body = "Just taking my new cat out for a walk. Catch you all in a bit.",
                    PostId = 1,
                    Path = "1.3",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 7,
                    Poster = "JimBob23",
                    Body = "Wow I wish I had a cat too! But dog is better for me.",
                    PostId = 1,
                    Path = "1.3.1",
                    ParentPath = "0.6",
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 8,
                    Poster = "oloughlinc",
                    Body = "NEAT!",
                    PostId = 2,
                    Path = "2.1",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 12,
                },
                new Comment {
                    Id = 9,
                    Poster = "JimBob23",
                    Body = "RIGHT!!??",
                    PostId = 2,
                    Path = "2.1.1",
                    ParentPath = "0.8",
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 10,
                    Poster = "MarySue99",
                    Body = "You sir are an idiot, and here is why:   ",
                    PostId = 2,
                    Path = "2.1.1.1",
                    ParentPath = "0.8.9",
                    Depth = 2,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = -10,
                },
                new Comment {
                    Id = 11,
                    Poster = "MarySue99",
                    Body = "OK I take it back.",
                    PostId = 2,
                    Path = "2.1.1.1.1",
                    ParentPath = "0.8.9.10",
                    Depth = 3,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 5,
                },
                new Comment {
                    Id = 12,
                    Poster = "JohnSmith123",
                    Body = "Every time I walked away from something I wanted to forget, I told myself it was for a cause that I believed in. A cause that was worth it. Without that, we are lost.",
                    PostId = 3,
                    Path = "3.1",
                    ParentPath = "0",
                    Depth = 1,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 23,
                },
                new Comment {
                    Id = 13,
                    Poster = "NotHowStuffWorks_OFFICIAL",
                    Body = "I like to creep around my home and act like a goblin. I don’t know why but I just enjoy doing this. Maybe it’s my way of dealing with stress or something but I just do it about once every week. Generally I’ll carry around a sack and creep around in a sort of crouch-walking position making goblin noises, then I’ll walk around my house and pick up various different “trinkets” and put them in my bag while saying stuff like “I’ll be having that” and laughing maniacally in my goblin voice (“trinkets” can include anything from shit I find on the ground to cutlery or other utensils). The other day I was talking with my neighbours and they mentioned hearing weird noises like what I wrote about and I was just internally screaming the entire conversation. I’m 99% sure they don’t know it’s me but god that 1% chance is seriously weighing on my mind.",
                    PostId = 3,
                    Path = "3.2",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 24,
                },
                new Comment {
                    Id = 14,
                    Poster = "JimBob23",
                    Body = "touch grass is not an insult towards gamers, rather it is advice for them.",
                    PostId = 3,
                    Path = "3.3",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = DateTimeOffset.UtcNow,
                    Upsends = 25,
                }
            );
            context.SaveChanges();
        }

    }
}