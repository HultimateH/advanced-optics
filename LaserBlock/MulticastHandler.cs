using spaar.ModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class MulticastHandler : SingleInstance<MulticastHandler>
    {
        public enum BitOp
        {
            OR, AND, XOR
        }
        private Dictionary<Color, List<MulticastTag>> _mtRecievers = new Dictionary<Color, List<MulticastTag>>();
        private Dictionary<Color, List<MulticastTag>> _mtTransmitters = new Dictionary<Color, List<MulticastTag>>();

        public override string Name { get { return "MulticastHandler"; } }

        public void Clear(bool isSimulating)
        {
            if (isSimulating) return; // don't clear lists if simulation just started
            _mtRecievers.Clear();
            _mtTransmitters.Clear();
        }
        public void Send(Color colour)
        {
            if (!_mtRecievers.ContainsKey(colour)) return;
            else
            {
                foreach (MulticastTag br in _mtRecievers[colour])
                {
                    br.Recieve(DoOperator(colour, br.bitOp));
                }
            }
        }
        public bool DoOperator (Color colour, BitOp bo)
        {
            switch(bo)
            {
                case BitOp.OR:
                    foreach(MulticastTag br in _mtTransmitters[colour])
                    {
                        if (br.onOff) return true; // as soon as one is set to true, return true
                    } return false;
                case BitOp.AND:
                    bool anySet = false;
                    foreach (MulticastTag br in _mtTransmitters[colour])
                    {
                        if (!br.onOff) return false; // as soon as one is set to false, return false
                        anySet = true;
                    }
                    return anySet;
                case BitOp.XOR:
                    bool oddSet = false;
                    foreach (MulticastTag br in _mtTransmitters[colour])
                    {
                        if (br.onOff)
                        {
                            oddSet ^= true; // toggle oddSet. even parity -> 0 1 0, odd parity -> 0 1 0 1
                        }
                    }
                    return oddSet;
                /*case BitOp.NOT:
                    foreach (BroadcastTag br in _brTransmitters[colour])
                    {
                        if (br.onOff) return false; // as soon as one is set to true, return false
                    }
                    return true;*/
            }
            return false;
        }
        public void RegisterReciever(MulticastTag mt)
        {
            Color colour = mt.colour;
            if (!_mtRecievers.ContainsKey(colour)) _mtRecievers.Add(colour, new List<MulticastTag>());
            if (_mtRecievers[colour].Contains(mt)) return;
            else
            {
                _mtRecievers[colour].Add(mt);
            }
        }
        public void RegisterTransmitter(MulticastTag mt)
        {
            Color colour = mt.colour;
            if (!_mtTransmitters.ContainsKey(colour)) _mtTransmitters.Add(colour, new List<MulticastTag>());
            if (_mtTransmitters[colour].Contains(mt)) return;
            else
            {
                _mtTransmitters[colour].Add(mt);
            }
        }
        public void Start()
        {
            Game.OnSimulationToggle += Clear; // I guess I didn't really understand delegates before... I do now.
        }
    }
}
