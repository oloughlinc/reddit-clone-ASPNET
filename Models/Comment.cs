
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RedditCloneASP.Models;

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

    public static List<CommentDTO> BuildTreeFromComments(List<Comment> comments_list) {
        
        var comments_tree = new List<CommentDTO>();
        // TODO: wrap in try and revert to linear build if failed.
        comments_list.ForEach(comment => {
            List<string> path_arr = comment.ParentPath.Split('.').ToList<string>();
            RecurseBuildTree(path_arr, comment, comments_tree);
        });
        return comments_tree;
    }
    
    // can use binary search for n log n complexity, as the database query result is sorted by id already
    private static void RecurseBuildTree(List<string> path_arr, Comment comment, List<CommentDTO> comment_tree) {
        
        path_arr.RemoveAt(0);
        if (path_arr.Count is 0) {
            comment_tree.Add(new CommentDTO(comment));
            return;
        }
        var comment_search_term = new CommentDTO(new Comment() { Id = int.Parse(path_arr[0]) });
        int parent_index = comment_tree.BinarySearch(comment_search_term, new CommentComparer());
        if (parent_index >= 0) RecurseBuildTree(path_arr, comment, comment_tree[parent_index].Replies);
        else throw new Exception("Could not find ParentID with Binary Search");
    }
}

public class CommentComparer : IComparer<CommentDTO> {
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