using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RimWorld;
using RimWorld.SquadAI;
using Verse;
using Verse.AI;

namespace Vehicle
{
    public class RaidStrategyWorker_StageThenAttackWithVehicle : RaidStrategyWorker
    {
        public override StateGraph MakeBrainGraph(ref IncidentParms parms)
        {
            IntVec3 siegePositionFrom = FindSiegePositionFrom(parms.spawnCenter);
            return GraphMaker.StageThenAttackGraph(parms.faction, siegePositionFrom);
        }

        public override bool CanUseWith(IncidentParms parms)
        {
            if (!base.CanUseWith(parms))
                return false;
            return parms.faction.def.canStageAttacks;
        }

        private bool TryFindSiegePosition(IntVec3 entrySpot, float minDistToColony, out IntVec3 result)
        {
            CellRect cellRect = CellRect.CenteredOn(entrySpot, 60);
            cellRect.ClipInsideMap();
            cellRect = cellRect.ContractedBy(14);
            List<IntVec3> list = new List<IntVec3>();
            foreach (Pawn pawn in Find.ListerPawns.FreeColonists)
                list.Add(pawn.Position);
            using (HashSet<Building>.Enumerator enumerator = Find.ListerBuildings.allBuildingsColonistCombatTargets.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Building current = enumerator.Current;
                    list.Add(current.Position);
                }
            }
            float num1 = minDistToColony * minDistToColony;
            int num2 = 0;
            IntVec3 randomCell;
            bool flag;
            do
            {
                do
                {
                    ++num2;
                    if (num2 <= 200)
                        randomCell = cellRect.RandomCell;
                    else
                        goto label_21;
                }
                while (!GenGrid.Standable(randomCell) || !GenGrid.SupportsStructureType(randomCell, TerrainAffordance.Heavy) || (!GenGrid.SupportsStructureType(randomCell, TerrainAffordance.Light) || !Reachability.CanReach(randomCell, (TargetInfo)entrySpot, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Some)) || !Reachability.CanReachColony(randomCell));
                flag = false;
                for (int index = 0; index < list.Count; ++index)
                {
                    if ((double)(list[index] - randomCell).LengthHorizontalSquared < (double)num1)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            while (flag);
            result = randomCell;
            return true;
        label_21:
            result = IntVec3.Invalid;
            return false;
        }

        private IntVec3 FindSiegePositionFrom(IntVec3 entrySpot)
        {
            int num = 70;
            while (num >= 20)
            {
                IntVec3 result;
                if (TryFindSiegePosition(entrySpot, (float)num, out result))
                    return result;
                num -= 10;
            }
            Log.Error("Could not find siege spot from " + entrySpot + ", using " + entrySpot);
            return entrySpot;
        }
    }

}
