# Reddit-Clone-ASPNET

A front-to-back web application that mirrors the basic functionality of a comment-reply forum such as Reddit. This project is built as an exercise. The backend is built in C# on .NET 7 with Entity Framework and uses a PostgreSQL database server. 
I chose the architecture before planning the rest of the project, with the idea that part of the challenge here is applying a given system to the needs of a single application.

The front-end of this project is kept basic using static file hosting on a .NET server along with plain HTML / JavaScript. Bootstrap is used for simple and responsive CSS formatting and styling.

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

### Authentication

A basic model is used for a user with each user possessing a username, email address, and password. Microsoft Identity is used to facilitate storage of user information and passwords on the database. Although Identity is geared towards MVC with integration of authentication into the creted View, it contains several helpful features specifically for the backend, including automated password hashing/salting, table management, and automated credential verification in the middleware. Identity also by default enforces password complexity for us.

### Authorization

The Auth API sends a JWT token response that authorizes the requesting client to perform certain actions such as posting new comments or replies. The auth implementation was custom built as an exercise but follows Auth0 guidelines by providing both a short-lived authorization token and a longer-life refresh token that can be used to obtain new authorization tokens. The web client is built in a way to closely guard the JWT authorization token value, as anyone with this token information is authorized as the user. The token is kept in browser-memory and is inaccessible across scripts. This necessitates a refresh of the token on every page reload. The refresh token is stored as an HttpOnly cookie. This is slightly more public but the value is still not obtainable by client JavaScript. The refresh token is single use and a new refresh token is issued with each refresh request. The valid refresh and expiry is stored in the Identity table by extending the basic authentication context.

### Testing

A seperate MSTest project is included with unit tests for application logic on the server. For integration tests of the web api, we use a test database server which includes the same stored procedures as the production database. This allows us to easily test api functions without having to implement a new in-memory mock and duplicate procedures. This also allows us to explicitly test ltree functions for the database retrieval and storage.


