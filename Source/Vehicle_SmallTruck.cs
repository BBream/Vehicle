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
    public class Vehicle_SmallTruck : Vehicle
    {
        private bool isStorageFull;
        public Zone_Stockpile deliveryZone1;
        public Zone_Stockpile deliveryZone2;

        public override void MountOn(Pawn pawn)
        {
            base.MountOn(pawn);
            //if (pawn.Faction == Faction.OfColony)
            //    this.drafter.Drafted = true;
        }

        public override void CheckCallDriver()
        {
            if (this.IsMounted)
                callDriver = false;

            isStorageFull = crewContainer.Count > this.maxNumOfCrews ? true : false;
            if (isStorageFull && deliveryZone2 != null && !deliveryZone2.ContainsCell(this.Position))
                callDriver = true;
            else if (crewContainer.Count <= 0 && deliveryZone1 != null && !deliveryZone1.ContainsCell(this.Position))
                callDriver = true;
            else
                callDriver = false;
        }

        public override void SpawnSetup()
        {
            base.SpawnSetup();

            deliveryZone1 = null;
            deliveryZone2 = null;

        }

        public override void DrawAt(Vector3 drawLoc)
        {
            base.DrawAt(drawLoc);

            //Show inner truck
            if (!Find.Selector.IsSelected(this))
            {
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var baseGizmo in base.GetGizmos())
                yield return baseGizmo;

            Designator_DeliveryZone designator1 = new Designator_DeliveryZone();

            designator1.vehicle = this;
            designator1.stockpileNum = 0;
            designator1.defaultLabel = "Set delivery zone1";
            designator1.defaultDesc = "Set delivery zone1";
            designator1.icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone");
            designator1.activateSound = SoundDef.Named("Click");

            yield return designator1;

            Designator_DeliveryZone designator2 = new Designator_DeliveryZone();

            designator2.vehicle = this;
            designator2.stockpileNum = 1;
            designator2.defaultLabel = "Set delivery zone2";
            designator2.defaultDesc = "Set delivery zone2";
            designator2.icon = ContentFinder<Texture2D>.Get("UI/Commands/RenameZone");
            designator2.activateSound = SoundDef.Named("Click");

            yield return designator2;

            Command_Action gizmo = new Command_Action();

            if (this.IsMounted)
            {
                gizmo.defaultLabel = "Transfer";
                //gizmo.icon = ContentFinder<Texture2D>.Get("UI/Commands/IconDismount");
                gizmo.activateSound = SoundDef.Named("Click");
                gizmo.defaultDesc = "Transfer";
                gizmo.action = () => 
                { 
                    IntVec3 dest = IntVec3.Invalid;
                    if (this.Position.GetZone() == this.deliveryZone1)
                        dest = deliveryZone2.Cells.First();
                    else if (this.Position.GetZone() == this.deliveryZone2)
                        dest = deliveryZone1.Cells.First();
                    else
                        dest = deliveryZone1.Cells.First();
                    Job jobNew = new Job(JobDefOf.Goto, dest);
                    this.drafter.TakeOrderedJob(jobNew);
                };

                yield return gizmo;
            }
        }
    }
}