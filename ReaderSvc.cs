using System;
using System.IO;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ThingMagic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;

namespace portal
{
    public class ReaderSvc
    {
        private const string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";
        private Reader _reader;
        public ReaderSvc(MySqlConnection _connection) 
        {
            this._connection = _connection;
               
        }
        private MySqlConnection _connection {get; set;}
        public ReaderSvc()
        {
            _reader = createReader("tcp://192.168.0.101:8081");
            _connection = new MySqlConnection(_connectionString);
            try
            {
                _connection.Open();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("DB Conectado.");

        }

        public void CloseConn(object sender, System.EventArgs e)
        {
            try
            {
                _reader.Destroy();
            }
            catch
            {
                Console.WriteLine("Não foi possível finalizar a conexão ao leitor!");
            }
            Console.WriteLine("Conexão finalizada!");
        }
        public Reader createReader(string uri)
        {
            ThingMagic.Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
            Reader reader = Reader.Create(uri);
            return reader;
        }
        public void InsertTagsDB()
        {
            try
            {
                _reader.Connect();
            }
            catch(SocketException e)
            {
                Console.WriteLine("Erro na conexão com o leitor!");
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }
            
            Console.WriteLine("Conectado ao leitor!");
            _reader.ParamSet("/reader/region/id", (ThingMagic.Reader.Region)255);
            SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
            _reader.ParamSet("/reader/metadata", flagSet);
            _reader.ParamSet("/reader/radio/readPower", 2200);
            
            SimpleReadPlan plan = new SimpleReadPlan(null, TagProtocol.GEN2, null, null, 1000);
            _reader.ParamSet("/reader/read/plan", plan);
            TagReadData[] tags;


            Console.WriteLine("Read PWR: " + _reader.ParamGet("/reader/radio/readPower") + "mdBm");
            while(true)
            {
                tags = _reader.Read(250);
                OnTagRead(tags);
            }
        }
        
        public void OnTagRead(TagReadData[] tags)
        {
            if(tags.Length > 0)
            {
                foreach(var i in tags)
                {
                    Console.WriteLine("Tag read: " + i.EpcString);

                    if(selectDB(i.EpcString).Length < 3)
                    {
                        string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), \""+ i.EpcString +"\")";
                        MySqlCommand cmd = new MySqlCommand(sql, _connection);
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch(MySqlException e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        Console.WriteLine("Tag inserida: " + i.EpcString);
                    }
                }  
            }
        }

        public string selectDB(string rdrTag)
        {
            List<Tag> lista = new List<Tag>();
            string result = "";
            string sql = "SELECT * FROM saida WHERE tag = \""+ rdrTag +"\"";
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