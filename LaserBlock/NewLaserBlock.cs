using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using spaar.ModLoader;
using UnityEngine;

namespace ImprovedLaserBlock
{
    public class NewLaserBlock : BlockScript
    {
        // Besiege-specific stuff
        protected MMenu LaserEditModeMenu;
        protected MMenu LaserAbilityModeMenu;
        protected MMenu LaserCosmeticModeMenu;

        protected MColourSlider LaserColourSlider;

        protected MSlider LaserFocusSlider;
        protected MSlider LaserLengthSlider;
        //protected MSlider LaserKineticUpDownSlider;
        protected MSlider LaserKineticInOutSlider;
        // protected MSlider LaserKineticSideSlider;
        protected MSlider LaserCosmeticThetaSlider;
        protected MSlider LaserCosmeticAmplitudeSlider;

        protected MToggle LaserOnOffToggle;
        //protected MToggle LaserFastUpdateToggle; // temporary 5-tick update limit bypass
        protected MKey LaserOnOffKey;
        protected MKey LaserAbilityKey;

        protected MSlider LaserWidth;

        // Block-specific stuff
        private bool laserOnOff;

        public override void SafeAwake()
        {
            // Setup config window
            LaserEditModeMenu = AddMenu("laserEditMode", 0, new List<string>() { "Ability", "Cosmetic" });
            LaserAbilityModeMenu = AddMenu("laserAbilityMode", 0, new List<string>() { "Fire", "Kinetic", "Freeze", "Cosmetic", "Tag" });
            LaserCosmeticModeMenu = AddMenu("laserCosmeticMode", 0, new List<string>() { "Off", "Trig", "Inv Trig", "Lightning" });

            LaserColourSlider = AddColourSlider("Beam Colour", "laserColour", Color.red);

            LaserFocusSlider = AddSlider("Laser Focus", "laserFocus", 0.08f, 0.08f, 0.5f);
            LaserLengthSlider = AddSlider("Laser Length", "laserLength", 200f, 0.1f, Mathf.Infinity);
            //LaserKineticUpDownSlider = AddSlider("Up/Down Force", "laserKinUpDown", 1f, -2.5f, 2.5f);
            LaserKineticInOutSlider = AddSlider("In/Out Force", "laserKinInOut", 0f, -2.5f, 2.5f);
            //LaserKineticSideSlider = AddSlider("Sideways Force", "laserKinSide", 0f, -2.5f, 2.5f);
            LaserCosmeticThetaSlider = AddSlider("Theta Modifier", "laserThetaMod", 1f, 0f, 2.5f);
            LaserCosmeticAmplitudeSlider = AddSlider("Amplitude Modifier", "laserAmpMod", 0.5f, 0f, 1.5f);

            //LaserFastUpdateToggle = AddToggle("Fast Raycasting", "laserFastUpdate", false);
            LaserOnOffToggle = AddToggle("Start On", "laserOnOffToggle", true);
            LaserOnOffKey = AddKey("Toggle On/Off", "laserOnOffKey", KeyCode.Y);
            LaserAbilityKey = AddKey("Laser Ability", "laserAbilityKey", KeyCode.J);

            LaserWidth = AddSlider("Laser Width", "laserWidth", 0.5f, 0.001f, 10f);

            // register mode switching functions with menu delegates
            LaserEditModeMenu.ValueChanged += CycleEditMode;
            LaserAbilityModeMenu.ValueChanged += CycleAbilityMode;
            LaserCosmeticModeMenu.ValueChanged += CycleCosmeticMode;
        }

