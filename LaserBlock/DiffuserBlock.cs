using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    /* as of time of writing, the diffuser doesn't work as it should...
     not to mention it'll be made obsolete by the PESWMR and its functions. */
    //public class DiffuserBlock : BlockScript
    //{
    //    //protected MSlider SizeSlider;
    //    //protected MMenu DiffuserModeMenu;
    //    //private List<LaserHandler> beamIn;
    //    //public bool OnOff;
    //    protected MMenu MulticastModeMenu;
    //    protected MColourSlider MulticastFreqSlider;
    //    protected MColourSlider OnColourSlider;
    //    protected MToggle InvertMulticastToggle;

    //    private GameObject innerCube; private Renderer iCR;
    //    private GameObject outerCube; private Renderer oCR;
    //    //private Light activeLight;
    //    private MulticastTag mTag;
        
    //    /*public void SetLaserHit(LaserHandler lh)
    //    {
    //        if (beamIn.Keys.Contains(lh)) return;
    //        beamIn.Add(lh, new DiffuserVolume());
    //    }
    //    public void UnsetLaserHit(LaserHandler lh)
    //    {
    //        if (beamIn.Keys.Contains(lh))
    //        {
    //            beamIn.Remove(lh);
    //        }
    //    }*/
        
    //    public override void SafeAwake()
    //    {
    //        MulticastModeMenu = AddMenu("multicastMode", 0, new List<string>() { "OR", "AND", "XOR" });
    //        MulticastFreqSlider = AddColourSlider("Multicast\nFrequency", "multicastFreq", Color.red);
    //        OnColourSlider = AddColourSlider("Active Colour", "activeColour", Color.red);
    //        InvertMulticastToggle = AddToggle("Invert Operator", "invertMulticast", false);
    //        //DiffuserModeMenu = AddMenu("diffuserMode", 0, new List<string>() { "Multicast Glow" });
    //        //SizeSlider = AddSlider("Diffusion Angle", "diffuserAngle", 5f, 0f, 45f);

    //        mTag = gameObject.AddComponent<MulticastTag>();
    //        //Debug.Log("safeawake called");

    //        //if (!gameObject.GetComponent<Light>()) activeLight = gameObject.AddComponent<Light>();
    //        //else activeLight = gameObject.GetComponent<Light>();
    //        //Debug.Log(gameObject.GetComponent<Light>());
    //        //activeLight.type = LightType.Point; activeLight.color = Color.Lerp(OnColourSlider.Value, Color.black, 0.45f);
    //        //activeLight.range = 15f;
    //        Color colour = Visuals[0].GetComponent<Renderer>().material.color; colour.a = 0.4f;
    //        Visuals[0].GetComponent<Renderer>().material = new Material(Shader.Find("Transparent/Diffuse"));
    //        Visuals[0].GetComponent<Renderer>().material.color = colour;
    //        innerCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        if (innerCube.GetComponent<Collider>())
    //            Destroy(innerCube.GetComponent<Collider>());
            
    //        outerCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    //        if (outerCube.GetComponent<Collider>())
    //            Destroy(outerCube.GetComponent<Collider>());
            
    //        Debug.Log(innerCube.GetComponent<Renderer>());
    //        Debug.Log(outerCube.GetComponent<Renderer>());
    //        iCR = innerCube.GetComponent<Renderer>();
    //        oCR = outerCube.GetComponent<Renderer>();
    //        innerCube.transform.position = transform.position + 0.5f * transform.forward;
    //        outerCube.transform.position = transform.position + 0.5f * transform.forward;
    //        innerCube.transform.SetParent(transform); innerCube.transform.localScale *= 0.8f;
    //        outerCube.transform.SetParent(transform); outerCube.transform.localScale *= 1.1f;
    //        colour = Color.Lerp(OnColourSlider.Value, Color.black, 0.45f); colour.a = 0.75f;
    //        iCR.material = new Material(Shader.Find("Transparent/Diffuse"));
    //        iCR.material.color = colour;
    //        colour = Color.Lerp(OnColourSlider.Value, Color.black, 0.45f); colour.a = 0.35f;
    //        oCR.material = new Material(Shader.Find("Transparent/Diffuse"));
    //        oCR.material.color = colour;

    //        InvertMulticastToggle.Toggled += InvertMulticastNames;
    //        InvertMulticastToggle.Toggled += ToggleLightCubes;
    //        OnColourSlider.ValueChanged += ChangeCubeColours;
    //        mTag.Recieve += OnReceive;

    //        InvertMulticastNames(InvertMulticastToggle.IsActive);
    //        ToggleLightCubes(InvertMulticastToggle.IsActive);
    //    }

    //    private void ChangeCubeColours(Color value)
    //    {
    //        value.a = 0.2f;
    //        iCR.material.color = value;
    //        value.a = 0.1f;
    //        oCR.material.color = value;
    //    }

    //    private void OnReceive(bool b) // used by mTag
    //    {
    //        ToggleLightCubes(InvertMulticastToggle.IsActive);
    //    }
    //    private void ToggleLightCubes(bool isActive)
    //    {
    //        iCR.enabled = isActive ^ mTag.onOff;
    //        oCR.enabled = isActive ^ mTag.onOff;
    //        //activeLight.enabled = isActive ^ mTag.onOff;
    //    }

    //    private void InvertMulticastNames(bool isActive)
    //    {
    //        if (isActive)
    //            MulticastModeMenu.Items = new List<string>() { "NOR", "NAND", "XNOR" };
    //        else
    //            MulticastModeMenu.Items = new List<string>() { "OR", "AND", "XOR" };
    //        MulticastModeMenu.DisplayName = MulticastModeMenu.Items[MulticastModeMenu.Value];
    //    }
        
    //    protected override void OnSimulateStart()
    //    {
    //        mTag.RegisterReciever(MulticastFreqSlider.Value, (MulticastHandler.BitOp)MulticastModeMenu.Value);
            
    //    }
    //    protected override void OnSimulateUpdate()
    //    {

    //    }
    //    protected override void OnSimulateExit()
    //    {
    //        //beamIn.Clear();
    //        mTag.Reset();
    //    }
    //    public override void OnLoad(XDataHolder data)
    //    {
    //        LoadMapperValues(data);
    //        //InvertMulticastNames(InvertMulticastToggle.IsActive);
    //        //ToggleLightCubes(InvertMulticastToggle.IsActive);
    //    }
    //    public override void OnSave(XDataHolder data)
    //    {
    //        SaveMapperValues(data);
    //    }
    //}
    /*public class DiffuserVolume
    {
        public LaserHandler lh;
        public GameObject GO;

    }*/
}
