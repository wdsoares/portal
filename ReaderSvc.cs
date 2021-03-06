using System;
using MySql.Data.MySqlClient;
using ThingMagic;
using Microsoft.Extensions.Configuration;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace portal
{
    public class ReaderSvc
    {
        private Reader _reader;
        private bool readerConnStat {get; set;}
        private Database db = new Database();
        private DatabaseAdonis dbAdonis = new DatabaseAdonis();
        private string readerAddr { get; set; }
        public ReaderSvc()
        {
            readerConnStat = false;
            readJsonSettings();
            _reader = createReader(readerAddr);
            try
            {
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

        public void readJsonSettings()
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
                readerAddr = (string)obj["leitorConfigs"]["readerAddr"];
            }
        }
        public void startReading()
        {
            TagReadData[] tags;
            configReader();
            while(true) // Loop infinito de leitura e inserção de tags
            {
                if(!readerConnStat) // Checa se existe conexão com o leitor RFID, caso contrário, executa o método configReader() para conectar.
                    configReader();
                try // Tenta executar a leitura, caso ocorra exceção seta o status da conexão para falso
                {
                    tags = _reader.Read(1000);
                    Task rdTask = Task.Run(() => onTagRead(tags));
                }
                catch
                {
                    Console.WriteLine("Houve um erro na leitura, tentando conectar novamente em 10s.");
                    readerConnStat = false;
                    _reader.Destroy();
                    System.Threading.Thread.Sleep(10000);
                    _reader = createReader(readerAddr);
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
                    Console.WriteLine("Address: " + readerAddr);
                    readerConnStat = true;
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
            }while(!readerConnStat);
        }  
        private void onTagRead(TagReadData[] tags)
        {
            if(tags.Length > 0)
            {
                foreach(var i in tags)
                {
                    Console.WriteLine("Tag read: " + i.EpcString);

                    if(Regex.IsMatch(i.EpcString, @"^[0-9]*$"))
                    {
                        if(db.checkDupe(i.EpcString) == 0)
                        {
                            db.insertDB(i.EpcString);
                            dbAdonis.update(i.EpcString);
                        }
                        else
                        {
                            Console.WriteLine("Tag duplicada");
                        }
                    }
                    else
                    {
                        Console.WriteLine("TAG INVÁLIDA: " + i.EpcString);
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