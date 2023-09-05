using System.Diagnostics;
using NuGet.ContentModel;
using NuGet.Protocol;
using RedditCloneASP.Builders;
using RedditCloneASP.Controllers;
using RedditCloneASP.Models;

namespace RedditCloneASP_Tests;

[TestClass]
public class CommentBuilderTests {

    [TestMethod]
    public void CommentBuilder_BuildTree_OnGoodInput_ReturnsNestedList() {

        // Arrange
        // Act
        var result = CommentsBuilder.BuildTreeFromComments(GetTestComments());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(result.ToJson().ToString(), GoodJsonResult);
    }

    [TestMethod]
    public void CommentBuilder_BuildTree_OnEmptyInput_ReturnsEmpty() {

        // Arrange
        // Act
        var result = CommentsBuilder.BuildTreeFromComments(new List<Comment>());

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(List<CommentDTO>));
        Assert.IsTrue(result.Count() is 0);
    }

    [TestMethod]
    public void CommentBuilder_BuildTree_OnBadInput_ThrowsException() {

        // Arrange
        // Act
        // Assert
        Assert.ThrowsException<Exception>(() => 
            CommentsBuilder.BuildTreeFromComments(new List<Comment>() { new Comment() {Id = 1, ParentPath = "0.2"}}));

    }

    private List<Comment> GetTestComments() {

        return new List<Comment>() {
            new Comment {
                    Id = 1,
                    Poster = "JimBob23",
                    Body = "Hello from database test!!! It is nice to meet you.",
                    PostId = 1,
                    Path = "1.1",
                    ParentPath = "0",
                    Depth = 0,
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
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
                    PostDate = new DateTimeOffset(633452259920000000, new TimeSpan(1, 0, 0)),
                    Upsends = 5,
                },
        };
    }
    private string GoodJsonResult { get {
            return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "goodNestedJson.txt"));
        }
    }
}