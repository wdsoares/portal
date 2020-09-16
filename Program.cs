using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;

namespace portal
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            CreateHostBuilder(args).Build().RunAsync();
            Task<int> rdTask = foreverInsert();
        }
        public static MySqlConnection _connection {get; set;}
        public static Task<int> foreverInsert()
        {
            while(true)
            {
                _connection = new MySqlConnection("server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal");
                _connection.Open();
                string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), 'tag')";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);
                cmd.ExecuteNonQuery();
                _connection.Close();
                Thread.Sleep(3000);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
