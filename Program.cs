using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace portal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ReaderSvc rdr = new ReaderSvc();
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(rdr.CloseConn);
            Task rdTask = Task.Run(() => rdr.startReading());
            await CreateHostBuilder(args).Build().RunAsync();
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("http://0.0.0.0:5000", "http://0.0.0.0:5001")
                              .UseStartup<Startup>();
                });
    }
}
