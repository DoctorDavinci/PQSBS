using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PQSBlackSpell
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class BlackSpell : MonoBehaviour
    {
        int reset = 111;
        int stage = 0;
        const int vesLoad = 13;
        IEnumerator<Vessel> vesEnume;
        Vessel tvel;
        //float thrattle;
        bool loading = true;
        bool crashDamage;
        bool joints;
        double gravMult;
        //Quaternion sq;
        //Vector3 sp;

        void CastSpell()
        {
            var pqs = FlightGlobals.currentMainBody.pqsController;
            pqs.horizonDistance = 300000;
            pqs.maxDetailDistance = 300000;
            pqs.minDetailDistance = 300000;
            pqs.visRadSeaLevelValue = 200;
            pqs.collapseSeaLevelValue = 200;
            /*
            if (loading && stage == 1)
            {
                //pqs.SetTarget(tvel.transform);
                pqs.GetAltitude(tvel.CoM);
                //Vector3 pos = sp - FlightGlobals.ActiveVessel.transform.position + tvel.transform.position;
                //pqs.secondaryTarget.parent.SetPositionAndRotation(pos, Quaternion.FromToRotation(Vector3.up, pos));
            }
            */
        }
        void FixedUpdate() => CastSpell();
        void LateUpdate() => CastSpell();
        void Update()
        {
            CastSpell();
            if (loading)
            {
                //Debug.Log($"reset: {reset}, stage: {stage}, grav: {PhysicsGlobals.GraviticForceMultiplier}, pos: {FlightGlobals.currentMainBody.pqsController.target.position}");
                if (!FlightGlobals.currentMainBody.pqsController.isBuildingMaps)
                    --reset;
                if (reset <= 0)
                {
                    reset = vesLoad;
                    switch (stage)
                    {
                        case 0:
                            vesEnume = FlightGlobals.VesselsLoaded.ToList().GetEnumerator();
                            tvel = FlightGlobals.ActiveVessel;
                            //thrattle = FlightGlobals.ActiveVessel.ctrlState.mainThrottle;
                            //sq = FlightGlobals.currentMainBody.pqsController.secondaryTarget.parent.rotation;
                            //sp = FlightGlobals.currentMainBody.pqsController.secondaryTarget.parent.position;
                            ++stage;
                            break;
                        case 1:
                            if (vesEnume.Current != null)
                                vesEnume.Current.OnFlyByWire -= thratlarasat;
                            if (vesEnume.MoveNext())// && sortaLanded(vesEnume.Current))
                            {
                                if (sortaLanded(vesEnume.Current))
                                    FlightGlobals.ForceSetActiveVessel(vesEnume.Current);
                                vesEnume.Current.OnFlyByWire += thratlarasat;
                                //    tvel = vesEnume.Current;
                            }
                            else
                            {
                                vesEnume.Dispose();
                                ++stage;
                                FlightGlobals.ForceSetActiveVessel(tvel);
                                //tvel = FlightGlobals.ActiveVessel;
                                //FlightGlobals.currentMainBody.pqsController.secondaryTarget.parent.SetPositionAndRotation(sp, sq);
                            }
                            Debug.LogError($"Black Spell entangling {vesEnume.Current?.vesselName}");
                            break;
                        case 2:
                            Debug.LogError("Black Spell condensing");
                            ++stage;
                            break;
                        case 3:
                            Debug.LogError("Black Spell releasing energies");
                            reset = 100;
                            ++stage;
                            break;
                        case 4:
                            CheatOptions.NoCrashDamage = crashDamage;
                            CheatOptions.UnbreakableJoints = joints;
                            PhysicsGlobals.GraviticForceMultiplier = gravMult;
                            //FlightGlobals.ActiveVessel.ctrlState.mainThrottle = thrattle;
                            loading = false;
                            Debug.LogError("Black Spell complete");
                            break;
                    }
                }
                if (stage == 4)
                    PhysicsGlobals.GraviticForceMultiplier = gravMult * (1 - reset / 100);
                else
                    using (var v = FlightGlobals.VesselsLoaded.GetEnumerator())
                        while (v.MoveNext())
                            if (sortaLanded(v.Current))
                                v.Current.SetWorldVelocity(Vector3d.zero);
            }
        }
        void Awake()
        {
            Debug.LogError("Black Spell channeling");
            crashDamage = CheatOptions.NoCrashDamage;
            joints = CheatOptions.UnbreakableJoints;
            gravMult = PhysicsGlobals.GraviticForceMultiplier;
            CheatOptions.NoCrashDamage = true;
            CheatOptions.UnbreakableJoints = true;
            PhysicsGlobals.GraviticForceMultiplier = 0.000001;
        }
        bool sortaLanded(Vessel v) => v.mainBody.GetAltitude(v.CoM) - Math.Max(v.terrainAltitude, 0) < 100;
        void thratlarasat(FlightCtrlState s)
        {
            s.wheelThrottle = 0;
            s.mainThrottle = 0;
        }
    }
}
