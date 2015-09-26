using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

using Verse;
using Verse.AI;
using RimWorld;


namespace Vehicle
{
    public class JobDriver_CallDriver : JobDriver
    {
        //Constants
        private const TargetIndex DriverInd = TargetIndex.A;

        public JobDriver_CallDriver() : base() { }

        public override string GetReport()
        {
            Pawn driver = pawn.jobs.curJob.targetA.Thing as Pawn;

            string repString;
            if (driver != null)
                repString = "Waiting ".Translate(driver);
            else
                repString = "Waiting";
            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Pawn driver = CurJob.targetA.Thing as Pawn;
            Vehicle vehicle = pawn as Vehicle;

            ///
            //Set fail conditions
            ///

            Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("Mount"), vehicle, vehicle.MountPos);
            this.FailOnDowned(DriverInd);
            this.AddEndCondition(() => { return vehicle.IsMounted ? JobCondition.Succeeded : JobCondition.Ongoing; });

            ///
            //Define Toil
            ///



            Toil toilCallDriver = new Toil();
            toilCallDriver.initAction = () => 
            {
                driver.jobs.StartJob(jobNew, JobCondition.Incompletable);
            };

            Toil toilCheckDriverOnGoing = new Toil();
            toilCheckDriverOnGoing.initAction = () =>
            {
                if (driver.CurJob != jobNew)
                    this.EndJobWith(JobCondition.Incompletable);
            };
            Toil toilUnmounted = new Toil();
            toilUnmounted.initAction = () =>
            {
                vehicle.pather.StopDead();
            };
            toilUnmounted.defaultCompleteMode = ToilCompleteMode.Delay;
            toilUnmounted.defaultDuration = 120;


            ///
            //Toils Start
            ///

            //Reserve thing to be stored and storage cell 
            yield return Toils_Reserve.Reserve(DriverInd);

            yield return toilCallDriver;

            yield return toilCheckDriverOnGoing;

            yield return toilUnmounted;

            yield return Toils_Jump.Jump(toilCheckDriverOnGoing);
        }

    }
}
