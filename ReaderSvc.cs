using System;
using System.IO;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using ThingMagic;

namespace portal
{
    public class ReaderSvc
    {
        private const string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";

        private List<string> _readedTags;
        private Reader _reader;
        public ReaderSvc(MySqlConnection _connection) 
        {
            this._connection = _connection;
               
        }
                private MySqlConnection _connection {get; set;}

        public ReaderSvc(string uri)
        {
            _reader = createReader(uri);
        }
        public Reader createReader(string uri)
        {
            ThingMagic.Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
            Reader reader = Reader.Create(uri);
            return reader;
        }
        public void InsertTagsDB()
        {
            _connection = new MySqlConnection(_connectionString);
            _connection.Open();
            try
            {
                _reader.Connect();
                Console.WriteLine("Conectado!");
            }
            catch(IOException e)
            {
                Console.WriteLine(e.Message);
            }

            _reader.ParamSet("/reader/region/id", (ThingMagic.Reader.Region)255);
            SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
            _reader.ParamSet("/reader/metadata", flagSet);
            _reader.ParamSet("/reader/radio/readPower", 1800);
            _reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
            
            StopOnTagCount cnt = new StopOnTagCount();
            cnt.N = 8;
            StopTriggerReadPlan StopReadPlan = new StopTriggerReadPlan(cnt, null, TagProtocol.GEN2, null, null, 1000);
            _reader.ParamSet("/reader/read/plan", StopReadPlan);
            TagReadData[] tags;

            while(true)
            {
                tags = _reader.Read(500);
                OnTagRead(tags);
            }
        }
        
        public void OnTagRead(TagReadData[] tags)
        {
            if(tags.Length > 0)
            {
                foreach(var i in tags)
                {
                        if(selectDB(i).Length == 0)
                        {
                            string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), \""+ i.EpcString +"\")";
                            MySqlCommand cmd = new MySqlCommand(sql, _connection);
                            cmd.ExecuteNonQuery();
                        }
                }  
            }
        }

        public string selectDB(string rdrTag)
        {
            List<Tag> lista = new List<Tag>();
            string result = "";
            string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";

            MySqlConnection _connection;
            _connection = new MySqlConnection(_connectionString);
            _connection.Open();
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