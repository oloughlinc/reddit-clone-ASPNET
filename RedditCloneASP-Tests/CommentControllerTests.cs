using RedditCloneASP.Auth;
using RedditCloneASP.Controllers;
using RedditCloneASP.Models;

using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

using Moq;

namespace RedditCloneASP_Tests;

[TestClass]
public class CommentControllerTests
{

    DbContextOptions<RedditContext> dbOptions = new DbContextOptionsBuilder<RedditContext>()
    .UseNpgsql("Server=localhost; Port=5432; Database=reddit-clone-test; Username=postgres; Password=password")
    .Options;


    [TestMethod]
    public async Task GetComments_ShouldReturnAllCommentsForPostOne()
    {   
        // Arrange
        var dbcontext = new RedditContext(dbOptions);
        var controller = new CommentsController(dbcontext);

        // Act
        var resp = await controller.GetComments(1);

        // Assert
        Assert.IsFalse(resp.Value == null);
        Assert.IsTrue(resp.Value.ToList().Count() == 4);
    }
    
    [TestMethod]
    public async Task GetComments_ShouldReturnAllCommentsForPostTwo()
    {   
        // Arrange
        var dbcontext = new RedditContext(dbOptions);
        var controller = new CommentsController(dbcontext);

        // Act
        var resp = await controller.GetComments(2);

        // Assert
        Assert.IsFalse(resp.Value == null);
        Assert.IsTrue(resp.Value.ToList().Count() == 1);
    }

    [TestMethod]
    public async Task GetComments_ShouldReturnAllCommentsForPostThree()
    {   
        // Arrange
        var dbcontext = new RedditContext(dbOptions);
        var controller = new CommentsController(dbcontext);

        // Act
        var resp = await controller.GetComments(3);

        // Assert
        Assert.IsFalse(resp.Value == null);
        Assert.IsTrue(resp.Value.ToList().Count() == 3);
    }

    [TestMethod]
    public async Task GetComments_ShouldReturnNothing_PostNonexistant()
    {   
        // Arrange
        var dbcontext = new RedditContext(dbOptions);
        var controller = new CommentsController(dbcontext);

        // Act
        var resp = await controller.GetComments(4);

        // Assert
        Assert.IsFalse(resp.Value == null);
        Assert.IsTrue(resp.Value.ToList().Count() == 0);
    }


}