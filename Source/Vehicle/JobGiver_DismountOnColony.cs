﻿using System;
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
    public class JobGiver_DismountOnColony : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            if (pawn is Vehicle)
            {
                Vehicle vehicle = pawn as Vehicle;
                if (vehicle.IsMounted)
                {
                    //Not implemented collectly
                    //IntVec3 dismountCell = Find.HomeRegionGrid.ActiveCells.RandomElement<IntVec3>();
                    IntVec3 dismountCell = vehicle.Position;
                    dismountCell.x += 1;
                    //if (dismountCell == null)
                    //    dismountCell = vehicle.Position;
                    Job jobDismount = new Job(DefDatabase<JobDef>.GetNamed("DismountOnCell"), dismountCell);

                    return jobDismount;
                }
                else
                    return null;
            }
            else
                return null;
        }
    }
}