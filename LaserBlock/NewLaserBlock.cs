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
        protected MSlider PenetrativeLengthMultiplier;

        protected MToggle LaserOnOffToggle;
        //protected MToggle LaserFastUpdateToggle; // temporary 5-tick update limit bypass
        protected MKey LaserOnOffKey;

        protected MSlider LaserWidth;

        // Block-specific stuff
        private bool laserOnOff;


        public float LaserLength;

        public override void SafeAwake()
        {
            // Setup config window
            LaserEditModeMenu = AddMenu("laserEditMode", 0, new List<string>() { "Ability", "Misc." });
            LaserAbilityModeMenu = AddMenu("laserAbilityMode", 0, new List<string>() { "Fire", "Kinetic", "Freeze", "Explosive" });
            LaserCosmeticModeMenu = AddMenu("laserCosmeticMode", 0, new List<string>() { "Off", "Trig", "Inv Trig", "Lightning" });

            LaserColourSlider = AddColourSlider("Beam Colour", "laserColour", Color.red);

            LaserFocusSlider = AddSlider("Laser Focus", "laserFocus", 1f, 0.08f, 0.5f);
            LaserLengthSlider = AddSlider("Laser Length", "laserLength", 200f, 0.1f, Mathf.Infinity);
            //LaserKineticUpDownSlider = AddSlider("Up/Down Force", "laserKinUpDown", 1f, -2.5f, 2.5f);
            LaserKineticInOutSlider = AddSlider("In/Out Force", "laserKinInOut", 0f, -2.5f, 2.5f);
            //LaserKineticSideSlider = AddSlider("Sideways Force", "laserKinSide", 0f, -2.5f, 2.5f);

            //LaserFastUpdateToggle = AddToggle("Fast Raycasting", "laserFastUpdate", false);
            LaserOnOffToggle = AddToggle("Start On", "laserOnOffToggle", true);
            LaserOnOffKey = AddKey("Toggle On/Off", "laserOnOffKey", KeyCode.Y);

            LaserWidth = AddSlider("Laser Width", "laserWidth", 0.5f, 0.001f, 10f);

            PenetrativeLengthMultiplier = AddSlider("Penetrative Multiplier", "PeneMulty", 0, 0, 1);

            // register mode switching functions with menu delegates
            LaserEditModeMenu.ValueChanged += CycleEditMode;
            LaserAbilityModeMenu.ValueChanged += CycleAbilityMode;
            LaserCosmeticModeMenu.ValueChanged += CycleCosmeticMode;
        }

        // cycle through settings etc.
        protected override void BuildingUpdate()
        {
            PenetrativeLengthMultiplier.Value = Mathf.Clamp01(PenetrativeLengthMultiplier.Value);
            LaserFocusSlider.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserLengthSlider.DisplayInMapper = LaserEditModeMenu.Value == 1;
            PenetrativeLengthMultiplier.DisplayInMapper = LaserEditModeMenu.Value == 1;
            LaserWidth.DisplayInMapper = LaserEditModeMenu.Value == 1;
        }
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
            //if (value == 1)
            //{
            //LaserKineticUpDownSlider.DisplayInMapper = true;
            LaserKineticInOutSlider.DisplayInMapper = value == 1;
            //LaserKineticSideSlider.DisplayInMapper = true;
            //}
            /*else if (value == 4)*/
            LaserColourSlider.DisplayInMapper = value == 4;

        }
        private void CycleCosmeticMode(int value)
        {
            HideEverything();
            LaserCosmeticModeMenu.DisplayInMapper = true;
            LaserFocusSlider.DisplayInMapper = true;
            LaserColourSlider.DisplayInMapper = true;
            ////if (value == 1 || value == 2)
            ////{
            //LaserCosmeticThetaSlider.DisplayInMapper = value == 1 || value == 2;
            ////}
            ////if (value != 0)
            ////{
            //LaserCosmeticAmplitudeSlider.DisplayInMapper = value != 0;
            ////}
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
            if (LaserOnOffKey.IsReleased)
            {
                laserOnOff = !laserOnOff;
            }
            CheckIfNeedsUpdate(); // better to optimise more expensive stuff instead (eg. trig functions)

            if (!laserOnOff)
                doPassiveAbility();

            SetBeamWidth();

        }
        protected void Ignite(RHInfo rH)
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
            foreach (RHInfo rHinfo in rHInfos)
            {
                RHInfo rH = rHinfo;
                if (!rH.transform)
                {
                    continue;
                }
                switch (LaserAbilityModeMenu.Value)
                {
                    case 0: // fire
                        Ignite(rH);
                        break;
                    case 1: // kinetic
                        if (rH.rigidBody != null)
                        {
                            rH.rigidBody.AddForceAtPosition(LaserKineticInOutSlider.Value * (rH.rigidBody.transform.position - this.transform.position).normalized * 1000, rH.point);
                        }
                        //IExplosionEffect IEE = rH.transform.GetComponent<BreakOnForceNoSpawn>();

                        if (rH.transform.GetComponent<BreakOnForceNoSpawn>()) // explode stuff 
                        {
                            rH.transform.GetComponent<BreakOnForceNoSpawn>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<BreakOnForce>())
                        {
                            rH.transform.GetComponent<BreakOnForce>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<BreakOnForceNoScaling>())
                        {
                            rH.transform.GetComponent<BreakOnForceNoScaling>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        else if (rH.transform.GetComponent<CastleWallBreak>()) // explode ipsilon stuff
                        {
                            rH.transform.GetComponent<CastleWallBreak>().BreakExplosion(400f, rH.point, 10f, 0f);
                        }
                        ReduceBreakForce(rH.Joint);
                        break;
                    case 2: // freeze
                        IceTag IT = rH.transform.GetComponent<IceTag>();
                        if (IT)
                        {
                            IT.Freeze();
                        }
                        break;
                    case 3: // bomb
                        GameObject BOMB = (GameObject)GameObject.Instantiate(PrefabMaster.BlockPrefabs[23].gameObject, rH.point, new Quaternion());
                        DestroyImmediate(BOMB.GetComponent<Renderer>());
                        BOMB.GetComponent<Rigidbody>().detectCollisions = false;
                        BOMB.GetComponentInChildren<Collider>().isTrigger = true;
                        BOMB.GetComponent<ExplodeOnCollideBlock>().Explodey();
                        ReduceBreakForce(rH.Joint);
                        break;
                }

            }
        }
        public void ReduceBreakForce(ConfigurableJoint Jointo)
        {
            if (Jointo && Jointo.breakForce == Mathf.Infinity)
            {
                Jointo.breakForce = 50000;
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
            public ConfigurableJoint Joint;
            public RHInfo(Vector3 v, Transform t, Collider c, Rigidbody r, ConfigurableJoint jjint)
            {
                point = v;
                transform = t;
                collider = c;
                rigidBody = r;
                Joint = jjint;
            }
        }

        private LineRenderer lr;                // laser beam

        public Color colour;

        // need to port over everything from the old version
        public List<RHInfo> rHInfos;
        public bool BeamHitAnything;            // also used by laser block for abilities

        protected override void OnSimulateStart()
        {
            rHInfos = new List<RHInfo>();
            if (lr == null)
            {
                lr = this.gameObject.AddComponent<LineRenderer>();
            }
            lr.material = new Material(Shader.Find("Particles/Additive"));
            lr.SetWidth(0.08f, 0.08f);

            Color ArgColour = LaserColourSlider.Value;

            lr.SetColors(Color.Lerp(ArgColour, Color.black, 0.45f),
                Color.Lerp(ArgColour, Color.black, 0.45f));
            lr.SetVertexCount(0);

            laserOnOff = !LaserOnOffToggle.IsActive;
        }
        public void SetBeamWidth()
        {
            // temporary fix for people who REALLY want to change the laser width
            lr.SetWidth(Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserWidth.Value, Mathf.Max(this.transform.localScale.x, this.transform.localScale.y) * LaserFocusSlider.Value * LaserWidth.Value);
        }
        public void CheckIfNeedsUpdate()
        {

            if (!laserOnOff)
            {
                UpdateFromPoint(this.transform.TransformPoint(0, 0, 1.3f), this.transform.forward);
                DrawBeam();
                SetBeamWidth();
            }
            else
            {
                lr.SetVertexCount(0);
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
            Vector3 lastPoint = point;
            Vector3 lastDir = dir;
            BeamHitAnything = false;
            rHInfos.Clear();

            LaserLength = LaserLengthSlider.Value;

            BeamHitAnything = false;

            foreach (RaycastHit Hito in Physics.RaycastAll(lastPoint, lastDir, LaserLength))
            {
                if (!Hito.collider.isTrigger)
                {
                    rHInfos.Add(new RHInfo(Hito.point, Hito.transform, Hito.collider, Hito.rigidbody, Hito.transform.GetComponent<ConfigurableJoint>()));
                    BeamHitAnything = true;
                    {

                        float SqrDist = (this.transform.position - Hito.point).sqrMagnitude;
                        if (SqrDist >= Mathf.Pow(LaserLength * PenetrativeLengthMultiplier.Value, 2))
                        {
                            LaserLength = Mathf.Sqrt(SqrDist);
                        }
                        else
                        {
                            LaserLength *= PenetrativeLengthMultiplier.Value;
                        }
                    }
                }
            }
        }
        private void DrawBeam()
        {
            //if (!laserOnOff)
            //{
            //    if (lLength != 0)
            //    {
            //        lLength = 0;
            //        lr.SetVertexCount(0);
            //    }
            //    return;
            //}
            //lLength = beamPoints.Count;
            //lr.SetVertexCount(lLength);
            //for (int i = 0; i < lLength; i++)
            //{
            //    lr.SetPosition(i, beamPoints[i]);
            //}
            //BeamLastPoint = beamPoints[beamPoints.Count - 1];
            lr.SetVertexCount(2);
            lr.SetPositions(new Vector3[] { this.transform.position + this.transform.forward * 0.8f, this.transform.position + this.transform.forward * LaserLength });
        }
    }
}
