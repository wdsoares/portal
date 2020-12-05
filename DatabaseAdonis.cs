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

                this._connectionString = "server="+host+";user id="+user+";password="+password+";port="+port+";database=adonis";
                this.setConnection(_connectionString);
            }
        }
        public void update(string epc)
        {
            int barcode_id = this.getBarcodeID(epc);
            int product_id = this.getProductID(epc);

            string sql = "INSERT INTO saidas(id_produto, epc, barcode_id, created_at, updated_at) VALUES (" + product_id + ", \"" + epc + "\", " + barcode_id + ", now(), now())";
            if(barcode_id != -1 && product_id != -1)
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

        public int getBarcodeID(string epc)
        {
            string sql = "SELECT barcode_id FROM tags WHERE epc = \"" + epc + "\"";
            int barcode_id = -1;
            MySqlDataReader rdr = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this._connection);
                this.openConnection();
                rdr = cmd.ExecuteReader();
                
                while(rdr.Read())
                {
                    barcode_id = rdr.GetInt32(0);
                }
            }
            catch(MySqlException e){
                Console.WriteLine(e.Message);
            }
            finally{
                this.closeConnection();
                rdr.Close();
            }

            return barcode_id;
        }

        public int getProductID(string epc)
        {
            string sql = "SELECT b.product_id FROM barcodes b JOIN tags t ON b.id = t.barcode_id WHERE t.epc = \"" + epc + "\"";
            int product_id = -1;
            MySqlDataReader rdr = null;

            try
            {
                MySqlCommand cmd = new MySqlCommand(sql, this._connection);
                this.openConnection();
                rdr = cmd.ExecuteReader();
                
                while(rdr.Read())
                {
                    product_id = rdr.GetInt32(0);
                }
            }
            catch(MySqlException e){
                Console.WriteLine(e.Message);
            }
            finally{
                this.closeConnection();
                rdr.Close();
            }

            return product_id;
        } 
    }
}