using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace Vehicle
{
    public class CompRaiderPrepare : ThingComp
    {
        public override void PostSpawnSetup()
        {
            base.PostSpawnSetup();
            Log.Message(parent.LabelCap + "check raider's prepare");
            if (parent is Vehicle && parent.Faction.HostileTo(Faction.OfColony))
            {
                Vehicle vehicle = parent as Vehicle;

                Log.Message(parent.LabelCap + "try add raider's driver and crew");
                PawnKindDef driverKindDef = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.defaultFactionType == parent.Faction.def && x.defName.Contains("Driver")).RandomElement();
                if (driverKindDef != null)
                {
                    Log.Message(parent.LabelCap + "try add raider's driver");
                    Pawn newPawn = PawnGenerator.GeneratePawn(driverKindDef, parent.Faction);
                    vehicle.MountOn(newPawn);
                }

                PawnKindDef crewKindDef = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.defaultFactionType == parent.Faction.def).RandomElement();
                if (crewKindDef != null)
                {
                    Log.Message(parent.LabelCap + "try add raider's crew");
                    Pawn newPawn = PawnGenerator.GeneratePawn(crewKindDef, parent.Faction);
                    vehicle.BoardOn(newPawn);
                }
            }
        }
    }
}
