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
    public class ThinkNode_ConditionalMounted : ThinkNode_Conditional
    {
        public ThinkNode_ConditionalMounted() : base() { }

        protected override bool Satisfied(Pawn pawn)
        {
            Vehicle vehicle = pawn as Vehicle;

            if (vehicle != null && vehicle.IsMounted)
            {
                Log.Message("ThinkNode_ConditionalMounted is True");
                return true;
            }
            return false;
        }
    }
}