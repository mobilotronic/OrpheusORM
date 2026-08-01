# Database creation flow
Orpheus will try to create the database, that it's trying to connect to, if it doesn't exist.

To do that, it will create a separate administrative connection — to `master` (SQL Server),
`sys` (MySQL), or `postgres` (PostgreSQL) — using the ServiceUserName/ServicePassword credentials
to connect to the server. If ServiceUserName/ServicePassword aren't configured, this
administrative connection falls back to the main UserName/Password instead.

The account used for this connection must have database creation priviliges.
