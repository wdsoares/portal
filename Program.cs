using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using ThingMagic;

namespace portal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ReaderSvc rdr = new ReaderSvc("tcp://192.168.0.101:8081");
            Task rdTask = Task.Run(() => rdr.InsertTagsDB());
            await CreateHostBuilder(args).Build().RunAsync();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
