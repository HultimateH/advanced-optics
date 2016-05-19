using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class NewMulticastHandler : SingleInstance<NewMulticastHandler>
    {
        public override string Name { get { return "NewMulticastHandler"; } }
        // split global into data & instruction
        private Dictionary<uint, HashSet<NewMulticastTag>> _mtTransmitters = new Dictionary<uint, HashSet<NewMulticastTag>>();
        public bool DoOperator(uint freq, BitOp bitOp)
        {
            switch (bitOp)
            {
                case BitOp.OR:
                    foreach (NewMulticastTag br in _mtTransmitters[freq])
                    {
                        if (br.onOff) return true; // as soon as one is set to true, return true
                    }
                    return false;
                case BitOp.AND:
                    bool anySet = false;
                    foreach (NewMulticastTag br in _mtTransmitters[freq])
                    {
                        if (!br.onOff) return false; // as soon as one is set to false, return false
                        anySet = true;
                    }
                    return anySet;
                case BitOp.XOR:
                    bool oddSet = false;
                    foreach (NewMulticastTag br in _mtTransmitters[freq])
                    {
                        oddSet ^= br.onOff;
                    }
                    return oddSet;
            }
            return false;
        }
        public delegate void RecheckFrequency(uint frequency);
        RecheckFrequency recheckFrequency;
        /*public Dictionary<uint, int> GlobalMemory;
        public bool RangeAllocated(uint offset, uint range)
        {
            for (uint i = offset; i < offset + range; i++)
            {
                if (!GlobalMemory.ContainsKey(i)) return false;
            }
            return true;
        }*/
    }
    public class NewMulticastTag : MonoBehaviour
    {
        private static NewMulticastHandler mh = NewMulticastHandler.Instance;
        private Dictionary<string, bool> localVariables = new Dictionary<string, bool>();
        public bool onOff;
        private bool DoOperator(HashSet<string> vars, BitOp bitOp)
        {
            return false;
        }
        //public Stack<int> stack;
        //public int[] Registers = new int[16];
        /*public void DoOperator(int input, int arg, uint offset, BitOp bitOp)
        {
            if (!mh.GlobalMemory.ContainsKey(offset)) return;
            int working;
            switch (bitOp)
            {
                case BitOp.OR: // bitwise OR
                    mh.GlobalMemory[offset] = input | arg;
                    break;
                case BitOp.AND: // bitwise AND
                    mh.GlobalMemory[offset] = input & arg;
                    break;
                case BitOp.XOR: // bitwise exclusive OR
                    mh.GlobalMemory[offset] = input ^ arg;
                    break;
                case BitOp.NOT: // bitwise NOT
                    mh.GlobalMemory[offset] = ~input;
                    break;
                case BitOp.SHFTL: // bitShiFT Left
                    mh.GlobalMemory[offset] = input << arg;
                    break;
                case BitOp.SHFTR: // bitShiFT Right
                    mh.GlobalMemory[offset] = input >> arg;
                    break;
                case BitOp.SFTRN: // bitShiFT Right, non-preserving
                    working = input & int.MaxValue; // remove sign
                    mh.GlobalMemory[offset] = working >> arg;
                    break;
                case BitOp.COMP: // compile input or up to 8 bools from memory into set of flags
                    if (input < 1) return;
                    if (input > 32) input = 32;
                    if (!mh.RangeAllocated(offset, (uint)input)) return;
                    working = 0;
                    for (uint i = offset; i < offset+input; i++)
                    {
                        working += (stack.Pop() != 0) ? 1 << (int)i : 0;
                    }
                    return working;
                case BitOp.DCOMP: // decompile arg flags from input & add to stack
                    if (arg <= 0) return int.MinValue;
                    if (arg > 32) arg = 32;
                    working = 0;
                    for (int i = 0; i < arg; i++)
                    {
                        working = 1 << (arg - i);
                        stack.Push((working & input) >> (arg - i));
                    }
                    return 0;
                case BitOp.GT: // comparison operators
                    return (input > arg) ? 1 : 0;
                case BitOp.LT:
                    return (input < arg) ? 1 : 0;
                case BitOp.GTE:
                    return (input >= arg) ? 1 : 0;
                case BitOp.LTE:
                    return (input <= arg) ? 1 : 0;
                case BitOp.EQ:
                    return (input == arg) ? 1 : 0;
                case BitOp.NEQ:
                    return (input != arg) ? 1 : 0;
                case BitOp.ADD: // basic math operators
                    return input + arg;
                case BitOp.SUB:
                    return input - arg;
                case BitOp.MUL:
                    return input * arg;
                case BitOp.DIV:
                    return input / arg;
                case BitOp.MOD: // remainder from division
                    return input % arg;
                case BitOp.NEG: // 2's compliment
                    return -input;
                case BitOp.ABS: // 2's compliment if negative
                    return Math.Abs(input);
                default:
                    return int.MinValue;
            }
        }*/


    }
    public enum BitOp
    {
        OR, AND, XOR
    }
    /*public struct Instruction
    {
        public uint A, B; // input, args
        public uint Addr; // address
        public BitOp instr;
        public Instruction(uint a, uint b, uint flags, BitOp bitOp)
        {
            A = a; B = b;
            Addr = flags;
            instr = bitOp;
        }
    }*/
}
