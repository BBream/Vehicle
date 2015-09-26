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
    public class ThinkNode_ConditionalNeedDriver : ThinkNode_Conditional
    {
        public ThinkNode_ConditionalNeedDriver() : base() { }

        protected override bool Satisfied(Pawn pawn)
        {
            Vehicle vehicle = pawn as Vehicle;

            if (vehicle != null)
            {
                if (!vehicle.IsMounted && vehicle.callDriver)
                {
                    Log.Message("ThinkNode_ConditionalNeedDriver is True");
                    return true;
                }
                else
                    return false;
            }
            return false;
        }
    }
}