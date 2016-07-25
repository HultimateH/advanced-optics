using System;
using spaar.ModLoader;
using UnityEngine;
using TheGuysYouDespise;
using Blocks;
using System.Collections.Generic;

namespace AdvancedLaserBlock
{

    public class LaserMod : BlockMod
    {
        /// ModLoader stuff
        /// Set name, author and so on for the blockloader to know what you made.
        public override string Name { get { return "advancedLasers"; } }
        public override string DisplayName { get { return "Advanced Laser Mod"; } }
        public override string Author { get { return "Pixali"; } }
        public override Version Version { get { return new Version(1, 1, 3, 1); } }

        // Raycast optimisation options
        /*
        public float MaxRaycastDistance = 200f;         // How far to raycast from each point
        public int RaycastRequestLifetime = 5;          // How many fixed updates before a request has to be resubmitted & immediately eval'd
        public int RaycastRequestPortion = 4;           // Do 1/n of the request stack each update if smoothing is on
        public bool SmoothRaycastsOverUpdates = true;   // Stagger raycasts over the update interval (above) to reduce CPU load
        public bool LimitRaycastsPerUpdate = false;     // Limits the number of raycasts per physics update
        public int RaycastsPerUpdate = 10;              // If potato is on, limits raycasts per physics updates to this
        private float version = 0.2f;*/

        //public static LaserMod Instance;
        

        /// <Block-loading-info>
        /// Place .obj file in Mods/Blocks/Obj
        /// Place texture in Mods/Blocks/Textures
        /// Place any additional resources in Mods/Blocks/Resources
        /// </Block-loading-info>

        protected Block laser = new Block()
            .ID(778)
            .BlockName("Advanced Laser")
            .Obj(new List<Obj> { new Obj("pixLaser.obj", "CockpitBomber.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)))
            })
            .IconOffset(new Icon(new Vector3(0.95f, 0.95f, 0.95f), new Vector3(0f, 0f, 0f), new Vector3(0f, 230f, 0f)))
            .Components(new Type[] {typeof(NewLaserBlock)})
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze", "Beam", "RIPTesseractCat"})
            )
            .Mass(0.3f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(0.5f, 0.5f, 1.5f), new Vector3(0f, 0f, 0.7f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>())
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});

        protected Block optics = new Block()
            .ID(779)
            .BlockName("Optics")
            .Obj(new List<Obj> { new Obj("MetalCube.obj", "OpticBlockTexture.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)))
            })
            .IconOffset(new Icon(new Vector3(1f, 1f, 1f), new Vector3(-0.1f, -0.1f, 0f), new Vector3(0f, 0f, 0f)))
            .Components(new Type[] {typeof(NewOpticsBlock)})
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze", "Beam", "RIPTesseractCat"}))
            .Mass(0.3f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>())
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});
        // diffuser is obsolete, just keeping code for now for posterity
        /*protected Block diffuser = new Block()
            .ID(780)
            .BlockName("Diffuser")
            .Obj(new List<Obj> { new Obj("MetalCube.obj", "OpticBlockTexture2.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)), Opacity.Translucent)
            })
            .IconOffset(new Icon(new Vector3(1f, 1f, 1f), new Vector3(-0.1f, -0.1f, 0f), new Vector3(0f, 0f, 0f)))
            .Components(new Type[] { typeof(DiffuserBlock) })
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze", "Beam", "RIPTesseractCat"}))
            .Mass(0.3f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>())
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});
                */
        public override void OnLoad()
        {
            LoadBlock(laser);
            LoadBlock(optics);
            //LoadBlock(diffuser);  // The diffuser (laser-based floodlight) comes in v4.

            //Game.OnSimulationToggle += ClearWirelessFrequencies;
            //UnityEngine.Object.DontDestroyOnLoad(MulticastHandler.Instance.gameObject);
            /*UnityEngine.Object.DontDestroyOnLoad(RaycastHandler.Instance.gameObject);

            // Configuration
            if (Configuration.GetFloat("version", 0f) < version)
            {
                Configuration.SetFloat("version", version);
                Configuration.SetFloat("maxRaycastDistance", 200f);
                Configuration.SetInt("raycastRequestLifetime", 5);
                Configuration.SetInt("raycastRequestPortion", 4);
                Configuration.SetBool("doSmoothRaycastsOverUpdates", true);
                Configuration.SetBool("doLimitRaycastsPerUpdate", false);
                Configuration.SetInt("raycastsPerUpdate", 10);
            }
            MaxRaycastDistance = Configuration.GetFloat("maxRaycastDistance", 200f);
            RaycastRequestLifetime = Configuration.GetInt("raycastRequestLifetime", 5);
            RaycastRequestPortion = Configuration.GetInt("raycastRequestPortion", 4);
            SmoothRaycastsOverUpdates = Configuration.GetBool("doSmoothRaycastsOverUpdates", true);
            LimitRaycastsPerUpdate = Configuration.GetBool("doLimitRaycastsPerUpdate", false);
            RaycastsPerUpdate = Configuration.GetInt("raycastsPerUpdate", 10);

            RaycastHandler.Instance.DoOptionSetup(MaxRaycastDistance, RaycastRequestLifetime, RaycastRequestPortion,
                SmoothRaycastsOverUpdates, LimitRaycastsPerUpdate, RaycastsPerUpdate);
            */
        }

        public override void OnUnload() { }
    }
}
