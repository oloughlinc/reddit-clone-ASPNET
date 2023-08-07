using System.Collections.Generic;
namespace RedditCloneASP.Models;

public class Comment {

    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public long PostId { get; set; }

    public long ParentId { get; set; }

    public long Depth { get; set; }

    public DateTime PostDate { get; set; }

    public long Upsends { get; set; }
}