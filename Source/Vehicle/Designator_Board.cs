﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;

namespace Vehicle
{
    class Designator_Board : Designator
    {
        private const string txtCannotBoard = "CannotBoard";

        public Thing vehicle;
        public IntVec3 mountPos;

        public Designator_Board()
            : base()
        {
            useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Click;
        }

        public override int DraggableDimensions { get { return 1; } }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            List<Thing> thingList = loc.GetThingList();

            foreach (var thing in thingList)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null && (pawn.Faction == Faction.OfColony || (pawn.RaceProps.Animal && pawn.drafter != null)))
                    return true;
            }
            return new AcceptanceReport(txtCannotBoard.Translate());
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            List<Thing> thingList = c.GetThingList();
            foreach (var thing in thingList)
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null && (pawn.Faction == Faction.OfColony || (pawn.RaceProps.Animal && pawn.drafter != null)))
                {
                    Pawn driver = pawn;
                    Job jobNew = new Job(DefDatabase<JobDef>.GetNamed("Board"));
                    Find.Reservations.ReleaseAllForTarget(vehicle);
                    jobNew.targetA = vehicle;
                    jobNew.targetB = mountPos;
                    driver.drafter.TakeOrderedJob(jobNew);
                    break;
                }
            }
            DesignatorManager.Deselect();
        }
    }
}