using RedditCloneASP.Models;

namespace RedditCloneASP.Builders;

// this static class is stateless. It is potentially used in multiple threads via Task.
public static class CommentsBuilder {

/// <summary>
/// Static method <c>BuildTreeFromComments</c> generates a nested array of transfer objects which each hold one<br/>
/// or more <c>Comments</c>. This is the preferred method of sending data to a view member, as these objects <br/>
/// are easily iterated and displayed on client-side.
/// </summary>
/// <param name="comments_list">A flat list of comment objects (List&lt;Comment&lt;), such as returned from a basic SQL query through EF.</param>
/// <returns>A list of <c>CommentDTO</c> objects, each with their own nested list of replies.</returns>
    public static List<CommentDTO> BuildTreeFromComments(List<Comment> comments_list) {
        
        var comments_tree = new List<CommentDTO>();
        // TODO: wrap in try and revert to linear build if failed.
        comments_list.ForEach(comment => {
            List<string> path_arr = comment.ParentPath.Split('.').ToList<string>();
            RecurseBuildTree(path_arr, comment, comments_tree);
        });
        return comments_tree;
    }
    
    // uses recursion and a supplied path (this is generated from string data in a column each Comment must have) to populate an existing tree.
    private static void RecurseBuildTree(List<string> path_arr, Comment comment, List<CommentDTO> comment_tree) {
        
        // base case: strip the previous address, then if no directions remain, add the comment at the current recursion depth
        path_arr.RemoveAt(0);
        if (path_arr.Count is 0) {
            comment_tree.Add(new CommentDTO(comment));
            return;
        }

        // recurse: there is a direction, so find it using binary search, then head down the next node
        // (can use binary search for n log n complexity as the database query result is sorted by id already)
        var comment_search_term = new CommentDTO(new Comment() { Id = int.Parse(path_arr[0]) });
        int parent_index = comment_tree.BinarySearch(comment_search_term, new CommentComparerById());
        if (parent_index >= 0) RecurseBuildTree(path_arr, comment, comment_tree[parent_index].Replies);
        else throw new Exception("Could not find ParentID with Binary Search");
    }

/// <summary>
/// Static method <c>CommentDTO.Sort</c> sorts a nested array list using a defined comparator and built-in List&lt;T&lt;.Sort(Comparator).
/// Operates in average O(n log n) time for most cases.<br/>
/// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=net-7.0"/>
/// </summary>
/// <param name="comment_tree">A populated list of CommentDTOs (List&lt;CommentDTO&lt;)</param>
/// <param name="value">A <c>CommentValues</c> enum representing the field by which to sort. <see cref="CommentValues"/></param>
    public static void Sort(List<CommentDTO> comment_tree, CommentValues value) {
        
        comment_tree.ForEach(comment => {
            if (comment.Replies.Count > 0) Sort(comment.Replies, value);
        });
        switch (value) {
            case CommentValues.Upsends:
                comment_tree.Sort(new CommentComparerByUpsend());
                break;
        }
    }

/// <summary>
/// Static method <c>BuildNewComment</c> creates a new database comment from a <c>PostComment</c> reply as sent from a client.<br/>
/// A verified poster must be supplied, along with the last sibling of this comment. Further, a parent <c>Comment</c> should be <br/>
/// supplied if this comment is a a direct reply to another comment.
/// </summary>
/// <param name="comment">The <c>PostComment</c> model reply</param>
/// <param name="poster">A verified string containing the poster username. This should be retrieved from a secured bearer token.</param>
/// <param name="parent">If needed, the parent <c>Comment</c> from the database.</param>
/// <param name="lastChild">The last sibling <c>Comment</c> from the database. Needed to properly increment storage path on database.></param>
/// <returns>A new <c>Comment</c> properly serialized and ready for database insertion.</returns>
    public static Comment BuildNewComment(PostComment comment, string poster, Comment parent, Comment lastChild) {

        #pragma warning disable CS8604
        return new Comment() {

            Poster = poster,
            Body = comment.ReplyBody,
            PostId = parent.PostId,
            Path = parent.Path + "." + (Char.GetNumericValue(lastChild.Path.Last()) + 1),
            ParentPath = parent.ParentPath + "." + parent.Id,
            Depth = parent.Depth + 1,
            PostDate = DateTimeOffset.UtcNow,
            Upsends = 0,

        };
        #pragma warning restore CS8604
    }
/// <summary>
/// Static method <c>BuildNewComment</c> creates a new database comment from a <c>PostComment</c> reply as sent from a client.<br/>
/// A verified poster must be supplied, along with the last sibling of this comment. Further, a parent <c>Comment</c> should be <br/>
/// supplied if this comment is a a direct reply to another comment.
/// </summary>
/// <param name="comment">The <c>PostComment</c> model reply</param>
/// <param name="poster">A verified string containing the poster username. This should be retrieved from a secured bearer token.</param>
/// <param name="lastChild">The last sibling <c>Comment</c> from the database. Needed to properly increment storage path on database.></param>
/// <returns>A new <c>Comment</c> properly serialized and ready for database insertion.</returns>
    public static Comment BuildNewComment(PostComment comment, string poster, Comment lastChild) {

        #pragma warning disable CS8604
        return new Comment() {

            Poster = poster,
            Body = comment.ReplyBody,
            PostId = lastChild.PostId,
            Path = lastChild.PostId + "." + (Char.GetNumericValue(lastChild.Path.Last()) + 1),
            ParentPath = "0",
            Depth = 0,
            PostDate = DateTimeOffset.UtcNow,
            Upsends = 0,
            
        };
        #pragma warning restore CS8604
    }

}

public class CommentComparerById : IComparer<CommentDTO> {
    public int Compare(CommentDTO? x, CommentDTO? y) {

        if (x is null) {
            if (y is null) return 0;
            else return -1;
        }
        if (y is null) return 1;

        if (x.Comment.Id < y.Comment.Id) return -1;
        else if (x.Comment.Id > y.Comment.Id) return 1;
        else return 0;
    }
}

public class CommentComparerByUpsend : IComparer<CommentDTO> {
    public int Compare(CommentDTO? x, CommentDTO? y) {

        if (x is null) {
            if (y is null) return 0;
            else return -1;
        }
        if (y is null) return 1;

        if (x.Comment.Upsends > y.Comment.Upsends) return -1;
        else if (x.Comment.Upsends < y.Comment.Upsends) return 1;
        else return 0;
    }
}