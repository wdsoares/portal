using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace portal
{
    public class Database
    {
        public Database()
        {
            this.readDBSettings();
        }    
        private string _connectionString { get; set; }
        private MySqlConnection _connection {get; set;}

        public void setConnection(string conn)
        {
            _connection = new MySqlConnection(conn);
        }
        public void openConnection()
        {
            try
            {
                _connection.Open();
            }
            catch(MySqlException ex)
            {
                Console.WriteLine("Não foi possível abrir conexão com o banco!");
                Console.WriteLine(ex.Message);
            }

        }
        public void closeConnection()
        {
            _connection.Close();
        }
        public int checkDupe(string rdrTag)
        {
            int result = -1;
            string sql = "SELECT COUNT(id) FROM saida WHERE tag = '"+ rdrTag +"'";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            try
            {
                this.openConnection();
                MySqlDataReader dtr = cmd.ExecuteReader();
                while(dtr.Read())
                {
                    result = dtr.GetInt32(0);
                }
                dtr.Close();
                this.closeConnection();
            }
            catch
            {
                Console.WriteLine("Não foi possível checar duplicatas!");
            }
            return result;
        }

        public void readDBSettings()
        {
            string arq = "";

            try
            {
                arq = File.ReadAllText("./Resources/dbSettings.json");
            }
            catch(IOException e)
            {
                Console.WriteLine("Não foi possível ler o arquivo de configs!");
                Console.WriteLine(e.Message);
            }

            if(arq.Length > 1)
            {
                JObject obj = JObject.Parse(arq);
                string user = (string)obj["portal"]["user"];
                string password = (string)obj["portal"]["password"];
                string host = (string)obj["portal"]["host"];
                string port = (string)obj["portal"]["port"];

                this._connectionString = "server="+host+";user id="+user+";password="+password+";port="+port+";database=portal";
                this.setConnection(_connectionString);
            }
        }

        public void createDB()
        {
            string createSchema = "CREATE TABLE IF NOT EXISTS `portal`.`saida` " + 
            "(`id` int NOT NULL AUTO_INCREMENT, `dataHora` datetime NOT NULL," + 
            "`tag` varchar(100) DEFAULT NULL, PRIMARY KEY (`id`)) " +
            "ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8mb4;";
            
            MySqlCommand cmd = new MySqlCommand(createSchema, _connection);

            try
            {
                this.openConnection();
                cmd.ExecuteNonQuery();
                this.closeConnection();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void insertDB(string epc)
        {
            string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), \""+ epc +"\")";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            try
            {
                this.openConnection();
                cmd.ExecuteNonQuery();
                this.closeConnection();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Tag inserida: " + epc);
        }

        public string get()
        {
            List<Tag> lista = new List<Tag>();
            string result = "";

            try
            {
                string sql = "SELECT * FROM saida";
                MySqlCommand cmd = new MySqlCommand(sql, _connection);
                this.openConnection();
                MySqlDataReader rdr = cmd.ExecuteReader();

                while(rdr.Read())
                {
                    lista.Add(new Tag(rdr.GetInt32(0), Convert.ToString(rdr.GetDateTime(1)), rdr.GetString(2)));
                }
                this.closeConnection();
                rdr.Close();
                result = String.Concat(result, JsonConvert.SerializeObject(lista));
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
        public string get(string tag)
        {
            List<Tag> lista = new List<Tag>();
            string result = "";

            string sql = "SELECT * FROM saida WHERE tag = \"" + tag + "\"";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);

            try
            {
                this.openConnection();
                MySqlDataReader rdr = cmd.ExecuteReader();

                while(rdr.Read())
                {
                    lista.Add(new Tag(rdr.GetInt32(0), Convert.ToString(rdr.GetDateTime(1)), rdr.GetString(2)));
                }
                this.closeConnection();
                rdr.Close();
                result = String.Concat(result, JsonConvert.SerializeObject(lista));
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }

            return result;
        }
    }
}