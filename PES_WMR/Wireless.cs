using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PES_WMR
{
    public class MulticastHandler
    {
        private static MulticastHandler instance;
        public static MulticastHandler Instance // create singleton
        {
            get
            {
                if (instance == null)
                {
                    instance = new MulticastHandler();
                }
                return instance;
            }
        }

        public short[] WirelessMemory = new short[0x10000];
    }
    public class Multicaster
    {

    }
}
