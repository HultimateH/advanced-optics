using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PES_WMR
{
    public class newCPU
    {
        /*  
         *  New PESWMR stuff
         *  Screw streams. everything's fixed size anyway, so why stick with that sort of overhead & pure difficulty? anyways, K.I.S.S.
         *  Then again, the easy copying etc. is really nice...
         *  Addresses are 2 bytes (0xAA 0xBB); they point to the start byte of sets of information (float, vector, int, etc.)
         *  Memory probably should be byte-addressable; would be annoying to deal with everything in bytes & shorts simultaneously
         *  
         *  Memory:     63KiB usable in boot segment, 1KiB reserved for bootloader, device constants etc.
         *      segments can be switched by simply saving & loading to file (basically paging).
         *  Wireless:   64KiB public (chunks may be sent directly through p2p connections)
         *  
         *  Instruction format:
         *      00 01 02 03 04 05 06
         *      oo aa aa bb bb cc cc
         *  
         *  Where:
         *      oo      - opcode
         *      aaaa    - operand A (usually address/destination/etc.)
         *      bbbb    - operand B
         *      cccc    - operand C
         *  Any time the instruction doesn't need 3 operands, the extras should be omitted (otherwise they'd be parsed as the start of the next instruction!)
         *  Instructions are of variable width after all.
         *  
         */

        public byte[] localMem;
        public static byte[] bootloader;

        public void Tick()
        {

        }

    }
    public class Instr
    {
        public byte op, flags, length;
    }
    public class Memory
    {
        public const ushort rwMem = 0xEFFF; // ??? not sure what I'm doing just yet
    }
}
