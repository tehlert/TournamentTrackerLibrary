
CREATE DATABASE Tournaments;

SELECT 
	*
FROM 
	People
;


SELECT
	top 1000 
	FirstName as 'First Name', 
	LastName  as 'Surname'
FROM
	People
WHERE
	FirstName = 'tyler' AND  -- use AND/OR to add multiple parameters to the where statement of the query
	LastName like 'Ehl%'     -- the % is the wildcard character in SQL 
ORDER BY
	LastName
;


exec dbo.TestPerson_GetByLastName 'Ehlert'