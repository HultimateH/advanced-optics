using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PCode
{
    class Program
    {
        static Regex preprocTags = new Regex(
            @"^#if\s\w+$|^#else$|^#elseif\s\w+$|^#define\s[a-zA-Z]+\w{3,}$|^#precomp\s[0-9a-fA-F]{4}\s?[0-9a-fA-F]{4}\s?[0-9a-fA-F]{4}\s?[0-9a-fA-F]{4}$");
        static Regex opCodeTags = new Regex(@"^\w{1,4}$");
        static Regex opModeTags = new Regex(@"^\w{5}$");
        static Regex valueTags = new Regex(@"^[]");
        enum PreprocState
        {
            None,
            If,
            Elseif,
            Else,
            Define
        }
        static void Main(string[] args)
        {
            if (!File.Exists(args[0])) return;
            loadLookupValues();
            PreprocState preprocState = PreprocState.None;
            List<string> linesToParse = File.ReadLines(args[0]).ToList();
            List<byte> bytesToWrite = new List<byte>();
            List<string> preprocStrings = new List<string>();
            bool skipLine = false;
            bool lastIfTrue = false;
            bool errored = false;
            string errorStr = "";
            foreach (string str in linesToParse)
            {
                if (skipLine)
                {
                    skipLine = false;
                    continue;
                }
                string[] commands = str.Split('.', ' ');
                if (preprocTags.IsMatch(str))
                {
                    switch (commands[0])
                    {
                        case "#if":
                            preprocState = PreprocState.If;
                            if (preprocStrings.Contains(commands[1]))
                            {
                                lastIfTrue = true;
                                continue;
                            }
                            else
                            {
                                skipLine = true;
                                lastIfTrue = false;
                                continue;
                            }
                        case "#elseif":
                            if (preprocState != PreprocState.If && preprocState != PreprocState.Elseif)
                            {
                                errored = true;
                                errorStr = "Preprocessor state invalid for #elseif command!";
                                break;
                            }
                            else
                            {
                                preprocState = PreprocState.Elseif;
                                if (lastIfTrue)
                                {
                                    skipLine = true;
                                    continue;
                                }
                                if (preprocStrings.Contains(commands[1]))
                                {
                                    lastIfTrue = true;
                                    continue;
                                }
                                else
                                {
                                    skipLine = true;
                                    lastIfTrue = false;
                                    continue;
                                }
                            }
                        case "#else":

                            if (preprocState != PreprocState.If && preprocState != PreprocState.Elseif)
                            {
                                errored = true;
                                errorStr = "Preprocessor state invalid for #else command!";
                                break;
                            }
                            else
                            {
                                preprocState = PreprocState.Else;
                                if (lastIfTrue)
                                {
                                    skipLine = true;
                                    continue;
                                }
                                continue;
                            }
                        case "#define":
                            if (!preprocStrings.Contains(commands[1]))
                                preprocStrings.Add(commands[1]);
                            continue;
                        case "#precomp":
                            bytesToWrite.AddRange(
                                new byte[] {
                                    (byte)(short.Parse(commands[1]) >> 8),
                                    (byte)(short.Parse(commands[1]) & 0xFF),
                                    (byte)(short.Parse(commands[2]) >> 8),
                                    (byte)(short.Parse(commands[2]) & 0xFF),
                                    (byte)(short.Parse(commands[3]) >> 8),
                                    (byte)(short.Parse(commands[3]) & 0xFF),
                                    (byte)(short.Parse(commands[4]) >> 8),
                                    (byte)(short.Parse(commands[4]) & 0xFF)
                                });
                            continue;
                    }
                    if (errored)
                    {
                        // handle an error
                        return;
                    }
                }
                else
                {
                    if (str[0] != '#')
                    {

                    }
                }
            }
        }
        static byte[] ParseLine(string line)
        {
            return new byte[] { };
        }
        static Dictionary<string, byte> opcodeLookup = new Dictionary<string, byte>();
        static void loadLookupValues()
        {
            string[] keys = Enum.GetNames(typeof(Opcodes));
            byte[] values = (byte[])Enum.GetValues(typeof(Opcodes));
            for (int i = 0; i < keys.Length; i++)
            {
                opcodeLookup.Add(keys[i], values[i]);
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
    }
    public class InstrTemplate
    {
        public string Name;
        public byte Length, DestBus;
        public byte Opcode, ValidModMask;

        public InstrTemplate(string name, byte length, byte dest, byte op, byte mMask)
        {
            Name = name;
            Length = length;
            DestBus = dest;
            Opcode = op;
            ValidModMask = mMask;
        }
        public bool IsValidMod(byte mods)
        {
            return (ValidModMask ^ mods) == 0;
        }
        public bool InstMatchTemp(Instruction instr) // Instruction Matches this Template
        {
            return (instr.op == Opcode) && ((instr.mod & ~ValidModMask) == 0); // also need to check length
        }
    }
    public struct Instruction
    {
        public byte op, mod, addrL, addrH, opAL, opAH, opBL, opBH;
        public Instruction(params byte[] args)
        {
            byte[] nArgs = new byte[8];
            if (args.Length == 8) args.CopyTo(nArgs, 0);
            op = nArgs[0];
            mod = nArgs[1];
            addrL = nArgs[2];
            addrH = nArgs[3];
            opAL = nArgs[4];
            opAH = nArgs[5];
            opBL = nArgs[6];
            opBH = nArgs[7];
        }
    }
}
