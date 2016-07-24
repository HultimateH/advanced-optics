using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PCode
{
    class InstructionParser
    {
        short[] locMem = new short[0x10000]; // 0x0000 to 0xFFFF

        #region Opcode definitions
        List<Instr> OpDefs = new List<Instr>()
        {
            new Instr("nop", 0x00),
            new Instr("halt", 0x0F),

            new Instr("add", 0x10),// new Instr("addi", 0x10, 0x01), new Instr("addu", 0x10, 0x02),
            new Instr("sub", 0x11),// new Instr("subi", 0x11, 0x01), new Instr("subu", 0x11, 0x02),
            new Instr("mul", 0x12),// new Instr("muli", 0x12, 0x01), new Instr("mulu", 0x12, 0x02),
            new Instr("div", 0x13),// new Instr("divi", 0x13, 0x01), new Instr("divu", 0x13, 0x02),
            new Instr("abs", 0x14),// new Instr("absi", 0x14, 0x01),
            new Instr("mod", 0x15),// new Instr("divi", 0x15, 0x01), new Instr("divu", 0x15, 0x02),
            new Instr("neg", 0x16),
            new Instr("inc", 0x17),
            new Instr("dec", 0x18),

            new Instr("or", 0x20),
            new Instr("and", 0x21),
            new Instr("xor", 0x22),
            new Instr("nor", 0x23),
            new Instr("nand", 0x24),
            new Instr("xnor", 0x25),

            new Instr("sl", 0x30),
            new Instr("sr", 0x31),
            new Instr("srn", 0x32),
            new Instr("rl", 0x33),
            new Instr("rr", 0x34),

            new Instr("gt", 0x40),
            new Instr("lt", 0x41),
            new Instr("eq", 0x42),
            new Instr("gte", 0x43),
            new Instr("lte", 0x44),
            new Instr("neq", 0x45),
            new Instr("bit", 0x46),
            new Instr("bcon", 0x47),

            new Instr("move", 0x50),
            new Instr("copy", 0x51),
            new Instr("save", 0x52),
            new Instr("load", 0x53),
            new Instr("swap", 0x54),
            new Instr("cmp", 0x55),
            new Instr("dcmp", 0x56),

            new Instr("jump", 0x60),
            new Instr("call", 0x61),
            new Instr("ret", 0x62),
            new Instr("wait", 0x63),
            new Instr("sync", 0x64),
            new Instr("race", 0x65),
            new Instr("wint", 0x66),

            new Instr("badr", 0x70),
            new Instr("bpsh", 0x71),
            new Instr("bpop", 0x72),
            new Instr("bpek", 0x73),
            new Instr("bbit", 0x74),
            new Instr("bdrn", 0x75),
        };
        #endregion
        public void ParseFile(params string[] args)
        {
            if (args.Length == 0) return;
            if (!args[0].EndsWith(".pasm")) return;
            string[] lines = File.ReadAllLines(args[0]);
            List<Sym> symbols = new List<Sym>();
            ushort memPos = 0x0000;
            ushort symPos = 0xEFFF;
            // add magic numbers to start of file
            locMem[memPos++] = 0x5045;  // P E
            locMem[memPos++] = 0x5357;  // S W
            locMem[memPos++] = 0x4D52;  // M R
            memPos++;                   // . .

            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                Console.WriteLine("LINE:\t" + i + "\nTEXT:\t" + lines[i] + "\nSIZE:\t" + lines[i].Length);
                string[] str = PCodeUtil.SplitOperands(lines[i]); // splits, trims comments, and preserves strings & literals
                if (str.Length == 0) continue;
                str = PCodeUtil.PadStringArray(str, 5); // would be 4, but #precomp needs 4 operands (5 entries).
                Console.WriteLine("TRIM:\t" +
                    str[0] + ' ' +
                    str[1] + ' ' +
                    str[2] + ' ' +
                    str[3] + ' ' +
                    str[4] + ' '
                    + "\n");
                #region PreProc Handling
                if (str[0].ToCharArray()[0] == '#')
                {
                    switch (str[0])
                    {
                        case "#if":
                            if (args.Contains("token:" + str[1] ?? string.Empty)) continue;
                            i++;
                            continue; // skip next line if #if token and token not provided
                        case "#def":
                            if (symbols.Find(x => x.name == str[1]) != null) continue;
                            symbols.Add(new Sym(str[1], symPos--)); // add symbol (named address)
                            continue;
                        case "#precomp": // precompiled code? lolwut, but it IS the user's program...
                            locMem[memPos++] = PCodeUtil.HexParse(str[1]);
                            locMem[memPos++] = PCodeUtil.HexParse(str[2]);
                            locMem[memPos++] = PCodeUtil.HexParse(str[3]);
                            locMem[memPos++] = PCodeUtil.HexParse(str[4]);
                            continue;
                        case "#string": // #string dest offset "string"
                            // first put the string somewhere, adding the offset to each char
                            short offset = PCodeUtil.HexParse(str[2]);
                            char[] cArray = str[3].ToCharArray();
                            for (int j = str[3].Length - 1; j >= 0; j--) // symPos is decreasing, so we need to store the string backwards as well
                            {
                                locMem[symPos--] = (short)(cArray[j] + offset);
                            }
                            locMem[memPos++] = 0x5100;                      // copy
                            locMem[memPos++] = PCodeUtil.HexParse(str[1]);  // destination address
                            locMem[memPos++] = (short)(symPos + 1);         // source start address
                            locMem[memPos++] = (short)(str[3].Length - 1);  // source end offset
                            continue;
                    }
                }
                #endregion

                Instr instr = OpDefs.Find(x => x.name == str[0]);
                if (instr != null)
                {
                    instr.variation = PCodeUtil.VariationWithLiterals(instr.variation, str[2], str[3], symbols);
                    locMem[memPos++] = (short)(instr.code << 8 | instr.variation);
                    locMem[memPos++] = PCodeUtil.FindSymbolOrParse(symbols, str[1]);
                    locMem[memPos++] = PCodeUtil.FindSymbolOrParse(symbols, str[2]);
                    locMem[memPos++] = PCodeUtil.FindSymbolOrParse(symbols, str[3]);
                }
            }
            using (BinaryWriter bin = new BinaryWriter(File.Open("pbin_test.pbin", FileMode.Create)))
            {
                for (int j = 0; j < locMem.Length; j++)
                {
                    bin.Write((byte)(locMem[j] >> 8));
                    bin.Write((byte)(locMem[j] & 0xFF));
                }
            }
            Console.WriteLine("ADDR  \t 0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F");
            for (int j = 0; j < locMem.Length; )
            {
                if (locMem[j] == 0 &&     // 0 1
                    locMem[j + 1] == 0 && // 2 3
                    locMem[j + 2] == 0 && // 4 5
                    locMem[j + 3] == 0 && // 6 7
                    locMem[j + 4] == 0 && // 8 9
                    locMem[j + 5] == 0 && // A B
                    locMem[j + 6] == 0 && // C D
                    locMem[j + 7] == 0)   // E F
                {
                    j += 8;
                    continue;
                }
                Console.WriteLine("{0:X5}:\t{1:X2} {2:X2} {3:X2} {4:X2} {5:X2} {6:X2} {7:X2} {8:X2} {9:X2} {10:X2} {11:X2} {12:X2} {13:X2} {14:X2} {15:X2} {16:X2}",
                    (j / 8) << 4, // makes displayed address consistent with what eg. HxD shows
                    ((ushort)locMem[j]) >> 8,
                    locMem[j] & 0xFF,
                    ((ushort)locMem[j + 1]) >> 8,
                    locMem[j + 1] & 0xFF,
                    ((ushort)locMem[j + 2]) >> 8,
                    locMem[j + 2] & 0xFF,
                    ((ushort)locMem[j + 3]) >> 8,
                    locMem[j + 3] & 0xFF,
                    ((ushort)locMem[j + 4]) >> 8,
                    locMem[j + 4] & 0xFF,
                    ((ushort)locMem[j + 5]) >> 8,
                    locMem[j + 5] & 0xFF,
                    ((ushort)locMem[j + 6]) >> 8,
                    locMem[j + 6] & 0xFF,
                    ((ushort)locMem[j + 7]) >> 8,
                    locMem[j + 7] & 0xFF
                    );
                j += 8;
            }
            Console.WriteLine("\n\nDone. Press any key to continue...");
            Console.ReadKey();
        }
        
    }
    public class Instr
    {
        public string name;
        public byte code;
        public byte variation;
        public Instr(string n, byte c, byte v)
        {
            name = n;
            code = c;
            variation = v;
        }
        public Instr(string n, byte c)
        {
            name = n;
            code = c;
            variation = 0x00;
        }
    }
    public class Sym
    {
        public string name;
        public ushort addr;
        public Sym(string n, ushort a)
        {
            name = n;
            addr = a;
        }
    }

    public static class PCodeUtil // utilities class
    {
        public static short HexParse(string s)
        {
            short x;
            string s2 = s.TrimStart('$'); // literal
            if (short.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out x)) return x;
            else return 0x0000;
        }
        public static string[] PadStringArray(string[] s, int size)
        {
            if (s.Length < size)
            {
                string[] tstr = new string[size];
                s.CopyTo(tstr, 0);
                for (int j = 0; j < tstr.Length; j++)
                {
                    tstr[j] = tstr[j] ?? string.Empty;
                }
                return tstr;
            }
            return s;
        }
        public static string[] SplitOperands(string s) // split line into operators/operands
        {
            bool canSplit = true;
            bool nextLiteral = false;
            List<string> strings = new List<string>();
            StringBuilder strB = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ' ' && canSplit) // split on space, if possible
                {
                    if (string.IsNullOrWhiteSpace(strB.ToString())) // discard split if it's just whitespace, empty or broken (somehow)
                    {
                        strB.Clear();
                        continue;
                    }
                    strings.Add(strB.ToString());
                    strB.Clear();
                    continue;
                }
                else if (s[i] == '\\' && !canSplit && !nextLiteral) // only works in strings
                {
                    nextLiteral = true; // just like how C# handles \ in strings
                    continue;
                }
                else if (!canSplit && nextLiteral)
                {
                    #region Escaped chars
                    switch (s[i])
                    {
                        case 'a':
                            strB.Append('\a'); break;
                        case 'b':
                            strB.Append('\b'); break;
                        case 'f':
                            strB.Append('\f'); break;
                        case 'n':
                            strB.Append('\n'); break;
                        case 'r':
                            strB.Append('\r'); break;
                        case 't':
                            strB.Append('\t'); break;
                        case 'v':
                            strB.Append('\v'); break;
                        default:
                            strB.Append(s[i]); break;
                    }
                    #endregion
                    nextLiteral = false;
                    continue;
                }
                else if (s[i] == '"' && !nextLiteral)
                {
                    canSplit ^= true; // toggle splitting on " so eg. #string 0209 "this is; a string" works
                    continue; // don't append " to strB
                }
                else if (s[i] == ';' && canSplit)
                {
                    if (!string.IsNullOrWhiteSpace(strB.ToString()))
                    {
                        strings.Add(strB.ToString());
                    }
                    return strings.ToArray();
                }
                strB.Append(s[i]);
            }
            if (!string.IsNullOrWhiteSpace(strB.ToString()))
            {
                strings.Add(strB.ToString());
            }
            return strings.ToArray();
        }
        public static short FindSymbolOrParse(List<Sym> symbols, string s)
        {
            Sym sym = symbols.Find(x => x.name == s); // sym is null if not found in the list
            return (short?)sym?.addr ?? HexParse(s); // the .? prevents an NRE, and the ?? makes the function just parse s if sym is null
        }
        public static byte VariationWithLiterals(byte v, string s1, string s2, List<Sym> symbols)
        {
            Sym x1 = symbols.Find(a => a.name == s1);
            Sym x2 = symbols.Find(a => a.name == s2);
            byte b1 = 0, b2 = 0;
            if (x1 == null && !string.IsNullOrWhiteSpace(s1) && s1[0] == '$') // true if s1 isn't a symbol, isn't null/empty/whitespace and its first char is '$'
            {
                b1 = 1 << 7;
            }
            if (x2 == null && !string.IsNullOrWhiteSpace(s2) && s2[0] == '$')
            {
                b2 = 1 << 6;
            }
            return (byte)(v | b1 | b2);
        }
        public static void ColourText(string s, byte colour)
        {
            // ref for colours:
            /*
             * 0: black     8: grey
             * 1: blue      9: light blue
             * 2: green     A: light green
             * 3: aqua      B: light aqua
             * 4: red       C: light red
             * 5: purple    D: light purple
             * 6: yellow    E: light yellow
             * 7: white     F: bright white
             */

            Console.BackgroundColor = (ConsoleColor)(colour >> 4);
            Console.ForegroundColor = (ConsoleColor)(colour & 0xF);
            Console.Write(s);
        }
        // error handling
        public static void FatalError(string s)
        {
            Console.WriteLine();
            ColourText("---------------\n", 0x04);
            ColourText("FATAL COMPILE ERROR! \n", 0x0C);
            ColourText("Details are below:\n", 0x04);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(s);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("---------------");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("Your data has not been modified, and you may now safely close the compiler.");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Press any key to quit.");
            Console.ReadKey();
            Environment.Exit(0);
        }
    }
}
