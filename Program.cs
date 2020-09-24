using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;

namespace portal
{
    public class Program
    {
        private const string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";

        private static MySqlConnection _connection {get; set;}

        public static async Task Main(string[] args)
        {
            //Task rdTask = Task.Run(() => InsertTagsDB());
            await CreateHostBuilder(args).Build().RunAsync();
        }
        public static void InsertTagsDB()
        {
            _connection = new MySqlConnection(_connectionString);
            while(true)
            {
                _connection.Open();
                string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), 'tag')";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);
                cmd.ExecuteNonQuery();
                _connection.Close();
                Thread.Sleep(500);
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
