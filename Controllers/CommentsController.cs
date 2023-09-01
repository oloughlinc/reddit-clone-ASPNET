using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RedditCloneASP.Models;
using RedditCloneASP.Builders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace RedditCloneASP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly RedditContext _context;

        public CommentsController(RedditContext context)
        {
            _context = context;
        }

        // GET: api/comments
        /// <summary>
        /// Get the entire comment tree for a specific post
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        [HttpGet("{postId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments(int postId)
        {
          if (_context.Comments == null)
          {
            return NotFound();
          }

            // query for a flat list of the requested data as a list of comment objects
            var comments_flat = await _context
                            .GetAllCommentsForPost(postId.ToString())
                            .ToListAsync();

            // build and return a nested list
            return await Task.Run(() => {
                var comment_tree = CommentsBuilder.BuildTreeFromComments(comments_flat);
                CommentsBuilder.Sort(comment_tree, CommentValues.Upsends);
                return comment_tree;
            });
        }

        // PUT: api/Comments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutComment(long id, Comment comment)
        {
            if (id != comment.Id)
            {
                return BadRequest();
            }

            _context.Entry(comment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Comments
        /// <summary>
        /// Add a new comment to a post or another comment
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(typeof(NoContentResult), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<Comment>> PostComment(PostComment comment)
        {

        /*  This POST endpoint requires a valid Bearer token from client in order to access.
            Token validation is performed by the default authentication middleware in the pipeline,
            as we defined in the program build.

            If the provided token does not exist or else is improperly signed, the request is rejected
            and 401 provided as response.

            Beyond here the token has been accepted. We use the embedded username claim to ID the poster
            for each new post. Since this is embedded in the signed token from server, the client cannot
            manipulate this value and still access this endpoint (without knowing the server private key).
        */

        // Perform basic content validation
        if (_context.Comments == null)
        {
            return Problem("Entity set 'RedditContext.Comments'  is null.");
        }
        if (!ModelState.IsValid) return BadRequest();
        if (User.FindFirst("username") == null) return BadRequest();

        // Initialize
        int parentId = comment.ParentID ?? 0;
        var postId = comment.PostID;
        Comment parent;
        Comment lastChild;
        List<Comment> queryResult;
        Comment newComment;

         #pragma warning disable CS8602 // null state is checked during validation
        string poster = User.FindFirst("username").Value;
         #pragma warning restore CS8602

        /*  PRE-INSERTION
        
            requires at least one database query in order to insert, where we find the parent
            and the last child of that parent for inserting next and properly setting path
            values.

            This query is split into two because it makes the code more explicit, and we are 
            not too worried about maximizing performance of write operations as they would be
            far less common than read operations and can tf take a split second longer to complete.
        */
        try {

            if (parentId == 0) { // this is a comment with no parent comment

            lastChild = await _context.GetLastChildOfPost(postId.ToString() + ".*{1}").FirstAsync();
            newComment = CommentsBuilder.BuildNewComment(comment, poster, lastChild);

        } else { // this is a comment that is a reply to another comment

            parent = await _context.GetParentById(parentId).FirstAsync();
            lastChild = await _context.GetLastChildOfParent(parentId).FirstAsync();
            newComment = CommentsBuilder.BuildNewComment(comment, poster, parent, lastChild);
        }

        } catch (OperationCanceledException) {
            return Problem("Database operation was cancelled. Please try again later.");
        } catch (Exception) {
            return BadRequest(); // likely bad data from client
        }

        //return Created("Dummy created!", newComment);

        /* INSERTION */

        try {

            // insert comment to database
            _context.Comments.Add(newComment);
            await _context.SaveChangesAsync();

            // verify insertion, we do not have the id since the db sets this, so instead we custom search by path
            queryResult = await _context.Comments.FromSql($"""SELECT * FROM "Comments" WHERE "Path" ~ {newComment.Path}::lquery;""").ToListAsync();
            return Created("resp header: uri location", new PublicComment(queryResult.First()));

        } catch (DbUpdateException) {
            return Problem("Database operation was cancelled. Please try again later.");
        } catch (Exception) {
            return Problem();
        }

        }

        // DELETE: api/Comments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(long id)
        {
            if (_context.Comments == null)
            {
                return NotFound();
            }
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CommentExists(long id)
        {
            return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
