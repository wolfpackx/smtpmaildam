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
using System.Reflection;
using System.Diagnostics;

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
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    //To get the location the assembly normally resides on disk or the install directory
                    string directory = GetCurrentDirectory();

                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(directory)
                        .AddJsonFile("appsettings.json")
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
                    //To get the location the assembly normally resides on disk or the install directory
                    string directory = GetCurrentDirectory();

                    configureBuilder
                        .SetBasePath(directory)
                        .AddJsonFile(@"appsettings.json", false)
                        .AddEnvironmentVariables();
                });

            return host;
        }

        private static string GetCurrentDirectory()
        {
            try
            {
                return new Uri(
                        Path.GetDirectoryName(
                            Assembly.GetExecutingAssembly().CodeBase)
                        ).LocalPath;
            }
            catch
            {
                return Directory.GetCurrentDirectory();
            }
        }
    }
}
