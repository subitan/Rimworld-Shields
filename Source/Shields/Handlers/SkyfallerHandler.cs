﻿using System;
using FrontierDevelopments.General;
using Harmony;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace FrontierDevelopments.Shields.Handlers
{
    public class SkyfallerHandler
    {
        static SkyfallerHandler()
        {
            Log.Message("Frontier Developments Shields :: Skyfaller handler enabled");
        }

        [HarmonyPatch(typeof(Skyfaller), "Impact")]
        static class Patch_ProjectileCE_Impact
        {
            static bool Prefix(Skyfaller __instance)
            {
                try
                {
                    var skyfaller = __instance;
                    // ignore incoming drop pods to let the ActiveDropPod handler take care of it
                    if (skyfaller.GetType() == typeof(DropPodIncoming)) return true;
                    return !Mod.ShieldManager.ImpactShield(skyfaller.Map, Common.ToVector3WithY(skyfaller.Position, 0), (shield, point) =>
                    {
                        if (shield.IsActive())
                        {
                            if(shield.Damage(Mod.Settings.SkyfallerDamage, point))
                            {
                                skyfaller.def.skyfaller.impactSound?.PlayOneShot(
                                    SoundInfo.InMap(new TargetInfo(skyfaller.Position, skyfaller.Map)));
                                skyfaller.Destroy();
                                Messages.Message("fd.shields.incident.skyfaller.blocked.body".Translate(), new GlobalTargetInfo(skyfaller.Position, skyfaller.Map), MessageTypeDefOf.NeutralEvent);
                                return true;
                            }
                            else
                            {
                                Messages.Message("fd.shields.incident.skyfaller.not_blocked.body".Translate(), new GlobalTargetInfo(skyfaller.Position, skyfaller.Map), MessageTypeDefOf.NegativeEvent);
                            }
                        }
                        return false;
                    });
                }
                catch (InvalidOperationException) {}
                return true;
            }
        }
    }
}
