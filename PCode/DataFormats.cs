using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCode
{
    //public class Chunk
    //{
    //    private Stack<byte> chunkStack;
    //    public Chunk(params byte[] bytes)
    //    {
    //        chunkStack = new Stack<byte>(bytes);
    //    }
    //    public Chunk(params short[] shorts)
    //    {
    //        byte[] bytes = new byte[shorts.Length * 2];
    //        for (int i = 0; i < shorts.Length; i++)
    //        {
    //            bytes[i * 2] = (byte)(shorts[i] >> 8);
    //            bytes[i * 2 + 1] = (byte)(shorts[i] & 0xFF);
    //        }
    //        chunkStack = new Stack<byte>(bytes);
    //    }
    //    public byte Pop
    //    {
    //        get { return chunkStack.Pop(); }
    //        set { chunkStack.Push(value); }
    //    }
    //    public byte Peek
    //    {
    //        get { return chunkStack.Peek(); }
    //        set { chunkStack.Push(value); }
    //    }
    //}
}
