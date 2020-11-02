using System;
using MySql.Data.MySqlClient;
using ThingMagic;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace portal
{
    public class ReaderSvc
    {
        private Reader _reader;
        private Database db = new Database();
        public ReaderSvc()
        {
            _reader = createReader("tcp://192.168.0.101:8081");
            try
            {
                db.openConnection();
                db.createDB();
                Console.WriteLine("DB Conectado.");
            }
            catch(MySqlException e)
            {
                Console.WriteLine(e.Message);
            }

        }
        public Reader createReader(string uri)
        {
            ThingMagic.Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
            Reader reader = Reader.Create(uri);
            return reader;
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
                //Environment.Exit(1);
            }
            
            Console.WriteLine("Conectado ao leitor!");
            _reader.ParamSet("/reader/region/id", (ThingMagic.Reader.Region)255);
            SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
            _reader.ParamSet("/reader/metadata", flagSet);
            _reader.ParamSet("/reader/radio/readPower", 2400);
            _reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
            

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
        private void OnTagRead(TagReadData[] tags)
        {
            if(tags.Length > 0)
            {
                foreach(var i in tags)
                {
                    Console.WriteLine("Tag read: " + i.EpcString);

                    if(db.checkDupe(i.EpcString) == 0)
                    {
                        db.insertDB(i.EpcString);
                    }
                }
            }  
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
    }
}