using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PES_WMR
{
    public interface IDataBusDevice
    {
        short busID { get; }
        void Update(bool clk);
    }
    public class DataBus
    {
        public List<IDataBusDevice> DataBusDevices;
        public bool ClockLine;
        public short LineAddr;
        public bool Rx, Tx; // relative to master: master sends on Tx, and recieves on Rx
    }
}
