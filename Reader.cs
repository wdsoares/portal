using System.Net.Sockets;
using ThingMagic;

namespace portal
{
    public class Reader
    {
        private static Reader _reader;

        private ThingMagic.Reader _portal;

        private Reader (string url)
        {
            ThingMagic.Reader.SetSerialTransport("tcp", SerialTransportTCP.CreateSerialReader);
            var portal = ThingMagic.Reader.Create(url);
            try
            {
                portal.Connect();
                _portal = portal;
            }
            catch
            {
                throw new SocketException(10061);
            }

        }
      
        public static Reader getInstance ()
        {
            if (_reader == null)
                _reader = new Reader("tcp://192.168.0.101:8081");

            return _reader;
        }

        public TagReadData[] ReadTags(int timeout)
        {
            _portal.ParamSet("/reader/region/id", (ThingMagic.Reader.Region)255);

            SerialReader.TagMetadataFlag flagSet = SerialReader.TagMetadataFlag.ALL;
            _portal.ParamSet("/reader/metadata", flagSet);

            _portal.ParamSet("/reader/transportTimeout", 5000);

            SimpleReadPlan plan = new SimpleReadPlan(null, TagProtocol.GEN2, null, null, 1000);

            _portal.ParamSet("/reader/read/plan", plan);

            var tags = _portal.Read(timeout);

            return tags;
        }
    }
}