        // cycle through settings etc.
        private void HideEverything()
        {
            LaserAbilityModeMenu.DisplayInMapper = false;
            LaserCosmeticModeMenu.DisplayInMapper = false;

            LaserColourSlider.DisplayInMapper = false;

            LaserFocusSlider.DisplayInMapper = false;
            LaserLengthSlider.DisplayInMapper = false;
            //LaserKineticUpDownSlider.DisplayInMapper = false;
            LaserKineticInOutSlider.DisplayInMapper = false;
            //LaserKineticSideSlider.DisplayInMapper = false;
            LaserCosmeticThetaSlider.DisplayInMapper = false;
            LaserCosmeticAmplitudeSlider.DisplayInMapper = false;

            //LaserFastUpdateToggle.DisplayInMapper = false;
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
            }
        }
        private void CycleAbilityMode(int value)
        {
            HideEverything();
            LaserAbilityModeMenu.DisplayInMapper = true;
            LaserLengthSlider.DisplayInMapper = true;
            //LaserFastUpdateToggle.DisplayInMapper = true;
            LaserOnOffToggle.DisplayInMapper = true;
            if (value == 1)
            {
                //LaserKineticUpDownSlider.DisplayInMapper = true;
                LaserKineticInOutSlider.DisplayInMapper = true;
                //LaserKineticSideSlider.DisplayInMapper = true;
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

        //protected override void OnSimulateStart()
        //{
        //    laserOnOff = LaserOnOffToggle.IsActive;
        //    laserHandler = this.gameObject.AddComponent<LaserScript>();
        //    Debug.Log("Ho");
        //    laserHandler.SetBeamWidth(LaserFocusSlider.Value);
        //    //laserHandler.skipTickLimit = LaserFastUpdateToggle.IsActive;
        //    laserHandler.beamRayLength = LaserLengthSlider.Value;
        //    laserHandler.onOff = laserOnOff;
        //}
        protected override void OnSimulateUpdate()
        {
            if (LaserOnOffKey.IsPressed)
            {
                laserOnOff = !laserOnOff;
            }
            onOff = laserOnOff; // ALU operations are blazingly-fast so doing this every tick is fine
            CheckIfNeedsUpdate(); // better to optimise more expensive stuff instead (eg. trig functions)

            doPassiveAbility();
            SetBeamWidth(LaserWidth.Value);
        }

        protected void IgniteAndBreak(RHInfo rH)
        {
            FireTag FT = rH.transform.GetComponentInChildren<FireTag>();
            if (FT) // ignite
            {
                FT.Ignite();
            }
            ///Just Ignite
            ///Meow
            ///
            /*
            else if (rH.transform.GetComponent(typeof(IExplosionEffect))) // explode stuff

            else if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff
                rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForce>())
                rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
            else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f); */
        }
        private void doPassiveAbility()
        {
            if (!BeamHitAnything) return;
            RHInfo rH = rHInfo;
            switch (LaserAbilityModeMenu.Value)
            {
                case 0: // fire
                    IgniteAndBreak(rH);
                    break;
                case 1: // kinetic
                    if (rH.rigidBody != null)
                    {
                        rH.rigidBody.AddForceAtPosition(LaserKineticInOutSlider.Value * (rH.rigidBody.transform.position - this.transform.position).normalized, rH.point);
                    }
                    //IExplosionEffect IEE = rH.transform.GetComponent<BreakOnForceNoSpawn>();

                    if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff
                        rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
                    else if (rH.transform.GetComponent<BreakOnForce>())
                        rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
                    else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                        rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
                    else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                        rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f);

                    break;
                case 2: // freeze

                    break;
                default: // cosmetic & tag
                    break;
            }
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





        public struct RHInfo
        {
            public Vector3 point;
            public Transform transform;
            public Collider collider;
            public Rigidbody rigidBody;
            public RHInfo(Vector3 v, Transform t, Collider c, Rigidbody r)
            {
                point = v;
                transform = t;
                collider = c;
                rigidBody = r;
            }
        }
        
        private LineRenderer lr;                // laser beam
        private List<Vector3> beamDirections;   // the transforms of hit objects
        private List<Vector3> beamPoints;       // each point where the beam changes direction
        private Transform beamFirstPoint;       // equivalent to laser emitter transform

        private int lLength;                    // used in calculating how many vertices the line renderer needs
        public Color colour;
        //private int updateCount = 0;
        //private List<LHData> triggers;
        private bool raycastShutdown = false;
        //public bool skipTickLimit = false;
        private bool timePausedFlag = true;     // true = time is flowing, false = time paused

        // need to port over everything from the old version
        public bool onOff = true;               // on by default
        public RHInfo rHInfo;
        public bool BeamHitAnything;            // also used by laser block for abilities
        public Vector3 BeamLastPoint = new Vector3();
        public float beamRayLength = 500f;

        protected override void OnSimulateStart()
        {
            // oh boy what a mess
            // setup GO container

            // create LineRenderer for laser
            lr = this.gameObject.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Additive"));
            lr.SetWidth(0.08f, 0.08f);

            Color ArgColour = LaserColourSlider.Value;

            lr.SetColors(Color.Lerp(ArgColour, Color.black, 0.45f),
                Color.Lerp(ArgColour, Color.black, 0.45f));
            lr.SetVertexCount(0);

            // redraw arguments (also filter checking)

            // more optimisation args
            //triggers = new List<LHData>();
        }
        public void SetBeamWidth(float f)
        {
            // temporary fix for people who REALLY want to change the laser width
            lr.SetWidth(Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * 0.08f, Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * f);
        }
        public void CheckIfNeedsUpdate()
        {
            if (raycastShutdown) return;
            if (Time.timeScale == 0f) // time paused
            {
                if (!timePausedFlag) Debug.Log("time paused, not raycasting");
                timePausedFlag = true;
                return;
            }
            else if (timePausedFlag) timePausedFlag = false;
            if (!onOff)
            {
                DrawBeam();
                return;
            }
            //updateCount++;
            //if (updateCount > 5 || skipTickLimit)
            //{
            //    UpdateFromPoint(beamFirstPoint.position + 0.5f * beamFirstPoint.forward - 0.8f * beamFirstPoint.up, -beamFirstPoint.up);
            //    updateCount = 0;
            //    return;
            //}

        }
        private void UpdateFromPoint(Vector3 point, Vector3 dir)
        {
            int i = beamPoints.IndexOf(point);
            //if (dir == Vector3.zero) i--; // recast from point previous to this one
            if (i < beamPoints.Count && i >= 0)
            {
                beamPoints.RemoveRange(i, beamPoints.Count - i);
                beamDirections.RemoveRange(i, beamDirections.Count - i);
            }
            else if (beamPoints.Count > 0)
            {
                beamPoints.Clear();
                beamDirections.Clear();
            }
            beamPoints.Add(point);
            beamDirections.Add(dir);
            Vector3 lastPoint = point;
            Vector3 lastDir = dir;
            BeamHitAnything = false;
            foreach (RaycastHit Hito in Physics.RaycastAll(lastPoint, lastDir, beamRayLength))
            {
                if (!Hito.collider.isTrigger)
                {
                    rHInfo = new RHInfo(Hito.point, Hito.transform, Hito.collider, Hito.rigidbody);
                    BeamHitAnything = true;
                    {
                        beamPoints.Add(Hito.point);
                        beamDirections.Add(lastDir);
                        DrawBeam();
                        return;
                    }
                }
                else
                {
                    BeamHitAnything = false;
                    beamPoints.Add(lastPoint + lastDir * beamRayLength);
                    beamDirections.Add(lastDir);
                    DrawBeam();
                    return;
                }
            }
            BeamHitAnything = false;
            raycastShutdown = true;
        }
        private void DrawBeam()
        {
            if (!onOff)
            {
                if (lLength != 0)
                {
                    lLength = 0;
                    lr.SetVertexCount(0);
                }
                return;
            }
            lLength = beamPoints.Count;
            lr.SetVertexCount(lLength);
            for (int i = 0; i < lLength; i++)
            {
                lr.SetPosition(i, beamPoints[i]);
            }
            BeamLastPoint = beamPoints[beamPoints.Count - 1];
        }
    }
}
