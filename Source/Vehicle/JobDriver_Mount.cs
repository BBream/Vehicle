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
    public class JobDriver_Mount : JobDriver
    {
        //Constants
        private const TargetIndex MountableInd = TargetIndex.A;
        private const TargetIndex MountCellInd = TargetIndex.B;

        public JobDriver_Mount() : base() { }

        public override string GetReport()
        {
            Vehicle vehicle = TargetThingA as Vehicle;

            string repString;
            repString = "ReportMounting".Translate(vehicle.LabelCap);

            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ///
            //Set fail conditions
            ///

            this.FailOnBurningImmobile(MountCellInd);
            this.FailOnDestroyed(MountableInd);
            //Note we only fail on forbidden if the target doesn't start that way
            //This helps haul-aside jobs on forbidden items
            if (!TargetThingA.IsForbidden(pawn.Faction))
                this.FailOnForbidden(MountableInd);



            ///
            //Define Toil
            ///




            ///
            //Toils Start
            ///

            yield return Toils_Goto.GotoCell(MountCellInd, PathEndMode.ClosestTouch);

            Toil toilMountOn = new Toil();
            toilMountOn.initAction = () =>
            {
                Pawn actor = toilMountOn.actor;
                Vehicle vehicle = TargetThingA as Vehicle;
                vehicle.MountOn(actor);
            };

            yield return toilMountOn;
        }

    }
}
