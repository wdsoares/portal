using System;
using System.IO;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ThingMagic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

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
                createDB();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("DB Conectado.");

        }

        public Reader createReader(string uri)
        {
            ThingMagic.Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
            Reader reader = Reader.Create(uri);
            return reader;
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
        public void StartReading()
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
            _reader.ParamSet("/reader/radio/readPower", 2400);
            _reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
            
            /* StopOnTagCount cnt = new StopOnTagCount();
            cnt.N = 12;
            StopTriggerReadPlan StopReadPlan = new StopTriggerReadPlan(cnt, null, TagProtocol.GEN2, null, null, 1000); */

            SimpleReadPlan readPlan = new SimpleReadPlan(null, TagProtocol.GEN2, null, null,  1000);
            _reader.ParamSet("/reader/read/plan", readPlan);
            TagReadData[] tags;

            Console.WriteLine("Read PWR: " + _reader.ParamGet("/reader/radio/readPower") + " mdBm");
            while(true)
            {
                tags = _reader.Read(1000);
                Task rdTask = Task.Run(() => OnTagRead(tags));
            }
        }
        
        public void OnTagRead(TagReadData[] tags)
        {
            if(tags.Length > 0)
            {
                foreach(var i in tags)
                {
                    Console.WriteLine("Tag read: " + i.EpcString);

                    if(checkDupe(i.EpcString) == 0)
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

        public int checkDupe(string rdrTag)
        {
            List<Tag> lista = new List<Tag>();
            int result;
            string sql = "SELECT COUNT(id) FROM saida WHERE tag = \""+ rdrTag +"\"";
            MySqlCommand cmd = new MySqlCommand(sql, _connection);
            result = int.Parse(cmd.ExecuteScalar().ToString());

            return result;
        }

        public void createDB()
        {
            string createSchema = "CREATE SCHEMA IF NOT EXISTS `portal` COLLATE = `utf8mb4_0900_ai_ci`;" +
            "CREATE TABLE IF NOT EXISTS `portal`.`saida` " + 
            "(`id` int NOT NULL AUTO_INCREMENT, `dataHora` datetime NOT NULL," + 
            "`tag` varchar(100) DEFAULT NULL, PRIMARY KEY (`id`)) " +
            "ENGINE=InnoDB AUTO_INCREMENT=0 DEFAULT CHARSET=utf8mb4;";
            
            MySqlCommand cmd = new MySqlCommand(createSchema, _connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}