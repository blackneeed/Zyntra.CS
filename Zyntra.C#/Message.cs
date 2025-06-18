using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zyntra.CS
{
    public class Message : PartialMessage
    {
        public string Content;
        public long BucketID;
        public User Sender;
        public bool InCache;
    }
}