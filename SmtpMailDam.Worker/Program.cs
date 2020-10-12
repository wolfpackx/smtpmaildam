using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmtpMailDam.Common.Repositories;
using SmtpMailDam.Common.Interfaces;
using SmtpMailDam.Common.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.IO;

namespace SmtpMailDam.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(@Directory.GetCurrentDirectory() + "/appsettings.json")
                    .AddEnvironmentVariables()
                    .Build();
                    var connectionString = configuration.GetConnectionString("DefaultConnection");

                    services.AddHostedService<Worker>();

                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseSqlServer(
                            configuration.GetConnectionString("DefaultConnection"));
                    });

                    services.AddScoped<IMailboxRepository, MailboxRepository>();
                    services.AddScoped<IMailRepository, MailRepository>();
                })
                .ConfigureLogging((loggerBuilder) => {
                    loggerBuilder.AddLog4Net();
                })
                .ConfigureAppConfiguration((configureBuilder) => 
                {
                    configureBuilder.AddJsonFile("appsettings.json", false)
                        .AddEnvironmentVariables();
                })
                .UseWindowsService();

            return host;
        }
    }
}
