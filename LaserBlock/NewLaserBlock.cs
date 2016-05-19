using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using spaar.ModLoader;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class NewLaserBlock : BlockScript
    {
        // Besiege-specific stuff
        protected MMenu LaserEditModeMenu;
        protected MMenu LaserAbilityModeMenu;
        protected MMenu LaserCosmeticModeMenu;
        protected MMenu LaserMulticastModeMenu;

        protected MColourSlider LaserColourSlider;
        protected MColourSlider LaserMulticastFrequencySlider;

        protected MSlider LaserFocusSlider;
        protected MSlider LaserLengthSlider;
        protected MSlider LaserKineticUpDownSlider;
        protected MSlider LaserKineticInOutSlider;
        protected MSlider LaserKineticSideSlider;
        protected MSlider LaserCosmeticThetaSlider;
        protected MSlider LaserCosmeticAmplitudeSlider;

        protected MToggle LaserOnOffToggle;
        protected MToggle LaserFastUpdateToggle; // temporary 5-tick update limit bypass
        protected MKey LaserOnOffKey;
        protected MKey LaserAbilityKey;

        // Block-specific stuff
        private LaserHandler laserHandler;
        private MulticastTag mTag;
        private bool laserOnOff;

        public override void SafeAwake()
        {
            // Setup config window
            LaserEditModeMenu = AddMenu("laserEditMode", 0, new List<string>() { "Ability", "Cosmetic", "Multicast" });
            LaserAbilityModeMenu = AddMenu("laserAbilityMode", 0, new List<string>() { "Fire", "Kinetic", "Freeze", "Cosmetic", "Tag" });
            LaserCosmeticModeMenu = AddMenu("laserCosmeticMode", 0, new List<string>() { "Off", "Trig", "Inv Trig", "Lightning" });
            LaserMulticastModeMenu = AddMenu("laserMulticastMode", 0, new List<string>() { "Off", "OR", "AND", "XOR" });

            LaserColourSlider = AddColourSlider("Beam Colour", "laserColour", Color.red);
            LaserMulticastFrequencySlider = AddColourSlider("Multicast Frequency", "laserMulticastFreq", Color.red);

            LaserFocusSlider = AddSlider("Laser Focus", "laserFocus", 0.08f, 0.08f, 0.5f);
            LaserLengthSlider = AddSlider("Laser Length", "laserLength", 200f, 0.1f, 500f);
            LaserKineticUpDownSlider = AddSlider("Up/Down Force", "laserKinUpDown", 1f, -2.5f, 2.5f);
            LaserKineticInOutSlider = AddSlider("In/Out Force", "laserKinInOut", 0f, -2.5f, 2.5f);
            LaserKineticSideSlider = AddSlider("Sideways Force", "laserKinSide", 0f, -2.5f, 2.5f);
            LaserCosmeticThetaSlider = AddSlider("Theta Modifier", "laserThetaMod", 1f, 0f, 2.5f);
            LaserCosmeticAmplitudeSlider = AddSlider("Amplitude Modifier", "laserAmpMod", 0.5f, 0f, 1.5f);

            LaserFastUpdateToggle = AddToggle("Fast Raycasting", "laserFastUpdate", false);
            LaserOnOffToggle = AddToggle("Start On", "laserOnOffToggle", true);
            LaserOnOffKey = AddKey("Toggle On/Off", "laserOnOffKey", KeyCode.Y);
            LaserAbilityKey = AddKey("Laser Ability", "laserAbilityKey", KeyCode.J);

            // register mode switching functions with menu delegates
            LaserEditModeMenu.ValueChanged += CycleEditMode;
            LaserAbilityModeMenu.ValueChanged += CycleAbilityMode;
            LaserCosmeticModeMenu.ValueChanged += CycleCosmeticMode;
            LaserMulticastModeMenu.ValueChanged += CycleMulticastMode;
            mTag = gameObject.AddComponent<MulticastTag>();
        }

        // cycle through settings etc.
        private void HideEverything()
        {
            LaserAbilityModeMenu.DisplayInMapper = false;
            LaserCosmeticModeMenu.DisplayInMapper = false;
            LaserMulticastModeMenu.DisplayInMapper = false;

            LaserColourSlider.DisplayInMapper = false;
            LaserMulticastFrequencySlider.DisplayInMapper = false;

            LaserFocusSlider.DisplayInMapper = false;
            LaserLengthSlider.DisplayInMapper = false;
            LaserKineticUpDownSlider.DisplayInMapper = false;
            LaserKineticInOutSlider.DisplayInMapper = false;
            LaserKineticSideSlider.DisplayInMapper = false;
            LaserCosmeticThetaSlider.DisplayInMapper = false;
            LaserCosmeticAmplitudeSlider.DisplayInMapper = false;

            LaserFastUpdateToggle.DisplayInMapper = false;
            LaserOnOffToggle.DisplayInMapper = false;
        }
        private void CycleEditMode(int value)
        {
            switch (value)
            {
                case 0:
                    CycleAbilityMode(LaserAbilityModeMenu.Value);
                    break;
                case 1:
                    CycleCosmeticMode(LaserCosmeticModeMenu.Value);
                    break;
                case 2:
                    CycleMulticastMode(LaserMulticastModeMenu.Value);
                    break;
            }
        }
        private void CycleAbilityMode(int value)
        {
            HideEverything();
            LaserAbilityModeMenu.DisplayInMapper = true;
            LaserLengthSlider.DisplayInMapper = true;
            if (value == 1)
            {
                LaserKineticUpDownSlider.DisplayInMapper = true;
                LaserKineticInOutSlider.DisplayInMapper = true;
                LaserKineticSideSlider.DisplayInMapper = true;
            }
            else if (value == 4) LaserColourSlider.DisplayInMapper = true;

        }
        private void CycleCosmeticMode(int value)
        {
            HideEverything();
            LaserCosmeticModeMenu.DisplayInMapper = true;
            LaserFocusSlider.DisplayInMapper = true;
            LaserColourSlider.DisplayInMapper = true;
            if (value == 1 || value == 2)
            {
                LaserCosmeticThetaSlider.DisplayInMapper = true;
            }
            if (value != 0)
            {
                LaserCosmeticAmplitudeSlider.DisplayInMapper = true;
            }
        }
        private void CycleMulticastMode(int value)
        {
            HideEverything();
            LaserMulticastModeMenu.DisplayInMapper = true;
            LaserFastUpdateToggle.DisplayInMapper = true;
            LaserOnOffToggle.DisplayInMapper = true;
            if (value != 0)
            {
                LaserMulticastFrequencySlider.DisplayInMapper = true;
            }
        }
        
        protected override void OnSimulateStart()
        {
            laserOnOff = LaserOnOffToggle.IsActive;
            laserHandler = new LaserHandler(transform, LaserColourSlider.Value);
            if (LaserMulticastModeMenu.Value != 0)
            {
                mTag.RegisterReciever(LaserMulticastFrequencySlider.Value,
                    (MulticastHandler.BitOp)LaserMulticastModeMenu.Value - 1);
            }
            laserHandler.SetBeamWidth(LaserFocusSlider.Value);
            laserHandler.skipTickLimit = LaserFastUpdateToggle.IsActive;
            laserHandler.beamRayLength = LaserLengthSlider.Value;
            laserHandler.onOff = laserOnOff;
        }
        protected override void OnSimulateUpdate()
        {
            if (LaserOnOffKey.IsPressed)
            {
                laserOnOff ^= laserOnOff;
            }
            laserHandler.onOff = laserOnOff ^ mTag.onOff; // ALU operations are blazingly-fast so doing this every tick is fine
            laserHandler.CheckIfNeedsUpdate(); // better to optimise more expensive stuff instead (eg. trig functions)

            //doPassiveAbility();
        }
        
        protected void IgniteAndBreak(RaycastHit rH)
        {
            if (rH.transform.GetComponent<FireTag>()) // ignite
            {
                FireTag fT = rH.transform.GetComponent<FireTag>();
                fT.Ignite();
                if (fT.bombCode) fT.bombCode.Explodey();
                if (fT.grenadeCode) fT.grenadeCode.Explode();
                if (fT.glowCode) fT.glowCode.Glow();
            }
            else if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff
                rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForce>())
                rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f);
        }
        private void doPassiveAbility()
        {
            if (!laserHandler.BeamHitAnything) return;
            RaycastHit rH = laserHandler.BeamLastHit;
            switch (LaserAbilityModeMenu.Value)
            {
                case 0: // fire
                    IgniteAndBreak(rH);
                    break;
                case 1: // kinetic

                    break;
                case 2: // freeze

                    break;
                default: // cosmetic & tag
                    break;
            }
        }
        protected override void OnSimulateExit()
        {
            mTag.Reset();
        }
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            CycleEditMode(LaserEditModeMenu.Value);
        }
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);
        }
    }
}
