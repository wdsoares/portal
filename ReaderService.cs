using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ThingMagic;

namespace portal
{
    public class ReaderService
    {
        private Reader _reader;

        public ReaderService()
        {
            _reader = Reader.getInstance();
        }

        public List<string> Get()
        {
            var tags = _reader.ReadTags(5000);

            var tagList = new List<string>(); 

            foreach(ThingMagic.TagReadData tag in tags)
            {
                tagList.Add(tag.EpcString);
            }

            return tagList;
        }

    }
}