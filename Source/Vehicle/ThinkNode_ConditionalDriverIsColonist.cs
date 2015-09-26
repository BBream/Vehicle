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
    public class ThinkNode_ConditionalDriverIsColonist : ThinkNode_Conditional
    {
        public ThinkNode_ConditionalDriverIsColonist() : base() { }

        protected override bool Satisfied(Pawn pawn)
        {
            Vehicle vehicle = pawn as Vehicle;

            Log.Message("In ThinkNode_ConditionalDriverIsColonist");
            if (vehicle != null)
                return vehicle.Driver.IsColonist;
            return false;
        }
    }
}