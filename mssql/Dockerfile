FROM mcr.microsoft.com/mssql/server:2017-latest-ubuntu

# Create work directory
RUN mkdir -p /usr/work/sql

WORKDIR /usr/work

# Copy all scripts into working directory
COPY ./tools /usr/work/

# Copy all sql scripts into working directory
COPY ./sql /usr/work/sql/

# Grant permissions for the import-data script to be executable
RUN chmod +x /usr/work/import-data.sh

EXPOSE 1433

CMD /usr/work/entrypoint.sh