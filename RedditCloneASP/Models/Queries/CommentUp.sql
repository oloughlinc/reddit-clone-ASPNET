/* These are query procedures that are referenced during inital database migration
	They are created when the migration is applied and then our controller can access 
	these functions instead of using raw sql in the api. This is safer, neater,
 	and slightly more performant.
*/

/* 
	This query takes advantage of the postgres 'ltree' module, which includes efficient methods for
	traversing branching data structures related by a character string 'path'. 'ltree' can efficiently
	find all ancestor / children for a given path using a binary search on a path index. 
	
	see https://www.postgresql.org/docs/current/ltree.html
*/
create or replace function getAllCommentsForPost(
	postId text
) returns setof "Comments"

language sql
as $$
	
	select * 
	from "Comments" 
	where postId::ltree @> "Path"
	order by "Depth", "Id";	
	
$$;

create or replace function getParentById(
	parentId integer
) returns setof "Comments"

language sql
as $$
	
	select * 
	from "Comments" 
	where "Id" = parentId;
	
$$;

create or replace function getLastChildOfParent(
	parentId integer
) returns setof "Comments"

language sql
as $$
	
	select *
	from "Comments"
	where "Path" ~
	(select "Path"::text::lquery || '.*{1}'
		from "Comments"
		where "Id" = parentId)::lquery
	order by "Path" desc
	limit 1;
	
$$;

create or replace function getLastChildOfPost(
	postId text
) returns setof "Comments"

language sql
as $$
	
	select *
	from "Comments"
	where "Path" ~
	postId::lquery
	order by "Path" desc
	limit 1;
	
$$;

