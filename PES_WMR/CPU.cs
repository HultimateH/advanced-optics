using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PES_WMR
{
    public class CPU
    {
        private static readonly byte[] MagicBytes = new byte[]
        {
            0x50, 0x45, 0x53,   // P E S
            0x57, 0x4D, 0x52,   // W M R
            0x00, 0x00          // . .
        };
        // specs here:
        /*
            internal set of ram/address space
            address space ranges from 0x0000 to 0xFFFF
            each address contains 2 bytes - 0x0000 to 0xFFFF
                totals ~64KB
            8 special registers:
                2 used by .range extension  (short)
                2 used as program counter   (short & byte)
                4 used as a set of flags    (byte)
            2 bus 'registers', used for communicating with
                extra memory, modules, etc.
            instructions are 2 bytes: 1 opcode, and
                1 set of modifiers (8 different flags)
            
            The PES-WMR doesn't support complex or floating-point numbers
                natively, but an extension could act as a delegate for these
                operations.
            Compiled files have the magic numbers
                0x50, 0x45, 0x53, 0x57, 0x4D, 0x52, 0x00, 0x00
                (PESWMR + 2 empty bytes in ASCII)
                at the very start, and have the extension pbin.
            Human-readable files don't have any magic numbers, and
                have the extension pasm. Instructions are seperated by
                'nix linebreaks (just \n)
            The PES-WMR supports user-defined opcodes... in a special way..?
                Possibly done by parsing files first for the bopc (0xF3) code,
                and linking the defined instruction to a set of addresses.
                To use bopc, simply write in code:
                bopc [function start] [where to set operands] [opcode byte representation]
                    Don't forget to end your definition with the ret (0x32) code,
                    or else execution will continue endlessly!
                Any and all mentions of uc[byte representation] will be equivalent to
                set [operand address] [A]           2800 [addr] [AAAA] 0000
                set [operand address + 0x0001] [B]  2800 [addr] [BBBB] 0000
                call [function start]               3100 [addr]  0000  0000
                Make sure that your chosen opcode doesn't overlap with any other, or else the
                CPU will be forced to dump a stack trace and halt. The advantage of using bopc
                is functions can be defined in the bootloader, downloaded off the internet (using extensions),
                and more, while only taking one cycle.
        */
        private string bootloaderPath;      // Location of compiled bootloader file
        private short bootloaderOffset;     // Start of bootloader instructions
        private short bootloaderRunOffset;  // Start of program instructions (0x0000 by default, used by bootloader)
        
        private short[] localMem = new short[0x10000]; // indices are zero-based
        private short rangeRegA, rangeRegB, PC;
        private byte PCstp;
        private short[] PCstack = new short[0x100];
        private ushort flagsLower, flagsUpper;
        private ushort condiFlagsL, condiFlagsU;
        /*
            lower flags:
            00: negative result
            01: zero result
            02: carry/overflow
            03: even parity
            04: comparison true 
            05: division by zero
            06: interruptable (NMIs still trigger)
            07: 'trap' (CPU must be stepped externally)
            08: fatal peripheral/bus error
            09: PC overflow
            10: out of bounds
            11: call stack overflow
            12: unauthorized operation
            13: invalid operation/fatal error
            14: kill signal(!), set by halt

            upper flags are user-defined.
        */

        private void CopyBytesToMem(byte[] bytes, int offset)
        {
            for (int i = 0; i < bytes.Length; i += 2)
            {
                byte bt;
                if (i * 2 >= bytes.Length)
                {
                    bt = 0; // right-pad the short with 0s if bytes has odd length
                }
                else bt = bytes[i * 2];
                localMem[offset + i / 2] = (short)(bytes[i] << 8 + bt); 
            }
        }
        private void LoadBootloaderIntoMemory() // does what it says on the tin
        {
            LoadFile(bootloaderPath, bootloaderOffset, 0x1000);
        }
        private bool LoadFile(string path, int offset, int size)
        {
            if (offset + size > localMem.Length) return false;
            if (Path.GetExtension(path) != ".pbin") return false;
            if (File.Exists(path))
            {
                // 0x1000 is 4096, and 0x001 to 0xfff is 4095: the extra 1 comes from 0x000 being valid
                byte[] codeBytes = new byte[size];
                using (FileStream file = File.OpenRead(path))
                {
                    byte[] magicTest = new byte[8]; // PESWMR & 2 empty bytes
                    file.Read(magicTest, 0, magicTest.Length);
                    // checking whether this is a valid code file
                    if (magicTest != MagicBytes) return false;
                    // filestream doesn't seek to the start of the file each time,
                    // so the magic numbers have already been trimmed for us
                    file.Read(codeBytes, 0, codeBytes.Length);
                }
                // EEAASSY MONEEEYYY
                CopyBytesToMem(codeBytes, offset);
                return true;
            }
            return false;
        }
        private void DumpFile(byte[] bytes)
        {
            DateTime dumpTime = DateTime.UtcNow;
            string timeFormat = "yyyyMMdd-HHmmss"; // eg. PESWMR-Dump-20160523-121734 for 12:17.34 23/05/16
            string fileName = "PESWMR-Dump-" + dumpTime.ToString(timeFormat) + ".pbin";

            using (FileStream file = File.OpenWrite(fileName))
            {
                file.Write(MagicBytes, 0, MagicBytes.Length);
                file.Write(bytes, 0, bytes.Length);
            }
        }
        public DataBus Bus;
        
        private short Compile2Bytes(byte b1, byte b2)
        {
            return (short)((b1 << 8) | b2);
        }
        private byte[] Parse4ShortsToBytes(short[] shorts) // just goes from memory (short[]) to file? (byte[])
        {
            if (shorts.Length != 4) return new byte[1];
            byte[] bytes = new byte[]
            {
                (byte)(shorts[0] >> 8),     // if 0xaabb is 1 short, then byte #0 = 0xaa
                (byte)(shorts[0] & 0xFF),   // and then byte #1 = 0xbb
                (byte)(shorts[1] >> 8),
                (byte)(shorts[1] & 0xFF),
                (byte)(shorts[2] >> 8),
                (byte)(shorts[2] & 0xFF),
                (byte)(shorts[3] >> 8),
                (byte)(shorts[3] & 0xFF)
            };
            return bytes;
        }
        private Instruction Parse4Shorts(short[] shorts)
        {
            if (shorts.Length != 4) return new Instruction();
            Instruction instr = new Instruction();
            instr.opcode = (Opcodes)(shorts[0] >> 8);
            instr.opmod = (Opmods)(shorts[0] & 0xFF);
            instr.operandStore = shorts[1];
            instr.operandA = shorts[2];
            instr.operandB = shorts[3];
            return instr;
        }
        private Instruction Parse8Bytes(byte[] bytes) // opcode, opmod, 2x addr, 4x operands = 8 bytes
        {
            if (bytes.Length != 8) return new Instruction(); // would return null but can't
            Instruction instr = new Instruction();
            instr.opcode = (Opcodes)bytes[0];
            instr.opmod = (Opmods)bytes[1];
            instr.operandStore = Compile2Bytes(bytes[2], bytes[3]);
            instr.operandA = Compile2Bytes(bytes[4], bytes[5]);
            instr.operandB = Compile2Bytes(bytes[6], bytes[7]);
            return instr;
        }
        /*private void DoInstruction(Instruction instr)
        {
            short stoValue, opA, opB;
            short[] wirelessMem;
            if (instr.opmod.HasFlag(Opmods.condi) && // conditional; check that we can do stuff
                (condiFlagsL & flagsLower) == 0 && (condiFlagsU & flagsUpper) == 0)
            {
                PC += 8; // go forth, brave knights!
                return;
            }
            // condi wasn't set or eval'd to true, so let's continue
            wirelessMem = MulticastHandler.Instance.WirelessMemory; // need to get wireless memory
            
            // deal with some special cases
            if (instr.opcode == Opcodes.mcop || instr.opcode == Opcodes.mmov)
            {
                // copy to/from wireless
                if (instr.opmod.HasFlag(Opmods.mcast))
                {
                    localMem[instr.operandStore & 0xFFFF] = wirelessMem[instr.operandA & 0xFFFF];
                    if (instr.opcode == Opcodes.mmov) wirelessMem[instr.operandA & 0xFFFF] = 0;
                }
                else
                {
                    wirelessMem[instr.operandStore & 0xFFFF] = localMem[instr.operandA & 0xFFFF];
                    if (instr.opcode == Opcodes.mmov) localMem[instr.operandA & 0xFFFF] = 0;
                }
                return;
            }
            if (instr.opmod.HasFlag(Opmods.litra)) opA = instr.operandA;
            //else opA = memory[instr.operandA & 0xFFFF]; // wouldn't be any good to have negative indices -_-
            if (instr.opmod.HasFlag(Opmods.litrb)) opB = instr.operandB;
            //else opB = memory[instr.operandB & 0xFFFF];
        }*/
        private bool OddParity(short s)
        {
            int x = s;
            x ^= x >> 8;
            x ^= x >> 4;
            x &= 0xf;
            return ((0x6996 >> x) & 1) == 1;

        }
        private void DoMath(Instruction instr)
        {
            short[] useMem = instr.opmod.HasFlag(Opmods.mcast) ? MulticastHandler.Instance.WirelessMemory : localMem;
            ushort addr = (ushort)(instr.operandStore & 0xFFFF);
            short opA = instr.opmod.HasFlag(Opmods.litra) ? instr.operandA : useMem[instr.operandA & 0xFFFF];
            short opB = instr.opmod.HasFlag(Opmods.litrb) ? instr.operandB : useMem[instr.operandB & 0xFFFF];
            flagsLower &= 0xFFC0; // reset math-related flags to zero
            switch (instr.opcode)
            {
                case Opcodes.add:
                    useMem[addr] = (short)(opA + opB);
                    flagsLower |= (ushort)((useMem[addr] != opA + opB) ? 4 : 0);  // overflow
                    break;
                case Opcodes.sub:
                    useMem[addr] = (short)(opA - opB);
                    flagsLower |= (ushort)((useMem[addr] != opA - opB) ? 4 : 0);
                    break;
                case Opcodes.mul:
                    useMem[addr] = (short)(opA * opB);
                    flagsLower |= (ushort)((useMem[addr] != opA * opB) ? 4 : 0);
                    break;
                case Opcodes.div:
                    if (opB == 0)
                    {
                        flagsLower |= 32; // division by zero
                        useMem[addr] = short.MinValue;
                        break;
                    }
                    useMem[addr] = (short)(opA / opB);
                    break;
                case Opcodes.abs:
                    useMem[addr] = Math.Abs(opA);
                    break;
                case Opcodes.mod:
                    if (opB == 0)
                    {
                        flagsLower |= 32;
                        useMem[addr] = short.MinValue;
                        break;
                    }
                    useMem[addr] = (short)(opA % opB);
                    break;
                case Opcodes.neg:
                    useMem[addr] = (short)-opA;
                    break;
            }
            flagsLower |= (ushort)((useMem[addr] < 0) ? 1 : 0);
            flagsLower |= (ushort)((useMem[addr] == 0) ? 2 : 0);
            flagsLower |= (ushort)(!OddParity(useMem[addr]) ? 8 : 0);
        }
        private void DoBoolean (Instruction instr)
        {
            short[] useMem = instr.opmod.HasFlag(Opmods.mcast) ? MulticastHandler.Instance.WirelessMemory : localMem;
            ushort addr = (ushort)(instr.operandStore & 0xFFFF);
            short opA = instr.opmod.HasFlag(Opmods.litra) ? instr.operandA : useMem[instr.operandA & 0xFFFF];
            short opB = instr.opmod.HasFlag(Opmods.litrb) ? instr.operandB : useMem[instr.operandB & 0xFFFF];
            flagsLower &= 0xFFC0; // reset math-related flags to zero
            switch (instr.opcode)
            {
                case Opcodes.or:
                    useMem[addr] = (short)(opA | opB);
                    break;
                case Opcodes.and:
                    useMem[addr] = (short)(opA & opB);
                    break;
                case Opcodes.xor:
                    useMem[addr] = (short)(opA ^ opB);
                    break;
                case Opcodes.nor:
                    useMem[addr] = (short)(~(opA | opB));
                    break;
                case Opcodes.nand:
                    useMem[addr] = (short)(~(opA & opB));
                    break;
                case Opcodes.xnor:
                    useMem[addr] = (short)(~(opA ^ opB));
                    break;
            }
            flagsLower |= (ushort)((useMem[addr] < 0) ? 1 : 0);           // negative result
            flagsLower |= (ushort)((useMem[addr] == 0) ? 2 : 0);          // zero result
            flagsLower |= (ushort)(!OddParity(useMem[addr]) ? 8 : 0);     // even parity
        }
        private void DoBitwise (Instruction instr)
        {
            short[] useMem = instr.opmod.HasFlag(Opmods.mcast) ? MulticastHandler.Instance.WirelessMemory : localMem;
            ushort addr = (ushort)(instr.operandStore & 0xFFFF);
            short opA = instr.opmod.HasFlag(Opmods.litra) ? instr.operandA : useMem[instr.operandA & 0xFFFF];
            short opB = instr.opmod.HasFlag(Opmods.litrb) ? instr.operandB : useMem[instr.operandB & 0xFFFF];
            flagsLower &= 0xFFC0; // reset math-related flags to zero
            switch (instr.opcode)
            {
                case Opcodes.sl:
                    useMem[addr] = (short)(opA << (ushort)opB);
                    break;
                case Opcodes.sr:
                    // use two shifts to preserve sign bit
                    useMem[addr] = (short)(((opA << 16) >> 16) >> (ushort)opB);
                    break;
                case Opcodes.srn:
                    useMem[addr] = (short)(((uint)opA) >> (ushort)opB);
                    break;
                case Opcodes.rl:
                    useMem[addr] = (short)(opA << (ushort)opB | opA >> (16 - (ushort)opB));
                    break;
                case Opcodes.rr:
                    useMem[addr] = (short)(opA >> (ushort)opB | opA << (16 - (ushort)opB));
                    break;
            }
            flagsLower |= (ushort)(!OddParity(useMem[addr]) ? 8 : 0);
        }
        private void DoComparison (Instruction instr)
        {
            short[] useMem = instr.opmod.HasFlag(Opmods.mcast) ? MulticastHandler.Instance.WirelessMemory : localMem;
            ushort addr = (ushort)(instr.operandStore & 0xFFFF);
            short opA = instr.opmod.HasFlag(Opmods.litra) ? instr.operandA : useMem[instr.operandA & 0xFFFF];
            short opB = instr.opmod.HasFlag(Opmods.litrb) ? instr.operandB : useMem[instr.operandB & 0xFFFF];
            flagsLower &= 0xFFC0; // flag 04 is the only one that needs to be set when doing a comparison
            flagsUpper &= (ushort)~addr;  // apart from the user-defined flags that is...
            bool result = false;
            switch (instr.opcode)
            {
                case Opcodes.gt:
                    result = opA > opB;
                    break;
                case Opcodes.lt:
                    result = opA < opB;
                    break;
                case Opcodes.eq:
                    result = opA == opB;
                    break;
                case Opcodes.gte:
                    result = opA >= opB;
                    break;
                case Opcodes.lte:
                    result = opA <= opB;
                    break;
                case Opcodes.neq:
                    result = opA != opB;
                    break;
                case Opcodes.bit:
                    result = (opA & (1 << opB)) != 0;
                    break;
            }
            flagsLower |= (ushort)(result ? 16 : 0);    // comparison true
            flagsUpper |= (ushort)(result ? addr : 0);  // user-defined flags - which ones to set are given in [addr]
        }
        private void DoMemoryOp (Instruction instr)
        {
            short[] useMem = instr.opmod.HasFlag(Opmods.mcast) ? MulticastHandler.Instance.WirelessMemory : localMem;
            ushort addr = (ushort)(instr.operandStore & 0xFFFF);
            short opA = instr.opmod.HasFlag(Opmods.litra) ? instr.operandA : useMem[instr.operandA & 0xFFFF];
            short opB = instr.opmod.HasFlag(Opmods.litrb) ? instr.operandB : useMem[instr.operandB & 0xFFFF];

            switch (instr.opcode)
            {
                case Opcodes.inc:
                    useMem[addr]++;
                    break;
                case Opcodes.dec:
                    useMem[addr]--;
                    break;
                case Opcodes.move:
                    useMem[addr] = opA;
                    if (!instr.opmod.HasFlag(Opmods.litra)) useMem[instr.operandA & 0xFFFF] = 0;
                    break;
                case Opcodes.copy:
                    useMem[addr] = opA;
                    break;
                case Opcodes.del:
                    useMem[addr] = 0;
                    break;
                case Opcodes.save: // this is the atomic version? Then again, you don't want stuff changing while dumping... :S
                    byte[] saveBytes = new byte[2*opA+2];
                    for (int i = 0; i < opA+1; i++)
                    {
                        saveBytes[i] = (byte)(useMem[addr + i] >> 8);          // big end
                        saveBytes[i + 1] = (byte)(useMem[addr + i] & 0xFF);    // little end
                    }
                    DumpFile(saveBytes);
                    break;
                case Opcodes.load:
                    LoadFile("PESWMR-File-" + opB.ToString("x4") + ".pbin", addr, opA); // eg. opB = 002a -> PESWMR-File-002a.pbin
                    break;
                case Opcodes.swap:
                    short a = useMem[addr], b = useMem[opA & 0xFFFF];
                    useMem[addr] = b; useMem[opA & 0xFFFF] = a;
                    break;
                case Opcodes.set:
                    useMem[addr] = opA; // functionally equivalent to copy, but .range is faster
                    break;
            }
        }

        private class LoopbackRef : IHighBandwidthBusDevice
        {
            private DataBus bus;
            private short[] mem;
            public short busID
            {
                get
                {
                    return 0x0000;
                }
            }
            public LoopbackRef (DataBus bus, short[] mem)
            {
                this.bus = bus;
                this.mem = mem;
            }
            public void PullChunk(short addr, short offset)
            {
                for (int i = 0; i <= offset; i++)
                {
                    bus.Rx.Push(mem[addr + i]); // read from addr to addr+offset (inclusive). PullChunk(addr, 0) reads 1 address.
                }
            }
            public void PushChunk(short addr, short offset)
            {
                for (int i = 0; i <= offset; i++)
                {
                    mem[addr + i] = bus.Tx.Pop();
                }
            }

            public void Update(bool clk)
            {
                if (!clk) return;
                byte nextA = (byte)(bus.Tx.Peek() >> 8);    // assuming an instruction was sent, nextA is the opcode (oommrrrraaaabbbb)
                byte nextB = (byte)(bus.Tx.Peek() & 0xFF);  
                if (nextA == 0x22 || nextA == 0x23 || nextA == 0x00) return; // WIP
            }
        }
    }
    
    public enum Opcodes : byte
    {
        // no operation
        nop = 0x00,
        // arithmatic
        // ... absolute, modulo, negate (0 - var)
        add = 0x01, sub = 0x02, mul = 0x03, div = 0x04,
        abs = 0x15, mod = 0x16, neg = 0x17,
        // boolean operators
        or = 0x05, and = 0x06, xor = 0x07, nor = 0x08,
        nand = 0x09, xnor = 0x0A,
        // bitwise operators
        // shift left, right, right-non-preserving (sign bit isn't preserved)
        // rotate left, right
        sl = 0x10, sr = 0x11, srn = 0x12, rl = 0x13,
        rr = 0x14,
        // comparison
        // > < == >= <= !=
        // check if bit set
        gt = 0x0B, lt = 0x0C, eq = 0x0D,
        gte = 0x1B, lte = 0x1C, neq = 0x1D, bit = 0x0F,
        // register/memory operators
        // var++, --, move to new location, copy to new location
        // destroy address (null), dump region to file, load region from file
        // swap values of addresses, set value of address
        // var allocated, var unallocated
        // compile 2 bytes to short
        // decompile short into 2 bytes
        inc = 0x20, dec = 0x21, move = 0x22,
        copy = 0x23, del = 0x24, save = 0x25, load = 0x26,
        swap = 0x27, set = 0x28, def = 0x0E, ndef = 0x1E,
        cmp = 0x1F, dcmp = 0x2F,
        // flow-of-execution
        // jump, call 'function', return execution from 'function'
        // wait cycles, wait until var changed
        // race condition (wait until any var changed)
        // ranged synchronise (wait until all changed)
        // switch which flag(s) to use with .condi modifier
        jmp = 0x30, call = 0x31, ret = 0x32, wait = 0x33,
        sync = 0x34, race = 0x35, usef = 0x36,
        // bus
        // clock, set bus address,
        // push, pop, peek, push bit,
        // wait until device ready
        bclk = 0x40, baud = 0x41, badr = 0x42, bpsh = 0x43,
        bpop = 0x44, bpek = 0x45, bbit = 0x46, bdrn = 0x47,
        // multicasting
        // name defined address,
        // check name of defined address against set of chars
        // copy to multicasting address, move to multicasting address
        mnme = 0x4C, mchk = 0x4D, mcop = 0x4E, mmov = 0x4F,
        // CPU state/bootloader only
        // set register value, get register value,
        // bind register to memory location, bind opcode to location(!)
        // set signal, clear signal, set non-maskable interrupt,
        // stack trace, reset CPU, halt CPU
        sreg = 0xF0, greg = 0xF1, breg = 0xF2, bopc = 0xF3,
        ssig = 0xFA, csig = 0xFB, snmi = 0xFC, trce = 0xFD,
        rst = 0xFE, halt = 0xFF,
    }
    [Flags]
    public enum Opmods : byte
    {
        // range causes operation to run on [addr] between rangeRegA and rangeRegB
        // likely destructive modifier
        range = 1, // 0000 0001
        // accum works like range, but instead of operating on [addr], it becomes an operand
        // shouldn't change anything except a register or two
        accum = 2, // 0000 0010
        // condi is simply a conditional modifier - eg. add.condi [store] [a] [b] becomes
        // if (flags[0] & 1) localMem[store] = byteReg[0] + byteReg[1]
        condi = 4, // 0000 0100
        // nflag just makes operators that normally set flags (overflow, carry etc.) not set them.
        nflag = 8, // 0000 1000
        // litra makes operators take A as a literal value instead of loading from localMem[addr]
        // so add.litra.litrb [store] 0x0F 0x2D is possible with this modifier & the following one.
        // (btw, the above stores 0x3C, or 60 in memory)
        litra = 16, // 0001 0000
        // litrb is like litra - it'll make operators take [B] as a literal value instead of a reference
        // to a value.
        litrb = 32, // 0010 0000
        // atomc forces instructions to be atomic, regardless of task size. So instead of copy.range
        // working over several ticks, copy.range.atomc will try to deal with the entire task at once.
        // not recommended for copying large amounts of data, dumping/loading from HDD etc. as the process
        // WILL lag. might be able to speed stuff up by using C#'s unsafe functionality...
        atomc = 64, // 0100 0000
        // causes function to operate on the wireless range (a separate chunk of memory that's global to all
        // CPUs). Address space is still limited to ~64kB max, but none of that is being used up by the bootloader
        // or code at simulation start.
        mcast = 128
    }
    public struct Instruction
    {
        public Opcodes opcode;
        public Opmods opmod;
        public short operandStore, operandA, operandB;
    }
    public static class CPUUtil
    {
        // returns true if matches contains toMatch. used for checking bytes against valid templates.
        public static bool MatchInSet<T>(T toMatch, params T[] matches) where T : IComparable<T>
        {
            for (int i = 0; i < matches.Length; i++)
            {
                if (toMatch.CompareTo(matches[i]) == 0) return true;
            }
            return false;
        }
        // compiles 2 bytes into a short
        public static short CompileBytesToShort(byte upper, byte lower)
        {
            return (short)((upper << 8) | lower);
        }
        // compiles a set of bytes into a set of shorts. appends 0x00 if number of input bytes is odd.
        // so eg. 0a 23 15 09 ff -> 0a23 1509 ff00
        public static short[] CompileBytesToShort(byte[] bytes)
        {
            byte[] tBytes = ((bytes.Length % 2) == 0) ? new byte[bytes.Length] : new byte[bytes.Length + 1];
            bytes.CopyTo(tBytes, 0);
            short[] shorts = new short[tBytes.Length / 2];
            for (int i = 0; i < shorts.Length; i++)
            {
                shorts[i] = CompileBytesToShort(tBytes[i * 2], tBytes[i * 2 + 1]);
            }
            return shorts;
        }
    }
}
