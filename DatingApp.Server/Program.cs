using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Hosting;

namespace DatingApp.Server
{
    public class Program
    {
        [Obsolete]
        public static async Task Main(string[] args)
        {
            var silo = new SiloHostBuilder()
                   .UseLocalhostClustering()
                   //.ConfigureServices(services =>
                   //{
                   //    services.AddDbContext<UserDbSet>(o => o.UseSqlite("Data Source=UserTest.db"));
                   //    services.AddMediatR(typeof(Startup).Assembly);
                   //})
                   .AddSimpleMessageStreamProvider("SMSProvider")
                   .AddMemoryGrainStorage("PubSubStore")
                   .EnableDirectClient()
                   .Build();

            await silo.StartAsync();

            var client = silo.Services.GetRequiredService<IClusterClient>();

            await Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(services =>
                    {
                        services.AddSingleton<IGrainFactory>(client);
                        services.AddSingleton<IClusterClient>(client);
                    });
                    webBuilder.UseStartup<Startup>();
                })
                .Build().RunAsync();
        }
    }

    public interface IMyGrain : IGrainWithIntegerKey
    {
        Task<T> Echo<T>(T value);
    }

    public class MyGrain : Grain, IMyGrain
    {
        public Task<T> Echo<T>(T value) => Task.FromResult(value);
    }
}
