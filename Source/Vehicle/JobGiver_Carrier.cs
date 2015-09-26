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
    public class JobGiver_Carrier : ThinkNode_JobGiver
    {
        bool flagInProgressDrop = false;
        int maxItem = 5;
        public int GetMaxStackCount {get {return maxItem*100;} }

        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            Thing closestHaulable;
            Job jobDropInCell = new Job(DefDatabase<JobDef>.GetNamed("DropInCell"));
            Job jobCollectThing = new Job(DefDatabase<JobDef>.GetNamed("CollectThing"));
            jobCollectThing.maxNumToCarry = 99999;
            jobCollectThing.haulMode = HaulMode.ToCellStorage;

            //Load inventory setting
            if (pawn is Pawn_Cargo)
            {
                Pawn_Cargo cargo = pawn as Pawn_Cargo;
            }

            //collectThing Predicate
            Predicate<Thing> predicate = (Thing t) =>
                (!t.IsForbidden(pawn.Faction) && !t.IsInValidStorage() &&pawn.CanReserve(t, ReservationType.Total));

            //Collect thing
            if (pawn.inventory.container.TotalStackCount < GetMaxStackCount && pawn.inventory.container.Contents.Count() < maxItem
                && !ListerHaulables.Haulables().NullOrEmpty() && flagInProgressDrop == false)
            {
                closestHaulable = GenClosest.ClosestThing_Global_Reachable(pawn.Position,
                                                                ListerHaulables.Haulables(),
                                                                PathMode.ClosestTouch,
                                                                TraverseParms.For(pawn, Danger.Deadly, false),
                                                                9999,
                                                                predicate);
                if (closestHaulable == null)
                {
                    flagInProgressDrop = true;
                    return null;
                }
                jobCollectThing.targetA = closestHaulable;
                return jobCollectThing;
            }
            else
            {
                //Drop in cell
                flagInProgressDrop = true;
                if (pawn.inventory.container.Contents.Count() <= 0)
                {
                    //No more thing to drop
                    flagInProgressDrop = false;
                    return null;
                }

                //Find cell to drop thing
                foreach (Zone zone in Find.ZoneManager.AllZones)
                    if (zone is Zone_Stockpile)
                        foreach (var zoneCell in zone.cells)
                        {
                            Thing dropThing = pawn.inventory.container.Contents.Last();

                            if (zoneCell.IsValidStorageFor(dropThing) && pawn.CanReserve(zoneCell, ReservationType.Store))
                            {
                                jobDropInCell.targetA = dropThing;
                                jobDropInCell.targetB = zoneCell;
                                return jobDropInCell;
                            }
                        }

                //No zone for stock
                return null;
            }
        }
    }
}
