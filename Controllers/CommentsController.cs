using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedditCloneASP.Models;

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
        [HttpGet("{postId}")]
        public async Task<ActionResult<IEnumerable<CommentDTO>>> GetComments(int postId)
        {
          if (_context.Comments == null)
          {
            return NotFound();
          }

          /* This query takes advantage of postgres recursive searching. It builds a temporary table by first 
            performing the base query, followed by recursively performing the UNION query and joining the wanted values
            into the new table until null is returned.

            The resulting temporary table can be queried as needed. This is known as a CTE (common table expression)

            The reason we do this is because our comment structure contains nested data, and by using this method we can selectively
            return only certain branches as needed. These recursive searches are fairly optimized in postgres and offer a good balance of
            simplicity vs speed for our needs.
            */
          FormattableString query = $"""  
                WITH RECURSIVE "comment_tree" AS (

                    SELECT *
                    FROM "Comments"
                    WHERE "PostId" = {postId} AND "ParentId" = 0 AND "Depth" <= 3
                    
                    UNION
                    
                        SELECT "c".*
                        FROM "Comments" "c" 
                        INNER JOIN "comment_tree" "ct"
                            ON "ct"."Id" = "c"."ParentId"

            ) SELECT * FROM "comment_tree" ORDER BY "Depth" ASC;
            """;

            // create new DTO that will hold the comment tree, it is a nested object
            var comments_tree = new List<CommentDTO>();

            // query for a flat list of the requested data as a list of comment objects
            var comments_flat = await _context.Comments.FromSql(query).ToListAsync();

            // get the initial depth of the root comment(s). The query is ordered by ascending depth so lowest depth always first index.
            long inital_depth = comments_flat[0].Depth;

            // build the tree out to depth 4
            comments_flat.ForEach(comment => {

                switch (comment.Depth - inital_depth) {

                    case 0: 
                        comments_tree.Add(new CommentDTO(comment));
                        break;

                    case 1:
                        comments_tree.Find(x => x.Comment.Id == comment.ParentId).Replies.Add(new CommentDTO(comment));
                        break;

                    case 2:
                        comments_tree.ForEach(c => {
                            var found = c.Replies.Find(x => x.Comment.Id == comment.ParentId);
                            if (found != null) {
                                found.Replies.Add(new CommentDTO(comment));
                            };
                        });
                        break;

                    case 3:
                        comments_tree.ForEach(c1 => {
                            c1.Replies.ForEach(c2 => {
                                var found = c2.Replies.Find(x => x.Comment.Id == comment.ParentId);
                                if (found != null) {
                                found.Replies.Add(new CommentDTO(comment));
                            };
                            });
                        });
                        break;
                }
            });

            // TODO: implement sorting functions

            return comments_tree;

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
        public async Task<ActionResult<Comment>> PostComment(Comment comment)
        {
          if (_context.Comments == null)
          {
              return Problem("Entity set 'RedditContext.Comments'  is null.");
          }
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetComment", new { id = comment.Id }, comment);
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
