using RedditCloneASP.Auth;
using RedditCloneASP.Controllers;
using RedditCloneASP.Models;

using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace RedditCloneASP_Tests;

[TestClass]
public class CommentControllerTests
{
    /* These are not unit tests but integration tests with a test database using a cloned DbContext from the main
    project pointing to a test database set. This is done because we are using stored procedures and custom methods on the context, 
    and wish to test them both. We could mock and test seperately but this requires substantially more code for both these tests and
    the then seperate database function tests.

    We only test the non-authorized controller points here. The authorized points are tested in a seperate file because we must also 
    mock an http client/response in order to utilize authorization to the controller.
    */

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