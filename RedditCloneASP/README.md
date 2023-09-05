# Reddit-Clone-ASPNET
**[This project is currently in progress]**

A front-to-back web application that mirrors the basic functionality of a comment-reply forum such as Reddit. This project is built as an exercise. The backend is built in C# on .NET 7 with Entity Framework and uses a PostgreSQL database server. 
I chose the architecture before planning the rest of the project, with the idea that part of the challenge here is applying a given system to the needs of a single application.

The front-end of this project is kept basic using static file hosting on a .NET server along with plain HTML / JavaScript. Bootstrap is used for simple and responsive CSS formatting.

### Database Considerations

Reddit-style comments can be nested to an arbitrary level, with a single parent comment having a one-to-many relation to children comments, which can themselves be parents to further children. There are several techniques by which you can
represent this data in a flat SQL table(s). I chose to use a module available standard in most installations of modern PostgreSQL, the 'ltree'. This module allows us to efficiently query large tables for nested items. Each comment 
entry stores its individual path in a column, and this column can be indexed for binary search.

A recursive search could also work well here, instead just storing each comment's direct parent ID. Recursive SQL searching is fairly optimized and any performance difference will likely not be noticeable for a reasonable depth level.
And it should be noted Reddit itself does not use a schema design at all for it's comment data.

### Returning Comments via API

The database returns a flat list of pre-defined objects through the Entity Framework, via a DbContext child class. However, we would like to serve nested data to our consumers. I chose to do this on server using a nested array object. The nested build 
operates on n log n time complexity thanks to Microsoft's List.BinarySearch, which allows us to efficiently find each parent as we move down the nested arrays. This implementation is possible because 1) the returning query is ordered by depth
then parent id, which means that as the nested arrays are populated they remain ordered by id, and 2) each row stores a parent path, directions which allow us to quickly route each child through the nested arrays.

A nested dictionary can instead be used to obtain linear time complexity, however sorting will become problematic later as each nested dictionary will first need to be converted to an array while keeping the nested structure intact. The 
performance increase is likely not worth the extra code. 
