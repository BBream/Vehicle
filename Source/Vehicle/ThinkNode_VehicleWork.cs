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
    class ThinkNode_VehicleWork : ThinkNode
    {
        public override ThinkResult TryIssueJobPackage(Pawn pawn)
        {
            Vehicle vehicle = pawn as Vehicle;
            if (vehicle == null)
                return ThinkResult.NoJob;
            //Note: It is not surly work for multiple jobGiver
            foreach (JobGiverDef jobGiverDef in vehicle.vehicleDef.vehicle.jobGiverDefs.OrderBy(x=> {return x.priorityInOrder;}))
            {
                return jobGiverDef.JobGiver.TryIssueJobPackage(pawn);
            }
            return ThinkResult.NoJob;
        }
    }
}
