using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using spaar.ModLoader;
using System.Collections;

// optimisations come next update, not now
/*namespace AdvancedLaserBlock
{
    public class RaycastHandler : SingleInstance<RaycastHandler>
    {
        public override string Name
        {
            get
            {
                return "RaycastHandler";
            }
        }
        
        public delegate void RHResult(RayResult rr);
        //public delegate RaycastRequest RHUrgent();
        private TimedList<RaycastRequest> requestStack; // request, request lifetime
        //private List<RaycastRequest> urgentStack;
        private float delta;
        private int updatesPassed;
        private int raycastsThisUpdate;
        private bool isSimulating;

        // optimisation
        private bool isOptionsSetup;
        private float maxRayDist;
        private int requestLifetime;
        private int requestPortion;
        private bool raycastOverInterval;
        private bool limitCastsPerUpdate;
        private int castLimit;

        private LayerMask layerMask = ~((1 << 20) + (1 << 2));
        public void Start ()
        {
            delta = 0f;
            updatesPassed = 0;
            raycastsThisUpdate = 0;
            isSimulating = false;
            isOptionsSetup = false;
            Game.OnSimulationToggle += OnSimulationToggle;
        }
        public void DoOptionSetup (float rayDist, int reqTime, int reqPortion, bool smooth, bool doLimit, int limit)
        {
            maxRayDist = rayDist; requestLifetime = reqTime; requestPortion = reqPortion; raycastOverInterval = smooth;
            limitCastsPerUpdate = doLimit; castLimit = limit;
            requestStack = new TimedList<RaycastRequest>();

            isOptionsSetup = true;
        }
        private void OnSimulationToggle(bool simulating)
        {
            delta = 0f;
            updatesPassed = 0;
            raycastsThisUpdate = 0;
            isSimulating = simulating;
        }

        public void FixedUpdate ()
        {
            if (isSimulating)
            {
                if (limitCastsPerUpdate && raycastsThisUpdate >= castLimit)
                {

                }
            }
        }
        private void doRequests (List<RaycastRequest> rayRequests)
        {
            foreach (RaycastRequest rq in rayRequests)
            {
                raycastFromPoint(rq, true);
                raycastsThisUpdate++;
            }
            rayRequests.Clear();
        }
        private void doRequests (TimedList<RaycastRequest> rayRequests)
        {
            List<RaycastRequest> reqTiered = new List<RaycastRequest>();
            reqTiered = rayRequests.GetRange(0f, requestLifetime);
            
        }
        private RayResult raycastFromPoint (RaycastRequest rq, bool doCallback)
        {
            RaycastHit rH;
            if (Physics.Raycast(rq.pos, rq.dir, out rH, rq.length, layerMask))
            {
                if (doCallback) rq.callback(new RayResult(true, rH));
                return new RayResult(true, rH);
            } else
            {
                if (doCallback) rq.callback(new RayResult());
                return new RayResult();
            }

        }
        private Vector3 getCorrTorque(Vector3 from, Vector3 to, Rigidbody rb, float scale)
        {
            Vector3 x = Vector3.Cross(from.normalized, to.normalized);                // axis of rotation
            float theta = Mathf.Asin(x.magnitude);                                    // angle between from & to
            Vector3 w = x.normalized * theta / (Time.fixedDeltaTime * scale);         // scaled angular acceleration
            Vector3 w2 = w - rb.angularVelocity;                                      // need to slow down at a point
            Quaternion q = rb.rotation * rb.inertiaTensorRotation;                    // transform inertia tensor
            return q * Vector3.Scale(rb.inertiaTensor, (Quaternion.Inverse(q) * w2)); // calculate final torque
        }
        private Vector3 UpdateAngularVelocity(Rigidbody rb, Vector3 to)
        {
            Quaternion a = new Quaternion(); a.SetFromToRotation(Vector3.forward, to);
            Transform from = rb.transform;
            Vector3 z = Vector3.Cross(from.forward, a * Vector3.forward);
            Vector3 y = Vector3.Cross(from.up, a * Vector3.up);
            float thetaZ = Mathf.Asin(z.magnitude);
            float thetaY = Mathf.Asin(y.magnitude);
            float dt = Time.fixedDeltaTime;
            Vector3 wZ = z.normalized * (thetaZ / dt);
            Vector3 wY = y.normalized * (thetaY / dt);
            Quaternion q = from.rotation * rb.inertiaTensorRotation;
            Vector3 T = q * Vector3.Scale(rb.inertiaTensor,
                Quaternion.Inverse(q) * (wZ + wY - rb.angularVelocity));
            return T;
        }
    }
    public struct RayResult
    {
        public bool hit;
        public RaycastHit rH;
        public RayResult (bool b, RaycastHit r)
        {
            hit = b;
            rH = r;
        }
    }
    public struct RaycastRequest
    {
        
        public Vector3 pos;
        public Vector3 dir;
        public float length;
        public RaycastHandler.RHResult callback;

        public RaycastRequest (Vector3 p, Vector3 d, float f, RaycastHandler.RHResult r)
        {
            pos = p;
            dir = d;
            length = f;
            callback = r;
        }
        public static bool Compare (RaycastRequest r1, RaycastRequest r2)
        {
            return (r1.pos == r2.pos &&
                r1.dir == r2.dir &&
                r1.length == r2.length &&
                r1.callback.GetInvocationList() == r2.callback.GetInvocationList());
        }
    }
    public class TimedList<T>
    {
        private Dictionary<float, List<T>> timedDict;
        public bool DoCutoff;
        public float Cutoff;
        public Dictionary<float, List<T>> TimedDict { get { return timedDict; } }
        public TimedList ()
        {
            DoCutoff = false;
            Cutoff = 5f;
            timedDict = new Dictionary<float, List<T>>();
        }
        public void SetCutoff (bool onOff, float lifetime)
        {
            Cutoff = lifetime;
            DoCutoff = onOff;
        }
        public void Tick (float deltaTime)
        {
            Dictionary<float, List<T>> newDict = new Dictionary<float, List<T>>();
            foreach (KeyValuePair<float, List<T>> kv in timedDict)
            {
                if (DoCutoff && (kv.Key + deltaTime > Cutoff)) continue;
                newDict.Add(kv.Key + deltaTime, kv.Value);
            }
            timedDict = newDict;
        }
        public void Add (T t, float deltaTime)
        {
            if (DoCutoff && deltaTime > Cutoff) return;
            if (timedDict.ContainsKey(deltaTime)) timedDict[deltaTime].Add(t);
            else timedDict.Add(deltaTime, new List<T>() { t });
        }
        public void Add (T t)
        {
            Add(t, 0f);
        }
        public void AddRange (KeyValuePair<float, List<T>> kv)
        {
            if (kv.Value == null || kv.Value.Count <= 0) return;
            if (timedDict.ContainsKey(kv.Key)) timedDict[kv.Key].AddRange(kv.Value);
            else timedDict.Add(kv.Key, kv.Value);
        }
        public List<T> GetRange (float f1, float f2)
        {
            List<T> result = new List<T>();
            foreach(KeyValuePair<float, List<T>> kv in timedDict)
            {
                if (kv.Key < f1 || kv.Key > f2) continue;
                result.AddRange(kv.Value);
            }
            return result;
        }
        public TimedList<T> GetTimedRange (float f1, float f2)
        {
            TimedList<T> result = new TimedList<T>();
            foreach (KeyValuePair<float, List<T>> kv in timedDict)
            {
                if (kv.Key < f1 || kv.Key > f2) continue;
                result.AddRange(kv);
            }
            return result;
        }
    }
}*/
