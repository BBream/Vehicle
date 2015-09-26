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
    public class JobDriver_NotMounted : JobDriver
    {

        public JobDriver_NotMounted() : base() { }

        public override string GetReport()
        {
            string repString;
            repString = "Unmounted";
            return repString;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Vehicle vehicle = pawn as Vehicle;
            ///
            //Set fail conditions
            ///

            ///
            //Define Toil
            ///



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

            yield return toilUnmounted;
        }

    }
}

