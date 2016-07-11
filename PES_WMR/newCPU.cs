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
         *  To start, memory is represented as a MemoryStream (allows for fast copying, easy read/write etc.)
         *  This also means that under the hood, one address refers to 2 bytes in big-endian format (0xAA 0xBB) rather than a short (0xAABB)
         *  Memory was found to total 128KiB I think, not something else
         *  
         *  Memory:     120KiB usable, 8KiB reserved for bootloader etc.
         *  Wireless:   128KiB public (chunks may be sent directly through p2p connections)
         *  
         *  Instruction format:
         *      00 01 02 03 04 05 06 07
         *      oo mm aa aa bb bb cc cc
         *  Where:
         *      oo      - opcode
         *      mm      - variation
         *      aaaa    - operand A (usually address/destination/etc.)
         *      bbbb    - operand B
         *      cccc    - operand C
         *  Instructions are of fixed length (makes it easier to parse stuff); missing/blank/etc. operands are replaced with 0000 when compiling
         *  Invalid instructions are ignored? tossed? treated as potentially fatal errors? o_O
         *  
         */

        public MemoryStream localMem;
        public static MemoryStream bootloader;

    }
}
