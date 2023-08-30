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

            /* 
                This query takes advantage of the postgres 'ltree' module, which includes efficient methods for
                traversing branching data structures related by a character string 'path'. 'ltree' can efficiently
                find all ancestor / children for a given path using a binary search on a path index. 
                
                see https://www.postgresql.org/docs/current/ltree.html
            */

            string pathId = postId.ToString();
            FormattableString query = $"""  
                SELECT *
                FROM "Comments"
                WHERE {pathId}::ltree @> "Path"
                ORDER BY "Depth", "Id";
            """;

            // query for a flat list of the requested data as a list of comment objects
            var comments_flat = await _context.Comments.FromSql(query).ToListAsync();

            // build and return a nested list
            return await Task.Run<List<CommentDTO>>(() => {
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
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
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

          if (_context.Comments == null)
          {
              return Problem("Entity set 'RedditContext.Comments'  is null.");
          }

          var uid = User.FindFirst("username")?.Value;
          return Ok("Speak friend and enter. Your username is: " + uid);

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
