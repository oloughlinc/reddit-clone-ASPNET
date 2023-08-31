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

        /*  This query returns first the parent comment by id, and then,
            the last of any children by path value. The last child is needed
            in order to properly increment the path for this new comment.

            The first query gets the path value of the id we want to reply to.
            To this path we append the lquery syntax .*{1} which with the ~
            operator will return all direct children at that path. From those
            children the highest (last) final path value is returned.

            The second query simply finds the parent post by Id.

            Both are returned by the union.
            If there are no children yet on the parent, the query returns the parent
            comment only. In this case, we set the last child path to 0 on server later.
        */

        var parentId = comment.ParentID;
        var postId = comment.PostID;

        //TODO: Utilize stored procedures instead

        FormattableString queryWithParent_old = $$""" 

            SELECT * FROM 
                (SELECT *
                FROM "Comments"
                WHERE "Path" ~
                (SELECT "Path"::text::lquery || '.*{1}' 
                    FROM "Comments" 
                    WHERE "Id" = {{parentId}})::lquery
                ORDER BY "Path" DESC
                LIMIT 1) as last_child
            
            UNION

            SELECT * 
            FROM "Comments" 
            WHERE "Id" = {{parentId}}

            ORDER BY "Depth";

            """;

        FormattableString queryForLastChild = $$""" 

            SELECT *
            FROM "Comments"
            WHERE "Path" ~
            (SELECT "Path"::text::lquery || '.*{1}' 
                FROM "Comments" 
                WHERE "Id" = {{parentId}})::lquery
            ORDER BY "Path" DESC
            LIMIT 1;

            """;

        FormattableString queryForParent = $$""" 

            SELECT * 
            FROM "Comments" 
            WHERE "Id" = {{parentId}};

            """;
        

        string postIdQuery = postId.ToString() + ".*{1}";
        FormattableString queryNoParent = $$""" 

            SELECT *
            FROM "Comments"
            WHERE "Path" ~
            {{postIdQuery}}::lquery 
            ORDER BY "Path" DESC 
            LIMIT 1;

            """;

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

            queryResult = await _context.Comments.FromSql(queryNoParent).ToListAsync();
            lastChild = queryResult.First();

            newComment = CommentsBuilder.BuildNewComment(comment, poster, lastChild);

        } else { // this is a comment that is a reply to another comment

            queryResult = await _context.Comments.FromSql(queryForParent).ToListAsync();
            parent = queryResult.First();

            queryResult = await _context.Comments.FromSql(queryForLastChild).ToListAsync();
            lastChild = queryResult.First();

            newComment = CommentsBuilder.BuildNewComment(comment, poster, parent, lastChild);
        }
        } catch (OperationCanceledException) {
            return Problem("Database operation was cancelled. Please try again later.");
        } catch (Exception) {
            return BadRequest(); // likely bad data from client
        }

        // return Created("Dummy created!", newComment);

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
