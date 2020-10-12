# wait for the SQL Server to come up

echo "Waiting for Sql server to become ready" >> output.txt

until /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "SELECT GETDATE()"; do sleep 1S; done

echo "Creating database if nescessary" >> output.txt

#run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -Q "If(db_id(N'$DATABASE') IS NULL) CREATE DATABASE $DATABASE"

echo "Setting up changes to database" >> output.txt

/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SA_PASSWORD" -d $DATABASE -i sql/setup.sql

echo "Done setting up database schema" >> output.txt