# Database creation flow
Orpheus will try to create the database, that it's trying to connect to, if it doesn't exist.

To do that, it will create a second connection, using the ServiceUserName/ServicePassword
credentials to connect to the server.

The account configured in ServiceUserName/ServicePassword must have database creation priviliges.
