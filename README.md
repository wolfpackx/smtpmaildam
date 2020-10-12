# SmtpMailDam

> See al the mail your systems send

SmtpMailDam is intended to function as an SMTP mail server in a development
setting. When you have multiple environments from multiple customers and you
have to deal with mails and you need a simple way to view them all.

## Features

The features of SmtpMailDam

- Setup a mailbox for each environment that you need
- Every mailbox has a unique username/password combination
- View and download mail messages through the website component

## Installing / Getting started

### Docker

The simplest way to start up SmtpMailDam is to use Docker. You need a running
Docker setup with Docker-Compose.

```shell
git clone https://github.com/wolfpackx/smtpmaildam.git
cd smtpmaildam
docker-compose up -d
```

The above code will download the code for SmtpMailDam to your local server. After
that, docker-compose will start the SmtpMailDam SMTP server and website.

### Standalone

Besides the docker-compose way you can run the SmtpMailDam.Worker and SmtpMailDam.Website components individually. You could even run the SmtpMailDam.Worker component as a Windows Service.

```shell
dotnet publish -c Release -o c:\sampleservice
sc create “SmtpMailDam.Worker Service” binPath=c:\sampleservice\SmtpMailtrap.Worker.exe
sc start “SmtpMailDam.Worker Service”
```

The above instructions will wrap the SmtpMailDam.Worker component into a Windows Service.

When using a standalone database you will have to set it up yourself. You should have a Sql Server instance where you can create the database. This database should then be initialized with the ./mssql/sql/setup.sql file. After that you need to change the Connectionstring setting in the appsettings.json files for the SmtpMailDam.Worker and SmtpMailDam.Website components.

## Developing

When starting to add you own things to this project you could use any IDE that can build dotnet core projects. Just use the following instructions and get started.

```shell
git clone https://github.com/wolfpackx/smtpmaildam.git
cd smtpmaildam/
```

### Generating sql changes through Package Manager Console

When using the Package Manager Console in Visual Studio to generate change files through Add-Migration the default project of the Console should be SmtpMailDam.Common. The startup project of the solution should be set to SmtpMailDam.Website, because it contains the Designing Nuget package that is used when generating the changes.

To transform the model changes to sql statements use the following command in the Package Manager Console: Script-Migration -i -o mssql/sql/setup.sql

## Building

The project is build and debugged using Visual Studio 2019.

## Deploying / Publishing

Docker is the simplest way to deploy SmtpMailDam to a server.

The alternative is to publish the SmpMailDam.Worker and/or SmtpMailDam.Website component in Visual Studio using the Publish function. This will generate a Release configuration version to a local folder. Besides that you could the "dotnet publish -c Release -o c:\publishpath" command.

The SmtpMailDam.Worker component can be run standalone as an application or Windows service. The SmtpMailDam.Website component could be run standalone or using a webserver such as Apache or IIS.

## Configuration

The configuration comes in different parts. There is the configuration for Docker-Compose that in some cases overrides the configuration of the SmtpMailDam.Worker or SmtpMailDam.Website components. Therefor the configuration for both situations is explained.

### Docker-Compose

For Docker-Compose there is a single .env file that contains all the configuration settings.

| Setting | Description | Default value |
| --- | ---| --- |
| CONNECTIONSTRING |Connectionstring for the Sql server database | "Server=db;Database=smtpmailtrap;User Id=sa;Password=Welkom01!;" |
| DB_PASSWORD | Sa user password | "P@ssw0rd" |
| HTTP_PORT | HTTP port for the SmtpMailDam.Website | 80 |
| SMTP_PORT | Smtp port the SmtpMailDam.Worker | 587 |
| MSSQL_PID | The MS Sql server product type. See it with the Docker images for more information. The default value is for the "free" version with limits. | Express |
| DATABASE | The database name that contains the data of SmtpMailDam. | smtpmailtrap |
| SMTPSECURE | Secure SMTP with a certificate | false |
| SMTPCERTIFICATEFILEPATH | The path in the SmtpMailDam.Worker container where the Smtp certificate can be found | "" |
| SMTPCERTIFICATEPASSWORDFILEPATH | The path in the SmpMailDam.Worker container where the Smtp certificate password can be found. | "" |

### SmtpMailDam.Worker appsettings.json

The configuration for the logging can be found in the log4net.config file.

| Setting | Description | Default value |
| --- | ---| --- |
| ConnectionStrings:DefaultConnection | The database connection string used by the SmtpMailDam.Worker component. This can be overriden in Docker-Compose with the CONNECTIONSTRING setting. | "Server=192.168.1.202;Database=smtpmaildam;User Id=sa;Password=P@ssw0rd;" |
| SmtpServer:Enabled | Enable the SmtpMailDam.Worker SMTP server. | true |
| SmtpServer:Ports | The port on which the SMTP server will listen. | 587 |
| SmtpServer:ServerName | The that will be shown by the SMTP server when called by a client. | "SmtpMailDam" |
| SmtpServer:SupportedSslProtocols | The supported SSL protocol by the SMTP server. | "Tls" |
| SmtpServer:Secure | Should the SMTP server use TLS SSL certificate. | false |
| SmtpServer:CertificateFilePath | The path to the SSL certificate file. | "" |
| SmtpServer:CertificatePasswordFilePath | The path to the SSL certificate password file. | "" |

### SmptMailDam.Website appsettings.json

The configuration for the logging can be found in the log4net.config file.

| Setting | Description | Default value |
| --- | ---| --- |
| ConnectionStrings:DefaultConnection | The database connection string used by the SmtpMailDam.Website component. This can be overriden in Docker-Compose with the CONNECTIONSTRING setting. | "Server=192.168.1.202;Database=smtpmaildam;User Id=sa;Password=P@ssw0rd;" |

## Licensing

The code in this project is licensed under MIT license.
