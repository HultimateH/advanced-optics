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
        static void Main(string[] args)
        {
            InstructionParser instrParser = new InstructionParser();
            instrParser.ParseFile(args);
        }
    }
}
