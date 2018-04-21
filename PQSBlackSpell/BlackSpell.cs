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
        bool loading = true;
        bool crashDamage;
        bool joints;

        void CastSpell()
        {
            var pqs = FlightGlobals.currentMainBody.pqsController;
            pqs.horizonDistance = 300000;
            pqs.maxDetailDistance = 300000;
            pqs.minDetailDistance = 300000;
            pqs.visRadSeaLevelValue = 200;
            pqs.collapseSeaLevelValue = 200;

            if (loading)
                using (var v = FlightGlobals.VesselsLoaded.GetEnumerator())
                    while (v.MoveNext())
                        if (isEligibleVessel(v.Current))
                            switch(stage)
                            {
                                case 0:
                                    v.Current.SetWorldVelocity(v.Current.gravityForPos * -4 * Time.fixedDeltaTime);
                                    break;
                                case 1:
                                    v.Current.SetWorldVelocity(v.Current.gravityForPos * -2 * Time.fixedDeltaTime);
                                    break;
                                case 4:
                                    v.Current.SetWorldVelocity(v.Current.velocityD / 2);
                                    break;
                                default:
                                    v.Current.SetWorldVelocity(Vector3d.zero);
                                    break;
                            }

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
                            ++stage;
                            break;
                        case 1:
                            if (vesEnume.Current != null)
                                vesEnume.Current.OnFlyByWire -= thratlarasat;
                            if (vesEnume.MoveNext())
                            {
                                if (isEligibleVessel(vesEnume.Current))
                                    FlightGlobals.ForceSetActiveVessel(vesEnume.Current);
                                vesEnume.Current.OnFlyByWire += thratlarasat;
                            }
                            else
                            {
                                vesEnume.Dispose();
                                ++stage;
                                FlightGlobals.ForceSetActiveVessel(tvel);
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
                            loading = false;
                            Debug.LogError("Black Spell complete");
                            break;
                    }
                }
            }
        }

        void Awake()
        {
            Debug.LogError("Black Spell channeling");
            crashDamage = CheatOptions.NoCrashDamage;
            joints = CheatOptions.UnbreakableJoints;
            CheatOptions.NoCrashDamage = true;
            CheatOptions.UnbreakableJoints = true;
        }

        bool isEligibleVessel(Vessel v)
        {
            if (v.rootPart.Modules.Contains("ModuleEnemyMine_Naval")) return false;
            return v.mainBody.GetAltitude(v.CoM) - Math.Max(v.terrainAltitude, 0) < 100;
        }

        void thratlarasat(FlightCtrlState s)
        {
            s.wheelThrottle = 0;
            s.mainThrottle = 0;
        }
    }
}
