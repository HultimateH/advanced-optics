using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class LaserBlock : BlockScript
    {
        //BlockSettings

        protected bool doTestMode = true;
        
        protected MKey laserToggleKey;
        protected MColourSlider laserHueSlider;      // changes colour of laser beam
        protected MMenu laserCosmeticModeMenu;       // plain, trig, random or trigstart
        protected MSlider laserCosmeticXScaleSlider; // if using trig outer beam, cos
        protected MSlider laserCosmeticYScaleSlider; // "                      ", sin
        protected MSlider laserCosmeticZScaleSlider; // "                      ", theta multiplier
        protected MToggle startOnToggle;
        protected MToggle cosmeticEditToggle;        // Makes functional buttons disappear and cosmetic ones appear
        protected MMenu laserModeMenu;               // Fire, Kinetic, Freeze, Cosmetic, Tag or Sensor
        protected MKey laserAbilityKey;              // Laser special ability - Fire explodes, Freeze zaps
        //protected MSlider explosionPowerSlider;    // Didn't want that slider anyway
        protected MSlider kineticInOutSlider;        // aligned to laser beam
        protected MSlider kineticSideSlider;         // perpendicular to beam
        protected MSlider kineticVertSlider;         // vertical
        //protected MKey freezeNaturesWrathKey;      // if on, zaps instead of freezing.
        protected MColourSlider sensorRecieverSlider;// if laser mode is set to Sensor, its on/off state will be startOnToggle XOR brTag.onOff
        protected MMenu sensorModeMenu;
        protected LayerMask rayCastLayerMask;
        protected LaserHandler lH;
        protected MulticastTag brTag;

        protected GameObject innerGlowCore;
        protected GameObject innerGlowOuter;

        protected GameObject lRG;
        protected GameObject lRG2;
        protected LineRenderer lRBeam;
        protected LineRenderer lRN;
        protected int lSet;
        protected int lLength;
        protected float animTick;
        protected bool isLaserOn;
        protected Vector3 kinVec3;
        private List<string> ignoreFreeze = new List<string>();
        private Texture2D iceTexture = new Texture2D(2, 2);
        private Dictionary<MeshRenderer, Color> painted = new Dictionary<MeshRenderer, Color>();

        /*protected enum LaserMode
        {
            Fire, Kinetic, Freeze, Cosmetic, Paint /// Lol TGYD
        }
        protected enum LaserCosmeticMode
        {
            None, Trig, TrigStart, Random
        }
        protected LaserMode laserMode;
        protected LaserCosmeticMode laserCMode;*/

        private void GetIceTexture()
        {
            Texture2D[] texture2DArray = Resources.FindObjectsOfTypeAll<Texture2D>();
            int num = 0;
            while (num < texture2DArray.Length)
            {
                Texture2D texture2D = texture2DArray[num];
                if (!(texture2D.name == "IceTexture")) num++;
                else
                {
                    iceTexture = texture2D;
                    break;
                }
            }
        }
        private void AddIceTagAndTexture(GameObject tempGameObject)
        {
            IceTag componentsInChildren = tempGameObject.AddComponent<IceTag>();
            componentsInChildren.renderers = tempGameObject.GetComponentsInChildren<Renderer>();
            componentsInChildren.frozen = false;
            componentsInChildren.iceTexture = iceTexture;
            componentsInChildren.sendFrozenMessage = new Transform[] { componentsInChildren.transform };
        }

        //BlockLoader Specific method: is called right after the script is made - usually,
        //this is done for all blocks of this type and is safe as it waits for stuff like
        //colliders, visuals, resources and so on
        public override void SafeAwake()
        {
            // layer mask: need to only hit general stuff (default layers?), along with layer 27
            rayCastLayerMask = 1 << 20; // Fire
            rayCastLayerMask += 1 << 2; // Ignore raycast
            rayCastLayerMask = ~rayCastLayerMask;

            GetIceTexture();

            //innerGlowCore = GameObject.CreatePrimitive(PrimitiveType.Cube); innerGlowCore.transform.SetParent(transform);
            //innerGlow.transform.

            laserToggleKey = AddKey("Toggle On/Off", "toggleOnOff", KeyCode.Y);
            startOnToggle = AddToggle("Start On", "startOn", true);
            //switchLaserModeButton = AddToggle("Fire Beam", "switchMode", false);
            laserModeMenu = AddMenu("laserMode", 0,
                new List<string> { "Fire Beam", "Kinetic Beam", "Freeze Beam", "Cosmetic Beam", "Tag Beam", "Sensor Beam" });
            sensorRecieverSlider = AddColourSlider("Sensor Frequency", "sensorFreq", Color.red);
            sensorModeMenu = AddMenu("sensorMode", 0,
                new List<string> { "OR", "AND", "XOR", "NOT" });
            laserAbilityKey = AddKey("Laser Ability", "ability", KeyCode.J);

            // Cosmetic
            //laserHueSlider = AddSlider("Laser Hue", "laserHue", 0f, 0f, 360f);
            laserHueSlider = AddColourSlider("Laser Hue", "laserColour", Color.red);
            //laserCosmeticModeMenu = AddToggle("No Effects", "cosmeticMode", false);
            laserCosmeticModeMenu = AddMenu("cosmeticMode", 0,
                new List<string> { "No Effects", "Trig Effects", "Inv Trig Effects", "Lightning Effects" });
            laserCosmeticXScaleSlider = AddSlider("Cos Mod", "cos", 1f, 0.1f, 3f);
            laserCosmeticYScaleSlider = AddSlider("Sin Mod", "sin", 1f, 0.1f, 3f);
            laserCosmeticZScaleSlider = AddSlider("Theta Mod", "theta", 1f, 0.1f, 10f);
            cosmeticEditToggle = AddToggle("Cosmetic Edit\nOn/Off", "cosmeticEdit", false);

            //cSlider = AddColourSlider("test", "test", Color.red);
            // Fire mode
            //explosionKey = AddKey("Explosive Impulse", "explode", KeyCode.J);
            //explosionPowerSlider = AddSlider("Explosion Power", "explodePower", 1f, 0.5f, 5f);

            // Kinetic mode
            kineticInOutSlider = AddSlider("In/Out Force", "inOut", 1f, -10f, 10f); // + -> away from laser
            kineticSideSlider = AddSlider("Sideways Force", "side", 0f, -10f, 10f); // + -> to right of laser
            kineticVertSlider = AddSlider("Vertical Force", "vert", 1f, -10f, 10f); // + -> up

            // Freeze mode
            //freezeNaturesWrathKey = AddKey("Nature's Wrath", "wrath", KeyCode.J);

            isLaserOn = startOnToggle.IsActive;

            //laserModeMenu.IsButton = true;
            //laserCosmeticModeMenu.IsButton = true;

            lRG = new GameObject(); lRG.transform.SetParent(transform); // new GO, set its transform to this block's.
            lRG.AddComponent<LineRenderer>(); lRBeam = lRG.GetComponent<LineRenderer>(); // add a line renderer to the GO and grab a reference
            lRBeam.material = new Material(Shader.Find("Particles/Additive"));
            lRBeam.SetWidth(0.1f, 0.1f); lRBeam.SetColors(Color.Lerp(laserHueSlider.Value, Color.black, 0.5f),
                Color.Lerp(laserHueSlider.Value, Color.white, 0.5f));
            lRBeam.SetVertexCount(0);

            lRG2 = new GameObject(); lRG2.transform.SetParent(transform);
            lRG2.AddComponent<LineRenderer>(); lRN = lRG2.GetComponent<LineRenderer>();
            lRN.material = new Material(Shader.Find("Particles/Additive")); lRN.SetWidth(0.5f, 0.5f);
            lRN.SetColors(Color.Lerp(Color.blue, Color.white, 0.25f), Color.Lerp(Color.blue, Color.white, 0.25f));
            lRN.SetVertexCount(0);

            if (doTestMode) lH = new LaserHandler(transform, laserHueSlider.Value);
            if (doTestMode) brTag = gameObject.AddComponent<MulticastTag>();

            // hide all the irrelevant stuff
            CycleLaserMode(laserModeMenu.Value);
            CycleLaserCosmeticMode(laserCosmeticModeMenu.Value);
            ToggleLaserCosmeticEdit(false);

            // assign functions to cycle buttons
            laserModeMenu.ValueChanged += CycleLaserMode;
            laserCosmeticModeMenu.ValueChanged += CycleLaserCosmeticMode;
            cosmeticEditToggle.Toggled += ToggleLaserCosmeticEdit;
        }
        
        protected void NaturesWrath(RaycastHit rH)
        {
            Vector3 v3 = rH.point;
            Vector3 v32 = v3 + Vector3.up * 150;
            int lastSet = 41;
            float distRemain = 125f;
            lRN.SetVertexCount(42);
            for (int i = 0; i < 5; i++)
            {
                Vector3 v4 = v32 - Vector3.up * (distRemain / (5 - i)) * UnityEngine.Random.Range(0.5f, 0.9f) + (Vector3)UnityEngine.Random.insideUnitCircle * 5;
                lRN.SetPosition(lastSet - 1, v4);
                Vector3[] vs = NWCalcBranch(v4);
                lRN.SetPosition(lastSet - 2, vs[0]); lRN.SetPosition(lastSet - 6, vs[2]);
                lRN.SetPosition(lastSet - 3, vs[1]); lRN.SetPosition(lastSet - 7, vs[1]);
                lRN.SetPosition(lastSet - 4, vs[2]); lRN.SetPosition(lastSet - 8, vs[0]);
                lRN.SetPosition(lastSet - 5, vs[3]);
                lastSet -= 8;
                distRemain = v4.y - v3.y;
                v32 = new Vector3(v3.x, distRemain, v3.z);
            }

            lRN.SetPosition(41, v32);
            lRN.SetPosition(0, v3);
            lSet = 41;
        }
        protected Vector3[] NWCalcBranch(Vector3 brP) // brP = branch point
        {
            Vector3 v1 = (Vector3)UnityEngine.Random.insideUnitCircle * 10f + brP - Vector3.up * UnityEngine.Random.Range(1f, 10f);
            Vector3 v2 = (UnityEngine.Random.insideUnitSphere - Vector3.up) * UnityEngine.Random.Range(1f, 10f) + v1;
            Vector3 v3 = (UnityEngine.Random.insideUnitSphere - Vector3.up) * UnityEngine.Random.Range(1f, 10f) + v2;
            Vector3 v4 = (UnityEngine.Random.insideUnitSphere - Vector3.up) * UnityEngine.Random.Range(1f, 10f) + v3;
            return new Vector3[] { v1, v2, v3, v4 };
        }
        protected void FlingInSphere(Vector3 v3, RaycastHit rH, bool ignite, bool spin)
        {
            RaycastHit[] rayHits;
            Vector3 v4;
            rayHits = Physics.SphereCastAll(rH.point, 3f, -Vector3.up, 2.5f, rayCastLayerMask);
            if (rayHits.Length > 0/* &&
                rayHit.collider.attachedRigidbody != null &&
                rayHit.collider.attachedRigidbody != transform.GetComponent<Rigidbody>()*/)
            {
                foreach (RaycastHit rayHit in rayHits)
                {
                    if (rayHit.collider.attachedRigidbody == null ||
                        rayHit.collider.attachedRigidbody == transform.GetComponent<Rigidbody>()) continue;
                    //Debug.Log(rayHit.collider.attachedRigidbody + ", " + rayHit.collider.gameObject.name);
                    if (v3.magnitude <= 0) v4 = UnityEngine.Random.insideUnitSphere + Vector3.up;
                    else v4 = v3;
                    rayHit.collider.attachedRigidbody.WakeUp();
                    rayHit.collider.attachedRigidbody.AddForce(v4 * 1000f);
                    if (spin) rayHit.collider.attachedRigidbody.AddTorque(0f, UnityEngine.Random.Range(-250f, 250f), 0f); // put a new spin on things
                    if (rayHit.collider.attachedRigidbody.GetComponent<FireTag>() && ignite)
                        rayHit.collider.attachedRigidbody.GetComponent<FireTag>().Ignite();
                }
            }
        }
        protected void FlingPoint(Vector3 v3, RaycastHit rH, bool ignite, bool spin)
        {
            Vector3 v4;
            if (v3.magnitude <= 0) v4 = UnityEngine.Random.insideUnitSphere + Vector3.up;
            else v4 = v3;
            if (rH.collider.attachedRigidbody != null &&
                rH.collider.attachedRigidbody != transform.GetComponent<Rigidbody>())
            {
                //Debug.Log(rH.collider.attachedRigidbody + ", " + rH.collider.gameObject.name);
                rH.collider.attachedRigidbody.WakeUp();
                rH.collider.attachedRigidbody.AddForce(v4 * 1000f);
                if (spin) rH.collider.attachedRigidbody.AddTorque(0f, 100f, 0f); // put a new spin on things
                if (rH.collider.attachedRigidbody.GetComponent<FireTag>() && ignite)
                    rH.collider.attachedRigidbody.GetComponent<FireTag>().Ignite();
            }
        }
        protected void CycleLaserMode(int value)
        {
            // prepare to cycle
            //explosionKey.DisplayInMapper = false;
            //explosionPowerSlider.DisplayInMapper = false;
            kineticInOutSlider.DisplayInMapper = false;
            kineticSideSlider.DisplayInMapper = false;
            kineticVertSlider.DisplayInMapper = false;
            sensorModeMenu.DisplayInMapper = false;
            sensorRecieverSlider.DisplayInMapper = false;
            
            switch (value)
            {
                case 0:
                    //explosionPowerSlider.DisplayInMapper = true;
                    return;
                case 1:
                    kineticInOutSlider.DisplayInMapper = true;
                    kineticSideSlider.DisplayInMapper = true;
                    kineticVertSlider.DisplayInMapper = true;
                    return;
                case 2:
                case 3:
                case 4:
                    return;
                case 5:
                    sensorModeMenu.DisplayInMapper = true;
                    sensorRecieverSlider.DisplayInMapper = true;
                    return;

            }
        }
        private void ToggleLaserCosmeticEdit(bool isActive)
        {
            laserModeMenu.DisplayInMapper = !isActive;
            if (laserModeMenu.Value == 0)
            {
                //explosionKey.DisplayInMapper = !isActive;
                //explosionPowerSlider.DisplayInMapper = !isActive;
            }
            else if (laserModeMenu.Value == 1)
            {
                kineticInOutSlider.DisplayInMapper = !isActive;
                kineticSideSlider.DisplayInMapper = !isActive;
                kineticVertSlider.DisplayInMapper = !isActive;
            } else if (laserModeMenu.Value == 5)
            {
                sensorModeMenu.DisplayInMapper = !isActive;
                sensorRecieverSlider.DisplayInMapper = !isActive;
            }
            laserHueSlider.DisplayInMapper = isActive;
            laserCosmeticModeMenu.DisplayInMapper = isActive;
            if (laserCosmeticModeMenu.Value == 1 || laserCosmeticModeMenu.Value == 2)
            {
                laserCosmeticXScaleSlider.DisplayInMapper = isActive;
                laserCosmeticYScaleSlider.DisplayInMapper = isActive;
                laserCosmeticZScaleSlider.DisplayInMapper = isActive;
            }
        }
        private void CycleLaserCosmeticMode(int value)
        {
            
            
            switch (value)
            {
                case 1:
                case 2:
                    laserCosmeticXScaleSlider.DisplayInMapper = true;
                    laserCosmeticYScaleSlider.DisplayInMapper = true;
                    laserCosmeticZScaleSlider.DisplayInMapper = true;
                    return;
                case 0:
                case 3:
                    laserCosmeticXScaleSlider.DisplayInMapper = false;
                    laserCosmeticYScaleSlider.DisplayInMapper = false;
                    laserCosmeticZScaleSlider.DisplayInMapper = false;
                    return;
            }


        }
        /*protected Color HueToRGB(float hue)
        {
            if (hue < 0f) hue += (float)Math.Floor(-hue / 360) * 360; // correct values
            if (hue >= 360f) hue -= (float)Math.Floor(hue / 360) * 360;
            // hue is from 0 to 360
            if (0f <= hue && hue < 60f) return new Color(1f, hue / 60f, 0f);
            if (60f <= hue && hue < 120f) return new Color(1f - (hue - 60f) / 60f, 1f, 0f);
            if (120f <= hue && hue < 180f) return new Color(0f, 1f, (hue - 120f) / 60f);
            if (180f <= hue && hue < 240f) return new Color(0f, 1f - (hue - 180f) / 60f, 1f);
            if (240f <= hue && hue < 300f) return new Color((hue - 240f) / 60f, 0f, 1f);
            if (300f <= hue && hue < 360f) return new Color(1f, 0f, 1f - (hue - 300f) / 60f);
            return new Color(1f, 0f, 0f);
        }*/
        //BlockLoader Specific method: Is the safe 1 time called method for the prefab - that's the master gameobject, or template so to speak
        //if you need to make alterations to the block you couldn't do with the standard framework, do it here.
        public override void OnPrefabCreation()
        {
        }

        //BlockLoader Specific method: When the player presses spacebar or the simulate/play button in the upper left corner
        protected override void OnSimulateStart()
        {
            isLaserOn = startOnToggle.IsActive;
            lRBeam.SetColors(Color.Lerp(laserHueSlider.Value, Color.black, 0.5f),
                Color.Lerp(laserHueSlider.Value, Color.black, 0.5f));
            lRBeam.SetVertexCount(isLaserOn ? 2 : 0);
            kinVec3 = new Vector3(kineticInOutSlider.Value, kineticVertSlider.Value, kineticSideSlider.Value);
            if (laserCosmeticModeMenu.Value == 0) lLength = 2;

            /*if (laserModeMenu.Value == 5)
            {
                if (LoadLaserBlock.wirelessFrequencies.ContainsKey(sensorRecieverSlider.Value))
                    LoadLaserBlock.wirelessFrequencies.Add(sensorRecieverSlider.Value, new List<Transform> { transform });
                else LoadLaserBlock.wirelessFrequencies[sensorRecieverSlider.Value].Add(transform);
            }*/
            if (doTestMode)
            {
                brTag.RegisterReciever(laserHueSlider.Value, (MulticastHandler.BitOp)sensorModeMenu.Value);
                brTag.Recieve = new MulticastTag.RecieveDelegate(SensorOnOff);

            }

        }
        protected void Explode(RaycastHit rH)
        {
            GameObject GO = spaar.ModLoader.Game.AddPiece.blockTypes[23].gameObject;
            GameObject GO2 = (GameObject)Instantiate(GO, rH.point, new Quaternion()); // spawn in a new bomb off the original
            Destroy(GO2.GetComponent<Rigidbody>()); GO2.AddComponent<Rigidbody>(); // reset bomb clone's rigidbody
            GO2.AddComponent<KillIfEditMode>(); // if the player goes back to edit mode, destroy the clone
            GO2.GetComponentInChildren<FireTag>().Ignite(); // kaboom.
        }
        protected void SetCorrectBeam(Vector3 point)
        {
            if (!isLaserOn) return;
            lRBeam.SetPosition(1, transform.position + transform.forward);
            lRBeam.SetPosition(0, point);
            switch (laserCosmeticModeMenu.Value)
            {
                case 0:
                    break;
                case 1:
                    lLength = 2 + (int)((point - (transform.position + transform.forward)).magnitude);
                    lRBeam.SetVertexCount(lLength);
                    for (int i = 2; i < lLength - 1; i++)
                    {
                        lRBeam.SetPosition(i, transform.TransformDirection(new Vector3(
                            Mathf.Cos((i - 2 + animTick) * laserCosmeticZScaleSlider.Value) * laserCosmeticXScaleSlider.Value,
                            Mathf.Sin((i - 2 + animTick) * laserCosmeticZScaleSlider.Value) * laserCosmeticYScaleSlider.Value,
                            i - 2))+transform.position+transform.forward);
                    }
                    lRBeam.SetPosition(lLength - 1, point);
                    break;
                case 2:
                    lLength = 2 + (int)((point - (transform.position + transform.forward)).magnitude);
                    lRBeam.SetVertexCount(lLength);
                    for (int i = 2; i < lLength - 1; i++)
                    {
                        lRBeam.SetPosition(i, transform.TransformDirection(new Vector3(
                            Mathf.Cos((i - 2 + animTick) * laserCosmeticZScaleSlider.Value) * laserCosmeticXScaleSlider.Value / (i - 1),
                            Mathf.Sin((i - 2 + animTick) * laserCosmeticZScaleSlider.Value) * laserCosmeticYScaleSlider.Value / (i - 1),
                            i - 2)) + transform.position + transform.forward);
                    }
                    lRBeam.SetPosition(lLength - 1, point);
                    break;
                case 3:
                    lLength = 2 + (int)((point - (transform.position + transform.forward)).magnitude / 4);
                    lRBeam.SetVertexCount(lLength);
                    for (int i = 2; i < lLength - 1; i++)
                    {
                        lRBeam.SetPosition(i, transform.TransformDirection(new Vector3(
                            UnityEngine.Random.Range(-1f, 1f),
                            UnityEngine.Random.Range(-1f, 1f),
                            (i - 2)*4)) + transform.position + transform.forward);
                    }
                    lRBeam.SetPosition(lLength - 1, point);
                    break;
            }

        }
        protected void RenderExtraGlow()
        {

        }
        public void SensorOnOff (bool onOff)
        {
            isLaserOn = !isLaserOn;
            lRBeam.SetVertexCount(isLaserOn ? lLength : 0);
        }

        //BlockLoader Specific method: When the player is simulating instead of building
        /*protected override void OnSimulateUpdate()
        {
            RaycastHit rayHit;
            animTick += Mathf.PI/(32*laserCosmeticZScaleSlider.Value);
            if (animTick >= 200 * Mathf.PI) animTick = 0;
            if (laserToggleKey.IsPressed)
            {
                isLaserOn = !isLaserOn;
                lRBeam.SetVertexCount(isLaserOn ? lLength : 0);
            }
            if (isLaserOn)
            {
                
                //if (!Physics.Raycast(transform.position+transform.forward, transform.forward, out rayHit, Mathf.Infinity, rayCastLayerMask))
                //    SetCorrectBeam(transform.position + transform.forward * 200);
                //else
                {
                    //SetCorrectBeam(rayHit.point);
                    //Debug.Log("Hit object's name: "+rayHit.transform.gameObject.name+"\nHit object's layer: "+rayHit.transform.gameObject.layer);
                    switch (laserModeMenu.Value)
                    {
                        case 0:
                            IgniteAndBreak(rayHit);
                            break;
                        case 1:
                            FlingPoint(transform.TransformDirection(kinVec3), rayHit, false, false);
                            break;
                        case 2:
                            FreezeAndBreak(rayHit);
                            break;
                        case 3:
                        case 4:
                            break;
                    }
                    if (laserAbilityKey.IsPressed)
                    {
                        if (laserModeMenu.Value == 2) { NaturesWrath(rayHit); FlingInSphere(Vector3.zero, rayHit, true, true); }
                        else if (laserModeMenu.Value == 0) Explode(rayHit);
                        else if (laserModeMenu.Value == 4 && rayHit.transform.gameObject.GetComponent<MeshRenderer>())
                        {
                            MeshRenderer mr = rayHit.transform.gameObject.GetComponent<MeshRenderer>();
                            painted.Add(mr, mr.material.color);
                            mr.material.color = laserHueSlider.Value;
                            
                        }
                    }
                }
            }
            if (lSet > 0)
            {
                lSet--;
                lRN.SetVertexCount(lSet);
            }
            //if (doTestMode) lH.ForceUpdate();
            if (doTestMode) lH.CheckIfNeedsUpdate();

        }*/
        protected override void OnSimulateFixedUpdate()
        {
            // ONLY PUT KINETIC MODE CODE HERE.
            base.OnSimulateFixedUpdate();
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
        protected void FreezeAndBreak(RaycastHit rH) // shamelessly stolen from ITR's frostthrower mod
        {
            Transform transforms = rH.transform;
            string str = transforms.name;
            int num = 0;
            while (num < ignoreFreeze.Count)
            {
                if (!(str == ignoreFreeze[num]))
                {
                    num++;
                }
                else
                {
                    return;
                }
            }
            if (transforms.GetComponent<FireTag>() && !transforms.GetComponent<IceTag>())
                AddIceTagAndTexture(transforms.gameObject);
            IceTag component = transforms.GetComponent<IceTag>();
            while (component == null)
            {
                if (!(transforms.parent == null))
                {
                    transforms = transforms.parent;
                    component = transforms.GetComponent<IceTag>();
                }
                else
                {
                    ignoreFreeze.Add(str);
                    return;
                }
            }
            if (transforms.GetComponent<BleedOnJointBreak>() && !transforms.GetComponent<BleedOnJointBreak>().aiCode.isDead)
                transforms.GetComponent<BleedOnJointBreak>().KillMe(true);
            if (component != null && !component.frozen) component.Freeze();
        }
        //BlockLoader Specific method: When we are done simulating, usually you don't need to do anything here,
        //as the block in simulation mode is deleted, but if you have static variables or similar you might want to update it here.
        protected override void OnSimulateExit()
        {
            foreach (MeshRenderer mr in painted.Keys) // reset paint
            {
                mr.material.color = painted[mr];
            }
            painted.Clear(); // and reset dictionary of paint
        }

        //The following functions are for saving, loading, copying and pasting the values

        //Besiege Specific method
        public override void OnSave(XDataHolder data)
        {
            SaveMapperValues(data);

        }

        //Besiege Specific method
        public override void OnLoad(XDataHolder data)
        {
            LoadMapperValues(data);
            CycleLaserMode(laserModeMenu.Value);
            CycleLaserCosmeticMode(laserCosmeticModeMenu.Value);
        }
    }
}
