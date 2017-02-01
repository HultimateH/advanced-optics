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
        public override string Name { get { return "ImprovedLaserMod"; } }
        public override string DisplayName { get { return "Improved Laser Mod"; } }
        public override string Author { get { return "wang_w571 From Pixail's code"; } }
        public override Version Version { get { return new Version(1,0); } }
        public override void OnLoad()
        {
            Block laser = new Block()
            .ID(577)
            .BlockName(!Configuration.GetBool("UseChinese", false) ? "Improved Laser Emitter" : "改进型激光发生器")
            //.BlockName("Improved Laser Emitter" )
            .Obj(new List<Obj> { new Obj("LaserBlock2.obj", "LaserBlock2.png",
                new VisualOffset(new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)))
            })
            .IconOffset(new Icon(Vector3.one * 3f, new Vector3(0.2f, -0.3f, -2.15f), new Vector3(30f, 230f, 0f)))
            .Components(new Type[] { typeof(NewLaserBlock) })
            .Properties(new BlockProperties().SearchKeywords(new string[] {
                "Laser", "Fire", "Kinetic", "Freeze","Explosive","Weapon", "Beam", "RIPTesseractCat"})
            )
            .Mass(0.3f)
            .ShowCollider(false)
            .CompoundCollider(new List<ColliderComposite> {
                ColliderComposite.Box(new Vector3(0.5f, 0.5f, 1.1f), new Vector3(0f, 0f, 0.55f), new Vector3(0f, 0f, 0f))})
            .IgnoreIntersectionForBase()
            .NeededResources(new List<NeededResource>()
            {
                new NeededResource(ResourceType.Texture,"LaserParticle.png")
            })
            .AddingPoints(new List<AddingPoint> {
                new BasePoint(false, true).Motionable(false,false,false).SetStickyRadius(0.5f)});

            LoadBlock(laser);
            
        }
    }
}
