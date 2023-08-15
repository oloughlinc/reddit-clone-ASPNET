
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedditCloneASP.Models;


/// <summary>
/// Enum <c>CommentValues</c> holds different possible field types for a <c>Comment</c> data class.
/// Currently only Upsends is implemented. This is at the moment only used for referencing a sorting comparator.
/// More sorting functions can be easily added by including a new <c>CommentValue</c> type and an associated sorter class,
/// making this design more extensible.
/// </summary>
public enum CommentValues {
    Upsends
}

/// <summary>
/// Class <c>Comment<c> is an Entity Framework Data Access Object which allows us to easily map class members to a database.
/// Contains all necessary data fields for retrieval and data display.
/// </summary>
public class Comment {

    public Comment() {
        this.ParentPath = "0";
    }

    public int Id { get; set; }

    public string? Poster { get; set; }

    public string? Body { get; set; }

    public long PostId { get; set; }

    [Column(TypeName = "ltree")]
    public string? Path { get; set; }

    public string ParentPath { get; set; }

    public long Depth { get; set; }

    public DateTimeOffset PostDate { get; set; }

    public long Upsends { get; set; }
}

public class CommentDTO {

    public CommentDTO(Comment comment) {
        Comment = comment;
        Replies = new List<CommentDTO>();
    }

    public Comment Comment { get; set; }

    public List<CommentDTO> Replies { get; set; }

    /// <summary>
    /// Static Method <c>BuildTreeFromComments</c> generates a nested array of transfer objects which each hold one
    /// or more <c>Comments</c>. This is the preferred method of sending data to a view member, as these objects 
    /// are easily iterated and displayed on client-side.
    /// </summary>
    /// <param name="comments_list">A flat list of comment objects (List<Comment>), such as returned from a basic SQL query through EF.</param>
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
    /// Static Method <c>CommentDTO.Sort</c> sorts a nested array list using a defined comparator and built-in List<T>.Sort(Comparator).
    /// Operates in average O(n log n) time for most cases.
    /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.list-1.sort?view=net-7.0"/>
    /// </summary>
    /// <param name="comment_tree">A populated list of CommentDTOs (List<CommentDTO>)</param>
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