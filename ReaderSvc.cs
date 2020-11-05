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

        private bool readerConnStat {get; set;}
        private Database db = new Database();
        public ReaderSvc()
        {
            this.readerConnStat = false;
            this._reader = createReader("tcp://192.168.0.101:8081");
            try
            {
                this.db.createDB();
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
        public void startReading()
        {
            TagReadData[] tags;
            this.configReader();
            while(true) // Loop infinito de leitura e inserção de tags
            {
                if(!this.readerConnStat) // Checa se existe conexão com o leitor RFID, caso contrário, executa o método configReader() para conectar.
                    this.configReader();
                try // Tenta executar a leitura, caso ocorra exceção seta o status da conexão para falso
                {
                    tags = this._reader.Read(1000);
                    Task rdTask = Task.Run(() => onTagRead(tags));
                }
                catch(SystemException e)
                {
                    Console.WriteLine(e.Message);
                    this.readerConnStat = false;
                }
            }
        }

        public void configReader()
        {
            do
            {
                try
                {
                    _reader.Connect();
                    Console.WriteLine("Conectado ao leitor!");
                    this.readerConnStat = true;
                    _reader.ParamSet("/reader/region/id", (ThingMagic.Reader.Region)255);
                    SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
                    _reader.ParamSet("/reader/metadata", flagSet);
                    _reader.ParamSet("/reader/radio/readPower", 2400);
                    _reader.ParamSet("/reader/gen2/q", new Gen2.StaticQ(4));
                    SimpleReadPlan readPlan = new SimpleReadPlan(null, TagProtocol.GEN2, null, null,  1000);
                    _reader.ParamSet("/reader/read/plan", readPlan);
                }
                catch(SocketException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Erro na conexão com o leitor, será realizada nova tentativa de conexão!");
                    System.Threading.Thread.Sleep(2000);
                }
            }while(!this.readerConnStat);
        }  
        private void onTagRead(TagReadData[] tags)
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