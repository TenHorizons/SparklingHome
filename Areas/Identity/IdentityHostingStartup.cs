using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SparklingHome.Areas.Identity.Data;
using SparklingHome.Data;

[assembly: HostingStartup(typeof(SparklingHome.Areas.Identity.IdentityHostingStartup))]
namespace SparklingHome.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<SparklingHomeContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("SparklingHomeContextConnection")));

                services.AddDefaultIdentity<SparklingHomeUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<SparklingHomeContext>();
            });
        }
    }
}