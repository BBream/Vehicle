using RimWorld;
using Verse;

namespace Vehicle
{
    public class ThingWithCompsSpawnVehicle : ThingWithComps
    {
        public override void SpawnSetup()
        {
            base.SpawnSetup();
            ThingWithCompsSpawnVehicleDef thingDef = (ThingWithCompsSpawnVehicleDef)def;
            if (thingDef != null && thingDef.spawnPawnDef != null)
            {
                Pawn newPawn = PawnGenerator.GeneratePawn(thingDef.spawnPawnDef, Faction.OfColony);
                newPawn.ageTracker.AgeBiologicalTicks = 0;
                IntVec3 spawnPos = this.Position;
                foreach (IntVec3 pos in GenAdj.CellsAdjacent8WayAndInside(this))
                    if (pos.Standable())
                    {
                        spawnPos = pos;
                        break;
                    }
                GenSpawn.Spawn(newPawn, spawnPos);
            }
            else
                Log.Error(this.LabelCap + " has null spawnPawnDef.");
            Destroy();
        }
    }
}
