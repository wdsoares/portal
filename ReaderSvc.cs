using System;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using ThingMagic;

namespace portal
{
    public class ReaderSvc
    {
        private const string _connectionString = "server=127.0.0.1;user id=root;password=senhaforte;port=3306;database=portal";

        private List<string> _readedTags;
        private Reader _reader;
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
            Console.WriteLine("debug");
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
            _reader.ParamSet("/reader/radio/readPower", 1200);
            _reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
            
            StopOnTagCount cnt = new StopOnTagCount();
            cnt.N = 5;
            StopTriggerReadPlan StopReadPlan = new StopTriggerReadPlan(cnt, null, TagProtocol.GEN2, null, null, 1000);
            _reader.ParamSet("/reader/read/plan", StopReadPlan);
            TagReadData[] tags;

            while(true)
            {
                tags = _reader.Read(500);
                foreach(var i in tags)
                {
                    Console.WriteLine("Tag read: " + i.EpcString);
                }
                OnTagRead(tags);
            }
        }
        
        public void OnTagRead(TagReadData[] tags)
        {
            foreach(var i in tags)
            {
                if(tags.Length > 0)
                {
                    string sql = "INSERT INTO saida(dataHora, tag) VALUES (now(), \" "+ i.EpcString +" \")";
                    MySqlCommand cmd = new MySqlCommand(sql, _connection);
                    cmd.ExecuteNonQuery();
                }  
            }
        }
    }
}