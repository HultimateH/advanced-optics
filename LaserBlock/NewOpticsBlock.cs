using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class NewOpticsBlock : BlockScript
    {
        /*
        Bit of documentation here
        So. The optics block has several modes for redirecting lasers, providing structural support in laser corridors,
        and detecting lasers, filtering them or both.
        The glass, mirror, redirection & filter modes should be fairly self-explainatory. It's the multicast mode that needs documentation.
        First, the multicast tag has 2 modes: sending & receiving. Both have an attached frequency, and on/off state.
        The sending mode simply broadcasts the tag's state (logic is on receiving end).
        The receiving mode works by taking the list of all tags broadcasting on its frequency, and applies a boolean operation to the list's on/off states.
        'Off' denotes that the tag isn't sending or receiving.
        'OR' makes the tag on if at least one other tag is (sending) on.
        'AND' returns true if and only if all other tags are on.
        'XOR', aka. exclusive or, is an odd-parity function. returns true only if an odd number of tags are on (all off = false)
        There are inverse versions as well:
        'NOR' turns off if any other tag is on.
        'NAND' turns off if all other tags are on.
        'XNOR' is an even-parity function. returns true if an even number of tags are on (all off = true)
        
        Sensors, lasers & receivers can be chained together to make complex laser-based wireless circuitry.
            */

        protected MMenu OpticModeMenu;                      // Switches between optical modes (glass, mirror, redirection, filter, sensor)
        protected MMenu MulticastModeMenu;                  // Switches between multicast modes (off, or, and, xor)
        protected MToggle InvertFilterToggle;               // In filter mode, switches from whitelist to blacklist.
        protected MToggle InvertMulticastToggle;            // In multicast mode, applies NOT.
        protected MColourSlider OpticFilterSlider;          // If set to filter/multicast, this dictates which colours are let through
        protected MColourSlider MulticastFreqSlider;        // Multicasting frequency. Used for both send/recieve

        public int OpticalMode;
        private List<LaserHandler> beamIn = new List<LaserHandler>();
        private MulticastTag mTag;
        public override void SafeAwake()
        {
            OpticModeMenu = AddMenu("opticMode", 0, new List<string>() { "Glass", "Mirror", "Redirection", "Filter", "Sensor" });
            MulticastModeMenu = AddMenu("multicastMode", 0, new List<string>() { "Off", "OR", "AND", "XOR" });
            OpticFilterSlider = AddColourSlider("Whitelist Filter", "opticFilter", Color.red);
            MulticastFreqSlider = AddColourSlider("Multicast Frequency", "multicastFrequency", Color.red);
            InvertFilterToggle = AddToggle("Invert Filter", "invertFilter", false);
            InvertMulticastToggle = AddToggle("Invert Operator", "invertMulticast", false);

            OpticModeMenu.ValueChanged += CycleOpticMode;
            MulticastModeMenu.ValueChanged += CycleMulticastMode;
            InvertFilterToggle.Toggled += SwitchInvertFilterName;
            InvertMulticastToggle.Toggled += SwitchInvertMulticastNames;
            mTag = gameObject.AddComponent<MulticastTag>();
        }
        private void HideEverything()
        {
            MulticastModeMenu.DisplayInMapper = false;
            OpticFilterSlider.DisplayInMapper = false;
            MulticastFreqSlider.DisplayInMapper = false;
            InvertFilterToggle.DisplayInMapper = false;
            InvertMulticastToggle.DisplayInMapper = false;
        }
        private void SwitchInvertFilterName(bool isActive)
        {
            if (isActive)
            {
                OpticFilterSlider.DisplayName = "Blacklist Filter";
            }
            else
            {
                OpticFilterSlider.DisplayName = "Whitelist Filter";
            }
        }
        private void SwitchInvertMulticastNames(bool isActive)
        {
            if (isActive)
            {
                MulticastModeMenu.Items = new List<string>() { "Off", "NOR", "NAND", "XNOR" };
            }
            else
            {
                MulticastModeMenu.Items = new List<string>() { "Off", "OR", "AND", "XOR" };
            }
            // current selection name doesn't change until user picks different item
            MulticastModeMenu.DisplayName = MulticastModeMenu.Items[MulticastModeMenu.Value];
        }
        private void CycleMulticastMode(int value)
        {

        }
        private void CycleOpticMode(int value)
        {
            HideEverything();
            switch (value)
            {
                case 3: // filter
                    OpticFilterSlider.DisplayInMapper = true;
                    MulticastFreqSlider.DisplayInMapper = true;     // Multicasting stuff is here because A. mode menu is recieving modes
                    MulticastModeMenu.DisplayInMapper = true;       // and B. how else does one do any more fancy stuff? Lasers aren't enough.
                    InvertFilterToggle.DisplayInMapper = true;
                    InvertMulticastToggle.DisplayInMapper = true;
                    SwitchInvertFilterName(InvertFilterToggle.IsActive);
                    SwitchInvertMulticastNames(InvertMulticastToggle.IsActive);
                    break;
                case 4: // sensor
                    OpticFilterSlider.DisplayInMapper = true;       // Need the filter slider here because the multicast mode should be able to detect
                    MulticastFreqSlider.DisplayInMapper = true;     // just one, or all but one colour.
                    InvertFilterToggle.DisplayInMapper = true;      // Invert Filter toggle serves the same purpose here as well.
                    SwitchInvertFilterName(InvertFilterToggle.IsActive);
                    break;
            }
        }

        public void setLaserHit(LaserHandler lh)
        {
            if (beamIn.Contains(lh)) return;
            beamIn.Add(lh);
            UpdateTagState();
            Debug.Log("added lh");
        }
        public void unsetLaserHit(LaserHandler lh)
        {
            if (beamIn.Contains(lh))
            {
                beamIn.Remove(lh);
                UpdateTagState();
                Debug.Log("removed lh");
            }
        }
        public void UpdateTagState()
        {
            if (OpticalMode != 4) return; // not a sensor? not a chance.
            Color colour = OpticFilterSlider.Value;
            foreach (LaserHandler lh in beamIn)
            {
                if (GetFilterMatch(lh.colour))
                {
                    mTag.onOff = true;
                    mTag.Send();
                    return;
                }
            }
            mTag.onOff = false;
            mTag.Send();
            return;
            // no need to update tag state for filter mode - multicast handler does that for us
        }
        private void doRegisterTag()
        {
            if (OpticalMode == 3 && MulticastModeMenu.Value != 0)
            {
                mTag.RegisterReciever(MulticastFreqSlider.Value,
                    (MulticastHandler.BitOp)MulticastModeMenu.Value - 1);
            }
            else if (OpticalMode == 4)
            {
                mTag.RegisterTransmitter(MulticastFreqSlider.Value);
            }
        }
        public bool GetFilterMatch(Color colour)
        {
            if (InvertFilterToggle.IsActive)
                return colour != OpticFilterSlider.Value;   // filter inverted
            else return colour == OpticFilterSlider.Value;  // filter not inverted
        }
        public bool GetMFilterMatch(Color colour)
        {
            if (MulticastModeMenu.Value == 0) return GetFilterMatch(colour);
            // defining stuff...
            // filter is ACTIVE if mTag.onOff ^ InvertMulticastToggle.IsActive == true
            // filter is transparent to colour if active && GetFilterMatch(colour) == true, OR
            // filter is transparent to everything if INACTIVE.
            else
            {
                if (mTag.onOff ^ InvertMulticastToggle.IsActive)
                    return GetFilterMatch(colour); // filter active
                else return true; // filter inactive
            }
        }
        protected override void OnSimulateStart()
        {
            OpticalMode = OpticModeMenu.Value;
            doRegisterTag();
        }
        protected override void OnSimulateUpdate()
        {

        }
        protected override void OnSimulateExit()
        {
            mTag.Reset();   // multicast handler will not loop through each tag and reset them, so you have to do it yourself.
                            // good thing each block with this functionality only has one.
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            CycleOpticMode(OpticModeMenu.Value);
        }
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }
    }
}
