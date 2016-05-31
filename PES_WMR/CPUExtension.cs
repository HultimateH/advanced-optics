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
        public short LineAddr; // attempting to change bus address while either Rx or Tx aren't empty kills CPU
        public Stack<short> Rx, Tx; // CPU recieves on Rx, sends to devices on Tx
        
    }
    public interface IHighBandwidthBusDevice : IDataBusDevice
    {
        void PushChunk(short addr, short offset); // reads from Tx into device memory
        void PullChunk(short addr, short offset); // pushes device memory to Rx
    }
}
