using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using Verse;
using Verse.AI;

namespace Vehicle
{
    public class WorkGiver_Drive : WorkGiver_Scanner
    {
        public WorkGiver_Drive() : base() { ;}
        /*
        public virtual PathEndMode PathEndMode { get; }
        public virtual ThingRequest PotentialWorkThingRequest { get; }

        public virtual bool HasJobOnCell(Pawn pawn, IntVec3 c);
        public virtual bool HasJobOnThing(Pawn pawn, Thing t);
        public virtual Job JobOnCell(Pawn pawn, IntVec3 cell);
        public virtual Job JobOnThing(Pawn pawn, Thing t);
        public PawnActivityDef MissingRequiredActivity(Pawn pawn);
        public virtual IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn);
        public virtual IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn Pawn);
        public virtual bool ShouldSkip(Pawn pawn);
         */

        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            /*Vehicle_SmallTruck smallTruck = pawn as Vehicle_SmallTruck;
            if (smallTruck != null)
                if (smallTruck.deliveryZone1.ContainsCell(smallTruck.Position))
                    return smallTruck.deliveryZone2.Cells;
                else if (smallTruck.deliveryZone2.ContainsCell(smallTruck.Position))
                    return smallTruck.deliveryZone1.Cells;
                else
                    return smallTruck.deliveryZone1.Cells;
            else
                return null;*/
            return null;
        }

        public override bool HasJobOnCell(Pawn pawn, IntVec3 c)
        {
            return false;
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 cell)
        {
            return null;
        }
    }

}