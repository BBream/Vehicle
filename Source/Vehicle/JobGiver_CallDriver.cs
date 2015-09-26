using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;         // Always neededJobGiver_Test
//using VerseBase;         // Material/Graphics handling functions are found here
using Verse;               // RimWorld universal objects are here (like 'Building')
using Verse.AI;          // Needed when you do something with the AI
//using Verse.Sound;       // Needed when you do something with Sound
//using Verse.Noise;       // Needed when you do something with Noises
using RimWorld;            // RimWorld specific functions are found here (like 'Building_Battery')
//using RimWorld.Planet;   // RimWorld specific functions for world creation
//using RimWorld.SquadAI;  // RimWorld specific functions for squad brains 

namespace Vehicle
{
    public class JobGiver_CallDriver : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (pawn is Vehicle)
            {
                Predicate<Thing> predicate = driver
                => driver.Faction == pawn.Faction && driver.SpawnedInWorld && driver.def.race.Humanlike
                                        && pawn.CanReserveAndReach(driver, PathEndMode.Touch, Danger.Deadly);
                Thing calledDriver = GenClosest.ClosestThing_Global(pawn.Position, Find.ListerPawns.AllPawns, 99999, predicate);

                if (calledDriver != null)
                {
                    Job jobCallDriver = new Job(DefDatabase<JobDef>.GetNamed("CallDriver"), calledDriver);
                    return jobCallDriver;
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}