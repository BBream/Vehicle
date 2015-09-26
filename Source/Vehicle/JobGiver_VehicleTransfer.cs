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
    class JobGiver_VehicleTransfer : ThinkNode_JobGiver
    {
        protected override Job TryGiveTerminalJob(Pawn pawn)
        {
            Vehicle vehicle = pawn as Vehicle;
            if (vehicle == null)
                return null;

            Job jobNew = new Job();
            return jobNew;
        }
    }
}
