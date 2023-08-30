
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedditCloneASP.Models;


/// <summary>
/// <para>Enum <c>CommentValues</c> holds different possible field types for a <c>Comment</c> data class.<br/>
/// Currently only Upsends is implemented. This is at the moment only used for referencing a sorting comparator. <br/>
/// More sorting functions can be easily added by including a new <c>CommentValue</c> type and an associated sorter class,<br/>
/// making this design more extensible.</para>
/// </summary>
public enum CommentValues {
    Upsends
}

/// <summary>
/// Class <c>Comment</c> is an Entity Framework Data Access Object which allows us to easily map class members to a database.
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

/// <summary>
/// Class <c>PublicComment</c> represents the fields and value that need to be served by the API to the general consumer.
/// This class hides data from the client that is necessary only for database retrieval and sorting.
/// </summary>
public class PublicComment {

    /// <summary>
    /// Create a new <c>PublicComment</c> with values mapped from an internal <c>Comment</c> data access object.
    /// </summary>
    /// <param name="comment">Any single <c>Comment</c></param>
    public PublicComment(Comment comment) {
        this.Id = comment.Id;
        this.Poster = comment.Poster;
        this.Body = comment.Body;
        this.PostId = comment.PostId;
        this.PostDate = comment.PostDate;
        this.Upsends = comment.Upsends;
    }

    public int Id { get; set; }

    public string? Poster { get; set; }

    public string? Body { get; set; }

    public long PostId { get; set; }

    public DateTimeOffset PostDate { get; set; }

    public long Upsends { get; set; }
}

/// <summary>
/// Class <c>PostComment</c> represents the fields and values necessary for the client to provide for creating a new comment.
/// </summary>
public class PostComment {

    public string? ReplyBody { get; set; }
    public string? ParentID { get; set; }

}

public class CommentDTO {

    public CommentDTO(Comment comment) {
        Comment = new PublicComment(comment);
        Replies = new List<CommentDTO>();
    }

    public PublicComment Comment { get; set; }

    public List<CommentDTO> Replies { get; set; }

    
}
