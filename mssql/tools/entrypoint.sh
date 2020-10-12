echo "Starting initialization" > output.txt

nohup /usr/work/import-data.sh > /dev/null 2>&1 &

echo "Starting mssql server" >> output.txt

/opt/mssql/bin/sqlservr