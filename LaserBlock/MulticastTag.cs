using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class MulticastTag : MonoBehaviour
    {
        private bool sendReceive; // true = sending, false = receiving
        private bool dirtyFlag = true;
        public bool onOff;
        public Color colour;
        public MulticastHandler.BitOp bitOp;
        public delegate void RecieveDelegate(bool onOff);
        public RecieveDelegate Recieve;
        public void Send()
        {
            if (dirtyFlag || !sendReceive) return;
            MulticastHandler.Instance.Send(colour);
        }
        public void RegisterTransmitter(Color colourBand)
        {
            if (!dirtyFlag) return;
            colour = colourBand;
            MulticastHandler.Instance.RegisterTransmitter(this);
            sendReceive = true;
            dirtyFlag = false;
        }
        public void RegisterReciever(Color colourBand, MulticastHandler.BitOp bitOp)
        {
            if (!dirtyFlag) return;
            colour = colourBand; this.bitOp = bitOp;
            MulticastHandler.Instance.RegisterReciever(this);
            sendReceive = false;
            dirtyFlag = false;
            Recieve += setOnOff;
        }
        public void Reset()
        {
            if (!sendReceive) Recieve -= setOnOff;
            dirtyFlag = true;
            sendReceive = false;
            onOff = false;
        }
        private void setOnOff(bool onOff)
        {
            this.onOff = onOff;
        }
    }
}
