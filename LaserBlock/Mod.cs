using System;
using spaar.ModLoader;
using UnityEngine;
using TheGuysYouDespise;
using Blocks;
using System.Collections.Generic;

namespace ImprovedLaserBlock
{

    public class LaserMod : BlockMod
    {
        /// ModLoader stuff
        /// Set name, author and so on for the blockloader to know what you made.
        public override string Name { get { return "ImprovedLaserMod"; } }
        public override string DisplayName { get { return "Improved Laser Mod"; } }
        public override string Author { get { return "wang_w571 From Pixail's code"; } }
        public override Version Version { get { return new Version(1, 1, 3, 1); } }
        protected Block laser = new Block()
            .ID(778)
            .BlockName("Improved Laser")
            .Obj(new List<Obj> { new Obj("LaserBlock2.obj", "LaserBlock2.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)))
            })
            .IconOffset(new Icon(new Vector3(0.95f, 0.95f, 0.95f), new Vector3(0f, 0f, 0f), new Vector3(0f, 230f, 0f)))
            .Components(new Type[] { typeof(NewLaserBlock) })
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze", "Beam", "RIPTesseractCat"})
            )
            .Mass(0.3f)
            .ShowCollider(true)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(0.5f, 0.5f, 1.5f), new Vector3(0f, 0f, 0.7f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>())
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});
        public override void OnLoad()
        {
            LoadBlock(laser);
        }
    }
}
