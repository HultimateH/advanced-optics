using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedLaserBlock
{
    public class LaserHandler
    {
        private GameObject GOContainer;
        private LineRenderer lr;                // laser beam
        private List<Vector3> beamDirections;   // the transforms of hit objects
        private List<Vector3> beamPoints;       // each point where the beam changes direction
        private Transform beamFirstPoint;       // equivalent to laser emitter transform
        private List
            <NewOpticsBlock> sensorUpdate;      // update hit/not hit when updating
        private List
            <NewOpticsBlock> lastSensorUpdate;   // check if these need to be cleared
        private int lLength;                    // used in calculating how many vertices the line renderer needs
        private static LayerMask layerMask = ~((1 << 20) + (1 << 2));
        public Color colour;
        private int updateCount = 0;
        private Transform transform;
        private Vector3 parentPos;
        private Quaternion parentRot;
        //private List<LHData> triggers;
        private int raycastCount = 0;
        private bool raycastShutdown = false;
        private float laserFocus = 0.1f;
        public bool skipTickLimit = false;
        private bool timePausedFlag = true;     // true = time is flowing, false = time paused

        // need to port over everything from the old version
        public bool onOff = true;               // on by default
        public RaycastHit BeamLastHit;          // used by laser block for abilities
        public bool BeamHitAnything;            // also used by laser block for abilities
        public Vector3 BeamLastPoint = new Vector3();
        public float beamRayLength = 500f;

        public LaserHandler(Transform parent, Color colour)
        {
            // oh boy what a mess
            // setup GO container
            GOContainer = new GameObject();
            GOContainer.transform.SetParent(parent);

            // create LineRenderer for laser
            lr = GOContainer.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Additive"));
            lr.SetWidth(0.08f, 0.08f);
            lr.SetColors(Color.Lerp(colour, Color.black, 0.45f),
                Color.Lerp(colour, Color.black, 0.45f));
            lr.SetVertexCount(0);

            // redraw arguments (also filter checking)
            this.colour = colour;
            beamFirstPoint = parent;
            beamDirections = new List<Vector3>();
            beamPoints = new List<Vector3>();
            sensorUpdate = new List<NewOpticsBlock>();
            lastSensorUpdate = new List<NewOpticsBlock>();

            // parent transform ref, also optimisation args
            transform = parent;
            parentPos = transform.position;
            parentRot = transform.rotation;

            // more optimisation args
            //triggers = new List<LHData>();
            raycastCount = 0;
        }
        public void SetBeamWidth(float f)
        {
            laserFocus = f;
            // temporary fix for people who REALLY want to change the laser width
            lr.SetWidth(0.08f, f);
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
            if (parentPos != transform.position || parentRot != transform.rotation)
            {
                parentPos = transform.position; parentRot = transform.rotation;
                UpdateFromPoint(beamFirstPoint.position + 0.5f * beamFirstPoint.forward - 0.8f * beamFirstPoint.up, -beamFirstPoint.up);
                updateCount = 0;
                return;
            }

            updateCount++;
            if (updateCount > 5 || skipTickLimit)
            {
                UpdateFromPoint(beamFirstPoint.position + 0.5f * beamFirstPoint.forward - 0.8f * beamFirstPoint.up, -beamFirstPoint.up);
                updateCount = 0;
                return;
            }

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
            RaycastHit rayHit;
            lastSensorUpdate.AddRange(sensorUpdate);
            sensorUpdate.Clear();
            BeamHitAnything = false;
            while (raycastCount < 80) // 20 is actually really small
            {
                raycastCount++;
                if (Physics.Raycast(lastPoint, lastDir, out rayHit, beamRayLength, layerMask))
                {
                    BeamLastHit = rayHit;
                    BeamHitAnything = true;
                    if (rayHit.transform.GetComponent<NewOpticsBlock>())
                    {
                        NewOpticsBlock ob = rayHit.transform.GetComponent<NewOpticsBlock>();
                        if (ob.OpticalMode != 0)
                        {
                            if (!sensorUpdate.Contains(ob))
                            {
                                sensorUpdate.Add(ob);
                                if (lastSensorUpdate.Contains(ob))
                                    lastSensorUpdate.Remove(ob);
                                ob.setLaserHit(this);
                            }
                            else if (sensorUpdate.Contains(ob) && ob.OpticalMode == 2) // might be bugging out with redirection stuff...
                            {
                                beamPoints.Add(rayHit.point);
                                beamDirections.Add(lastDir);
                                DrawBeam();
                                return; // also prevents infinite loops of laser -> redirection x inf from crashing stuff.
                            }
                        }

                        switch (ob.OpticalMode)
                        {
                            case 0: // glass
                            case 4: // sensor (doesn't filter stuff, only detects)
                                lastPoint = rayHit.point + lastDir * 0.1f;
                                beamPoints.Add(lastPoint);
                                beamDirections.Add(lastDir);
                                break;
                            case 1: // mirror
                                lastPoint = rayHit.point;
                                lastDir = Vector3.Reflect(lastDir, rayHit.normal);
                                beamPoints.Add(lastPoint);
                                beamDirections.Add(lastDir);
                                break;
                            case 2: // redirection cube
                                lastPoint = rayHit.transform.position + 0.5f * rayHit.transform.forward;
                                lastDir = rayHit.transform.up;
                                beamPoints.Add(lastPoint);
                                beamDirections.Add(lastDir);
                                break;
                            case 3:
                                if (ob.GetFilterMatch(colour)) // test for transparency
                                {
                                    lastPoint = rayHit.point + lastDir * 0.1f;
                                    beamPoints.Add(lastPoint);
                                    beamDirections.Add(lastDir);
                                    break;
                                }
                                else
                                {
                                    beamPoints.Add(rayHit.point);
                                    beamDirections.Add(lastDir);
                                    DrawBeam();
                                    return;
                                }
                            default:
                                beamPoints.Add(rayHit.point);
                                beamDirections.Add(lastDir);
                                DrawBeam();
                                return;
                        }
                    }
                    else
                    {
                        beamPoints.Add(rayHit.point);
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
            Debug.LogError("Raycast count exceeded limit! Shutting down LH loop.");
            raycastShutdown = true;
            raycastCount = 0;
        }
        /*public Vector3 ImprovReflect(Vector3 vec, Vector3 norm)
        {
            Vector3 v = vec.normalized;
            Vector3 n = norm.normalized;
            Vector3 v2 = Vector3.Cross(v, n);
            return Quaternion.AngleAxis(Vector3.Angle(v, n) * 2, v2)*v;
        }*/
        /*public void ForceUpdate ()
        {
            beamDirections.Clear();
            beamPoints.Clear();
            beamPoints.Add(beamFirstPoint.position + beamFirstPoint.forward);
            beamDirections.Add(beamFirstPoint.forward);
            Vector3 lastPoint = beamFirstPoint.position;//+beamFirstPoint.GetComponent<Rigidbody>().velocity*Time.deltaTime;
            Vector3 lastDir = beamFirstPoint.forward;
            RaycastHit rayHit;
            while (Physics.Raycast(lastPoint, lastDir, out rayHit, Mathf.Infinity, layerMask))
            {
                //beamTransforms.Add(rayHit.transform); beamPoints.Add(rayHit.point);
                if (rayHit.transform.GetComponent<OpticBlock>())
                {
                    OpticBlock ob = rayHit.transform.GetComponent<OpticBlock>();
                    //Debug.Log(ob+" "+ob.gameObject.name);
                    switch (ob.opticType)
                    {
                        case 0: // glass
                            lastPoint = rayHit.point + lastDir * 0.1f;
                            break;
                        case 1: // mirror
                            beamPoints.Add(rayHit.point);
                            beamDirections.Add(Vector3.Reflect(lastDir, rayHit.normal));
                            Debug.Log(rayHit.normal);
                            lastPoint = rayHit.point;
                            lastDir = beamDirections[beamDirections.Count - 1];
                            //Debug.Log(lastDir+" "+Vector3.Reflect(beamDirections[beamDirections.Count-2], rayHit.normal));
                            break;
                        case 2: // redirection cube
                            beamPoints.Add(rayHit.transform.position+0.5f*rayHit.transform.forward);
                            beamDirections.Add(rayHit.transform.up);
                            lastPoint = rayHit.point;
                            lastDir = beamDirections[beamDirections.Count - 1];
                            break;
                        case 3:
                        case 4:
                        case 5:
                        case 6:
                            if (ob.GetTransparent(colour)) // test for transparency
                            {
                                lastPoint = rayHit.point + lastDir * 0.1f;
                                break;
                            }
                            else
                            {
                                beamPoints.Add(rayHit.point);
                                beamDirections.Add(lastDir);
                                DrawBeam();
                                return;
                            }
                    }
                } else
                {
                    beamPoints.Add(rayHit.point);
                    beamDirections.Add(lastDir);
                    DrawBeam();
                    return;
                }
            }
        }*/
        private void DrawBeam()
        {
            if (!onOff)
            {
                if (lLength != 0)
                {
                    lLength = 0;
                    lr.SetVertexCount(0);
                }
                if (sensorUpdate.Count != 0)
                {
                    foreach (NewOpticsBlock ob in sensorUpdate)
                    {
                        ob.unsetLaserHit(this);
                    }
                    sensorUpdate.Clear();
                }
                if (lastSensorUpdate.Count != 0) lastSensorUpdate.Clear();
                return;
            }
            // put a little bit of cleanup here
            List<NewOpticsBlock> sensorClear = new List<NewOpticsBlock>();
            foreach (NewOpticsBlock ob in lastSensorUpdate)
            {
                ob.unsetLaserHit(this);
                sensorClear.Add(ob);
            }
            foreach (NewOpticsBlock ob in sensorClear)
            {
                if (!lastSensorUpdate.Contains(ob))
                {
                    Debug.LogError("sensorClear contains ob not in lastSensorUpdate!");
                    continue;
                }
                lastSensorUpdate.Remove(ob);
            }
            sensorClear.Clear();


            lLength = beamPoints.Count;
            lr.SetVertexCount(lLength);
            for (int i = 0; i < lLength; i++)
            {
                lr.SetPosition(i, beamPoints[i]);
            }
            BeamLastPoint = beamPoints[beamPoints.Count - 1];
            //Debug.Log(beamPoints[beamPoints.Count - 1]);
            //Debug.Log("sensorUpdate: " + sensorUpdate.Count +
            //    "\nbeamPoints: " + beamPoints.Count + " | beamDirections: " + beamDirections.Count);
            //Debug.Log(raycastCount);
            raycastCount = 0;
        }
    }
    /*public class LHDHandler
    {
        private List<LHData> DataPool;
        public void Add(Transform t1, Transform t2, Vector3 f, Vector3 t, float w, LHData.RecastDelegate c)
        {
            GameObject GO = new GameObject();
            LHData lhData = GO.AddComponent<LHData>(); lhData.SetupInit(t1, t2, f, t, w, c);
            // continue from here later
        }
        public void Finalize(LHData lhd)
        {
            if (DataPool.Contains(lhd))
            {
                DataPool.Remove(lhd);
            }
        }
    }*/
    /*public class LHData : MonoBehaviour
    {
        public Transform FromTransform;
        public Transform ToTransform;
        public BoxCollider Trigger;
        public Vector3 from;
        public Vector3 to;
        public delegate void RecastDelegate(Vector3 from, Vector3 dir);

        public RecastDelegate recastDelegate;
        public static LHData Create()
        {
            GameObject GO = new GameObject();
            GO.AddComponent<DestroyIfEditMode>();
            return GO.AddComponent<LHData>();
        }
        public void SetupInit(Transform t1, Transform t2, Vector3 f, Vector3 t, RecastDelegate callback)
        {
            FromTransform = t1;
            ToTransform = t2;
            from = f; to = t;
            recastDelegate = callback;
            Trigger = gameObject.AddComponent<BoxCollider>();
            Trigger.center = Vector3.zero;
            Trigger.size = new Vector3(0.1f, 0.1f, Vector3.Distance(f, t));
            Trigger.isTrigger = true;
            transform.position = f + 0.5f * (t - f);
            transform.rotation.SetFromToRotation(Vector3.forward, (t - f).normalized);
        }
        /*public void Reset(Transform t1, Transform t2, Vector3 f, Vector3 t, RecastDelegate callback)
        {
            FromTransform = t1;
            ToTransform = t2;
            from = f; to = t;
            recastDelegate = callback;
            Trigger.size = new Vector3(0.1f, 0.1f, Vector3.Distance(f, t));
            transform.position = f + 0.5f * (t - f);
            transform.rotation.SetFromToRotation(Vector3.forward, (t - f).normalized);
        }
        void OnTriggerEnter(Collider other)
        {
            if (!(other.transform == FromTransform || other.transform == ToTransform))
            {
                if (other.gameObject.GetComponent<OpticBlock>() &&
                    other.gameObject.GetComponent<OpticBlock>().opticType == 0) return; // ignore glass blocks
                else if (other.isTrigger) return;
                else if (other.gameObject.layer == 20 || other.gameObject.layer == 2) return; // ignore things in layers 20 & 2
                recastDelegate(from, (to - from).normalized);
            }
        }
        void OnTriggerExit(Collider other)
        {
            if (other.transform == FromTransform) recastDelegate(from, Vector3.zero);
            else if (other.transform == ToTransform) recastDelegate(from, (to - from).normalized);
        }
        void OnDestroy()
        {
            FromTransform = null;
            ToTransform = null;
            recastDelegate = null;
        }
    }*/
}
