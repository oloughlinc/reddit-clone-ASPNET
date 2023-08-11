using System.Collections.Generic;

namespace RedditCloneASP.Models;

public class Post {

    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Body { get; set; }

    public string? Sub { get; set; }

    public string? Link { get; set; }

    public string? Poster { get; set; }

    public DateTimeOffset PostDate { get; set; }

    public long Upsends { get; set; }
}