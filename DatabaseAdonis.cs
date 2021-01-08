using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace portal
{
    public class DatabaseAdonis
    {

        private string _connectionString { get; set; }
        private MySqlConnection _connection {get; set;}

        private string readerAlias { get; set; }

        public DatabaseAdonis()
        {
            this.readDBSettings();
        }
        public void openConnection()
        {
            _connection.Open();
        }
        public void closeConnection()
        {
            _connection.Close();
        }
        public void setConnection(string conn)
        {
            _connection = new MySqlConnection(conn);
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
                string user = (string)obj["adonis"]["user"];
                string password = (string)obj["adonis"]["password"];
                string host = (string)obj["adonis"]["host"];
                string port = (string)obj["adonis"]["port"];
                string dbName = (string)obj["adonis"]["dbName"];
                readerAlias = (string)obj["leitorConfigs"]["portalName"];

                this._connectionString = "server="+host+";user id="+user+";password="+password+";port="+port+";database="+dbName+";";
                this.setConnection(_connectionString);
            }
        }
        public void update(string epc)
        {
            int tag_id = this.getTagID(epc);

            string sql = "INSERT INTO saidas(tag_id, created_at, updated_at, portalName) VALUES (" + tag_id + " , now(), now(), + \"" + readerAlias + "\")";
            if(tag_id != -1)
            {
                MySqlCommand cmd = new MySqlCommand(sql, this._connection);
                try
                {
                    this.openConnection();
                    cmd.ExecuteNonQueryAsync();
                }
                catch(MySqlException e)
                {
                    Console.WriteLine(e.Message);
                }
                finally
                {
                    this.closeConnection();
                }
            }
        }
        public int getTagID(string epc)
        {
            string sql = "SELECT t.id FROM tags t WHERE t.epc like \"" + epc + "\"";
            int tag_id = -1;
            MySqlDataReader rdr = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this._connection);
                this.openConnection();
                rdr = cmd.ExecuteReader();
                
                while(rdr.Read())
                {
                    tag_id = rdr.GetInt32(0);
                }
            }
            catch(MySqlException e){
                Console.WriteLine(e.Message);
            }
            finally{
                this.closeConnection();
                rdr.Close();
            }

            return tag_id;
        }
    }
}