using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PCode
{
    public class NewInstructionParser
    {
        public void DoParse(string filePath)
        {

        }
    }
    public static class PCU2 // PCodeUtil2
    {
        public static readonly byte CCNormal = 0xA0; // grey, black
        public static readonly byte CCWarning = 0x90; // yellow, black
        public static readonly byte CCError = 0xD0; // red, black

        public static void Fail(int code, string error)
        {
            error = string.IsNullOrWhiteSpace(error) ? "Epic Fail!" : error;
            CColour(error, CCError);
        }
        public static void CColour(string msg, byte colour)
        {
            ConsoleColor cf = Console.ForegroundColor; // get copy of current colours
            ConsoleColor cb = Console.BackgroundColor;
            Console.ForegroundColor = (ConsoleColor)(colour >> 4); // set colours according to the byte
            Console.BackgroundColor = (ConsoleColor)(colour & 0xF);
            Console.Write(msg); // just write - we can insert \n manually if needed
            Console.ForegroundColor = cf; // restore copies of the colours
            Console.BackgroundColor = cb;
        }
    }
}
