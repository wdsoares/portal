using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace portal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortalController : ControllerBase
    {
        /* public List<string> Get ()
        {
            var service = new ReaderService();

            var tags = service.Get();

            return tags;
        } */

        [HttpGet]
        public string consultaBD()
        {
            List<Tag> lista = new List<Tag>();
            string result = "";
            string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";

            MySqlConnection _connection;
            _connection = new MySqlConnection(_connectionString);
            _connection.Open();
            string sql = "SELECT * FROM saida";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while(rdr.Read())
            {
                lista.Add(new Tag(rdr.GetInt32(0), Convert.ToString(rdr.GetDateTime(1)), rdr.GetString(2)));
            }
            _connection.Close();
            rdr.Close();
            result = String.Concat(result, JsonConvert.SerializeObject(lista));

            return result;
        }
    }
}
