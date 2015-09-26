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
    class JobDriver_ClaimVehicle : JobDriver
    {
        //Constants
        private const TargetIndex TargetAInd = TargetIndex.A;
        private const int TickForClaim = 600;

        public JobDriver_ClaimVehicle() : base() { }

        public override string GetReport()
        {
            string repString;
            repString = "ReportClaiming".Translate(TargetThingA.LabelCap);

            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            ///
            //Set fail conditions
            ///

            this.FailOnBurningImmobile(TargetAInd);
            this.FailOnDestroyed(TargetAInd);

            ///
            //Define Toil
            ///




            ///
            //Toils Start
            ///

            //Reserve thing to be stored and storage cell 
            //yield return Toils_Reserve.Reserve(MountableInd, ReservationType.Total);

            //Mount on Target
            yield return Toils_Goto.GotoThing(TargetAInd, PathEndMode.ClosestTouch);

            yield return Toils_General.Wait(TickForClaim);

            Toil toilClaim = new Toil();
            toilClaim.initAction = () =>
            {
                Pawn actor = toilClaim.actor;
                TargetThingA.SetFaction(actor.Faction);
            };
            yield return toilClaim;
        }

    }
}
