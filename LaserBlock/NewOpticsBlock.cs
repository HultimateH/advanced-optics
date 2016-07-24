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
        protected MToggle InvertFilterToggle;               // In filter mode, switches from whitelist to blacklist.
        protected MColourSlider OpticFilterSlider;          // If set to filter/multicast, this dictates which colours are let through

        public int OpticalMode;
        private List<LaserHandler> beamIn = new List<LaserHandler>();
        public override void SafeAwake()
        {
            OpticModeMenu = AddMenu("opticMode", 0, new List<string>() { "Glass", "Mirror", "Redirection", "Filter" });
            OpticFilterSlider = AddColourSlider("Whitelist Filter", "opticFilter", Color.red);
            InvertFilterToggle = AddToggle("Invert Filter", "invertFilter", false);

            OpticModeMenu.ValueChanged += CycleOpticMode;
            InvertFilterToggle.Toggled += SwitchInvertFilterName;
        }
        private void HideEverything()
        {
            OpticFilterSlider.DisplayInMapper = false;
            InvertFilterToggle.DisplayInMapper = false;
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
        
        private void CycleOpticMode(int value)
        {
            HideEverything();
            switch (value)
            {
                case 3: // filter
                    OpticFilterSlider.DisplayInMapper = true;
                    InvertFilterToggle.DisplayInMapper = true;
                    SwitchInvertFilterName(InvertFilterToggle.IsActive);
                    break;
                case 4: // sensor
                    OpticFilterSlider.DisplayInMapper = true;
                    InvertFilterToggle.DisplayInMapper = true;
                    SwitchInvertFilterName(InvertFilterToggle.IsActive);
                    break;
            }
        }

        public void setLaserHit(LaserHandler lh)
        {
            if (beamIn.Contains(lh)) return;
            beamIn.Add(lh);
            //Debug.Log("added lh");
        }
        public void unsetLaserHit(LaserHandler lh)
        {
            if (beamIn.Contains(lh))
            {
                beamIn.Remove(lh);
                //Debug.Log("removed lh");
            }
        }
        public bool GetFilterMatch(Color colour)
        {
            if (InvertFilterToggle.IsActive)
                return colour != OpticFilterSlider.Value;   // filter inverted
            else return colour == OpticFilterSlider.Value;  // filter not inverted
        }
        protected override void OnSimulateStart()
        {
            OpticalMode = OpticModeMenu.Value;
        }
        protected override void OnSimulateUpdate()
        {

        }
        protected override void OnSimulateExit()
        {

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
